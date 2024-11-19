using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    bool alive = true;
    public float speed = 5;
    [SerializeField] Rigidbody rb;

    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;
    public float handRotationAngle = 25f;
    public float handRotationSpeed = 5f;

    private int currentLane = 1;
    private float laneDistance = 3.0f;
    private bool isMoving = false;

    public float speedIncreasePerPoint = 0.1f;
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float swipeThreshold = 50f;

    public float leanAngle = 15f;
    public float leanSpeed = 10f;
    public float shakeAmount = 0.05f;
    private Quaternion targetRotation;

    public float rampSpeedMultiplier = 1.2f;
    private bool onRamp = false;
    private Vector3 rampNormal;

    private void Start()
    {
        targetRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (!alive) return;

        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;

        if (onRamp)
        {
            forwardMove += Vector3.down * 0.1f; 
            rb.rotation = Quaternion.Lerp(rb.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, rampNormal)), Time.fixedDeltaTime * leanSpeed);
        }

        rb.MovePosition(rb.position + forwardMove);

        Vector3 targetPosition = new Vector3((currentLane - 1) * laneDistance, rb.position.y, rb.position.z);
        rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * 10));

        if (!onRamp)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * leanSpeed);
        }
    }

    private void Update()
    {
        if (transform.position.y < -5)
        {
            Die();
        }

        DetectInput();
    }

    private void DetectInput()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
            {
                currentLane--;
                ApplyLean(-leanAngle);
                RotateHands(-handRotationAngle);
                isMoving = true;
                Invoke("ResetMove", 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
            {
                currentLane++;
                ApplyLean(leanAngle);
                RotateHands(handRotationAngle);
                isMoving = true;
                Invoke("ResetMove", 0.1f);
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    startTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    endTouchPosition = touch.position;
                    Vector2 swipe = endTouchPosition - startTouchPosition;

                    if (Mathf.Abs(swipe.x) > swipeThreshold)
                    {
                        if (swipe.x > 0 && currentLane < 2)
                        {
                            currentLane++;
                            ApplyLean(leanAngle);
                            RotateHands(handRotationAngle);
                        }
                        else if (swipe.x < 0 && currentLane > 0)
                        {
                            currentLane--;
                            ApplyLean(-leanAngle);
                            RotateHands(-handRotationAngle);
                        }
                        isMoving = true;
                        Invoke("ResetMove", 0.1f);
                    }
                }
            }
        }
    }

    private void ApplyLean(float angle)
    {
        targetRotation = Quaternion.Euler(0, 0, angle);
    }

    private void RotateHands(float angle)
    {
        if (angle > 0)
        {
            leftHand.localRotation = Quaternion.Euler(0, 0, -handRotationAngle);
            rightHand.localRotation = Quaternion.Euler(0, 0, handRotationAngle);
        }
        else
        {
            leftHand.localRotation = Quaternion.Euler(0, 0, handRotationAngle);
            rightHand.localRotation = Quaternion.Euler(0, 0, -handRotationAngle);
        }
    }

    private void ResetMove()
    {
        isMoving = false;
        targetRotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(ResetHands());
    }

    private IEnumerator ResetHands()
    {
        Quaternion leftHandStartRotation = leftHand.localRotation;
        Quaternion rightHandStartRotation = rightHand.localRotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            leftHand.localRotation = Quaternion.Lerp(leftHandStartRotation, Quaternion.identity, elapsedTime);
            rightHand.localRotation = Quaternion.Lerp(rightHandStartRotation, Quaternion.identity, elapsedTime);
            elapsedTime += Time.deltaTime * handRotationSpeed;
            yield return null;
        }

        leftHand.localRotation = Quaternion.identity;
        rightHand.localRotation = Quaternion.identity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ramp"))
        {
            onRamp = true;
            rampNormal = collision.contacts[0].normal;
            speed *= rampSpeedMultiplier;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Ramp"))
        {
            onRamp = false;
            speed /= rampSpeedMultiplier;
        }
    }

    public void Die()
    {
        alive = false;
        Invoke("Restart", 2);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
