using System;
using System.Threading;

namespace GZipArchiver
{
    class ThreadPool
    {
        int numOfThreads = Environment.ProcessorCount;
        Thread[] threadPool;
        bool started = false;
        bool exceptionHappened = false;
        int tasksToFinish;

        PCQueue taskQueue = new PCQueue();
        AutoResetEvent resetEvent = new AutoResetEvent(false);

        public bool Started
        {
            get { return started; }
        }

        public bool ExceptionHappened
        {
            get { return exceptionHappened; }
        }

        public bool Finished
        {
            get { return tasksToFinish == 0; }
        }

        public ThreadPool(int tasksToFinish)
        {
            threadPool = new Thread[numOfThreads];
            this.tasksToFinish = tasksToFinish;
        }

        public void Start()
        {
            if (!started)
            {
                for (int i = 0; i < threadPool.Length; i++)
                {

                    threadPool[i] = new Thread(new ThreadStart(Run));
                    threadPool[i].Name = $"Worker_{i + 1}";
                    threadPool[i].Start();
                }
                started = true;
            }
        }

        //Let threads dequeue tasks while they're available
        private void Run()
        {
            while (!Finished && started)
            {
                if (taskQueue.IsEmpty() && !Finished)
                {
                    resetEvent.WaitOne();
                }
                try
                {
                    Task task = taskQueue.Dequeue();
                    task.StartTask();
                    Interlocked.Decrement(ref tasksToFinish);
                }
                catch (Exception ex)
                {
                    //If exception happend somwhere in task pocessing. Stop the pool.
                    exceptionHappened = true;
                    Console.WriteLine("Error occured. Stopping the program");
                    Console.WriteLine(ex.Message);
                    started = false;
                }
            }
        }

        public void AddTaskToQueue(Task task)
        {
            taskQueue.Enqueue(task);
            resetEvent.Set();
        }

        public void Stop()
        {
            lock (resetEvent)
            {
                if (started)
                {
                    started = false;
                    for (int i = 0; i < threadPool.Length; i++)
                    {
                        threadPool[i].Abort();
                    }
                }
            }
        }
    }
}
