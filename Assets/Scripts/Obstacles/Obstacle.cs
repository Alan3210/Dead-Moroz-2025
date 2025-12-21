using UnityEngine;


public class Obstacle : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private bool debugMode = true;
    private bool hasCollided = false;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private Vector3 particleOffset = Vector3.zero;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    [Header("VFX")]
    [SerializeField] private GameObject impactEffectPrefab;


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Spawn impact effect
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }

            // Trigger camera shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake();
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCollisionSound();
            }

            // Game Over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided) return;

        if (other.CompareTag("Player"))
        {
            hasCollided = true;

            if (debugMode)
            {
                Debug.Log($"Collision detected with: {gameObject.name} at position {transform.position}");
            }

            HandleCollision(other);
        }
    }

    private void HandleCollision(Collider player)
    {
        StopPlayer(player);

        SpawnImpactParticles(player.transform.position);

        TriggerCameraShake();

        PlayCollisionSound();

        TriggerGameOver();
    }


    private void SpawnImpactParticles(Vector3 collisionPoint)
    {
        if (impactParticlePrefab != null)
        {
            Vector3 spawnPosition = collisionPoint + particleOffset;
            GameObject particleObj = Instantiate(impactParticlePrefab, spawnPosition, Quaternion.identity);

            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(particleObj, 2f);
            }
            else
            {
                Destroy(particleObj, 2f);
            }
        }
    }

    private void StopPlayer(Collider player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.StopPlayer();
        }
        else
        {
            Debug.LogWarning("PlayerController not found on player object!");
        }
    }


    private void TriggerCameraShake()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(shakeDuration, shakeMagnitude);
        }
        else
        {
            Debug.LogWarning("CameraShake instance not found!");
        }
    }

    private void PlayCollisionSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCollisionSound();
        }
    }

    private void TriggerGameOver()
    {
        StartCoroutine(DelayedGameOver());
    }

    private System.Collections.IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(1f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }


    private void OnDisable()
    {
        hasCollided = false;
    }
}
