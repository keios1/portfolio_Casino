using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    public AudioClip sceneBGM;
    public bool playOnStart = true;

    private void Start()
    {
        if (playOnStart && sceneBGM != null)
        {
            AudioManager.Instance?.PlayBGM(sceneBGM);
        }
    }
}
