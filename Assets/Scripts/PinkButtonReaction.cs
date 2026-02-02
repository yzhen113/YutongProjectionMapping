using UnityEngine;

/// <summary>
/// Attach to each pink button (or add at runtime when spawned).
/// Holds the reaction type assigned when the button was placed.
/// </summary>
public class PinkButtonReaction : MonoBehaviour
{
    public enum ReactionType
    {
        BlueExplosion,
        Soul,
        Dash
    }

    public ReactionType reactionType;
}
