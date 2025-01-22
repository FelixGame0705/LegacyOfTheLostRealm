using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float maxHealth = 100f;
    public float maxSpeed = 3f;
    public LayerMask pushableLayer;
    public float damageCooldown = 1f;
}
