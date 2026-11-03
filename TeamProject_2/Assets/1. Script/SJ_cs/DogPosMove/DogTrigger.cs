using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    public GameObject nextQuestItem;
    public DogPosMove dogPosMove;
    public DogFacePuzzleAction dogFacePuzzleAction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        nextQuestItem.SetActive(true);

        if (dogPosMove != null)
        {
            dogPosMove.SpawnTarget(5, 0);
        }

        if (dogFacePuzzleAction != null)
        {
            dogFacePuzzleAction.EnableInteraction();
        }
    }
}