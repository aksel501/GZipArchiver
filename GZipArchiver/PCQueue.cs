using System.Collections.Generic;
using System.Threading;

namespace GZipArchiver
{
    class PCQueue
    {
        private Queue<Task> taskQueue = new Queue<Task>();
        private object locker = new object();

        public void Enqueue(Task task)
        {
            lock (locker)
            {
                taskQueue.Enqueue(task);
                Monitor.PulseAll(locker);
            }
        }

        public Task Dequeue()
        {
            lock (locker)
            {
                while (taskQueue.Count == 0)
                {
                    Monitor.Wait(locker);
                }
                return taskQueue.Dequeue();
            }
        }

        public bool IsEmpty()
        {
            return taskQueue.Count == 0 ? true : false;
        }
    }
}
