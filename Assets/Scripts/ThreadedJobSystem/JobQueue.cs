using System;
using UnityEngine;

namespace ThreadedJobSystem
{
    public partial class JobQueue : MonoBehaviour
    {
        [Tooltip("Time (in milliseconds) between loops when the system checks if there are job requests in the input queue")]
        public int checkJobQueueMilliseconds = 100;

        [Tooltip("Limit the maximum number of result callbacks to invoke by the main thread per frame. Set to 0 for no limit (not recommended).")]
        public int maxResultsPerFrame = 3;

        private static JobQueue instance;
        private Dispatcher dispatcher;

        private void Awake()
        {
            instance = FindObjectOfType<JobQueue>();
            if (instance == this)
            {
                dispatcher = new Dispatcher(checkJobQueueMilliseconds);
            }
        }

        private void OnValidate()
        {
            checkJobQueueMilliseconds = Mathf.Max(checkJobQueueMilliseconds, 10);
            maxResultsPerFrame = Mathf.Max(maxResultsPerFrame, 0);
        }

        public static void Run(Func<object> job, Action<object> callback)
        {
            instance.dispatcher.Enqueue(new JobInfo(job, callback));
        }

        private void Update()
        {
            JobResult[] results = instance.dispatcher.GetResults(maxResultsPerFrame);
            foreach (JobResult result in results)
            {
                result.callback(result.parameter);
            }
        }
    }
}
