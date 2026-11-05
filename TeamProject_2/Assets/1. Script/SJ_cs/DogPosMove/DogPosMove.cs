using UnityEngine;

public class DogPosMove : MonoBehaviour
{
    public GameObject targetPrefab;
    public Vector2 spawnPos;

    public DogMove dogScript;

    public void SpawnTarget(int targetX, int targetY)
    {
        spawnPos = new Vector2(targetX, targetY);

        GameObject targetClone = Instantiate(targetPrefab, spawnPos, Quaternion.identity);

        if (dogScript != null)
        {
            dogScript.AddTargetPoint(targetClone.transform);
        }
    }
}