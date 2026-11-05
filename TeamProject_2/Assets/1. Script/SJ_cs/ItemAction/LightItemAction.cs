using UnityEngine;

public class LightItemAction : ItemAction
{
    [Header("제어할 오브젝트")]
    public GameObject targetLight;

    public override void Execute(PickupItem item)
    {
        if (targetLight == null)
            return;

        targetLight.SetActive(!targetLight.activeSelf);
    }
}