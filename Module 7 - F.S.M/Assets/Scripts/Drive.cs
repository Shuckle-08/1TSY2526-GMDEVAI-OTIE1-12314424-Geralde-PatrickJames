using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour {

 	public float speed = 10.0F;
    public float rotationSpeed = 100.0F;
    public GameObject bullet;
    public GameObject turret;
    public KeyCode fireKey = KeyCode.Space;
    public float fireForce = 500f;
    public float maxHP = 100f;

    Animator animator;
    float currentHP;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        if (animator != null)
        {
            animator.SetFloat("health", currentHP);
        }
    }

    void Update() {
        float translation = Input.GetAxis("Vertical") * speed;
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed;
        translation *= Time.deltaTime;
        rotation *= Time.deltaTime;
        transform.Translate(0, 0, translation);
        transform.Rotate(0, rotation, 0);

        if (Input.GetKeyDown(fireKey))
        {
            Fire();
        }
 	}

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Max(0f, currentHP - amount);

        if (animator != null)
        {
            animator.SetFloat("health", currentHP);
        }

        if (currentHP <= 0f)
        {
            Destroy(gameObject);
        }
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

        b.GetComponent<Rigidbody>().AddForce(turret.transform.forward * fireForce);
    }
}
