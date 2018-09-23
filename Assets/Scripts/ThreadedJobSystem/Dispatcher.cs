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

            private Queue<JobInfo> jobQueue = new Queue<JobInfo>();
            private Queue<JobResult> resultQueue = new Queue<JobResult>();

            private Thread dispatcherThread;

            public Dispatcher(int cycleTimeMilliseconds)
            {
                this.cycleTimeMilliseconds = cycleTimeMilliseconds;
                StartDispatchThread();
            }

            public void Enqueue(JobInfo jobInfo)
            {
                if (!dispatcherThread.IsAlive)
                {
                    // Restart dispatcher thread in case it halted
                    StartDispatchThread();
                }
                lock (jobQueue)
                {
                    jobQueue.Enqueue(jobInfo);
                }
            }

            public JobResult[] GetResults(int limit)
            {
                int resultCount = resultQueue.Count;
                if (resultCount <= 0)
                {
                    return new JobResult[0];
                }

                int numberOfResultsToRetrieve = limit > 0 ? Mathf.Min(resultCount, limit) : resultCount;
                JobResult[] results = new JobResult[numberOfResultsToRetrieve];

                lock (resultQueue)
                {
                    for (int i = 0; i < numberOfResultsToRetrieve; i++)
                    {
                        results[i] = resultQueue.Dequeue();
                    }
                }
                return results;
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
                object resultData = jobInfo.job();
                JobResult jobResult = new JobResult(jobInfo.callback, resultData);
                lock (resultQueue)
                {
                    resultQueue.Enqueue(jobResult);
                }
            }
        }
    }
}