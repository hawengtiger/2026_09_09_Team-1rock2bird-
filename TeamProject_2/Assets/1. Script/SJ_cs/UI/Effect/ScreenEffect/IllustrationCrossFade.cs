
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using DG.Tweening;

public class IllustrationCrossFade : MonoBehaviour
{
    [Header("일러스트 화면")]
    public GameObject illustrationRoot;

    [Header("일러스트 전체 페이드")]
    public CanvasGroup illustrationCanvasGroup;

    [Header("현재 일러스트")]
    public Image imageA;

    [Header("다음 일러스트")]
    public Image imageB;

    [Header("순서대로 출력할 일러스트")]
    public Sprite[] illustrations;

    [Header("일러스트 유지 시간")]
    public float displayDuration = 2f;

    [Header("일러스트 전환 시간")]
    public float fadeDuration = 1f;

    [Header("전체 등장 / 퇴장 시간")]
    public float screenFadeDuration = 1f;

    [Header("X키로 일러스트 스킵")]
    public bool canSkip = true;

    [Header("일러스트 종료 후 실행")]
    public UnityEvent onIllustrationFinished;

    private Sequence sequence;
    private Tween screenFadeTween;

    private bool isPlaying = false;
    private bool isPrepared = false;

    // X키 입력이 다음 연출로 넘어가지 않도록
    private int startedFrame = -1;

    private void Awake()
    {
        if (illustrationRoot != null)
            illustrationRoot.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying || !canSkip)
            return;

        // 일러스트를 시작한 프레임에는 스킵 방지
        if (Time.frameCount == startedFrame)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.xKey.wasPressedThisFrame)
        {
            Skip();
        }
    }

    // 첫 일러스트 준비
    public void Prepare()
    {
        if (imageA == null ||
            imageB == null ||
            illustrationCanvasGroup == null ||
            illustrations == null ||
            illustrations.Length == 0 ||
            illustrations[0] == null)
        {
            Debug.LogWarning("일러스트 설정이 부족합니다.");
            isPrepared = false;
            return;
        }

        Stop();

        if (illustrationRoot != null)
            illustrationRoot.SetActive(true);

        imageA.gameObject.SetActive(true);
        imageB.gameObject.SetActive(true);

        imageA.sprite = illustrations[0];
        imageA.color = Color.white;

        imageB.sprite = null;
        imageB.color = new Color(1f, 1f, 1f, 0f);

        illustrationCanvasGroup.alpha = 0f;
        illustrationCanvasGroup.interactable = false;
        illustrationCanvasGroup.blocksRaycasts = false;

        isPrepared = true;
    }

    // 일러스트 연출 시작
    public void PlayWithFade()
    {
        if (isPlaying)
            return;

        if (!isPrepared)
            Prepare();

        if (!isPrepared)
            return;

        isPlaying = true;
        startedFrame = Time.frameCount;

        screenFadeTween?.Kill();

        screenFadeTween = illustrationCanvasGroup
            .DOFade(1f, Mathf.Max(0.01f, screenFadeDuration))
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                screenFadeTween = null;
                PlaySequence();
            });
    }

    public void Play()
    {
        PlayWithFade();
    }

    // 일러스트 순차 재생
    private void PlaySequence()
    {
        if (!isPlaying)
            return;

        sequence?.Kill();
        sequence = DOTween.Sequence();

        for (int i = 1 ; i < illustrations.Length ; i++)
        {
            Sprite nextSprite = illustrations[i];

            sequence.AppendInterval(
                Mathf.Max(0f, displayDuration)
            );

            sequence.AppendCallback(() =>
            {
                imageB.sprite = nextSprite;
                imageB.color = new Color(1f, 1f, 1f, 0f);
            });

            sequence.Append(
                imageB.DOFade(
                    1f,
                    Mathf.Max(0.01f, fadeDuration)
                ).SetEase(Ease.InOutSine)
            );

            sequence.AppendCallback(() =>
            {
                imageA.sprite = nextSprite;
                imageA.color = Color.white;

                imageB.color = new Color(1f, 1f, 1f, 0f);
            });
        }

        sequence.AppendInterval(
            Mathf.Max(0f, displayDuration)
        );

        sequence.OnComplete(() =>
        {
            sequence = null;
            FadeOutIllustration();
        });
    }

    // 일러스트 전체 페이드 아웃
    private void FadeOutIllustration()
    {
        if (!isPlaying)
            return;

        screenFadeTween?.Kill();

        screenFadeTween = illustrationCanvasGroup
            .DOFade(0f, Mathf.Max(0.01f, screenFadeDuration))
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                screenFadeTween = null;
                CompleteIllustration();
            });
    }

    // X키로 전체 일러스트 스킵
    public void Skip()
    {
        if (!isPlaying || !canSkip)
            return;

        // 모든 DOTween 중지
        sequence?.Kill();
        sequence = null;

        screenFadeTween?.Kill();
        screenFadeTween = null;

        // 전환 중이던 이미지 Tween도 중지
        if (imageA != null)
            imageA.DOKill();

        if (imageB != null)
            imageB.DOKill();

        // 화면을 즉시 투명하게
        if (illustrationCanvasGroup != null)
            illustrationCanvasGroup.alpha = 0f;

        Debug.Log("일러스트 스킵!");

        // 일반 종료와 동일하게 다음 이벤트 실행
        CompleteIllustration();
    }

    // 일반 종료와 스킵이 공통으로 사용하는 함수
    private void CompleteIllustration()
    {
        if (!isPlaying)
            return;

        isPlaying = false;
        isPrepared = false;

        if (illustrationRoot != null)
            illustrationRoot.SetActive(false);

        Debug.Log("일러스트 연출 종료!");

        onIllustrationFinished?.Invoke();
    }

    // 다음 컷씬에서 다른 일러스트 사용
    public void SetIllustrations(Sprite[] newIllustrations)
    {
        Stop();
        illustrations = newIllustrations;
    }

    // 연출 중지 (종료 이벤트 실행 안 함)
    public void Stop()
    {
        sequence?.Kill();
        sequence = null;

        screenFadeTween?.Kill();
        screenFadeTween = null;

        if (imageA != null)
            imageA.DOKill();

        if (imageB != null)
            imageB.DOKill();

        isPlaying = false;
        isPrepared = false;
    }

    // 화면 숨기기 (종료 이벤트 실행 안 함)
    public void Hide()
    {
        Stop();

        if (illustrationRoot != null)
            illustrationRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        Stop();
    }
}
