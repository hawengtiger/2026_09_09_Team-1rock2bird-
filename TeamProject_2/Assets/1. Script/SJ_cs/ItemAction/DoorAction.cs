using UnityEngine;

public class DoorAction : ItemAction
{
    [Header("텔포할 위치.")]
    public Transform targetDoor;

    [Header("플레이어")]
    [SerializeField] private GameObject player;

    [Header("변경할 플레이어 애니메이션")]
    public PlayerWalkChange playerWalkChange;

    private TextTyping typing;
    private PetPuzzleManager pet;

    public void Start()
    {
        player = GameObject.Find("PlayerOB");
        typing = FindAnyObjectByType<TextTyping>();
        pet = FindAnyObjectByType<PetPuzzleManager>();
        playerWalkChange = GetComponent<PlayerWalkChange>();
    }

    public override void Execute(PickupItem item)
    {
        if (targetDoor == null || player == null ||
            typing.darkUIEffect.isStory ||
            !(pet.currentState == PuzzleState.WaitToEnter))
            return;

        FadeManager.Instance.FadeOut(0.5f, DG.Tweening.Ease.InQuad, () =>
        {
            // 플레이어 텔레포트
            player.transform.position = targetDoor.position;

            // 애니메이션 변경
            if (playerWalkChange != null)
                playerWalkChange.ChangeWalk();

            FadeManager.Instance.Delay(0.5f, () =>
            {
                FadeManager.Instance.FadeIn(0.5f, DG.Tweening.Ease.InQuad);
            });
        });
    }
}
