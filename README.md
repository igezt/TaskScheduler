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

# Requirements for task execution

From here on, tasks refer to the original description of a workable task that a node can run, while jobs refer to the singular instance of a task being executed.

e.g. A task can be SendTelegramMessageTask which sends a telegram message from a bot to all subscribers.
The task can be then scheduled for a daily execution.
A job is then the SendTelegramMessageTask that executed on a specific day (e.g. 16/11/2024 11pm).

Let M > N mean that task M has to occur before task N.

- Ability to execute different tasks types
  - One-time single task execution
  - Scheduled single task execution
  - Scheduled and one-time chained tasks
    - e.g. Cases like A > B, C > D should execute in order where A is before B and C, and B and C execute before D
  - Scheduled and one-time batched tasks/jobs
    - e.g. Batching jobs A, B and C ensure that all 3 jobs only execute to completion if all three jobs are executable.
- Capabilities of tasks
  - Allow to define retry logic on failure of task
  - Allow for define retry logic on failure of node
  - Allow for auto-queuing of scheduled tasks and child tasks upon completion of parent tasks
  - Able to track tasks based on
    - Who created a task created and when
    - Who scheduled a task and when
    - State of the task **job** (Pending, In progress, Completed)
    - Which task depends on it (e.g. if A > B > C, then checking C, we can see A and B as its dependencies)
  - Allow to define retry logic for chained tasks
    - e.g. If one task fails, should the rest of the tasks be executed or should the task be retried

Current focuses:

- One time tasks
- Retries of nodes from transient failures
