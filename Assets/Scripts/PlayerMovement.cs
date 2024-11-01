using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {

    bool alive = true;
    public float speed = 5;
    [SerializeField] Rigidbody rb;

    private int currentLane = 1;
    private float laneDistance = 3.0f;
    private bool isMoving = false;

    public float speedIncreasePerPoint = 0.1f;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float swipeThreshold = 50f;

    private void FixedUpdate()
    {
        if (!alive) return;

        Vector3 forwardMove = transform.forward * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove);

        Vector3 targetPosition = new Vector3((currentLane - 1) * laneDistance, rb.position.y, rb.position.z);
        rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * 10));
    }

    private void Update()
    {
        if (transform.position.y < -5) {
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
                isMoving = true;
                Invoke("ResetMove", 0.1f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
            {
                currentLane++;
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
                        }
                        else if (swipe.x < 0 && currentLane > 0)
                        {
                            currentLane--;
                        }
                        isMoving = true;
                        Invoke("ResetMove", 0.1f);
                    }
                }
            }
        }
    }

    void ResetMove()
    {
        isMoving = false;
    }

    public void Die()
    {
        alive = false;
        Invoke("Restart", 2);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}