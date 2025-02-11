using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;
    public float gravityMultiplier = 2f;
    public float speedIncreaseRate = 0.05f;
    public float maxSpeed = 15f;
    public bool isRemote = false; // Set this to true for remote player

    private CharacterController controller;
    private Vector3 moveDirection;
    private int lane = 1;
    private bool isJumping = false, canJump = true;
    private int score = 0;
    private bool isGameOver = false;
    private Vector2 startTouch, swipeDelta;
    private bool isDragging;

    private float[] lanePositions = new float[] { -1.5f, 0f, 1.5f };
    private float[] lanePositionsRemote = new float[] { -15.5f, -14f, -12.5f }; // Lanes for remote player

    public GameObject gameOverScreen;
    public TextMeshProUGUI scoreText;


    public NetworkPlayerController networkPlayer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        UpdateScore(0);
    }

    void Update()
    {
        if (!GameStartManager.Instance.IsGameReady()) return;
        if (isGameOver) return;

        HandleInput();
        MovePlayer();
    }

    void FixedUpdate()
    {
        if (!isGameOver && speed < maxSpeed)
        {
            speed += speedIncreaseRate * Time.fixedDeltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameOver();
        }
        else if (other.CompareTag("Coin"))
        {
            CollectCoin(other.gameObject);
        }
    }

    public void Home()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(1);
    }

    void GameOver()
    {
        isGameOver = true;
        speed = 0;
        gameOverScreen.SetActive(true);
    }

    void CollectCoin(GameObject coin)
    {
        score += 10;
        UpdateScore(score);
        coin.SetActive(false);
    }

    void UpdateScore(int newScore)
    {
        scoreText.text = "Score- " + newScore;
    }

    void MovePlayer()
    {
        moveDirection.z = speed;
        if (controller.isGrounded)
        {
            if (isJumping)
            {
                moveDirection.y = jumpForce;
                isJumping = false;
                canJump = false;
            }
            else
            {
                canJump = true;
            }
        }
        else
        {
            moveDirection.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.x = isRemote ? lanePositionsRemote[lane] : lanePositions[lane];
        moveDirection.x = (targetPosition.x - transform.position.x) * 10f;

        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleInput()
    {
        if (Input.touches.Length > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                startTouch = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                ResetSwipe();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            ResetSwipe();
        }

        swipeDelta = Vector2.zero;
        if (isDragging)
        {
            if (Input.touches.Length > 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
        }

        if (swipeDelta.magnitude > 100)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                if (x < 0 && lane > 0) lane--;
                else if (x > 0 && lane < 2) lane++;
                SendMoveCommandToNetwork(lane);
            }
            else
            {
                if (y > 0 && canJump)
                {
                    isJumping = true;
                    canJump = false;
                    SendJumpCommandToNetwork(isJumping);
                }
            }
            ResetSwipe();
        }
    }
    void SendMoveCommandToNetwork(int newLane)
    {
        if (networkPlayer != null)
        {
            networkPlayer.MoveRemotePlayerServerRpc(newLane);
            int dataSize = sizeof(int);
            Debug.Log($"[Local] Sent RPC: Move to Lane {newLane}, Position: {transform.position}, Data Sent: {dataSize} bytes");

        }
    }

    void SendJumpCommandToNetwork(bool isJumping)
    {
        if (networkPlayer != null)
        {
            networkPlayer.SendJumpCommandToNetworkServerRpc(isJumping);
            int dataSize = sizeof(bool);
            Debug.Log($"[Local] Sent RPC: Jumped, Data Sent: {dataSize} byte");

        }
    }
    void ResetSwipe()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDragging = false;
    }
}
