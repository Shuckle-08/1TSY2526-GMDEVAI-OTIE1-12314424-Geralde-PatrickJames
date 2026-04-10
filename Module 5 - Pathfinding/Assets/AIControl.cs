using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class AIControl : MonoBehaviour
{
    enum AgentType
    {
        Agent1,
        Agent2,
        Agent3
    }

    NavMeshAgent agent;
    public GameObject target;
    public WASDMovement playerMovement;

    [SerializeField] AgentType agentType = AgentType.Agent1;
    [SerializeField] float detectionRange = 15f;

    Vector3 wanderTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerMovement = target.GetComponent<WASDMovement>();
    }

    void Seek(Vector3 target)
    {
        agent.SetDestination(target);
    }

    void Flee(Vector3 target)
    {
        Vector3 fleeDirection = target - this.transform.position;
        Vector3 newTarget = this.transform.position - fleeDirection;
        agent.SetDestination(newTarget);
    }

    void Pursue(Vector3 target)
    {
        Vector3 targetDirection = target - this.transform.position;
        float lookAhead = targetDirection.magnitude / (agent.speed + playerMovement.currentSpeed);
        Seek(target + playerMovement.transform.forward * lookAhead);
    }

    void Evade(Vector3 target)
    {
        Vector3 targetDirection = target - this.transform.position;
        float lookAhead = targetDirection.magnitude / (agent.speed + playerMovement.currentSpeed);
        Flee(target + playerMovement.transform.forward * lookAhead);
    }

    void Wander()
    {
        float wanderRadius = 20f;
        float wanderDistance = 10f;
        float wanderJitter = 1f;

        wanderTarget += new Vector3(Random.Range(-1f, 1f) * wanderJitter, 0, Random.Range(-1f, 1f) * wanderJitter);
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = this.gameObject.transform.InverseTransformVector(targetLocal);

        Seek(targetWorld);
    }

    void Hide()
    {
        GameObject[] hidingSpots = World.Instance.GetHidingSpots;

        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        int hidingSpotsCount = hidingSpots.Length;

        for (int i = 0; i < hidingSpotsCount; i++)
        {
            Vector3 hideDirection = hidingSpots[i].transform.position - target.transform.position;
            Vector3 hidePosition = hidingSpots[i].transform.position + hideDirection.normalized * 5f;

            float spotDistance = Vector3.Distance(this.transform.position, hidePosition);
            if (spotDistance < distance)
            {
                chosenSpot = hidePosition;
                distance = spotDistance;
            }
        }

        Seek(chosenSpot);
    }

    void CleverHide()
    {
        GameObject[] hidingSpots = World.Instance.GetHidingSpots;

        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 chosenDir = Vector3.zero;
        GameObject chosenGameObject = hidingSpots[0];

        int hidingSpotsCount = hidingSpots.Length;

        for (int i = 0; i < hidingSpotsCount; i++)
        {
            Vector3 hideDirection = hidingSpots[i].transform.position - target.transform.position;
            Vector3 hidePosition = hidingSpots[i].transform.position + hideDirection.normalized * 5f;

            float spotDistance = Vector3.Distance(this.transform.position, hidePosition);
            if (spotDistance < distance)
            {
                chosenSpot = hidePosition;
                chosenDir = hideDirection;
                chosenGameObject = hidingSpots[i];
                distance = spotDistance;
            }
        }

        Collider hideCol = chosenGameObject.GetComponent<Collider>();
        Ray back = new Ray(chosenSpot, -chosenDir.normalized);
        RaycastHit info;
        float rayDistance = 100.0f;

        if (hideCol.Raycast(back, out info, rayDistance))
        {
            Seek(info.point + chosenDir.normalized * 5f);
        }
        else
        {
            Seek(chosenSpot);
        }
    }

    bool canSeeTarget()
    {
        RaycastHit raycastInfo;
        Vector3 rayToTarget = target.transform.position - this.transform.position;
        if (Physics.Raycast(this.transform.position, rayToTarget, out raycastInfo))
        {
            return raycastInfo.transform.gameObject.tag == "Player";
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToTarget = Vector3.Distance(this.transform.position, target.transform.position);
        bool isTargetInRange = distanceToTarget <= detectionRange;

        switch (agentType)
        {
            case AgentType.Agent1:
                if (isTargetInRange)
                {
                    Pursue(target.transform.position);
                }
                else
                {
                    Wander();
                }
                break;

            case AgentType.Agent2:
                if (isTargetInRange && canSeeTarget())
                {
                    Hide();
                }
                else
                {
                    Wander();
                }
                break;

            case AgentType.Agent3:
                if (isTargetInRange)
                {
                    Evade(target.transform.position);
                }
                else
                {
                    Wander();
                }
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, detectionRange);
    }
}
