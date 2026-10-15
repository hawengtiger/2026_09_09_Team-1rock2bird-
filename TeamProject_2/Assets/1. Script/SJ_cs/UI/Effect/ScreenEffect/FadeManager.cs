using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    // 어디서나 접근 가능하도록 싱글톤 구현
    public static FadeManager Instance { get; private set; }

    [Header("전체 화면을 덮을 검은 패널")]
    [SerializeField] private Image blackPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 화면이 검은색에서 서서히 밝아지는 이펙트 (씬 시작 시 주로 사용)
    /// </summary>
    public void FadeIn(float duration, Ease easeType, Action onComplete = null)
    {
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 1f;
        blackPanel.color = c;

        blackPanel
            .DOFade(0f, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                blackPanel.gameObject.SetActive(false);
                onComplete?.Invoke(); // 완료 후 실행할 함수가 있다면 실행
            });
    }

    /// <summary>
    /// 화면이 서서히 검은색으로 어두워지는 이펙트 (씬 전환 시 주로 사용)
    /// </summary>
    public void FadeOut(float duration, Ease easeType, Action onComplete = null)
    {
        blackPanel.gameObject.SetActive(true);

        Color c = blackPanel.color;
        c.a = 0f;
        blackPanel.color = c;

        blackPanel
            .DOFade(1f, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                onComplete?.Invoke(); // 완료 후 실행할 함수가 있다면 실행
            });
    }
}
