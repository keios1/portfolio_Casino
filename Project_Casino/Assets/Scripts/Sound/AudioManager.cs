using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioSource bgmMain;
    public AudioSource bgmSub;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Mixer")]
    public AudioMixer mixer;

    private Coroutine fadeRoutine;

    private const string MASTER_KEY = "MASTER_VOLUME";
    private const string BGM_KEY = "BGM_VOLUME";
    private const string SFX_KEY = "SFX_VOLUME";

    [Header("Sound Libraries")]
    public SoundLibrary sounds;
    public SkillSoundLibrary skillSounds;
    public EnemySoundLibrary enemySounds;
    public StorySoundLibrary storySounds;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
    }

    // =========================
    // BGM 기본 재생
    // =========================
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmMain == null) return;

        if (bgmMain.clip == clip && bgmMain.isPlaying)
            return;

        bgmMain.clip = clip;
        bgmMain.loop = true;
        bgmMain.volume = 1f;
        bgmMain.Play();
    }

    // =========================
    // BGM 교체 (크로스페이드)
    // =========================
    public void CrossFadeBGM(AudioClip newClip, float duration = 1.5f)
    {
        if (newClip == null || bgmMain == null || bgmSub == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(CoCrossFade(newClip, duration));
    }

    private IEnumerator CoCrossFade(AudioClip newClip, float duration)
    {
        bgmSub.clip = newClip;
        bgmSub.volume = 0f;
        bgmSub.loop = true;
        bgmSub.Play();

        float time = 0f;
        float startMain = bgmMain.volume;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            bgmMain.volume = Mathf.Lerp(startMain, 0f, t);
            bgmSub.volume = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        bgmMain.Stop();

        var temp = bgmMain;
        bgmMain = bgmSub;
        bgmSub = temp;
    }

    // =========================
    // SFX
    // =========================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip == null || uiSource == null) return;
        uiSource.PlayOneShot(clip);
    }

    public void PlaySkillSFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
    // =========================
    // 볼륨 설정
    // =========================
    public void SetMasterVolume(float value)
    {
        SetMixerVolume("MasterVolume", value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float value)
    {
        SetMixerVolume("BGMVolume", value);
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SetMixerVolume("SFXVolume", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float bgm = PlayerPrefs.GetFloat(BGM_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        SetMixerVolume("MasterVolume", master);
        SetMixerVolume("BGMVolume", bgm);
        SetMixerVolume("SFXVolume", sfx);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (mixer == null) return;

        float clampedValue = Mathf.Clamp(value, 0.0001f, 1f);
        float db = Mathf.Log10(clampedValue) * 20f;
        mixer.SetFloat(parameterName, db);
    }
    public void StopBGM()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (bgmMain != null)
        {
            bgmMain.Stop();
            bgmMain.clip = null;
        }

        if (bgmSub != null)
        {
            bgmSub.Stop();
            bgmSub.clip = null;
        }
    }
}
