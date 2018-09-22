using System;
using UnityEngine;

namespace ThreadedJobSystem
{
    public partial class JobSystem : MonoBehaviour
    {
        public int dispatcherCycleTimeMilliseconds = 100;

        private static JobSystem instance;
        private Dispatcher dispatcher;

        private void Awake()
        {
            instance = FindObjectOfType<JobSystem>();
            if (instance == this)
            {
                dispatcher = new Dispatcher(dispatcherCycleTimeMilliseconds);
            }
        }

        public static void Run(Func<object> job, Action<object> callback)
        {
            instance.dispatcher.Enqueue(new JobInfo(job, callback));
        }

        private void Update()
        {
            JobResult[] results = instance.dispatcher.GetResults();
            foreach (JobResult result in results)
            {
                result.callback(result.parameter);
            }
        }
    }
}
