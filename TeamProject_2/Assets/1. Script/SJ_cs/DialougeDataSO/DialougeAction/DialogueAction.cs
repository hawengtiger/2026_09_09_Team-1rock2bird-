
using UnityEngine;
using UnityEngine.Events;

public class DialogueAction : MonoBehaviour
{
    [Header("자동 이동 완료 후 활성화할 오브젝트")]
    public GameObject showObject;

    [Header("NPC")]
    public GameObject npc;

    [Header("플레이어 애니메이터")]
    public Animator playerAnimator;

    [Header("플레이어")]
    public Transform player;

    [Header("플레이어 이동 스크립트")]
    public PlayerController playerController;

    [Header("대화 매니저")]
    public TextTyping textTyping;

    [Header("실행할 다이얼로그")]
    public DialogueDataSO dialogueData;

    [Header("대화 상대 NPC")]
    public DialogueSpeaker npcSpeaker;

    [Header("자동 이동 X 좌표")]
    public float leftX = -7f;
    public float rightX = 0f;

    [Header("왕복 횟수 (0 = 무한 반복)")]
    public int repeatCount = 0;

    [Header("이동 완료 후 실행")]
    public UnityEvent onMoveFinished;

    [Header("한 번만 실행")]
    public bool executeOnce = true;

    private bool hasExecuted = false;
    private bool isMoving = false;
    private bool movingLeft = true;

    private int completedRoundTrips = 0;

    private void Start()
    {
        if (showObject != null)
        {
            showObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isMoving || player == null || playerController == null)
            return;

        if (!movingLeft)
        {
            // 오른쪽 이동
            playerController.SetAutoMove(Vector2.right);

            if (player.position.x >= rightX)
            {
                movingLeft = true;
                playerController.SetAutoMove(Vector2.left);
            }
        }
        else
        {
            // 왼쪽 이동
            playerController.SetAutoMove(Vector2.left);

            if (player.position.x <= leftX)
            {
                completedRoundTrips++;

                if (repeatCount > 0 &&
                    completedRoundTrips >= repeatCount)
                {
                    FinishMove();
                    return;
                }

                movingLeft = false;
                playerController.SetAutoMove(Vector2.right);
            }
        }
    }
    // 스토리 이벤트 실행
    public void Execute()
    {
        if (executeOnce && hasExecuted)
            return;

        if (textTyping == null || dialogueData == null)
            return;

        if (textTyping.IsPlaying || isMoving)
            return;

        // 대화 상대 NPC 설정
        if (!textTyping.SetSpeakers(npcSpeaker))
            return;

        // NPC 비활성화
        if (npc != null)
            npc.SetActive(false);

        // 포옹 애니메이션 종료
        if (playerAnimator != null)
            playerAnimator.SetBool("isHug", false);

        // 대사 종료 이벤트 등록
        textTyping.OnDialogueFinished -= HandleDialogueFinished;
        textTyping.OnDialogueFinished += HandleDialogueFinished;

        // 다이얼로그 시작
        if (textTyping.StartDialogue(dialogueData))
        {
            hasExecuted = true;
        }
        else
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    // 다이얼로그 종료 후 이동
    private void HandleDialogueFinished()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        StartAutoMove();
    }

    // 자동 이동 시작
    private void StartAutoMove()
    {
        if (player == null || playerController == null)
        {
            Debug.LogWarning(
                "Player 또는 PlayerController가 없습니다.");
            return;
        }

        if (isMoving)
            return;

        isMoving = true;
        movingLeft = true;
        completedRoundTrips = 0;

        playerController.SetAutoMove(Vector2.right);

        Debug.Log("자동 이동 시작");
    }

    // 이동 완료
    private void FinishMove()
    {
        StopMove();

        Debug.Log("왕복 이동 완료");

        // 이동 완료 후 오브젝트 활성화
        if (showObject != null)
        {
            showObject.SetActive(true);
        }

        // 기존 이벤트 실행
        onMoveFinished?.Invoke();
    }

    // 외부에서 자동 이동 중지
    public void StopMove()
    {
        if (!isMoving)
            return;

        isMoving = false;

        if (playerController != null)
        {
            playerController.StopAutoMove();
        }
    }

    private void OnDisable()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        StopMove();
    }

    private void OnDestroy()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }
}
