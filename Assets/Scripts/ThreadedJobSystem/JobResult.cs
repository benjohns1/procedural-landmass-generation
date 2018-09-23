using System;

namespace ThreadedJobSystem
{
    public partial class JobQueue
    {
        private struct JobResult
        {
            public readonly Action<object> callback;
            public readonly object parameter;

            public JobResult(Action<object> callback, object parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }
    }
}