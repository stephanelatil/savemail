namespace Backend.Utils
{
    public class AsyncQueue<T> 
    {
        private readonly Queue<T> _queue;
        private readonly SemaphoreSlim _queueSem;
        public AsyncQueue()
        {
            this._queue = new Queue<T>();
            this._queueSem = new SemaphoreSlim(0);
        }

        public int Count => this._queueSem.CurrentCount;

        public void Enqueue(T item)
        {
            this._queue.Enqueue(item);
            this._queueSem.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
        {
            await this._queueSem.WaitAsync(cancellationToken);
            return this._queue.Dequeue();
        }

        public bool Contains(T obj)
        {
            return this._queue.Contains(obj);
        }
    }
}