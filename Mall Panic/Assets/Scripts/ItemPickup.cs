using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        Valuable,
        LowValue
    }

    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.Valuable;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool allowAiShoppers = true;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Visual Model")]
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject[] modelPrefabs;
    [SerializeField] private Transform spawnPoint;

    public ItemType Type => itemType;

    public void SetItemType(ItemType type)
    {
        itemType = type;
    }

    private bool collected = false;

    private void Start()
    {
        Transform parent = spawnPoint != null ? spawnPoint : transform;
        GameObject selectedModel = GetRandomModelPrefab();

        if (selectedModel != null)
        {
            Instantiate(selectedModel, parent.position, parent.rotation, parent);
        }
    }

    private GameObject GetRandomModelPrefab()
    {
        if (modelPrefabs != null && modelPrefabs.Length > 0)
        {
            int startIndex = Random.Range(0, modelPrefabs.Length);
            for (int i = 0; i < modelPrefabs.Length; i++)
            {
                int index = (startIndex + i) % modelPrefabs.Length;
                if (modelPrefabs[index] != null)
                {
                    return modelPrefabs[index];
                }
            }
        }

        return modelPrefab;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryCollect(collision.gameObject);
    }

    public bool TryCollect(GameObject collector)
    {
        if (collector == null || collected || !IsValidCollector(collector))
        {
            return false;
        }

        PickupItem(collector);
        return true;
    }

    private bool IsValidCollector(GameObject collector)
    {
        if (collector.CompareTag(playerTag))
        {
            return true;
        }

        if (allowAiShoppers && collector.GetComponent<AIShopperController>() != null)
        {
            return true;
        }

        return false;
    }

    private void PickupItem(GameObject collector)
    {
        collected = true;

        AIShopperController aiShopper = collector.GetComponent<AIShopperController>();
        if (aiShopper != null)
        {
            aiShopper.OnCollectedItem(itemType);
        }
        else if (GameManager.Instance != null)
        {
            if (itemType == ItemType.Valuable)
            {
                GameManager.Instance.AddValuableItem(1);
            }
            else
            {
                GameManager.Instance.AddLowValueItem(1);
            }
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
