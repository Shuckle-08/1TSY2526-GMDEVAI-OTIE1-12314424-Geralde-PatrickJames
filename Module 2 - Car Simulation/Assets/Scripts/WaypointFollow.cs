using UnityEngine;
using UnityStandardAssets.Utility;

public class WaypointFollow : MonoBehaviour
{
    //public GameObject[] waypoints;
    public UnityStandardAssets.Utility.WaypointCircuit circuit;
    private int currentWaypointIndex = 0;

    float speed = 5;
    float rotationSpeed = 2f;
    float accuracy = 1;

    private void Start()
    {
        //waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
    }

    private void LateUpdate()
    {
        if (circuit.Waypoints.Length == 0) return;

        GameObject currentWaypoint = circuit.Waypoints[currentWaypointIndex].gameObject;

        Vector3 lookAtGoal = new Vector3(currentWaypoint.transform.position.x, transform.position.y, currentWaypoint.transform.position.z);

        Vector3 direction = (lookAtGoal - transform.position);

        if (direction.magnitude < 1.0f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= circuit.Waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        this.transform.Translate(0, 0, speed * Time.deltaTime);
    }
}
