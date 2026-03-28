using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    public Transform goal;
    public float speed = 0f;
    public float rotationSpeed = 2f;
    public float acceleration = 0.1f;
    public float deceleration = 0.1f;
    public float minSpeed = 0f;
    public float maxSpeed = 10f;
    public float breakAngle = 45f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        Vector3 lookAtGoal = new Vector3(goal.position.x, transform.position.y, goal.position.z);
        Vector3 direction = (lookAtGoal - transform.position);

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);

        speed = Mathf.Clamp(speed + (acceleration * Time.deltaTime), minSpeed, maxSpeed);

        if (Vector3.Angle(goal.forward, this.transform.forward) > breakAngle)
        {
            speed = Mathf.Clamp(speed - (deceleration * Time.deltaTime), minSpeed, maxSpeed);
        } else
        {
            speed = Mathf.Clamp(speed + (acceleration * Time.deltaTime), minSpeed, maxSpeed);
        }

        this.transform.Translate(0, 0, speed * Time.deltaTime);
    }
}
