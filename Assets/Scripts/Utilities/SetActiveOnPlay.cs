using UnityEngine;

namespace Utilities
{
    public class SetActiveOnPlay : MonoBehaviour
    {
        public bool deactivateSelfOnPlay;
        public GameObject[] activateObjectsOnPlay;
        private void Start()
        {
            if (deactivateSelfOnPlay)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject go in activateObjectsOnPlay)
            {
                go.SetActive(true);
            }
        }
    }
}
