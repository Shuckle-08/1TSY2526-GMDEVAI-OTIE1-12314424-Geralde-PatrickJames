using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	public GameObject explosion;
    public float damage = 20f;
	
	void OnCollisionEnter(Collision col)
    {
        GameObject hitObject = col.rigidbody != null ? col.rigidbody.gameObject : col.gameObject;

        TankAI ai = hitObject.GetComponent<TankAI>();
        if (ai == null)
        {
            ai = hitObject.GetComponentInParent<TankAI>();
        }
        if (ai == null)
        {
            ai = hitObject.GetComponentInChildren<TankAI>();
        }

        if (ai != null)
        {
            ai.TakeDamage(damage);
        }
        else
        {
            Drive drive = hitObject.GetComponent<Drive>();
            if (drive == null)
            {
                drive = hitObject.GetComponentInParent<Drive>();
            }
            if (drive == null)
            {
                drive = hitObject.GetComponentInChildren<Drive>();
            }

            if (drive != null)
            {
                drive.TakeDamage(damage);
            }
        }

    	GameObject e = Instantiate(explosion, this.transform.position, Quaternion.identity);
    	Destroy(e,1.5f);
    	Destroy(this.gameObject);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
