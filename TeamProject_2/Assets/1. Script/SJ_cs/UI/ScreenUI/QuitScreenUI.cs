using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class QuitScreenUI : MonoBehaviour
{
    [Header("검은 패널")]
    public Image blackPanel;

    private bool isMoved;
    private bool isTweening;

    private void Start()
    {
        // 시작할 때 검은 화면
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 1f;
        blackPanel.color = c;

        // 천천히 페이드 아웃 → 로고 보임
        blackPanel
            .DOFade(0f, 1.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                blackPanel.gameObject.SetActive(false);
            });
    }

    public void QuitGame()
    {
        SoundManager.Instance.StopMusic();
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        blackPanel
            .DOFade(1f, 0.8f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                #if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
                #else
                                Application.Quit();
                #endif
            });
    }
}