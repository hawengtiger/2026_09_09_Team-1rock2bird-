using UnityEngine;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

public class SpriteTextSize : MonoBehaviour
{
    // 유니티 에디터에서 Target Player 오브젝트를 드래그 앤 드롭으로 넣어주세요.
    public Transform playerTransform;

    [Header("텍스트")]
    public TextMeshPro textMesh;

    [Header("배경 스프라이트")]
    public SpriteRenderer background;

    [Header("여백")]
    public float paddingX = 0.5f;
    public float paddingY = 0.3f;

    private void LateUpdate()
    {

        if (playerTransform != null)
        {
            // Player의 피벗 위치(position)를 그대로 내 위치로 가져옵니다.
            transform.position = playerTransform.position;
        }

        // 텍스트가 비어있으면 배경 크기 초기화
        if (string.IsNullOrWhiteSpace(textMesh.text))
        {
            background.size = Vector2.zero;
            background.enabled = false;
            return;
        }

        background.enabled = true;

        // 텍스트 크기 갱신
        textMesh.ForceMeshUpdate();

        // 텍스트 실제 크기 가져오기
        Vector2 textSize = textMesh.GetRenderedValues(false);

        // 텍스트 크기 + 여백
        background.size = new Vector2(
            textSize.x + paddingX,
            textSize.y + paddingY
        );
    }
}