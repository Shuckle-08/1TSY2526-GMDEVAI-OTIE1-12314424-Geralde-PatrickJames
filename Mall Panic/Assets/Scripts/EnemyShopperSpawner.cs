using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShopperSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject shopperPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerPrefab;

    [Header("Player Spawning")]
    [SerializeField] private bool spawnPlayerOnRoundStart = true;

    [Header("Initial Enemy Spawning")]
    [SerializeField] private int shopperCount = 5;
    [SerializeField] private float startDelaySeconds = 5f;
    [SerializeField] private float spawnIntervalSeconds = 1f;

    [Header("Ongoing Enemy Spawning")]
    [SerializeField] private bool spawnContinuously = true;
    [SerializeField] private float minContinuousSpawnDelay = 8f;
    [SerializeField] private float maxContinuousSpawnDelay = 15f;

    private readonly List<GameObject> spawnedShoppers = new List<GameObject>();
    private Coroutine spawnRoutine;

    private GameObject spawnedPlayer;
    private bool ownsSpawnedPlayer;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (spawnPoint == null)
        {
            GameObject taggedSpawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (taggedSpawnPoint != null)
            {
                spawnPoint = taggedSpawnPoint.transform;
            }
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged += HandleStateChanged;
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }

        StopSpawning();
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            if (spawnPlayerOnRoundStart)
            {
                SpawnOrPositionPlayer();
            }

            BeginSpawning();
            return;
        }

        StopSpawning();

        if (state == GameManager.GameState.None)
        {
            ClearSpawnedShoppers();
            ClearSpawnedPlayer();
        }
    }

    private void BeginSpawning()
    {
        StopSpawning();
        ClearSpawnedShoppers();

        if (shopperPrefab == null || spawnPoint == null)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnShoppersRoutine());
    }

    private void SpawnOrPositionPlayer()
    {
        if (spawnPoint == null)
        {
            return;
        }

        if (playerPrefab != null)
        {
            ClearSpawnedPlayer();
            spawnedPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            ownsSpawnedPlayer = true;
            return;
        }

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer == null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                existingPlayer = playerController.gameObject;
            }
        }

        if (existingPlayer == null)
        {
            return;
        }

        CharacterController characterController = existingPlayer.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        existingPlayer.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        existingPlayer.SetActive(true);

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        spawnedPlayer = existingPlayer;
        ownsSpawnedPlayer = false;
    }

    private void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnShoppersRoutine()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        for (int i = 0; i < shopperCount; i++)
        {
            if (!CanSpawn())
            {
                yield break;
            }

            SpawnOneShopper();

            if (i < shopperCount - 1)
            {
                yield return new WaitForSeconds(spawnIntervalSeconds);
            }
        }

        while (spawnContinuously)
        {
            if (!CanSpawn())
            {
                yield break;
            }

            float delay = Random.Range(minContinuousSpawnDelay, Mathf.Max(minContinuousSpawnDelay, maxContinuousSpawnDelay));
            yield return new WaitForSeconds(delay);

            if (!CanSpawn())
            {
                yield break;
            }

            SpawnOneShopper();
        }

        spawnRoutine = null;
    }

    private bool CanSpawn()
    {
        return gameManager != null && gameManager.State == GameManager.GameState.Playing && shopperPrefab != null && spawnPoint != null;
    }

    private void SpawnOneShopper()
    {
        GameObject shopper = Instantiate(shopperPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedShoppers.Add(shopper);
    }

    private void ClearSpawnedShoppers()
    {
        for (int i = spawnedShoppers.Count - 1; i >= 0; i--)
        {
            if (spawnedShoppers[i] != null)
            {
                Destroy(spawnedShoppers[i]);
            }
        }

        spawnedShoppers.Clear();
    }

    private void ClearSpawnedPlayer()
    {
        if (ownsSpawnedPlayer && spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }

        spawnedPlayer = null;
        ownsSpawnedPlayer = false;
    }
}
