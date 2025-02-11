using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
public class GameStartManager : MonoBehaviour
{
    public static GameStartManager Instance { get; private set; }

    public GameObject loadingScreen;
    public TextMeshProUGUI loadingText;
    private bool isGameReady = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        loadingScreen.SetActive(true);
        loadingText.text = "Waiting for Remote Player...";
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
    }

    private void OnPlayerConnected(ulong clientId)
    {

        if (NetworkManager.Singleton.ConnectedClients.Count == 1)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        isGameReady = true;
        loadingScreen.SetActive(false);
    }

    public bool IsGameReady()
    {
        return isGameReady;
    }
}
