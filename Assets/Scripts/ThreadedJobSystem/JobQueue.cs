using System;
using UnityEngine;

namespace ThreadedJobSystem
{
    public partial class JobQueue : MonoBehaviour
    {

        [Tooltip("Turn off async processing and force all tasks to run on the main thread immediately when queued (will need to force start a new dispatcher after changing this)")]
        public bool forceSynchronous;

        [Tooltip("Time (in milliseconds) between loops when the system checks if there are job requests in the input queue (will need to force start a new dispatcher after changing this)")]
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

        public static JobError[] GetErrors(int maxErrors = 0)
        {
           return dispatcher.GetErrors(maxErrors);
        }

        public static int GetJobQueueCount()
        {
            return dispatcher.GetJobQueueCount();
        }

        public static int GetResultQueueCount()
        {
            return dispatcher.GetResultQueueCount();
        }

        public static int GetErrorQueueCount()
        {
            return dispatcher.GetErrorQueueCount();
        }

        public void ForceCreateNewDispatcher()
        {
            CreateDispatcher(true);
        }

        private void Awake()
        {
            CreateDispatcher();
        }

        private void OnValidate()
        {
            checkJobQueueMilliseconds = Mathf.Max(checkJobQueueMilliseconds, 10);
            maxResultsPerFrame = Mathf.Max(maxResultsPerFrame, 0);
            CreateDispatcher();
        }

        private void CreateDispatcher(bool force = false)
        {
            if (dispatcher == null || force)
            {
                dispatcher = new Dispatcher(checkJobQueueMilliseconds, forceSynchronous);
            }
        }

        private void Update()
        {
            RunResultCallbacks(maxResultsPerFrame);
        }
    }
}
