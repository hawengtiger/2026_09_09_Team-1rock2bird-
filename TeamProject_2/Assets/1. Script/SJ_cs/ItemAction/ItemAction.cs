using UnityEngine;

public abstract class ItemAction : MonoBehaviour
{
    public abstract void Execute(PickupItem item);
}