using UnityEngine;
using UnityEngine.AI;

public class GenerateGroceries : MonoBehaviour
{
    [Header("Item Prefabs")]
    [SerializeField] private GameObject valuableItemPrefab;
    [SerializeField] private GameObject lowValueItemPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int valuableItemCount = 5;
    [SerializeField] private int lowValueItemCount = 3;
    [SerializeField] private float itemSpacing = 2f;
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);
    [SerializeField] private Collider gameAreaBoundsCollider;

    [Header("Spawn Constraints")]
    [SerializeField] private bool constrainToNavMesh = true;
    [SerializeField] private float navMeshSampleDistance = 1.5f;
    [SerializeField] private int maxSpawnPositionAttempts = 20;

    [Header("Spawn Timing")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool spawnOnRoundStart = true;

    [Header("Progressive Spawning")]
    [SerializeField] private bool spawnOverTime = true;
    [SerializeField] private float spawnIntervalSeconds = 10f;
    [SerializeField] private int spawnBatchSize = 1;
    [SerializeField, Range(0f, 1f)] private float valuableSpawnChance = 0.6f;

    private float nextProgressiveSpawnTime;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnGroceries();
        }

        if (spawnOnRoundStart && GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged += HandleGameStateChanged;
        }
    }

    private void Update()
    {
        if (!spawnOverTime || GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing)
        {
            return;
        }

        if (Time.time < nextProgressiveSpawnTime)
        {
            return;
        }

        int count = Mathf.Max(1, spawnBatchSize);
        for (int i = 0; i < count; i++)
        {
            SpawnRandomItem();
        }

        nextProgressiveSpawnTime = Time.time + Mathf.Max(0.1f, spawnIntervalSeconds);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Playing)
        {
            ClearGroceries();
            SpawnGroceries();
            nextProgressiveSpawnTime = Time.time + Mathf.Max(0.1f, spawnIntervalSeconds);
        }
    }

    [ContextMenu("Spawn Groceries")]
    public void SpawnGroceries()
    {
        for (int i = 0; i < valuableItemCount; i++)
        {
            SpawnItem(ItemPickup.ItemType.Valuable);
        }

        for (int i = 0; i < lowValueItemCount; i++)
        {
            SpawnItem(ItemPickup.ItemType.LowValue);
        }
    }

    private void SpawnRandomItem()
    {
        bool spawnValuable = Random.value <= valuableSpawnChance;
        SpawnItem(spawnValuable ? ItemPickup.ItemType.Valuable : ItemPickup.ItemType.LowValue);
    }

    private void SpawnItem(ItemPickup.ItemType itemType)
    {
        GameObject prefab = itemType == ItemPickup.ItemType.Valuable ? valuableItemPrefab : lowValueItemPrefab;
        if (prefab == null)
        {
            return;
        }

        Vector3 randomPosition = GetRandomSpawnPosition();
        GameObject itemInstance = Instantiate(prefab, randomPosition, Quaternion.identity, transform);

        ItemPickup itemPickup = itemInstance.GetComponent<ItemPickup>();
        if (itemPickup == null)
        {
            itemPickup = itemInstance.AddComponent<ItemPickup>();
        }

        itemPickup.SetItemType(itemType);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = gameAreaBoundsCollider != null
            ? gameAreaBoundsCollider.bounds
            : new Bounds(spawnAreaCenter, new Vector3(spawnAreaSize.x, 1f, spawnAreaSize.z));

        int attempts = Mathf.Max(1, maxSpawnPositionAttempts);
        for (int i = 0; i < attempts; i++)
        {
            Vector3 candidate = GetRandomPointInBounds(bounds);

            if (constrainToNavMesh)
            {
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    continue;
                }

                candidate = hit.position;
            }

            if (gameAreaBoundsCollider == null || bounds.Contains(candidate))
            {
                return candidate;
            }
        }

        return spawnAreaCenter;
    }

    private Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        return new Vector3(randomX, transform.position.y, randomZ);
    }

    private void ClearGroceries()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            ItemPickup itemPickup = child.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
