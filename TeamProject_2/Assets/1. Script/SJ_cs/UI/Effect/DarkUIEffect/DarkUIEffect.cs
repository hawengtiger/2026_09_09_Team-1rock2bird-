using UnityEngine;
using UnityEngine.UI;

public class DarkUIEffect : MonoBehaviour
{
    [Header("상태 설정")]
    public bool isStory;

    [Header("대상 UI 이미지 2개")]
    public Image firstImage;
    public Image secondImage;

    [Header("변화 속도 (높을수록 빠름)")]
    [Range(0.1f, 10f)]
    public float fillSpeed = 2f;

    private void Update()
    {
        // 1. isStory 값에 따라 목표 fillAmount 설정 (true면 1f, false면 0f)
        float targetFill = isStory ? 1f : 0f;

        // 2. 현재 fillAmount에서 목표 fillAmount로 매 프레임 부드럽게 이동
        if (firstImage != null)
        {
            firstImage.fillAmount = Mathf.MoveTowards(firstImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
        }

        if (secondImage != null)
        {
            secondImage.fillAmount = Mathf.MoveTowards(secondImage.fillAmount, targetFill, fillSpeed * Time.deltaTime);
        }
    }
}
