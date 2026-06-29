using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Header("UI")]
    public AudioClip uiClick;
    public AudioClip uiOpen;
    public AudioClip uiClose;
    public AudioClip cashSound;

    [Header("Battle")]
    public AudioClip cardInven;
    public AudioClip cardDeck;
    public AudioClip cardUseDraw;
    public AudioClip diceRoll;
    public AudioClip diceClick;
    public AudioClip playerHit;
    public AudioClip heal;

    [Header("WheelMachine")]
    public AudioClip wheelRolling;
    public AudioClip wheelStop;
}
