using UnityEngine;
using UnityEditor;

namespace ThreadedJobSystem
{
    [CustomEditor(typeof(JobQueue))]
    public class JobQueueEditor : Editor
    {
        private JobQueue jobQueue;
        private System.Timers.Timer jobCheckTimer;
        private int jobQueueCount = 0;
        private int jobResultCount = 0;
#if UNITY_EDITOR
        private void OnEnable()
        {
            jobQueue = (JobQueue)target;

            jobCheckTimer = new System.Timers.Timer(500);
            jobCheckTimer.Elapsed += JobCheckTimer_Elapsed;
            jobCheckTimer.AutoReset = true;
            jobCheckTimer.Enabled = true;
        }

        private void JobCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int newJobCount = JobQueue.GetJobQueueCount();
            if (newJobCount != jobQueueCount)
            {
                jobQueueCount = newJobCount;
                Repaint();
            }

            int newResultCount = JobQueue.GetResultQueueCount();
            if (newResultCount != jobResultCount)
            {
                jobResultCount = newResultCount;
                Repaint();
            }
        }
#endif

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Label("Queued jobs: " + jobQueueCount);
            GUILayout.Label("Results waiting: " + jobResultCount);
            if (jobResultCount > 0
                && (jobQueue.autoRunResultCallbacksInEditor || GUILayout.Button("Run Result Callbacks")))
            {
                RunResultCallbacks();
            }
        }

        private void RunResultCallbacks()
        {
            int resultCount = JobQueue.RunResultCallbacks();
            Debug.Log("JobQueue: Ran " + resultCount + " result callbacks ");
        }
    }
}