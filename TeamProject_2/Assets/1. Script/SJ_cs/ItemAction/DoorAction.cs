using UnityEngine;

public class DoorAction : ItemAction
{
    [Header("텔포할 위치.")]
    public Transform targetDoor;

    [Header("플레이어")]
    [SerializeField] private GameObject player;

    public void Start()
    {
        player = GameObject.Find("PlayerOB");
    }

    public override void Execute(PickupItem item)
    {
        if (targetDoor == null || player == null) return;

        FadeManager.Instance.FadeOut(0.5f, DG.Tweening.Ease.InQuad, () =>
        {
            player.transform.position = targetDoor.transform.position;

            FadeManager.Instance.Delay(0.5f, () =>
            {
                FadeManager.Instance.FadeIn(0.5f, DG.Tweening.Ease.InQuad);
            });
        });
    }
}
