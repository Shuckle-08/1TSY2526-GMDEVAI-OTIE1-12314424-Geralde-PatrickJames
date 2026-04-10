using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private bool findPlayerByTagIfMissing = true;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
    }

    private void Start()
    {
        if (target == null && findPlayerByTagIfMissing)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void Update()
    {
        if (target == null || !agent.isOnNavMesh)
        {
            return;
        }

        if (agent.stoppingDistance != stoppingDistance)
        {
            agent.stoppingDistance = Mathf.Max(0f, stoppingDistance);
        }

        agent.SetDestination(target.position);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
