﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private bool alive = true;

    [Header("Speed Settings")]
    [SerializeField] private float speed;
    private float maxSpeed;

    [SerializeField] private Rigidbody rb;

    [Header("Hand Rotation Settings")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;
    [SerializeField] private float handRotationAngle = 25f;
    [SerializeField] private float handRotationSpeed = 5f;

    private int currentLane = 1;
    private float laneDistance = 3.0f;
    private bool isMoving = false;
    private bool controlsEnabled = false;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float swipeThreshold = 50f;

    [Header("Lean Settings")]
    [SerializeField] private float leanAngle = 15f;
    [SerializeField] private float leanSpeed = 10f;
    private Quaternion targetRotation;

    [Header("Ramp and Boost Settings")]
    [SerializeField] private float rampSpeedMultiplier = 1.2f;
    private bool onRamp = false;

    [SerializeField] private float arrowSpeedMultiplier = 1.5f;
    [SerializeField] private float arrowBoostDuration = 2.0f;
    private bool speedBoostActive = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip swipeSound;
    [SerializeField] private AudioClip jumpSound;

    private void Start()
    {
        targetRotation = transform.rotation;
        maxSpeed = GameManager.inst.MaxSpeed;
        controlsEnabled = false;
        StartCoroutine(EnableControlsAfterDelay(2f));
    }

    public void InitializeSpeed(float initialSpeed)
    {
        speed = initialSpeed;
    }

    private IEnumerator EnableControlsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        controlsEnabled = true;
    }

    private void FixedUpdate()
    {
        if (!alive) return;

        Vector3 forwardMove = Vector3.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        Vector3 targetPosition = new Vector3((currentLane - 1) * laneDistance, rb.position.y, rb.position.z);
        Vector3 newPosition = Vector3.MoveTowards(rb.position, targetPosition, laneDistance * Time.fixedDeltaTime * 10);
        rb.MovePosition(new Vector3(newPosition.x, rb.position.y, rb.position.z));

        if (!onRamp)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.fixedDeltaTime * leanSpeed);
        }
    }

    private void Update()
    {
        if (!alive) return;

        if (transform.position.y < -5)
        {
            Die();
        }

        if (controlsEnabled)
        {
            DetectInput();
        }
    }

    private void DetectInput()
    {
        if (!GameManager.inst.IsGameActive || isMoving) return;

        bool moved = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
        {
            currentLane--;
            moved = true;
            ApplyLean(-leanAngle);
            RotateHands(-handRotationAngle);
            PlaySound(swipeSound); 
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
        {
            currentLane++;
            moved = true;
            ApplyLean(leanAngle);
            RotateHands(handRotationAngle);
            PlaySound(swipeSound); 
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
                        moved = true;
                        PlaySound(swipeSound); 
                    }
                    else if (swipe.x < 0 && currentLane > 0)
                    {
                        currentLane--;
                        ApplyLean(-leanAngle);
                        RotateHands(-handRotationAngle);
                        moved = true;
                        PlaySound(swipeSound);
                    }
                }
            }
        }

        if (moved)
        {
            isMoving = true;
            StartCoroutine(ResetMove());
        }
    }

    private void ApplyLean(float angle)
    {
        targetRotation = Quaternion.Euler(0, 0, angle);
    }

    private void RotateHands(float angle)
    {
        float leftHandAngle = angle > 0 ? -handRotationAngle : handRotationAngle;
        float rightHandAngle = angle > 0 ? handRotationAngle : -handRotationAngle;
        leftHand.localRotation = Quaternion.Euler(0, 0, leftHandAngle);
        rightHand.localRotation = Quaternion.Euler(0, 0, rightHandAngle);
    }

    private IEnumerator ResetMove()
    {
        yield return new WaitForSeconds(0.1f);
        isMoving = false;
        targetRotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine(ResetHands());
    }

    private IEnumerator ResetHands()
    {
        Quaternion leftHandStartRotation = leftHand.localRotation;
        Quaternion rightHandStartRotation = rightHand.localRotation;
        float elapsedTime = 0f;
        float duration = 1f / handRotationSpeed;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            leftHand.localRotation = Quaternion.Lerp(leftHandStartRotation, Quaternion.identity, t);
            rightHand.localRotation = Quaternion.Lerp(rightHandStartRotation, Quaternion.identity, t);
            elapsedTime += Time.deltaTime;
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
            float newSpeed = speed * rampSpeedMultiplier;
            speed = Mathf.Min(newSpeed, maxSpeed);
            PlaySound(jumpSound); 
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow") && !speedBoostActive)
        {
            StartCoroutine(ApplySpeedBoost());
        }
    }

    private IEnumerator ApplySpeedBoost()
    {
        speedBoostActive = true;
        float originalSpeed = speed;
        float newSpeed = speed * arrowSpeedMultiplier;
        speed = Mathf.Min(newSpeed, maxSpeed);
        yield return new WaitForSeconds(arrowBoostDuration);
        speed = originalSpeed;
        speedBoostActive = false;
    }

    public void Die()
    {
        if (!alive) return;

        alive = false;
        speed = 0f;

        if (GameManager.inst != null)
        {
            GameManager.inst.OnPlayerCrash();
        }

        Invoke("Restart", 2f);
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}