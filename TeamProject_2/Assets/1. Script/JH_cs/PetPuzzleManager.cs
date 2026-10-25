
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public enum PuzzleState
{
    WaitToEnter,
    PuzzleActive,
    ResultWait
}

public class PetPuzzleManager : MonoBehaviour
{
    [Header("퍼즐 설정")]
    public int correctOptionIndex = 0;

    [Header("UI 요소")]
    public GameObject puzzleUI;
    public Image[] optionHighlights;
    public TextMeshProUGUI statusText;

    [Header("강아지 오브젝트 (몸+표정 통째로 켜고 끄기)")]
    public GameObject petRoot;

    [Header("강아지 표정 - 표시 대상")]
    public Image petFaceImage;

    [Header("강아지 표정 - 아트")]
    public Sprite idleFace;
    public Sprite wantFace;
    public Sprite successFace;
    public Sprite failFace;

    [Tooltip("선택지별로 다른 반응 표정을 쓰고 싶을 때. 비워두면 successFace / failFace 사용")]
    public Sprite[] optionReactionFaces;

    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip kkiingSound;

    public PuzzleState currentState = PuzzleState.WaitToEnter;
    private int currentSelectedIndex = -1;
    public int CurrentSelectedIndex => currentSelectedIndex;

    private void Start()
    {
        ResetToEmptyScreen();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (currentState == PuzzleState.PuzzleActive)
        {
            HandleKeyboardSelection();
        }
    }

    private void ResetToEmptyScreen()
    {
        currentState = PuzzleState.WaitToEnter;

        if (puzzleUI != null)
            puzzleUI.SetActive(false);

        if (statusText != null)
            statusText.text = "";

        if (petRoot != null)
            petRoot.SetActive(false);

        SetPetFace(idleFace);
    }

    private void EnterPuzzle()
    {
        currentState = PuzzleState.PuzzleActive;

        if (puzzleUI != null)
            puzzleUI.SetActive(true);

        if (petRoot != null)
            petRoot.SetActive(true);

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
        if (Keyboard.current.digit1Key.wasPressedThisFrame ||
            Keyboard.current.numpad1Key.wasPressedThisFrame)
            SelectOption(0);

        else if (Keyboard.current.digit2Key.wasPressedThisFrame ||
                 Keyboard.current.numpad2Key.wasPressedThisFrame)
            SelectOption(1);

        else if (Keyboard.current.digit3Key.wasPressedThisFrame ||
                 Keyboard.current.numpad3Key.wasPressedThisFrame)
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
        for (int i = 0 ; i < optionHighlights.Length ; i++)
        {
            if (optionHighlights[i] != null)
                optionHighlights[i].color =
                    (i == currentSelectedIndex) ? Color.yellow : Color.white;
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

    // UI Image 표정 교체
    private void SetPetFace(Sprite face)
    {
        if (face == null) return;

        if (petFaceImage != null)
            petFaceImage.sprite = face;
    }

    public void Interact()
    {
        switch (currentState)
        {
            case PuzzleState.WaitToEnter:
                EnterPuzzle();
                break;

            case PuzzleState.ResultWait:
                ResetToEmptyScreen();
                break;
        }
    }
}
