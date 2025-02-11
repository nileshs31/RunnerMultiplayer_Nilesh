using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NetworkPlayerController : NetworkBehaviour
{
    private int lane = 1; // Default lane
    private readonly float[] lanePositions = { -15.5f, -14f, -12.5f };
    
    public float speed = 5f;
    public float jumpForce = 8f;
    public float gravityMultiplier = 2f;
    public float speedIncreaseRate = 0.05f;
    public float maxSpeed = 15f;
    private Vector3 moveDirection;
    private bool isJumping = false;
    private int score = 0;
    private bool isGameOver = false;

    private CharacterController controller;
    public GameObject gameOverScreen;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        UpdateScore(0);
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
    [ServerRpc(RequireOwnership =false)]
    public void MoveRemotePlayerServerRpc(int newLane)
    {
        MoveRemotePlayerClientRpc(newLane);
    }
    void Update()
    {
        if (!GameStartManager.Instance.IsGameReady()) return;
        if (isGameOver) return;

        MovePlayer();
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
            }
        }
        else
        {
            moveDirection.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.x = lanePositions[lane];
        moveDirection.x = (targetPosition.x - transform.position.x) * 10f;

        controller.Move(moveDirection * Time.deltaTime);
    }

    [ClientRpc]
    public void MoveRemotePlayerClientRpc(int newLane)
    {
        lane = newLane;

        Vector3 targetPosition = transform.position;
        targetPosition.x = lanePositions[lane];
        moveDirection.x = (targetPosition.x - transform.position.x) * 10f;

        int dataSize = sizeof(int); // Integer size in bytes
        Debug.Log($"[Remote] Player moved to Lane {lane}, Position: {transform.position}, Data Sent: {dataSize} bytes");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendJumpCommandToNetworkServerRpc(bool isJumping)
    {
        SendJumpCommandToNetworkClientRpc(isJumping);
    }


    [ClientRpc]
    public void SendJumpCommandToNetworkClientRpc(bool isJumpingg)
    {
        isJumping = isJumpingg;
        int dataSize = sizeof(bool); // bool size in bytes
        Debug.Log($"[Remote] Player Jumped, Data Sent: {dataSize} byte");
    }



}
