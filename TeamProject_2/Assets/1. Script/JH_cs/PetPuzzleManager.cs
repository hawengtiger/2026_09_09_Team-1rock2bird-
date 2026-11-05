
using System.Collections.Generic;
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

[System.Serializable]
public class PetPuzzleData
{
    [Header("상단 질문")]
    [TextArea(2, 5)]
    public string questionText;

    [Header("선택지 버튼 텍스트")]
    public List<string> optionTexts = new List<string>();

    [Header("정답 번호 (0부터 시작)")]
    public int correctOptionIndex = 0;

    [Header("성공 문구")]
    [TextArea(2, 4)]
    public string successText = "성공! 영룡이가 만족했습니다.";

    [Header("실패 문구")]
    [TextArea(2, 4)]
    public string failText = "실패! 영룡이가 경멸하는 표정을 짓습니다.";
}

public class PetPuzzleManager : MonoBehaviour
{
    [Header("퍼즐 데이터")]
    public List<PetPuzzleData> puzzles = new List<PetPuzzleData>();

    [Header("현재 퍼즐 번호")]
    public int currentPuzzleIndex = 0;

    [Header("UI 요소")]
    public GameObject puzzleUI;
    public Image[] optionHighlights;
    public TextMeshProUGUI[] optionButtonTexts;
    public TextMeshProUGUI statusText;

    [Header("강아지 오브젝트")]
    public GameObject petRoot;

    [Header("강아지 표정 - 표시 대상")]
    public Image petFaceImage;

    [Header("강아지 표정 - 아트")]
    public Sprite idleFace;
    public Sprite wantFace;
    public Sprite successFace;
    public Sprite failFace;

    [Tooltip("선택지별 반응 표정")]
    public Sprite[] optionReactionFaces;

    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip kkiingSound;

    public PuzzleState currentState = PuzzleState.WaitToEnter;

    private int currentSelectedIndex = -1;

    public int CurrentSelectedIndex => currentSelectedIndex;

    // 기존 DogFacePuzzleAction과 호환
    public int correctOptionIndex
    {
        get
        {
            if (puzzles == null ||
                currentPuzzleIndex < 0 ||
                currentPuzzleIndex >= puzzles.Count)
                return -1;

            return puzzles[currentPuzzleIndex].correctOptionIndex;
        }
    }

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
        if (puzzles == null ||
            currentPuzzleIndex < 0 ||
            currentPuzzleIndex >= puzzles.Count)
        {
            Debug.LogWarning("퍼즐 데이터가 없습니다.");
            return;
        }

        currentState = PuzzleState.PuzzleActive;

        if (puzzleUI != null)
            puzzleUI.SetActive(true);

        if (petRoot != null)
            petRoot.SetActive(true);

        currentSelectedIndex = -1;

        UpdateOptionHighlight();
        UpdatePuzzleUI();

        SetPetFace(wantFace);

        if (audioSource != null && kkiingSound != null)
            audioSource.PlayOneShot(kkiingSound);
    }

    private void UpdatePuzzleUI()
    {
        PetPuzzleData data = puzzles[currentPuzzleIndex];

        if (statusText != null)
            statusText.text = data.questionText;

        for (int i = 0 ; i < optionButtonTexts.Length ; i++)
        {
            if (optionButtonTexts[i] == null)
                continue;

            if (data.optionTexts != null &&
                i < data.optionTexts.Count)
            {
                optionButtonTexts[i].text = data.optionTexts[i];
            }
            else
            {
                optionButtonTexts[i].text = "";
            }
        }
    }

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

    public void OnOptionButtonClicked(int index)
    {
        if (currentState == PuzzleState.PuzzleActive)
            SelectOption(index);
    }

    private void SelectOption(int index)
    {
        if (currentState != PuzzleState.PuzzleActive)
            return;

        if (index < 0 || index >= optionButtonTexts.Length)
            return;

        PetPuzzleData data = puzzles[currentPuzzleIndex];

        if (data.optionTexts == null ||
            index >= data.optionTexts.Count)
            return;

        currentSelectedIndex = index;

        UpdateOptionHighlight();
        EvaluateChoice();
    }

    private void UpdateOptionHighlight()
    {
        if (optionHighlights == null) return;

        for (int i = 0 ; i < optionHighlights.Length ; i++)
        {
            if (optionHighlights[i] != null)
            {
                optionHighlights[i].color =
                    i == currentSelectedIndex
                    ? Color.yellow
                    : Color.white;
            }
        }
    }

    private void EvaluateChoice()
    {
        currentState = PuzzleState.ResultWait;

        PetPuzzleData data = puzzles[currentPuzzleIndex];

        bool isCorrect =
            currentSelectedIndex == data.correctOptionIndex;

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
            statusText.text =
                (isCorrect ? data.successText : data.failText)
                + "\n[F] 키를 눌러 나가기";
        }
    }

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

    // 외부에서 다음 퍼즐로 변경
    public void NextPuzzle()
    {
        if (currentState != PuzzleState.WaitToEnter)
            return;

        if (currentPuzzleIndex + 1 < puzzles.Count)
        {
            currentPuzzleIndex++;
        }
    }
}
