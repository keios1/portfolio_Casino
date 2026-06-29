using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StorySceneController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("자동으로 넘어갈 시간 (초)")]
    public float autoSkipTime = 10f;

    [Tooltip("다음에 이동할 씬 이름")]
    public string nextSceneName = "Stage_1";

    private bool isTransitioning = false; 

    private void Start()
    {
        AudioManager.Instance?.StopBGM();

        StartCoroutine(AutoSkipRoutine());
    }

    private void Update()
    {
        if (!isTransitioning && Input.GetKeyDown(KeyCode.K))
        {
            GoToNextScene();
        }
    }

    private IEnumerator AutoSkipRoutine()
    {
        yield return new WaitForSeconds(autoSkipTime);

        if (!isTransitioning)
        {
            GoToNextScene();
        }
    }

    private void GoToNextScene()
    {
        isTransitioning = true; 
        SceneManager.LoadScene(nextSceneName);
    }
    private void PlayStorySFX(AudioClip clip)
    {
        if (AudioManager.Instance == null) return;
        if (clip == null) return;

        AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayCut1Bell()
    {
        PlayStorySFX(AudioManager.Instance.storySounds.Bell);
    }

    public void PlayCut1MailBox()
    {
        PlayStorySFX(AudioManager.Instance.storySounds.MailBox);
    }

    public void PlayCut3Letter()
    {
        PlayStorySFX(AudioManager.Instance.storySounds.Letter);
    }

    public void PlayCut4Boom()
    {
        PlayStorySFX(AudioManager.Instance.storySounds.Boom);
    }

    public void PlayCut5Welcome()
    {
        PlayStorySFX(AudioManager.Instance.storySounds.Welcome);
    }
}
