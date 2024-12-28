using UnityEngine;

public class RankingSound : MonoBehaviour
{
    [SerializeField] SoundManager soundManager = null;
    [SerializeField] AudioClip mainSound;

    // Update is called once per frame
    void Update()
    {
        if (soundManager == null)
        {
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            soundManager.PlayBGM(mainSound);
        }
    }
    public void PlaySfx(AudioClip clip)
    {
        soundManager.PlaySFX(clip);
    }
}
