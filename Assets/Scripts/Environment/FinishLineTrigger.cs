using UnityEngine;

public class FinishLineTrigger : MonoBehaviour
{
    [Header("Finish Line Settings")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            Debug.Log("[FinishLineTrigger] 🏁 Player reached the finish line! Triggering victory!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }
            else
            {
                Debug.LogError("[FinishLineTrigger] GameManager.Instance is null!");
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.black;
    //    Gizmos.DrawCube(transform.position, GetComponent<BoxCollider>()?.size ?? Vector3.one * 10f);
    //}
}
