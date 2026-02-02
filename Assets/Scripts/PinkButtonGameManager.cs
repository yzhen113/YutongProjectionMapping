using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages pink buttons: spawns them randomly on a plane, assigns random reactions,
/// and triggers reactions when the motion-tracked capsule is within range (Vector3.Distance).
/// Soul = lose, Dash = win.
/// </summary>
public class PinkButtonGameManager : MonoBehaviour
{
    [Header("Spawn")]
    [Tooltip("Plane (Quad) defining spawn area. Uses transform position and lossyScale for bounds.")]
    public Transform spawnPlane;
    [Tooltip("Pink button prefab to instantiate.")]
    public GameObject pinkButtonPrefab;
    [Tooltip("Number of pink buttons to spawn on game start.")]
    public int buttonCount = 8;
    [Tooltip("Minimum distance between button centers (cell size). Buttons are placed in a grid so they never overlap.")]
    public float minSpacingBetweenButtons = 0.5f;
    [Tooltip("How far above the plane surface to spawn the buttons (in world units).")]
    public float buttonHeightAbovePlane = 0.02f;

    [Header("Trigger")]
    [Tooltip("Motion-tracked capsule (e.g. OptiTrack rigid body or bone transform).")]
    public Transform motionTrackedCapsule;
    [Tooltip("Distance within which the capsule is considered 'on' the button.")]
    public float triggerDistance = 0.5f;

    [Header("Reaction prefabs (URP)")]
    public GameObject blueExplosionUrpPrefab;
    public GameObject soulUrpPrefab;   // e.g. Blue Death Particle URP
    public GameObject dashUrpPrefab;

    [Header("Game state")]
    [Tooltip("If true, game has ended (win or lose).")]
    public bool gameEnded;
    [Tooltip("If true, reload the current scene after this many seconds when game ends.")]
    public bool reloadSceneOnEnd;
    public float reloadDelay = 2f;

    private readonly List<PinkButtonReaction> _activeButtons = new List<PinkButtonReaction>();

    /// <summary>Rotation so assets lie flat on the plane and are visible from bird's eye view (90° around X).</summary>
    private static readonly Quaternion BirdEyeRotation = Quaternion.Euler(90f, 0f, 0f);

    private void Start()
    {
        if (spawnPlane == null || pinkButtonPrefab == null)
        {
            Debug.LogWarning("PinkButtonGameManager: Assign spawnPlane and pinkButtonPrefab in the Inspector.");
            return;
        }

        SpawnButtons();
    }

    private void Update()
    {
        if (gameEnded || motionTrackedCapsule == null) return;

        for (int i = _activeButtons.Count - 1; i >= 0; i--)
        {
            PinkButtonReaction button = _activeButtons[i];
            if (button == null) { _activeButtons.RemoveAt(i); continue; }

            float distance = Vector3.Distance(motionTrackedCapsule.position, button.transform.position);
            if (distance < triggerDistance)
            {
                TriggerReaction(button);
                _activeButtons.RemoveAt(i);
            }
        }
    }

    private void SpawnButtons()
    {
        if (spawnPlane == null || pinkButtonPrefab == null) return;

        Vector3 scale = spawnPlane.lossyScale;
        float halfX = scale.x * 0.5f;
        float halfZ = scale.z * 0.5f;
        float cellSize = Mathf.Max(0.01f, minSpacingBetweenButtons);

        // Grid-based placement: one button per cell so overlap is impossible
        int cellsX = Mathf.Max(1, Mathf.FloorToInt((2f * halfX) / cellSize));
        int cellsZ = Mathf.Max(1, Mathf.FloorToInt((2f * halfZ) / cellSize));
        int totalCells = cellsX * cellsZ;

        if (buttonCount > totalCells)
        {
            Debug.LogWarning($"PinkButtonGameManager: Plane fits at most {totalCells} buttons at current spacing. Spawning {totalCells} instead of {buttonCount}. Enlarge the plane or reduce minSpacingBetweenButtons.");
            buttonCount = totalCells;
        }

        // Build list of cell indices and shuffle so placement is random
        var cellIndices = new List<int>();
        for (int i = 0; i < totalCells; i++)
            cellIndices.Add(i);
        Shuffle(cellIndices);

        for (int i = 0; i < buttonCount; i++)
        {
            int cellIndex = cellIndices[i];
            int ix = cellIndex % cellsX;
            int iz = cellIndex / cellsX;

            // Cell center in local space (plane's local XZ)
            float localX = -halfX + (ix + 0.5f) * cellSize;
            float localZ = -halfZ + (iz + 0.5f) * cellSize;
            // Random offset within cell so they're not on a perfect grid (keep away from cell edges)
            float margin = cellSize * 0.25f;
            localX += Random.Range(-margin, margin);
            localZ += Random.Range(-margin, margin);

            Vector3 localOffset = new Vector3(localX, 0f, localZ);
            Vector3 worldPos = spawnPlane.TransformPoint(localOffset) + spawnPlane.up * buttonHeightAbovePlane;

            GameObject instance = Instantiate(pinkButtonPrefab, worldPos, BirdEyeRotation, spawnPlane);
            var reaction = instance.GetComponent<PinkButtonReaction>();
            if (reaction == null)
                reaction = instance.AddComponent<PinkButtonReaction>();

            reaction.reactionType = (PinkButtonReaction.ReactionType)Random.Range(0, 3);
            _activeButtons.Add(reaction);
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void TriggerReaction(PinkButtonReaction button)
    {
        Vector3 pos = button.transform.position;
        GameObject vfxPrefab = GetPrefabForReaction(button.reactionType);
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, pos, BirdEyeRotation);
            Destroy(vfx, 5f);
        }

        if (button.gameObject != null)
            Destroy(button.gameObject);

        switch (button.reactionType)
        {
            case PinkButtonReaction.ReactionType.Soul:
                OnPlayerLose();
                break;
            case PinkButtonReaction.ReactionType.Dash:
                OnPlayerWin();
                break;
            case PinkButtonReaction.ReactionType.BlueExplosion:
                break;
        }
    }

    private GameObject GetPrefabForReaction(PinkButtonReaction.ReactionType type)
    {
        switch (type)
        {
            case PinkButtonReaction.ReactionType.BlueExplosion: return blueExplosionUrpPrefab;
            case PinkButtonReaction.ReactionType.Soul: return soulUrpPrefab;
            case PinkButtonReaction.ReactionType.Dash: return dashUrpPrefab;
            default: return null;
        }
    }

    private void OnPlayerLose()
    {
        gameEnded = true;
        Debug.Log("Game Over - You triggered Soul (lose).");
        EndGame(lost: true);
    }

    private void OnPlayerWin()
    {
        gameEnded = true;
        Debug.Log("You Win - You triggered Dash!");
        EndGame(lost: false);
    }

    private void EndGame(bool lost)
    {
        if (reloadSceneOnEnd && reloadDelay > 0f)
            Invoke(nameof(ReloadScene), reloadDelay);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
