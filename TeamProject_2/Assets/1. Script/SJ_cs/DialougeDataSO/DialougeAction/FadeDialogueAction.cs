using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class FadeDialogueAction : MonoBehaviour
{
    [Header("컷씬 종료 후 활성화할 오브젝트")]
    public GameObject[] showObject;

    [Header("대화 매니저")]
    public TextTyping textTyping;

    [Header("검은 화면에서 출력할 대사")]
    public DialogueDataSO dialogueData;

    [Header("대화 상대 NPC")]
    public DialogueSpeaker npcSpeaker;

    [Header("플레이어 (선택 사항)")]
    public Transform player;

    [Header("컷씬 종료 후 이동할 위치 (선택 사항)")]
    public Transform targetPosition;

    [Header("일러스트 연출 (선택 사항)")]
    public IllustrationCrossFade illustrationCrossFade;

    [Header("검은 화면 페이드 시간")]
    public float fadeDuration = 1f;

    [Header("전체 컷씬 종료 후 실행")]
    public UnityEvent onCutsceneFinished;

    [Header("한 번만 실행")]
    public bool executeOnce = true;

    [Header("시작할 때 검은 화면 페이드 사용")]
    public bool useStartFade = true;

    [Header("일러스트 종료 후 검은 화면 제거")]
    public bool fadeBackToGame = true;

    private bool hasExecuted = false;
    private bool isExecuting = false;
    private bool isWaitingForDialogue = false;

    // ChairAction 또는 이전 컷씬의 UnityEvent에서 호출
    public void Execute()
    {
        if (executeOnce && hasExecuted)
            return;

        if (isExecuting)
            return;

        if (textTyping == null ||
            dialogueData == null ||
            FadeManager.Instance == null)
        {
            Debug.LogWarning("FadeDialogueAction 설정이 부족합니다.");
            return;
        }

        if (textTyping.IsPlaying ||
            FadeManager.Instance.isFading)
            return;

        isExecuting = true;

        if (useStartFade)
        {
            FadeManager.Instance.FadeOut(
                fadeDuration,
                Ease.InOutQuad,
                () =>
                {
                    StartCutsceneDialogue();
                }
            );
        }
        else
        {
            // 이전 일러스트를 X로 스킵한 입력이
            // 다음 다이얼로그까지 전달되지 않게 한 프레임 대기
            StartCoroutine(StartDialogueNextFrame());
        }
    }

    private IEnumerator StartDialogueNextFrame()
    {
        yield return null;

        StartCutsceneDialogue();
    }

    // 기존 TextTyping을 이용한 대사 시작
    private void StartCutsceneDialogue()
    {
        // 대화 상대 설정
        if (!textTyping.SetSpeakers(npcSpeaker))
        {
            isExecuting = false;
            return;
        }

        // 종료 이벤트 등록
        textTyping.OnDialogueFinished -= HandleDialogueFinished;
        textTyping.OnDialogueFinished += HandleDialogueFinished;

        isWaitingForDialogue = true;

        // 다이얼로그 시작
        if (textTyping.StartDialogue(dialogueData))
        {
            hasExecuted = true;
        }
        else
        {
            isWaitingForDialogue = false;
            isExecuting = false;

            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    // 검은 화면 다이얼로그 종료
    private void HandleDialogueFinished()
    {
        if (!isWaitingForDialogue)
            return;

        isWaitingForDialogue = false;

        textTyping.OnDialogueFinished -= HandleDialogueFinished;

        // 1. 플레이어 순간이동 (선택 사항)
        if (player != null && targetPosition != null)
        {
            Rigidbody2D rb =
                player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;

                rb.position = new Vector2(
                    targetPosition.position.x,
                    targetPosition.position.y
                );
            }
            else
            {
                player.position = targetPosition.position;
            }
        }

        // 2. 오브젝트 활성화 (선택 사항)
        if (showObject != null)
        {
            foreach (GameObject obj in showObject)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        // 3. 일러스트 연출
        if (illustrationCrossFade != null)
        {
            illustrationCrossFade.onIllustrationFinished
                .RemoveListener(HandleIllustrationFinished);

            illustrationCrossFade.onIllustrationFinished
                .AddListener(HandleIllustrationFinished);

            // 검은 패널 위에 일러스트 표시
            illustrationCrossFade.PlayWithFade();
        }
        else
        {
            // 일러스트가 없으면 바로 컷씬 종료 처리
            CompleteCutscene();
        }
    }

    // 일러스트 연출 종료
    private void HandleIllustrationFinished()
    {
        if (illustrationCrossFade != null)
        {
            illustrationCrossFade.onIllustrationFinished
                .RemoveListener(HandleIllustrationFinished);
        }

        CompleteCutscene();
    }

    // 현재 컷씬 종료 처리
    private void CompleteCutscene()
    {
        if (fadeBackToGame)
        {
            // 마지막 컷씬:
            // 검은 패널 제거 후 게임 화면 복귀
            FinishCutscene();
        }
        else
        {
            // 이어지는 컷씬:
            // 검은 패널 유지 후 다음 이벤트 실행
            isExecuting = false;

            Debug.Log("현재 컷씬 종료 - 검은 화면 유지");

            onCutsceneFinished?.Invoke();
        }
    }

    // 검은 패널을 제거하고 전체 컷씬 종료
    private void FinishCutscene()
    {
        if (FadeManager.Instance == null)
        {
            isExecuting = false;
            return;
        }

        FadeManager.Instance.FadeIn(
            fadeDuration,
            Ease.InOutQuad,
            () =>
            {
                isExecuting = false;

                Debug.Log("전체 컷씬 종료!");

                onCutsceneFinished?.Invoke();
            }
        );
    }

    private void OnDisable()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        if (illustrationCrossFade != null)
        {
            illustrationCrossFade.onIllustrationFinished
                .RemoveListener(HandleIllustrationFinished);
        }

        isWaitingForDialogue = false;
        isExecuting = false;
    }

    private void OnDestroy()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        if (illustrationCrossFade != null)
        {
            illustrationCrossFade.onIllustrationFinished
                .RemoveListener(HandleIllustrationFinished);
        }
    }
}
