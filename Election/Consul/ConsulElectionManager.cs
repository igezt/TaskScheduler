using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using TaskScheduler.Communication;

namespace TaskScheduler.Election.Consul
{
    public class ConsulElectionManager : IElectionManager
    {
        private readonly IConsulClient _consul;
        private readonly ILogger<ConsulElectionManager> _logger;
        private readonly INodeCommunication _nodeCommunication;

        // private int? leaderId;

        private readonly int _nodeId;

        private readonly string _leaderKey = "leaderKey";

        private readonly string _host = "localhost";
        private readonly string _serviceName = "TaskSchedulerNode";
        private readonly int _port;

        private bool isRegistered = false;

        public ConsulElectionManager(
            IConsulClient consulClient,
            ILogger<ConsulElectionManager> logger,
            INodeCommunication nodeCommunication
        )
        {
            _consul = consulClient;
            _logger = logger;
            // leaderId = null;
            _nodeCommunication = nodeCommunication;
            if (
                Environment.GetEnvironmentVariable("NODE_ID") == null
                || !int.TryParse(Environment.GetEnvironmentVariable("NODE_ID"), out _nodeId)
            )
            {
                throw new ArgumentException("No Node Id provided.");
            }

            var port = Environment.GetEnvironmentVariable("PORT");
            if (!string.IsNullOrEmpty(port))
                _port = int.Parse(port);
        }

        public async Task<bool> IsLeader(CancellationToken token)
        {
            var leaderId = await RetrieveLeaderId(token);

            return leaderId.HasValue && leaderId.Value == _nodeId;
        }

        public Task ReleaseLeadership(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /**
        * Acquire leadership by leaving leader id in the KV pair.
        * If there is already a leader ID in the kv pair, then update leaderId field to that value.
        */
        public async Task<bool> TryAcquireLeadership(CancellationToken token)
        {
            if (!isRegistered)
            {
                await registerNode();
            }

            // Lock acquired
            var leaderId = await RetrieveLeaderId(token);
            var isLeaderAliveAndExist =
                leaderId.HasValue && await _nodeCommunication.PollNode(leaderId.Value);
            if (isLeaderAliveAndExist)
            {
                _logger.LogInformation(
                    "{string} {time} An active leader is found; No leadership attempt required.",
                    Environment.GetEnvironmentVariable("NODE_ID"),
                    DateTimeOffset.Now
                );
                return leaderId == _nodeId;
            }

            var consulLock = new ConsulLock(_consul);
            var kvPair = new KVPair(_leaderKey) { Value = BitConverter.GetBytes(_nodeId) };

            while (!token.IsCancellationRequested)
            {
                var acquired = await consulLock.AcquireLock(token);
                _logger.LogInformation("Attempt to acquire leadership lock: {Acquired}", acquired);

                _logger.LogInformation(
                    "All listed keys: {string}",
                    (await _consul.KV.List("", token))
                        .Response.Select(kvp => kvp.Key + "=" + kvp.Value)
                        .ToArray()
                );

                // Lock not acquired
                if (!acquired)
                {
                    var waitingTime = TimeSpan.FromSeconds(5);
                    _logger.LogInformation(
                        "Attempting to acquire leadership in {seconds} seconds.",
                        waitingTime.TotalSeconds
                    );
                    await Task.Delay(waitingTime, token);
                    continue;
                }

                // Lock acquired
                leaderId = await RetrieveLeaderId(token);
                isLeaderAliveAndExist =
                    leaderId.HasValue && await _nodeCommunication.PollNode(leaderId.Value);
                if (isLeaderAliveAndExist)
                {
                    // There is already an active leader, release the lock and return.
                    _logger.LogInformation(
                        "An active leader is found; releasing leadership attempt."
                    );
                    await consulLock.ReleaseLock(token);
                    return leaderId == _nodeId;
                }
                // There is no leader ID in the KV pair, the leaderID was null to begin with or the leader is offline
                bool putSuccess = (await _consul.KV.Put(kvPair, token)).Response;
                if (putSuccess)
                {
                    _logger.LogInformation(
                        "Successfully set leadership with node ID: {NodeId}",
                        _nodeId
                    );
                    await consulLock.ReleaseLock(token);
                    return true;
                }
                _logger.LogWarning("Failed to set leadership with put operation. Retrying...");
            }
            await consulLock.ReleaseLock(token);
            _logger.LogInformation("Leadership attempt canceled; released lock.");
            return false;
        }

        private async Task<int?> RetrieveLeaderId(CancellationToken token)
        {
            var queryOptions = new QueryOptions { Consistency = ConsistencyMode.Consistent };
            var res = await _consul.KV.Get(_leaderKey, queryOptions, token);

            if (
                res.Response != null
                && res.Response.Value != null
                && res.Response.Value.Length != 0
            )
            {
                // There is a leaderId
                var leaderIdBytes = res.Response.Value;
                var leaderId = BitConverter.ToInt32(leaderIdBytes, 0);
                return leaderId;
            }
            return null;
        }

        private async Task<bool> registerNode()
        {
            var heartbeatUrl = $"http://host.docker.internal:{_port}/api/heartbeat";
            var registration = new AgentServiceRegistration
            {
                ID = _nodeId.ToString(),
                Name = _serviceName,
                Address = _host,
                Port = _port,
                Check = new AgentServiceCheck()
                {
                    HTTP = heartbeatUrl,
                    Interval = TimeSpan.FromSeconds(0.5),
                    Timeout = TimeSpan.FromSeconds(5),
                },
                Tags = [$"Node {_nodeId}"],
            };
            await _consul.Agent.ServiceRegister(registration);

            _logger.LogInformation(
                "{string} registered with Consul on {string}:{int}. Heartbeat url: {string}",
                _serviceName,
                _host,
                _port,
                heartbeatUrl
            );

            isRegistered = true;
            return true;
        }
    }

    class ConsulLock(IConsulClient consul)
    {
        private readonly IConsulClient _consul = consul;
        private KVPair? _lock;
        private readonly string _lockName = "leaderLock";

        public async Task<bool> AcquireLock(CancellationToken token)
        {
            var session = await _consul.Session.Create(
                new SessionEntry
                {
                    TTL = TimeSpan.FromSeconds(10),
                    Behavior =
                        SessionBehavior.Delete // Automatically delete session on TTL expiration
                    ,
                }
            );
            _lock = new KVPair(_lockName) { Session = session.Response };
            var acquired = await _consul.KV.Acquire(_lock, token);
            return acquired.Response;
        }

        public async Task<bool> ReleaseLock(CancellationToken token)
        {
            return (await _consul.KV.Release(_lock, token)).Response;
        }
    }
}
