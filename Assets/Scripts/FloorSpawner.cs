using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorSpawner : MonoBehaviour
{
    public ObjectPool floorPool;
    public ObjectPool obstaclePool;
    public ObjectPool coinPool;

    public Transform player;
    public Transform remotePlayer;
    public int maxFloors = 15;
    public float floorLength = 5f;

    private List<GameObject> activeFloors = new List<GameObject>();
    private List<GameObject> activeRemoteFloors = new List<GameObject>();
    private float spawnZ = 0f;
    private List<Vector3> obstaclePositions = new List<Vector3>();
    private List<Vector3> coinPositions = new List<Vector3>();
    private Vector3 lastCoinPosition = Vector3.zero;
    private bool isPlayerGameOver = false;
    private bool isRemotePlayerGameOver = false;

    private readonly float[] lanePositions = new float[] { -1.5f, 0f, 1.5f };
    private readonly float[] remoteLanePositions = new float[] { -15.5f, -14f, -12.5f };

    void Start()
    {
        for (int i = 0; i < maxFloors; i++)
        {
            SpawnFloor(i == 0 ? false : true);
        }
    }

    void Update()
    {
        if (!GameStartManager.Instance.IsGameReady()) return;

        if (!isPlayerGameOver && player.position.z - 10f > spawnZ - (maxFloors * floorLength))
        {
            SpawnFloor(true);
        }
        DeleteOldFloor();
    }

    public void SetPlayerGameOver()
    {
        isPlayerGameOver = true;
    }

    public void SetRemotePlayerGameOver()
    {
        isRemotePlayerGameOver = true;
    }

    void SpawnFloor(bool spawnObjects)
    {
        GameObject floor = floorPool.GetObject(new Vector3(0, 0, spawnZ), Quaternion.identity);
        GameObject remoteFloor = floorPool.GetObject(new Vector3(-14, 0, spawnZ), Quaternion.identity);
        activeFloors.Add(floor);
        activeRemoteFloors.Add(remoteFloor);

        if (spawnObjects)
        {
            SpawnObstaclesAndCoins(floor.transform, remoteFloor.transform);
        }
        spawnZ += floorLength;
    }

    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeRemoteObstacles = new List<GameObject>();

    void SpawnObstaclesAndCoins(Transform floorTransform, Transform remoteFloorTransform)
    {
        int obstacleCount = Random.Range(1, 3);
        float minSpacing = 3.0f;
        int maxRetries = 5;

        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject obstacle = obstaclePool.GetObject(Vector3.zero, Quaternion.identity);
            GameObject remoteObstacle = obstaclePool.GetObject(Vector3.zero, Quaternion.identity);
            float randomX = lanePositions[Random.Range(0, lanePositions.Length)];
            float remoteX = remoteLanePositions[System.Array.IndexOf(lanePositions, randomX)];
            float obstacleY = 0.5f;
            float obstacleZ = 0f;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                obstacleZ = floorTransform.position.z + Random.Range(1f, floorLength - 1f);
                if (!IsTooClose(obstacleZ, obstaclePositions, minSpacing))
                {
                    break;
                }
            }

            Vector3 spawnPosition = new Vector3(randomX, obstacleY, obstacleZ);
            Vector3 remoteSpawnPosition = new Vector3(remoteX, obstacleY, obstacleZ);
            obstacle.transform.position = spawnPosition;
            remoteObstacle.transform.position = remoteSpawnPosition;
            obstacle.SetActive(true);
            remoteObstacle.SetActive(true);

            obstaclePositions.Add(spawnPosition);
            activeObstacles.Add(obstacle);
            activeRemoteObstacles.Add(remoteObstacle);
        }

        int coinCount = Random.Range(0, 2);
        for (int i = 0; i < coinCount; i++)
        {
            GameObject coin = coinPool.GetObject(Vector3.zero, Quaternion.identity);
            GameObject remoteCoin = coinPool.GetObject(Vector3.zero, Quaternion.identity);
            float randomX = lanePositions[Random.Range(0, lanePositions.Length)];
            float remoteX = remoteLanePositions[System.Array.IndexOf(lanePositions, randomX)];
            float coinY = 0.7f;
            float coinZ = floorTransform.position.z + 1f;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                coinZ = floorTransform.position.z + Random.Range(1f, floorLength - 1f);
                if (!IsTooClose(coinZ, coinPositions, minSpacing))
                {
                    break;
                }
            }

            Vector3 spawnPosition = new Vector3(randomX, coinY, coinZ);
            Vector3 remoteSpawnPosition = new Vector3(remoteX, coinY, coinZ);
            coin.transform.position = spawnPosition;
            remoteCoin.transform.position = remoteSpawnPosition;
            coin.SetActive(true);
            remoteCoin.SetActive(true);
            coinPositions.Add(spawnPosition);
        }
    }

    void DeleteOldFloor()
    {
        if (!isPlayerGameOver && activeFloors.Count > maxFloors)
        {
            Destroy(activeFloors[0]);
            activeFloors.RemoveAt(0);
        }
        if (!isRemotePlayerGameOver && activeRemoteFloors.Count > maxFloors)
        {
            Destroy(activeRemoteFloors[0]);
            activeRemoteFloors.RemoveAt(0);
        }
    }

    private bool IsTooClose(float positionZ, List<Vector3> positions, float minSpacing)
    {
        foreach (Vector3 pos in positions)
        {
            if (Mathf.Abs(pos.z - positionZ) < minSpacing)
            {
                return true;
            }
        }
        return false;
    }
}
