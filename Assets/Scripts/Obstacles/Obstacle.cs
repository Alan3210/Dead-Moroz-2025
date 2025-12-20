using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private bool debugMode = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (debugMode)
            {
                Debug.Log($"Collision detected with: {gameObject.name} at position {transform.position}");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.LogError("GameManager instance not found!");
            }
        }
    }
}
