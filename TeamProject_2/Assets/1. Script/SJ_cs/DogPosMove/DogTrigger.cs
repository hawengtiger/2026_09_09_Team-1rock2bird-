using UnityEngine;

public class DogTrigger : MonoBehaviour
{
    public DogPosMove dogPosMove;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        dogPosMove.SpawnTarget(3, 0);
    }
}