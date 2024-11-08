using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] Transform player;
    Vector3 offset;

    private void Start() {
        offset = transform.position - player.position;
    }

    private void LateUpdate() {
        Vector3 targetPos = new Vector3(player.position.x + offset.x, transform.position.y, player.position.z + offset.z);
        transform.position = targetPos;
    }
}