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
        private int jobErrorCount = 0;
        private GUIStyle errorStyle;
        private readonly GUILayoutOption[] layout = new GUILayoutOption[0];

        private void OnEnable()
        {
            jobQueue = (JobQueue)target;

            jobCheckTimer = new System.Timers.Timer(500);
            jobCheckTimer.Elapsed += JobCheckTimer_Elapsed;
            jobCheckTimer.AutoReset = true;
            jobCheckTimer.Enabled = true;

            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

            errorStyle = new GUIStyle
            {
                normal = new GUIStyleState
                {
                    textColor = Color.red
                }
            };
        }

        private void EditorApplication_playModeStateChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingEditMode)
            {
                jobCheckTimer.Enabled = false;
            }
            else if (stateChange == PlayModeStateChange.EnteredEditMode)
            {
                jobCheckTimer.Enabled = true;
            }
        }

        private void JobCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int newJobCount = JobQueue.GetJobQueueCount();

            bool repaint = false;
            if (newJobCount != jobQueueCount)
            {
                jobQueueCount = newJobCount;
                repaint = true;
            }

            int newResultCount = JobQueue.GetResultQueueCount();
            if (newResultCount != jobResultCount)
            {
                jobResultCount = newResultCount;
                repaint = true;
            }

            int newErrorCount = JobQueue.GetErrorQueueCount();
            if (newErrorCount != jobErrorCount)
            {
                jobErrorCount = newErrorCount;
                repaint = true;
            }

            if (repaint)
            {
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Force Create New Dispatcher"))
            {
                jobQueue.ForceCreateNewDispatcher();
            }
            DrawDefaultInspector();
            GUILayout.Label("Queued jobs: " + jobQueueCount);
            GUILayout.Label("Results waiting: " + jobResultCount);
            if (jobErrorCount > 0)
            {
                GUILayout.Label("Errors: " + jobErrorCount, errorStyle, layout);
            }
            if (jobResultCount > 0 && jobCheckTimer.Enabled
                && (jobQueue.autoRunResultCallbacksInEditor || GUILayout.Button("Run Result Callbacks")))
            {
                RunResultCallbacks();
            }
            if (jobErrorCount > 0 && jobCheckTimer.Enabled
                && (jobQueue.autoRunResultCallbacksInEditor || GUILayout.Button("Capture Errors")))
            {
                CaptureErrors();
            }
        }

        private void RunResultCallbacks()
        {
            int resultCount = JobQueue.RunResultCallbacks();
            Debug.Log("JobQueue: Ran " + resultCount + " result callbacks ");
        }

        private void CaptureErrors()
        {
            JobQueue.JobError[] errors = JobQueue.GetErrors();
            Debug.Log("JobQueue: " + errors.Length + " failed jobs!");
            for (int i = 0; i < errors.Length; i++)
            {
                Debug.LogError("(" + i + ") " + errors[i].exception.ToString());
            }
        }
    }
}