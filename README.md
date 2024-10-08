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
