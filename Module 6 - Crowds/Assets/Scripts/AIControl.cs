using UnityEngine;
using UnityEngine.AI;

public class AIControl : MonoBehaviour
{
    GameObject[] goalLocations;
    NavMeshAgent agent;
    Animator animator;

    float speedMultiplier;

    float detectionRadius = 5f;
    float fleeRadius = 10f;

    void ResetAgent()
    {
        speedMultiplier = Random.Range(0.1f, 1.5f);
        agent.speed = 2 * speedMultiplier;
        agent.angularSpeed = 120;
        animator.SetFloat("speedMultiplier", speedMultiplier);
        animator.SetTrigger("isWalking");
        agent.ResetPath();
    }

    void Start()
    {
        goalLocations = GameObject.FindGameObjectsWithTag("goal");
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(goalLocations[Random.Range(0, goalLocations.Length)].transform.position);

        animator = GetComponent<Animator>();
        animator.SetFloat("wOffset", Random.Range(0f, 1f));

        ResetAgent();
    }

    void Update()
    {
        if (agent.remainingDistance < 1f)
        {
            ResetAgent();
            agent.SetDestination(goalLocations[Random.Range(0, goalLocations.Length)].transform.position);
        }
    }

    public void DetectNewObstacle(Vector3 location)
    {
        if (Vector3.Distance(location, transform.position) < detectionRadius)
        {
            Vector3 fleeDirection = (transform.position - location).normalized;
            Vector3 newGoal = transform.position + fleeDirection * fleeRadius;

            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(newGoal, path);

            if (path.status != NavMeshPathStatus.PathInvalid && path.corners.Length > 0)
            {
                agent.SetDestination(path.corners[path.corners.Length - 1]);
                animator.SetTrigger("isRunning");
                agent.speed = 10f;
                agent.angularSpeed = 500f;
            } 
        }
    }

    public void FlockTo(Vector3 location)
    {
        // 15f is an arbitrary radius so the whole map doesn't flock unnecessarily, 
        // but large enough so nearby crowd flocks easily
        if (Vector3.Distance(location, transform.position) < detectionRadius * 3) 
        {
            agent.SetDestination(location);
            animator.SetTrigger("isRunning");
            agent.speed = 10f;
            agent.angularSpeed = 500f;
        }
    }
}
