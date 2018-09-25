using System;

namespace ThreadedJobSystem
{
    public partial class JobQueue
    {
        public struct JobError
        {
            public readonly JobInfo job;
            public readonly System.Exception exception;

            public JobError(JobInfo job, Exception exception)
            {
                this.job = job;
                this.exception = exception;
            }
        }
    }
}