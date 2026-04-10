using UnityEngine;

/// <summary>
/// Obstacle - Moves an obstacle bar leftward at a given speed.
/// Attached automatically by ObstacleSpawner, but can also be added manually.
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Tooltip("How fast this obstacle moves left (units/sec). Set by the spawner.")]
    public float moveSpeed = 4f;

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);
    }
}
