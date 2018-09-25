using System;

namespace ThreadedJobSystem
{
    public partial class JobQueue
    {
        public struct JobInfo
        {
            public readonly Func<object> job;
            public readonly Action<object> callback;

            public JobInfo(Func<object> job, Action<object> callback)
            {
                this.job = job;
                this.callback = callback;
            }
        }
    }
}