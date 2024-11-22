using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform fallbackTarget;
    Vector3 offset;

    private void Start()
    {
        if (player == null) return;
        offset = transform.position - player.position;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPos = new Vector3(
                player.position.x + offset.x,
                transform.position.y,
                player.position.z + offset.z
            );
            transform.position = targetPos;
        }
        else if (fallbackTarget != null)
        {
            Vector3 targetPos = new Vector3(
                fallbackTarget.position.x + offset.x,
                transform.position.y,
                fallbackTarget.position.z + offset.z
            );
            transform.position = targetPos;
        }
    }
}