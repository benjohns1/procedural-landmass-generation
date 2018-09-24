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

        public bool autoRunResultCallbacksInEditor;

        private static Dispatcher dispatcher;

        public static void Run(Func<object> job, Action<object> callback)
        {
            dispatcher.Enqueue(new JobInfo(job, callback));
        }

        public static int RunResultCallbacks(int maxResults = 0)
        {
            JobResult[] results = dispatcher.GetResults(maxResults);
            foreach (JobResult result in results)
            {
                result.callback(result.parameter);
            }
            return results.Length;
        }

        public static int GetJobQueueCount()
        {
            return dispatcher.GetJobQueueCount();
        }

        public static int GetResultQueueCount()
        {
            return dispatcher.GetResultQueueCount();
        }

        private void Awake()
        {
            if (dispatcher == null)
            {
                dispatcher = new Dispatcher(checkJobQueueMilliseconds);
            }
        }

        private void OnValidate()
        {
            checkJobQueueMilliseconds = Mathf.Max(checkJobQueueMilliseconds, 10);
            maxResultsPerFrame = Mathf.Max(maxResultsPerFrame, 0);

            if (dispatcher == null)
            {
                dispatcher = new Dispatcher(checkJobQueueMilliseconds);
            }
        }

        private void Update()
        {
            RunResultCallbacks(maxResultsPerFrame);
        }
    }
}
