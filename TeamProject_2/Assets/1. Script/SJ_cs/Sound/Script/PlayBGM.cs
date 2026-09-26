using UnityEngine;

/// <summary>
/// === | 브금 시작. | ===
/// </summary>
public class PlayBGM : MonoBehaviour
{
    public string bgmName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (bgmName == null) return;

        SoundManager.Instance.PlayMusic(bgmName);   
    }
}
