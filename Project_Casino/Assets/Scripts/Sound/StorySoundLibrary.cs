using UnityEngine;

[CreateAssetMenu(menuName = "Audio/StorySoundLibrary")]
public class StorySoundLibrary : ScriptableObject
{
    [Header("Cut1")]
    public AudioClip Bell;
    public AudioClip MailBox;

    [Header("Cut3")]
    public AudioClip Letter;

    [Header("Cut4")]
    public AudioClip Boom;

    [Header("Cut5")]
    public AudioClip Welcome;
}
