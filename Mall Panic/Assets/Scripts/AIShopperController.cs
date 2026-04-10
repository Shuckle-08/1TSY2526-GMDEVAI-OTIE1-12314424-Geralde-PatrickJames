using UnityEngine;
using UnityEngine.AI;

public class AIShopperController : MonoBehaviour
{
    public enum ShopperState
    {
        Idle,
        MoveToItem,
        CollectItem,
        MoveToCashier,
        Wander,
        MoveToRandomPoint,
        WaitAtRandomPoint
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CashierCheckoutTrigger cashierTrigger;

    [Header("Behavior")]
    [SerializeField] private bool prioritizeValuableItems = true;
    [SerializeField] private float retargetInterval = 0.5f;
    [SerializeField] private float collectDistance = 1.25f;
    [SerializeField] private float cashierCommitTime = 30f;

    [Header("Random Checkout")]
    [SerializeField] private bool allowRandomCheckout = true;
    [SerializeField] private float randomCheckoutCheckInterval = 2f;
    [SerializeField, Range(0f, 1f)] private float randomCheckoutChancePerCheck = 0.35f;
    [SerializeField] private int minItemsBeforeRandomCheckout = 0;

    [Header("Random Loiter")]
    [SerializeField] private bool allowRandomLoiter = true;
    [SerializeField] private float randomLoiterCheckInterval = 4f;
    [SerializeField, Range(0f, 1f)] private float randomLoiterChancePerCheck = 0.2f;
    [SerializeField] private float randomLoiterMoveRadius = 8f;
    [SerializeField] private float randomLoiterMinDuration = 2f;
    [SerializeField] private float randomLoiterMaxDuration = 5f;

    [Header("Wander")]
    [SerializeField] private float blockedCheckInterval = 0.5f;
    [SerializeField] private float blockedDistanceThreshold = 0.1f;
    [SerializeField] private float wanderRadius = 6f;

    public ShopperState State { get; private set; } = ShopperState.Idle;
    public int ValuableItems { get; private set; }
    public int LowValueItems { get; private set; }
    public int Score => ValuableItems - LowValueItems;

    private ItemPickup targetItem;
    private float nextRetargetTime;
    private float nextBlockedCheckTime;
    private float nextRandomCheckoutCheckTime;
    private float nextRandomLoiterCheckTime;
    private float loiterEndTime;
    private Vector3 lastBlockedCheckPosition;
    private bool checkedOut;
    private bool committedToCashier;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (agent != null)
        {
            agent.avoidancePriority = Random.Range(20, 80);
        }

        if (cashierTrigger == null)
        {
            cashierTrigger = FindObjectOfType<CashierCheckoutTrigger>();
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.State);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameManager.GameState.Playing || checkedOut)
        {
            return;
        }

        if (!committedToCashier && (ShouldCommitToCashierByTime() || ShouldCommitToCashierRandomly()))
        {
            committedToCashier = true;
            SetState(ShopperState.MoveToCashier);
        }

        if (!committedToCashier && ShouldStartRandomLoiter())
        {
            TryStartRandomLoiter();
        }

        switch (State)
        {
            case ShopperState.Idle:
                HandleIdleState();
                break;
            case ShopperState.MoveToItem:
                HandleMoveToItemState();
                break;
            case ShopperState.CollectItem:
                HandleCollectItemState();
                break;
            case ShopperState.MoveToCashier:
                HandleMoveToCashierState();
                break;
            case ShopperState.Wander:
                HandleWanderState();
                break;
            case ShopperState.MoveToRandomPoint:
                HandleMoveToRandomPointState();
                break;
            case ShopperState.WaitAtRandomPoint:
                HandleWaitAtRandomPointState();
                break;
        }
    }

    public void OnCollectedItem(ItemPickup.ItemType itemType)
    {
        if (itemType == ItemPickup.ItemType.Valuable)
        {
            ValuableItems++;
        }
        else
        {
            LowValueItems++;
        }
    }

    public void NotifyReachedCashier()
    {
        checkedOut = true;
        committedToCashier = true;
        targetItem = null;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        Destroy(gameObject);
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            ResetForRound();
            return;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        SetState(ShopperState.Idle);
    }

    private void ResetForRound()
    {
        ValuableItems = 0;
        LowValueItems = 0;
        checkedOut = false;
        committedToCashier = false;
        targetItem = null;
        nextRandomCheckoutCheckTime = Time.time + Mathf.Max(0.1f, randomCheckoutCheckInterval);
        nextRandomLoiterCheckTime = Time.time + Mathf.Max(0.1f, randomLoiterCheckInterval);
        loiterEndTime = 0f;

        if (cashierTrigger == null)
        {
            cashierTrigger = FindObjectOfType<CashierCheckoutTrigger>();
        }

        if (agent != null)
        {
            agent.isStopped = false;
            lastBlockedCheckPosition = transform.position;
        }

        SetState(ShopperState.Idle);
    }

    private void HandleIdleState()
    {
        if (ShouldCommitToCashier())
        {
            SetState(ShopperState.MoveToCashier);
            return;
        }

        targetItem = FindBestItemTarget();
        if (targetItem == null)
        {
            SetState(ShopperState.MoveToCashier);
            return;
        }

        if (agent != null)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }

            agent.SetDestination(targetItem.transform.position);
            lastBlockedCheckPosition = transform.position;
            nextBlockedCheckTime = Time.time + blockedCheckInterval;
        }

        nextRetargetTime = Time.time + retargetInterval;
        SetState(ShopperState.MoveToItem);
    }

    private void HandleMoveToItemState()
    {
        if (targetItem == null || !targetItem.gameObject.activeInHierarchy)
        {
            SetState(ShopperState.Idle);
            return;
        }

        if (agent == null)
        {
            SetState(ShopperState.CollectItem);
            return;
        }

        if (Time.time >= nextRetargetTime)
        {
            ItemPickup newTarget = FindBestItemTarget();
            if (newTarget != null)
            {
                targetItem = newTarget;
                agent.SetDestination(targetItem.transform.position);
            }

            nextRetargetTime = Time.time + retargetInterval;
        }

        if (Vector3.Distance(transform.position, targetItem.transform.position) <= collectDistance)
        {
            SetState(ShopperState.CollectItem);
            return;
        }

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            SetState(ShopperState.Wander);
            return;
        }

        if (Time.time >= nextBlockedCheckTime)
        {
            float movedDistance = Vector3.Distance(transform.position, lastBlockedCheckPosition);
            if (movedDistance <= blockedDistanceThreshold && agent.remainingDistance > collectDistance + 0.2f)
            {
                SetState(ShopperState.Wander);
                return;
            }

            lastBlockedCheckPosition = transform.position;
            nextBlockedCheckTime = Time.time + blockedCheckInterval;
        }
    }

    private void HandleCollectItemState()
    {
        if (targetItem != null)
        {
            targetItem.TryCollect(gameObject);
        }

        targetItem = null;
        SetState(ShopperState.Idle);
    }

    private void HandleMoveToCashierState()
    {
        if (checkedOut)
        {
            return;
        }

        if (cashierTrigger == null)
        {
            cashierTrigger = FindObjectOfType<CashierCheckoutTrigger>();
            if (cashierTrigger == null)
            {
                return;
            }
        }

        if (agent != null)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }

            agent.SetDestination(cashierTrigger.transform.position);
        }
    }

    private void HandleMoveToRandomPointState()
    {
        if (ShouldCommitToCashier())
        {
            SetState(ShopperState.MoveToCashier);
            return;
        }

        if (agent == null)
        {
            SetState(ShopperState.Idle);
            return;
        }

        if (!agent.pathPending && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f))
        {
            loiterEndTime = Time.time + Random.Range(randomLoiterMinDuration, Mathf.Max(randomLoiterMinDuration, randomLoiterMaxDuration));
            agent.isStopped = true;
            SetState(ShopperState.WaitAtRandomPoint);
        }
    }

    private void HandleWaitAtRandomPointState()
    {
        if (ShouldCommitToCashier())
        {
            SetState(ShopperState.MoveToCashier);
            return;
        }

        if (Time.time >= loiterEndTime)
        {
            if (agent != null)
            {
                agent.isStopped = false;
                agent.ResetPath();
            }

            SetState(ShopperState.Idle);
        }
    }

    private void HandleWanderState()
    {
        if (ShouldCommitToCashier())
        {
            SetState(ShopperState.MoveToCashier);
            return;
        }

        if (agent == null)
        {
            SetState(ShopperState.Idle);
            return;
        }

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * wanderRadius;
            randomOffset.y = 0f;
            Vector3 randomPoint = transform.position + randomOffset;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                }

                agent.SetDestination(hit.position);
            }

            SetState(ShopperState.Idle);
        }
    }

    private bool ShouldCommitToCashier()
    {
        return committedToCashier || ShouldCommitToCashierByTime();
    }

    private bool ShouldCommitToCashierByTime()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        return GameManager.Instance.TimeRemaining <= cashierCommitTime;
    }

    private bool ShouldCommitToCashierRandomly()
    {
        if (!allowRandomCheckout)
        {
            return false;
        }

        if (Time.time < nextRandomCheckoutCheckTime)
        {
            return false;
        }

        nextRandomCheckoutCheckTime = Time.time + Mathf.Max(0.1f, randomCheckoutCheckInterval);

        int totalItems = ValuableItems + LowValueItems;
        if (totalItems < Mathf.Max(0, minItemsBeforeRandomCheckout))
        {
            return false;
        }

        return Random.value <= randomCheckoutChancePerCheck;
    }

    private bool ShouldStartRandomLoiter()
    {
        if (!allowRandomLoiter)
        {
            return false;
        }

        if (State == ShopperState.MoveToCashier || State == ShopperState.MoveToRandomPoint || State == ShopperState.WaitAtRandomPoint)
        {
            return false;
        }

        if (Time.time < nextRandomLoiterCheckTime)
        {
            return false;
        }

        nextRandomLoiterCheckTime = Time.time + Mathf.Max(0.1f, randomLoiterCheckInterval);
        return Random.value <= randomLoiterChancePerCheck;
    }

    private ItemPickup FindBestItemTarget()
    {
        ItemPickup[] items = FindObjectsOfType<ItemPickup>();
        ItemPickup bestItem = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < items.Length; i++)
        {
            ItemPickup item = items[i];
            if (item == null || !item.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, item.transform.position);
            float valueBias = 0f;

            if (prioritizeValuableItems)
            {
                valueBias = item.Type == ItemPickup.ItemType.Valuable ? 10f : -10f;
            }

            float candidateScore = valueBias - distance;
            if (candidateScore > bestScore)
            {
                bestScore = candidateScore;
                bestItem = item;
            }
        }

        return bestItem;
    }

    private void TryStartRandomLoiter()
    {
        if (agent == null)
        {
            return;
        }

        Vector3 randomOffset = Random.insideUnitSphere * randomLoiterMoveRadius;
        randomOffset.y = 0f;
        Vector3 candidate = transform.position + randomOffset;

        if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, randomLoiterMoveRadius, NavMesh.AllAreas))
        {
            return;
        }

        if (agent.isStopped)
        {
            agent.isStopped = false;
        }

        targetItem = null;
        agent.SetDestination(hit.position);
        SetState(ShopperState.MoveToRandomPoint);
    }

    private void SetState(ShopperState newState)
    {
        State = newState;
    }
}
