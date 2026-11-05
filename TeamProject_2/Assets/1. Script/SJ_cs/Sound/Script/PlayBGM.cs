
using UnityEngine;

/// <summary>
/// === | 브금 시작. | ===
/// </summary>
public class PlayBGM : MonoBehaviour
{
    [Header("실행할 브금")]
    public string bgmName;

    private void Start()
    {
        if (string.IsNullOrEmpty(bgmName)) return;

        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager가 존재하지 않습니다.");
            return;
        }

        Debug.Log($"BGM 실행 요청: {bgmName}");

        SoundManager.Instance.PlayMusic(bgmName);
    }
}
