
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public enum TextState
{
    Idle,       // 대기
    Typing,     // 출력 중
    Deleting    // 삭제 중
}

public class TextTyping : MonoBehaviour
{
    [Header("텍스트")]
    public TextMeshPro textMesh;

    [Header("대사 데이터")]
    public DialogueDataSO dialogueData;

    [Header("스토리 UI 효과")]
    public DarkUIEffect darkUIEffect;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.1f;

    [Header("삭제 속도")]
    public float deleteSpeed = 0.05f;

    public TextState currentState { get; private set; } = TextState.Idle;

    private Coroutine typingCoroutine;
    private int currentDialogueIndex = 0;
    private bool isFinished = true;

    private void Start()
    {
        textMesh.text = "";
    }

    private void Update()
    {
        // 스토리가 진행 중이 아니면 입력 무시
        if (isFinished)
            return;

        // 좌클릭 : 대사 진행
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextDialogue();
        }

        // Z키 : 현재 출력 또는 삭제 즉시 완료
        if (Keyboard.current != null && Keyboard.current.zKey.wasPressedThisFrame)
        {
            FinishDialogue();
        }
    }

    // 스토리 시작
    public void StartDialogue(DialogueDataSO data)
    {
        if (data == null || data.dialogues == null || data.dialogues.Count == 0)
            return;

        StopCurrentCoroutine();

        dialogueData = data;
        currentDialogueIndex = 0;
        isFinished = false;
        currentState = TextState.Idle;

        textMesh.text = "";

        // 스토리 시작
        if (darkUIEffect != null)
        {
            darkUIEffect.isStory = true;
        }

        PlayTyping();
    }

    // 좌클릭으로 다음 대사 진행
    public void NextDialogue()
    {
        if (isFinished)
            return;

        // 출력 중이면 현재 문장 즉시 완성
        if (currentState == TextState.Typing)
        {
            SkipText();
            return;
        }

        // 삭제 중에는 중복 입력 방지
        if (currentState == TextState.Deleting)
        {
            SkipText();
            return;
        }

        // 마지막 대사여도 삭제 실행
        PlayDelete();
    }

    // 텍스트 출력 시작
    public void PlayTyping()
    {
        if (isFinished || dialogueData == null)
            return;

        StopCurrentCoroutine();

        currentState = TextState.Typing;
        typingCoroutine = StartCoroutine(Typing());
    }

    // 텍스트 삭제 시작
    public void PlayDelete()
    {
        if (isFinished)
            return;

        StopCurrentCoroutine();

        currentState = TextState.Deleting;
        typingCoroutine = StartCoroutine(Delete());
    }

    // 한 글자씩 출력
    private IEnumerator Typing()
    {
        textMesh.text = "";

        string dialogue = dialogueData.dialogues[currentDialogueIndex];

        for (int i = 0 ; i < dialogue.Length ; i++)
        {
            textMesh.text += dialogue[i];

            yield return new WaitForSeconds(typingSpeed);
        }

        currentState = TextState.Idle;
        typingCoroutine = null;
    }

    // 뒤에서부터 한 글자씩 삭제
    private IEnumerator Delete()
    {
        while (textMesh.text.Length > 0)
        {
            textMesh.text = textMesh.text.Substring(0, textMesh.text.Length - 1);

            yield return new WaitForSeconds(deleteSpeed);
        }

        typingCoroutine = null;
        currentState = TextState.Idle;

        // 마지막 대사라면 스토리 종료
        if (currentDialogueIndex + 1 >= dialogueData.dialogues.Count)
        {
            FinishDialogue();
            yield break;
        }

        MoveNextDialogue();
    }

    // 다음 대사로 이동
    private void MoveNextDialogue()
    {
        currentDialogueIndex++;

        typingCoroutine = null;
        currentState = TextState.Idle;

        PlayTyping();
    }

    // 실행 중인 코루틴 중지
    private void StopCurrentCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 현재 상태에 따라 즉시 출력 또는 삭제
    public void SkipText()
    {
        if (isFinished || currentState == TextState.Idle)
            return;

        TextState previousState = currentState;

        StopCurrentCoroutine();

        switch (previousState)
        {
            case TextState.Typing:

                textMesh.text = dialogueData.dialogues[currentDialogueIndex];
                currentState = TextState.Idle;

                break;

            case TextState.Deleting:

                textMesh.text = "";
                currentState = TextState.Idle;

                if (currentDialogueIndex + 1 >= dialogueData.dialogues.Count)
                {
                    FinishDialogue();
                }
                else
                {
                    MoveNextDialogue();
                }

                break;
        }
    }

    // 모든 대사 종료
    private void FinishDialogue()
    {
        StopCurrentCoroutine();     // 실행 중인 코루틴 강제 정지

        isFinished = true;
        currentState = TextState.Idle;
        typingCoroutine = null;

        textMesh.text = "";

        // 스토리 종료
        if (darkUIEffect != null)
        {
            darkUIEffect.isStory = false;
        }

        Debug.Log("모든 대사 종료");
    }
}
