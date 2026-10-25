using UnityEngine;

public class DoorAction : ItemAction
{
    [Header("텔포할 위치.")]
    public Transform targetDoor;

    [Header("플레이어")]
    [SerializeField] private GameObject player;

    private TextTyping typing;
    private PetPuzzleManager pet;

    public void Start()
    {
        player = GameObject.Find("PlayerOB");
        typing = FindAnyObjectByType<TextTyping>();
        pet = FindAnyObjectByType<PetPuzzleManager>();
    }

    public override void Execute(PickupItem item)
    {
        if (targetDoor == null || player == null || typing.darkUIEffect.isStory || !(pet.currentState == PuzzleState.WaitToEnter)) return;

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
