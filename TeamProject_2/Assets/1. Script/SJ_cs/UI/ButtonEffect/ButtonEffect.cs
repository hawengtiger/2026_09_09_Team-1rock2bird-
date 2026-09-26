using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image fillImage;

    public float highlightSpeed = 3f;

    private bool isHighlight;

    private void Update()
    {
        if (isHighlight)
        {
            fillImage.fillAmount += Time.deltaTime * highlightSpeed;
        }
        else
        {
            fillImage.fillAmount -= Time.deltaTime * highlightSpeed;
        }

        fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);
    }

    // 마우스가 버튼에 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHighlight = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHighlight = false;
    }
}