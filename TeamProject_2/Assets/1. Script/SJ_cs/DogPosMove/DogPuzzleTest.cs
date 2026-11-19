
using UnityEngine;
using UnityEngine.InputSystem;

public class DogPuzzleTest : MonoBehaviour
{
    [Header("퍼즐 매니저")]
    public PetPuzzleManager petPuzzleManager;

    [Header("강아지 제스처 퍼즐")]
    public DogFacePuzzleAction dogFacePuzzleAction;

    [Header("플레이어")]
    public PlayerController playerController;
    public Animator playerAnimator;

    [Header("상호작용 UI")]
    public GameObject interactionUI;

    [Header("Walk Blend Tree State 이름")]
    public string walkStateName = "PlayerWalkAnimationBlend";

    private bool isPlayerInside = false;
    private bool isPuzzleStarted = false;
    private bool isCompleted = false;
    private bool isWaitingForDialogue = false;

    private int lastResultClosedFrame = -1;
    private float originalAnimatorSpeed = 1f;

    private void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlayerInside || isCompleted || isWaitingForDialogue)
            return;

        if (petPuzzleManager == null ||
            dogFacePuzzleAction == null)
            return;

        // 결과 화면은 DogFacePuzzleAction에서 처리
        if (petPuzzleManager.currentState != PuzzleState.WaitToEnter)
            return;

        // 실패 후 결과 화면이 닫히면 재도전 대기
        if (isPuzzleStarted)
        {
            isPuzzleStarted = false;

            if (interactionUI != null)
                interactionUI.SetActive(true);
        }

        // 결과 화면을 닫은 프레임에는 재시작 방지
        if (Time.frameCount == lastResultClosedFrame)
            return;

        if (Keyboard.current == null ||
            !Keyboard.current.fKey.wasPressedThisFrame)
            return;

        if (petPuzzleManager.puzzles == null ||
            petPuzzleManager.puzzles.Count <= 1)
            return;

        // 2번 퍼즐 선택
        dogFacePuzzleAction.StartPuzzle(1);

        if (dogFacePuzzleAction.GetCurrentPuzzleIndex() != 1 ||
            !dogFacePuzzleAction.CanStartPuzzle())
            return;

        // 실제 퍼즐 실행
        petPuzzleManager.Interact();

        if (petPuzzleManager.currentState != PuzzleState.PuzzleActive)
            return;

        isPuzzleStarted = true;

        if (interactionUI != null)
            interactionUI.SetActive(false);

        Debug.Log("2번 퍼즐 시작!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") ||
            isCompleted || isPlayerInside)
            return;

        isPlayerInside = true;

        if (playerController == null)
            playerController =
                other.GetComponentInParent<PlayerController>();

        if (playerAnimator == null)
            playerAnimator =
                other.GetComponentInChildren<Animator>();

        LockPlayer();

        if (interactionUI != null)
            interactionUI.SetActive(true);

        Debug.Log("트리거 진입! 플레이어 이동 잠금");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInside = false;

        if (interactionUI != null)
            interactionUI.SetActive(false);
    }

    private void LockPlayer()
    {
        // 스크립트는 끄지 않고 이동만 제한
        if (playerController != null)
        {
            playerController.canMove = false;
        }
        else
        {
            Debug.LogError("PlayerController가 연결되지 않았습니다!");
        }

        // Walk Blend Tree의 Idle
        if (playerAnimator != null)
        {
            originalAnimatorSpeed = playerAnimator.speed;

            playerAnimator.speed = 1f;

            playerAnimator.SetBool("isHug", false);
            playerAnimator.SetBool("isWalk", true);
            playerAnimator.SetFloat("Speed", 0f);

            playerAnimator.Play(walkStateName, 0, 0f);
            playerAnimator.Update(0f);
        }

        Debug.Log("canMove = false / Idle 상태");
    }

    // 결과 화면에서 F키를 눌러 닫았을 때 호출
    public void NotifyResultClosed()
    {
        lastResultClosedFrame = Time.frameCount;
    }

    // 퍼즐 성공 확정 시 호출
    public void CompletePuzzle()
    {
        if (isCompleted)
            return;

        isCompleted = true;
        isWaitingForDialogue = true;
        isPuzzleStarted = false;

        // 이동 잠금 해제
        if (playerController != null)
        {
            playerController.canMove = true;
            playerController.enabled = true;

            Debug.Log(
                "퍼즐 성공! canMove = " +
                playerController.canMove
            );
        }

        // 성공했을 때만 Hug로 변경
        if (playerAnimator != null)
        {
            playerAnimator.speed = originalAnimatorSpeed;

            playerAnimator.SetBool("isWalk", false);
            playerAnimator.SetBool("isHug", true);
            playerAnimator.SetFloat("Speed", 0f);
        }

        if (interactionUI != null)
            interactionUI.SetActive(false);

        Debug.Log("2번 퍼즐 성공! Hug 애니메이션 적용");

        // 일회성 트리거 종료
        gameObject.SetActive(false);
    }
}
