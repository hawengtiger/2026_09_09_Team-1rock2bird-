
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

    [Header("강아지 몸 이미지")]
    public Sprite bodySprite;

    [Header("강아지 표정")]
    public Sprite idleFace;
    public Sprite wantFace;
    public Sprite successFace;
    public Sprite failFace;

    [Header("선택지별 반응 표정")]
    public Sprite[] optionReactionFaces;
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

    [Header("강아지 몸 - 표시 대상")]
    public Image petbodyImage;

    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip kkiingSound;

    public PuzzleState currentState = PuzzleState.WaitToEnter;

    private int currentSelectedIndex = -1;

    public int CurrentSelectedIndex => currentSelectedIndex;

    // 현재 퍼즐 데이터
    private PetPuzzleData CurrentPuzzle
    {
        get
        {
            if (puzzles == null ||
                currentPuzzleIndex < 0 ||
                currentPuzzleIndex >= puzzles.Count)
                return null;

            return puzzles[currentPuzzleIndex];
        }
    }

    // 기존 DogFacePuzzleAction과 호환
    public int correctOptionIndex
    {
        get
        {
            if (CurrentPuzzle == null)
                return -1;

            return CurrentPuzzle.correctOptionIndex;
        }
    }

    private void Start()
    {
        ResetToEmptyScreen();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

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

        if (CurrentPuzzle != null)
        {
            SetPetFace(CurrentPuzzle.idleFace);
            SetPetBody(CurrentPuzzle.bodySprite);
        }
    }

    private void EnterPuzzle()
    {
        if (CurrentPuzzle == null)
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

        SetPetFace(CurrentPuzzle.wantFace);
        SetPetBody(CurrentPuzzle.bodySprite);

        if (audioSource != null && kkiingSound != null)
            audioSource.PlayOneShot(kkiingSound);
    }

    private void UpdatePuzzleUI()
    {
        PetPuzzleData data = CurrentPuzzle;

        if (data == null)
            return;

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
        {
            SelectOption(0);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame ||
                 Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            SelectOption(1);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame ||
                 Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            SelectOption(2);
        }
    }

    public void OnOptionButtonClicked(int index)
    {
        if (currentState == PuzzleState.PuzzleActive)
        {
            SelectOption(index);
        }
    }

    private void SelectOption(int index)
    {
        if (currentState != PuzzleState.PuzzleActive)
            return;

        if (CurrentPuzzle == null)
            return;

        if (index < 0 || index >= optionButtonTexts.Length)
            return;

        PetPuzzleData data = CurrentPuzzle;

        if (data.optionTexts == null ||
            index >= data.optionTexts.Count)
            return;

        currentSelectedIndex = index;

        UpdateOptionHighlight();
        EvaluateChoice();
    }

    private void UpdateOptionHighlight()
    {
        if (optionHighlights == null)
            return;

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
        if (CurrentPuzzle == null)
            return;

        currentState = PuzzleState.ResultWait;

        PetPuzzleData data = CurrentPuzzle;

        bool isCorrect =
            currentSelectedIndex == data.correctOptionIndex;

        Sprite reaction = null;

        // 현재 퍼즐의 선택지별 표정
        if (data.optionReactionFaces != null &&
            currentSelectedIndex >= 0 &&
            currentSelectedIndex < data.optionReactionFaces.Length)
        {
            reaction = data.optionReactionFaces[currentSelectedIndex];
        }

        // 선택지 표정이 없다면 성공/실패 표정 사용
        if (reaction == null)
        {
            reaction = isCorrect
                ? data.successFace
                : data.failFace;
        }

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
        if (face == null)
            return;

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

            Debug.Log(
                $"현재 퍼즐 번호: {currentPuzzleIndex}"
            );
        }
    }

    // 외부에서 특정 퍼즐 선택
    public void SetPuzzle(int index)
    {
        if (index < 0 || index >= puzzles.Count)
        {
            Debug.LogWarning(
                $"존재하지 않는 퍼즐 번호: {index}"
            );
            return;
        }

        if (currentState != PuzzleState.WaitToEnter)
        {
            Debug.LogWarning(
                "퍼즐 진행 중에는 변경할 수 없습니다."
            );
            return;
        }

        currentPuzzleIndex = index;

        SetPetFace(CurrentPuzzle.idleFace);
        SetPetBody(CurrentPuzzle.bodySprite);

        Debug.Log($"{index + 1}번 퍼즐로 변경!");
    }

    private void SetPetBody(Sprite body)
    {
        if (body == null)
            return;

        if (petbodyImage != null)
            petbodyImage.sprite = body;
    }

}
