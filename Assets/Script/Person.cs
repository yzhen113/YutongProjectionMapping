using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Player controller / game logic:
/// - Top-down view.
/// - Player collects chickens to win.
/// - Player must avoid bombs (hitting a bomb = lose).
/// </summary>
public class Person : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip rewardSound;
    public AudioClip losingSound;
    public AudioClip winningSound;
    public float rewardSoundVolume = 1f;
    public float losingSoundVolume = 1f;
    public float winningSoundVolume = 1f;

    [Header("Game Settings")]
    [FormerlySerializedAs("totalGems")]
    public int totalChickens = 4;

    private AudioSource audioSource;
    private int chickensCollected = 0;
    private bool gameEnded = false;
    private bool hasPlayedWinSound = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    // ===============================
    // COLLISION HANDLING FOR BOMBS / CHICKENS
    // ===============================
    void OnCollisionEnter(Collision collision)
    {
        if (gameEnded) return;

        GameObject other = collision.gameObject;

        // Bomb detection: hitting a bomb ends the game
        if (other.CompareTag("Bomb"))
        {
            EndGameLoss();
            return;
        }

        // Collision with a chicken should also collect it (for non-trigger colliders)
        if (other.CompareTag("Chicken"))
        {
            CollectChicken(other);
        }
    }

    // ===============================
    // TRIGGER HANDLING FOR CHICKENS
    // ===============================
    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        // Walking over a chicken collects it
        if (other.CompareTag("Chicken"))
        {
            CollectChicken(other.gameObject);
        }
    }

    // ===============================
    // CHICKEN COLLECTION
    // ===============================
    private void CollectChicken(GameObject chicken)
    {
        chicken.SetActive(false);
        Destroy(chicken);

        chickensCollected++;
        Debug.Log("Chickens collected: " + chickensCollected);

        PlayRewardSound();

        if (chickensCollected >= totalChickens && !hasPlayedWinSound)
        {
            hasPlayedWinSound = true;
            PlayWinningSound();
        }
    }

    // ===============================
    // GAME END (LOSS)
    // ===============================
    private void EndGameLoss()
    {
        if (gameEnded) return;

        gameEnded = true;

        Debug.Log("Player hit a bomb. Game Over.");

        PlayLosingSound();

        // Optional: freeze game
        Time.timeScale = 0f;
    }

    // ===============================
    // AUDIO
    // ===============================
    private void PlayRewardSound()
    {
        if (rewardSound != null)
            audioSource.PlayOneShot(rewardSound, rewardSoundVolume);
    }

    private void PlayWinningSound()
    {
        if (winningSound != null)
            audioSource.PlayOneShot(winningSound, winningSoundVolume);
    }

    private void PlayLosingSound()
    {
        if (losingSound != null)
            audioSource.PlayOneShot(losingSound, losingSoundVolume);
    }
}
