using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ai = Object.FindAnyObjectByType<GooseAI>();
            if (ai != null) ai.SetPlayerSafe(true);
            Debug.Log("Player entered Safe Zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ai = Object.FindAnyObjectByType<GooseAI>();
            if (ai != null) ai.SetPlayerSafe(false);
            Debug.Log("Player exited Safe Zone");
        }
    }
}
