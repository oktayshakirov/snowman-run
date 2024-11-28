using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    private Vector3 defaultOffset;
    private Vector3 initialOffset;
    private float elapsedTime = 0f;
    [SerializeField] private float zoomDuration = 2f;
    [SerializeField] private float zoomFactor = 2f;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("CameraFollow script requires a player Transform to follow.");
            return;
        }
        defaultOffset = transform.position - player.position;
        initialOffset = defaultOffset * zoomFactor;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition;

        if (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            Vector3 currentOffset = Vector3.Lerp(initialOffset, defaultOffset, t);
            desiredPosition = new Vector3(
                player.position.x + currentOffset.x,
                transform.position.y,
                player.position.z + currentOffset.z
            );
        }
        else
        {
            desiredPosition = new Vector3(
                player.position.x + defaultOffset.x,
                transform.position.y,
                player.position.z + defaultOffset.z
            );
        }
        transform.position = desiredPosition;
    }
}
