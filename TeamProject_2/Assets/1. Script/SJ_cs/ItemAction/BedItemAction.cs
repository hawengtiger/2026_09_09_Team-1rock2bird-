using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BedItemAction : ItemAction
{
    [Header("제어할 오브젝트")]
    public GameObject bedLight;

    [Header("대화 매니저")]
    [SerializeField] private TextTyping textTyping;

    [Header("불이 켜져 있을 때")]
    public DialogueDataSO lightOnDialogue;
    
    [Header("잠을 잤을 때")]
    public DialogueDataSO sleepDialogue;

    [Header("전역 빛")]
    public Light2D light;

    [Header("잠을 잔 후 다이얼 로그")]
    public GameObject[] triggerWall;

    [Header("잠을 잔 후 다이얼 로그")]
    public GameObject yeonglong2;

    private bool isSleep;

    private void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();
        triggerWall[0].SetActive(false);
        triggerWall[1].SetActive(false);
        yeonglong2.SetActive(false);
    }

    public override void Execute(PickupItem item)
    {
        if (bedLight == null || textTyping == null)
            return;

        if (isSleep)
        {
            textTyping.StartDialogue(sleepDialogue);
            return;
        }

        if (bedLight.activeSelf)
        {
            textTyping.StartDialogue(lightOnDialogue);
        }
        else
        {
            FadeManager.Instance.FadeOut(0.5f, DG.Tweening.Ease.InQuad, () =>
            {
                light.intensity = 1f;
                
                FadeManager.Instance.Delay(1f, () =>
                {
                    isSleep = true;
                    FadeManager.Instance.FadeIn(0.5f, DG.Tweening.Ease.InQuad);
                    triggerWall[0].SetActive(true);
                    yeonglong2.SetActive(true);
                    triggerWall[1].SetActive(true);
                });
            });
        }
    }
}