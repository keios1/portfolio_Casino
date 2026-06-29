using UnityEngine;

[CreateAssetMenu(menuName = "Audio/EnemySoundLibrary")]
public class EnemySoundLibrary : ScriptableObject
{
    [Header("Enemy Common")]
    public AudioClip enemyHit;

    [Header("Slot Machine Enemy")]
    public AudioClip slotRolling;
    public AudioClip slotStop;

    [Header("Slot Machine Enemy Result")]
    public AudioClip slotSuccess;
    public AudioClip slotFail;
    public AudioClip slotJackpot;
    public AudioClip slotHeal;
    public AudioClip slotDead;

    [Header("Big Wheel Enemy")]
    public AudioClip wheelRolling;
    public AudioClip wheelStop;
    public AudioClip wheelHeal;
}
