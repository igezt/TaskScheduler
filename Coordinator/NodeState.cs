namespace TaskScheduler.Coordinator
{
    public enum NodeState
    {
        IDLE,
        RUNNING_TASK,
        CANDIDATE,
        AWAITING_LEADER,
    }
}
