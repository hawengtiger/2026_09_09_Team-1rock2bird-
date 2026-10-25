using UnityEngine;

public class DialogueItemAction : ItemAction
{
    public bool destroyAfterDialogue = true;
    public GameObject noroadDestroy;

    public GameObject lightRoad;

    public override void Execute(PickupItem item)
    {
        if (item.textTyping != null && item.dialogue != null)
        {
            item.textTyping.StartDialogue(item.dialogue);
        }

        if (destroyAfterDialogue)
        {
            Destroy(item.gameObject);
            Destroy(noroadDestroy);
        }

        if (lightRoad == null) return;

        lightRoad.SetActive(true);
    }
}