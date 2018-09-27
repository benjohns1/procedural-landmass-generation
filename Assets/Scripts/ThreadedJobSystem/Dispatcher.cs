using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace ThreadedJobSystem
{
    public partial class JobQueue
    {
        private class Dispatcher
        {
            private int cycleTimeMilliseconds;
            private bool forceSynchronous;

            private Queue<JobInfo> jobQueue = new Queue<JobInfo>();
            private Queue<JobResult> resultQueue = new Queue<JobResult>();
            private Queue<JobError> errorQueue = new Queue<JobError>();

            private Thread dispatcherThread;
            private bool IsDispatcherAlive
            {
                get
                {
                    if (dispatcherThread == null)
                    {
                        return false;
                    }
                    return dispatcherThread.IsAlive;
                }
            }

            public Dispatcher(int cycleTimeMilliseconds, bool forceSynchronous)
            {
                this.cycleTimeMilliseconds = cycleTimeMilliseconds;
                this.forceSynchronous = forceSynchronous;
                if (!forceSynchronous)
                {
                    StartDispatchThread();
                }
            }

            public void Enqueue(JobInfo jobInfo)
            {
                if (!IsDispatcherAlive)
                {
                    if (forceSynchronous)
                    {
                        RunJobSynchronously(jobInfo);
                        return;
                    }
                    else
                    {
                        // Restart dispatcher thread in case it halted
                        StartDispatchThread();
                    }
                }
                lock (jobQueue)
                {
                    jobQueue.Enqueue(jobInfo);
                }
            }

            public void RunJobSynchronously(JobInfo jobInfo)
            {
                object resultData = jobInfo.job();
                JobResult jobResult = new JobResult(jobInfo.callback, resultData);
                lock (resultQueue)
                {
                    resultQueue.Enqueue(jobResult);
                }
            }

            public JobResult[] GetResults(int limit)
            {
                return GetFromQueue(resultQueue, limit);
            }

            public JobError[] GetErrors(int limit)
            {
                return GetFromQueue(errorQueue, limit);
            }

            private T[] GetFromQueue<T>(Queue<T> queue, int limit)
            {
                int count = queue.Count;
                if (count <= 0)
                {
                    return new T[0];
                }

                int numberToRetrieve = limit > 0 ? Mathf.Min(count, limit) : count;
                T[] results = new T[numberToRetrieve];

                lock (resultQueue)
                {
                    for (int i = 0; i < numberToRetrieve; i++)
                    {
                        results[i] = queue.Dequeue();
                    }
                }
                return results;
            }

            public int GetJobQueueCount()
            {
                return jobQueue.Count;
            }

            public int GetResultQueueCount()
            {
                return resultQueue.Count;
            }

            public int GetErrorQueueCount()
            {
                return errorQueue.Count;
            }

            private void StartDispatchThread()
            {
                ThreadStart dispatchThread = delegate
                {
                    DispatcherLoop();
                };
                dispatcherThread = new Thread(dispatchThread);
                dispatcherThread.Start();
            }

            private void DispatcherLoop()
            {
                while (true)
                {
                    int jobCount = jobQueue.Count;
                    if (jobCount <= 0)
                    {
                        Thread.Sleep(cycleTimeMilliseconds);
                        continue;
                    }

                    // Get jobs from queue
                    JobInfo[] jobs = new JobInfo[jobCount];
                    lock (jobQueue)
                    {
                        for (int i = 0; i < jobCount; i++)
                        {
                            jobs[i] = jobQueue.Dequeue();
                        }
                    }

                    // Queue up jobs via thread pool
                    foreach (JobInfo job in jobs)
                    {
                        ThreadPool.QueueUserWorkItem(RunJob, job);
                    }
                }
            }

            private void RunJob(object rawJob)
            {
                JobInfo jobInfo = (JobInfo)rawJob;
                try
                {
                    RunJobSynchronously(jobInfo);
                }
                catch (System.Exception ex)
                {
                    JobError jobError = new JobError(jobInfo, ex);
                    lock (errorQueue)
                    {
                        errorQueue.Enqueue(jobError);
                    }
                }
            }
        }
    }
}