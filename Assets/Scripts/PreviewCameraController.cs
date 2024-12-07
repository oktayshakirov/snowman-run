using UnityEngine;
using System.Collections;

public class PreviewCameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float targetZPosition = -6f;   
    [SerializeField] private float moveDuration = 2f;      
    [SerializeField] private AnimationCurve movementCurve;  

    private Vector3 initialPosition;
    private bool isMoving = false;

    private void Awake()
    {
        initialPosition = transform.position;  
    }

    public void MoveCameraToPreviewPosition()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveCameraCoroutine());
        }
    }

    private IEnumerator MoveCameraCoroutine()
    {
        isMoving = true;
        float elapsed = 0f;
        Vector3 startPos = initialPosition;
        Vector3 endPos = new Vector3(startPos.x, startPos.y, targetZPosition);

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime; 
            float t = elapsed / moveDuration;
            if (movementCurve != null)
            {
                t = movementCurve.Evaluate(t);
            }

            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        transform.position = endPos;
        isMoving = false;
    }

    public void ResetCameraPosition()
    {
        transform.position = initialPosition;
    }
}