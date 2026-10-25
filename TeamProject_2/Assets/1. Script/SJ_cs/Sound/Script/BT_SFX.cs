using UnityEngine;
using UnityEngine.EventSystems;

public class BT_SFX : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)      // 마우스 포인터가 BT_SFX가 들어있는 오브젝트와 닿았을경우 활성화 됨.
    {
        SoundManager.Instance.PlaySFX("BT");
    }
}
