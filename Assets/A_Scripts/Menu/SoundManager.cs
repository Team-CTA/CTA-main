using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("BGMVOL"))
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVOL");
        }
        if (PlayerPrefs.HasKey("SFXVOL"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVOL");
        }
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 사운드 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
        if (SceneManager.GetActiveScene().name == "Login")
        {
            bgmSource.clip = null;
        }
    }

    /// <param name="clip">재생할 BGM AudioClip</param>
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return; // 같은 BGM이면 재생 중지
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <param name="clip">재생할 SFX AudioClip</param>
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void ChangeBGM_Vol(Slider slider)
    {
        PlayerPrefs.SetFloat("BGMVOL", slider.value);
        bgmVolume = slider.value;
    }
    public void ChangeSFX_Vol(Slider slider)
    {
        PlayerPrefs.SetFloat("SFXVOL", slider.value);
        sfxVolume = slider.value;
    }
}
