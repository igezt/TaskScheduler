# Distributed Task Scheduler

This is an attempt at making a distributed task scheduler using .NET's ASP.NET framework.

The idea is to have multiple instances of the same ASP.NET server interface with one another and coordinate execution of tasks in a distributed and parallel way.

---

## Added Features

- Leader Election

---

## Features in progress

### General server-side features

- Using gRPC instead of HTTP api calls
- Service discovery (as an automatic way of creating a list of instances)
- Authorization and Authentication
- Use of a database to save tasks and their progress
  - Also used to schedule tasks
- Implement queues for better concurrency

### Usage

- Execution of tasks

### Parallization Optimizations and Correctness

- Implementing more algorithms for Leader Election
- Instantiating instances using dynamically created ids instead of hard-coded ids
- Consensus Protocols

## Leader Election

Currently, since the number of nodes (n) is known, we use the simplest algorithm for known number of nodes to elect a leader

1. Upon startup or upon leader failure, all nodes will send (flood) their ids to other nodes
2. Upon reception of another node's id, select the larger id as the leader
3. At the end of n^2 network calls, each node will have elected the largest id seen as the leader id.

Leader election is useful to ensure that even upon failure of the leader node, our system functions properly.
Note that whenever we start up a new node, we have to run leader election again because the new node's id could be larger than our current leader node's id.

## New Leader Election

### On start up

1. All nodes will attempt to acquire a session and the lock on consul
2. Only the first node will get access to the KV pair behind the lock
   a. The first node will leave behind its id in the KV pair
   b. The first node will then release the lock for all other nodes to enter
3. All nodes who enter the lock to observe the KV pair will see the id of the first node which is now the leader

### On leader failure

1. All nodes will have a heartbeat to the leader as well as a check to see if the node is still the leader
   a. The heartbeat is to ensure that nodes will immediately know when the leader node has failed
   b. The check is if the leader node somehow comes back online in the time between leader failure and leader checking
   i. Without check: leader failure -> all other nodes deem leader as failed -> new leader is elected -> old leader comes back online -> late node sees old leader is still online -> Split brain will occur
   ii. With check: leader failure -> all other nodes deem leader as failed -> new leader is elected -> old leader comes back online -> late node sees old leader is still online -> checks if old leader recognizes itself as leader (returns false) -> goes to check KV Pair to find new leader
2. All idle nodes will then attempt to acquire a session and the lock on consul
3. First node will become the leader
4. All nodes that are late to the election will still be able to find the new leader id in the KV pair

### Multiple leaders

Change the KV pair from one leaderId to an array of multiple leaderIds where nodes that enter can leave their id in if there is a space.
