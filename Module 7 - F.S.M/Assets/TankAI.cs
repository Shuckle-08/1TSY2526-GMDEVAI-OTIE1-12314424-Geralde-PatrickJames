using UnityEngine;

public class TankAI : MonoBehaviour
{
    Animator animator;

    public GameObject player;

    public GameObject bullet;
    public GameObject turret;

    [Range(0f, 1f)]
    public float fleeThreshold = 0.2f;
    public float maxHP = 100f;

    float currentHP;
    bool hasFleeParameter;

    public GameObject GetPlayer()
    {
        return player;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = this.GetComponent<Animator>();
        currentHP = maxHP;
        animator.SetFloat("health", currentHP);
        hasFleeParameter = HasAnimatorParameter("flee");
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("distance", Vector3.Distance(player.transform.position, this.transform.position));

        if (hasFleeParameter)
        {
            animator.SetBool("flee", currentHP / maxHP <= fleeThreshold);
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - amount);
        animator.SetFloat("health", currentHP);

        if (currentHP <= 0f)
        {
            StopFiring();
            Destroy(gameObject);
        }
    }

    bool HasAnimatorParameter(string parameterName)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }

    void Fire()
    {
        Vector3 spawnPosition = turret.transform.position + turret.transform.forward * 1.5f;
        GameObject b = Instantiate(bullet, spawnPosition, turret.transform.rotation);

        Collider bulletCollider = b.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            Collider[] ownColliders = GetComponentsInChildren<Collider>();
            foreach (Collider ownCollider in ownColliders)
            {
                Physics.IgnoreCollision(bulletCollider, ownCollider);
            }
        }

        b.GetComponent<Rigidbody>().AddForce(turret.transform.forward * 500f);
    }

    public void StopFiring()
    {
        CancelInvoke("Fire");
    }

    public void StartFiring()
    {
        InvokeRepeating("Fire", 0.5f, 0.5f);
    }
}
