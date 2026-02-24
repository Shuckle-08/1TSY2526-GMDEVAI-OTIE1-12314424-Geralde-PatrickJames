using UnityEngine;

public class Pet : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    
    [Header("Movement Settings")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    
    [Header("Height Settings")]
    [SerializeField] private float hoverHeight = 0f;
    [SerializeField] private bool matchTargetHeight = false;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    void Update()
    {
        if (target == null) return;
        
        FollowTarget();
    }

    private void FollowTarget()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance > minDistance)
        {
            Vector3 targetPosition = target.position + Vector3.up * hoverHeight;
            
            if (!matchTargetHeight)
            {
                targetPosition.y = transform.position.y;
            }
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            float moveDistance;
            if (distance > maxDistance)
            {
                moveDistance = followSpeed * 2f;
            }
            else
            {
                moveDistance = followSpeed;
            }
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveDistance * Time.deltaTime);
        }
    }
}

