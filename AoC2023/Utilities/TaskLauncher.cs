using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Utilities
{
    public class TaskLauncher
    {
        LimitedConcurrencyLevelTaskScheduler scheduler;
        CancellationToken cancelToken = new();

        public List<Task> crunchingTasks = new();
        public int taskLimit = 10;
        int threadLimit = 10;
        public readonly int mainThreadId;
        object taskListLockObj = new();

        public TaskLauncher(int _taskLimit = 16, int _threadLimit = 16)
        {
            taskLimit = _taskLimit;
            threadLimit = _threadLimit;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            scheduler = new LimitedConcurrencyLevelTaskScheduler(Math.Max(2, threadLimit));
        }

        public bool CanAddWork()
        {
            if (taskLimit == 0)
                return false;

            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
                return true;

            //somtimes allows more tasks than we want...but avoids locking
            if (crunchingTasks.Count < taskLimit)
                return true;

            return false;
        }

        public Task AddWork(Action work)
        {
            lock (taskListLockObj)
            {
                Task t = Task.Factory.StartNew(work, cancelToken, TaskCreationOptions.LongRunning, scheduler);
                crunchingTasks.Add(t);
                return t;
            }
        }

        public Task<T> AddWork<T>(Func<T> work)
        {
            lock (taskListLockObj)
            {
                Task<T> t = Task.Factory.StartNew(work, cancelToken, TaskCreationOptions.LongRunning, scheduler);
                crunchingTasks.Add(t);
                return t;
            }
        }

        public void ClearFinishedWork()
        {
            lock (taskListLockObj)
            {
                for (int i = 0; i < crunchingTasks.Count; i++)
                {
                    if (crunchingTasks[i].Exception != null)
                    {
                        Console.WriteLine("EXCEPTION THROWN IN TASK " + crunchingTasks[i].Id + " : " + crunchingTasks[i].Exception!.InnerException!.ToString());
                        Debug.WriteLine("EXCEPTION THROWN IN TASK " + crunchingTasks[i].Id + " : " + crunchingTasks[i].Exception!.InnerException!.ToString());
                    }
                    if (crunchingTasks[i].IsCompleted)
                    {
                        crunchingTasks.RemoveAt(i);
                        --i;
                    }
                }
            }
        }
    }
}
