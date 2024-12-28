using UnityEngine;

public class RankingSound : MonoBehaviour
{
    [SerializeField] SoundManager soundManager = null;

    // Update is called once per frame
    void Update()
    {
        if (soundManager == null)
        {
            soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        }
    }
    public void PlaySfx(AudioClip clip)
    {
        soundManager.PlaySFX(clip);
    }
}
