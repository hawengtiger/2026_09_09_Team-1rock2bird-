using UnityEngine;

public class TriggerDogSound : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SoundManager.Instance.PlaySFX("Bark");
    }
}
