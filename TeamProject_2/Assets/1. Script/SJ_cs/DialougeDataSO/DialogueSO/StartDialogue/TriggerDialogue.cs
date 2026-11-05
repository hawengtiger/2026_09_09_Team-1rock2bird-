using UnityEngine;

public class TriggerDialogue : MonoBehaviour
{
    public TextTyping textTyping;
    public DialogueDataSO dialogue;

    [Header("플레이어 이동 거리")]
    public float offset = 2f;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        textTyping.StartDialogue(dialogue);

        if (gameObject.CompareTag("Noroad"))
        {
            // 플레이어와 트리거의 X좌표 비교
            float playerX = collision.transform.position.x;
            float triggerX = transform.position.x;

            // 왼쪽에서 진입했는지 오른쪽에서 진입했는지 확인
            if (playerX < triggerX)
            {
                // 왼쪽에서 진입 -> 왼쪽 오프셋으로 이동
                collision.transform.position -= Vector3.right * offset;
            }
            else
            {
                // 오른쪽에서 진입 -> 오른쪽 오프셋으로 이동
                collision.transform.position -= Vector3.left * offset;
            }
        }

        if (!gameObject.CompareTag("Noroad"))
        {
            Destroy(this.gameObject);
        }
    }
}

