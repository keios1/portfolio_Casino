using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SkillSoundLibrary")]
public class SkillSoundLibrary : ScriptableObject
{
    [Header("Shield")]
    public AudioClip shield;
    public AudioClip shieldBreak;

    [Header("Coin Toss")]
    public AudioClip coinThrow;
    public AudioClip coinLand;
    public AudioClip coinSuccess;
    public AudioClip coinFail;

    [Header("Trump")]
    public AudioClip trumpCardMove;
    public AudioClip trumpCardFlip;
    public AudioClip trumpAttack;

    [Header("Skill Common")]
    public AudioClip skillCast;
    public AudioClip skillSuccess;
    public AudioClip skillFail;

    [Header("SkillWheel")]
    public AudioClip wheelSpin;

    [Header("SkillHeal")]
    public AudioClip healSuccess;

    [Header("SkillParry")]
    public AudioClip Reflect;
    public AudioClip Nullify;
    public AudioClip Absorb;
}
