using UnityEngine;

/// <summary>
/// Anything that can take damage (the player now, maybe enemies or boxes later).
/// The enemy only knows "this thing is IDamageable", not that it is the player.
/// </summary>
public interface IDamageable
{
    /// <param name="amount">How much health to remove.</param>
    /// <param name="sourcePosition">Where the hit came from (used for knockback).</param>
    void TakeDamage(int amount, Vector2 sourcePosition);
}
