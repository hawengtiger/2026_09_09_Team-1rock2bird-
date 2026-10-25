using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PetPuzzleManager : MonoBehaviour
{
    public enum PuzzleState
    {
        WaitToEnter,
        PuzzleActive,
        ResultWait
    }

    [Header("퍼즐 설정")]
    public int correctOptionIndex = 0;

    [Header("UI 요소")]
    public GameObject puzzleUI;
    public Image[] optionHighlights;
    public TextMeshProUGUI statusText;

    [Header("강아지 오브젝트 (몸+표정 통째로 켜고 끄기)")]
    public GameObject petRoot;


    [Header("강아지 표정 - 표시 대상")]
    public Image petFaceImage;               // UI(Canvas) 안에 강아지가 있을 때
    public SpriteRenderer petFaceRenderer;   // 월드(씬)에 강아지가 있을 때

    [Header("강아지 표정 - 아트")]
    public Sprite idleFace;      // 평소 표정 (퍼즐 시작 전)
    public Sprite wantFace;      // 무언가 원하는 표정 (퍼즐 진입 시)
    public Sprite successFace;   // 정답 시 만족한 표정
    public Sprite failFace;      // 오답 시 경멸하는 표정 (기본값)

    [Tooltip("선택지별로 다른 반응 표정을 쓰고 싶을 때. 비워두면 successFace / failFace 사용")]
    public Sprite[] optionReactionFaces;

    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip kkiingSound;

    private PuzzleState currentState = PuzzleState.WaitToEnter;
    private int currentSelectedIndex = -1;

    private void Start()
    {
        ResetToEmptyScreen();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        switch (currentState)
        {
            case PuzzleState.WaitToEnter:
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    EnterPuzzle();
                break;

            case PuzzleState.PuzzleActive:
                HandleKeyboardSelection();
                break;

            case PuzzleState.ResultWait:
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    ResetToEmptyScreen();
                break;
        }
    }

    private void ResetToEmptyScreen()
    {
        currentState = PuzzleState.WaitToEnter;
        if (puzzleUI != null) puzzleUI.SetActive(false);
        if (statusText != null) statusText.text = "";

        if (petRoot != null) petRoot.SetActive(false);

        SetPetFace(idleFace);
    }

    private void EnterPuzzle()
    {
        currentState = PuzzleState.PuzzleActive;
        if (puzzleUI != null) puzzleUI.SetActive(true);

        if (petRoot != null) petRoot.SetActive(true);

        currentSelectedIndex = -1;
        UpdateOptionHighlight();

        SetPetFace(wantFace);

        if (audioSource != null && kkiingSound != null)
            audioSource.PlayOneShot(kkiingSound);

        if (statusText != null)
            statusText.text = "영룡이가 무언가 원합니다!\n1, 2, 3 키를 누르거나 클릭하세요.";
    }

    // 키보드 입력 처리
    private void HandleKeyboardSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectOption(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectOption(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            SelectOption(2);
    }

    // 마우스 클릭(버튼) 시 호출될 퍼블릭 함수
    public void OnOptionButtonClicked(int index)
    {
        if (currentState == PuzzleState.PuzzleActive)
            SelectOption(index);
    }

    // 선택 처리 (키보드, 버튼 공통 사용)
    private void SelectOption(int index)
    {
        currentSelectedIndex = index;
        UpdateOptionHighlight();
        EvaluateChoice();
    }

    private void UpdateOptionHighlight()
    {
        for (int i = 0; i < optionHighlights.Length; i++)
        {
            if (optionHighlights[i] != null)
                optionHighlights[i].color = (i == currentSelectedIndex) ? Color.yellow : Color.white;
        }
    }

    private void EvaluateChoice()
    {
        currentState = PuzzleState.ResultWait;
        bool isCorrect = currentSelectedIndex == correctOptionIndex;

        // 선택지별 표정이 지정돼 있으면 그걸 우선 사용
        Sprite reaction = null;
        if (optionReactionFaces != null &&
            currentSelectedIndex >= 0 &&
            currentSelectedIndex < optionReactionFaces.Length)
        {
            reaction = optionReactionFaces[currentSelectedIndex];
        }
        if (reaction == null)
            reaction = isCorrect ? successFace : failFace;

        SetPetFace(reaction);

        if (statusText != null)
        {
            statusText.text = isCorrect
                ? "성공! 영룡이가 만족했습니다. \n[F] 키를 눌러 나가기"
                : "실패! 영룡이가 경멸하는 표정을 짓습니다. \n[F] 키를 눌러 나가기";
        }
    }

    // 표정 교체 (UI Image / SpriteRenderer 둘 다 지원)
    private void SetPetFace(Sprite face)
    {
        if (face == null) return; // 아트가 비어 있으면 현재 표정 유지

        if (petFaceImage != null)
            petFaceImage.sprite = face;

        if (petFaceRenderer != null)
            petFaceRenderer.sprite = face;
    }
}
