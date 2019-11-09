namespace Vote.Workers
{
    public interface IQueueWorker {
        void Start();
        string Url { get; }
        string Description { get; }
    }
}