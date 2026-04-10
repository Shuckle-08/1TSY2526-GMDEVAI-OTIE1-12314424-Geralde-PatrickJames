using UnityEngine;

public class FollowPath : MonoBehaviour
{
    Transform goal;
    float speed = 5f;
    float accuracy = 1f;
    float rotSpeed = 5f;
    public GameObject wpManager;
    GameObject[] wps;
    GameObject currentNode;
    int currentWaypointIndex = 0;
    Graph graph;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (wpManager == null)
        {
            return;
        }

        WaypointManager manager = wpManager.GetComponent<WaypointManager>();
        if (manager == null)
        {
            return;
        }

        wps = manager.waypoints;
        graph = manager.graph;

        if (wps != null && wps.Length > 0)
        {
            currentNode = wps[currentWaypointIndex];
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (graph == null)
        {
            return;
        }

        int pathLength = graph.getPathLength();
        if (pathLength == 0 || currentWaypointIndex >= pathLength)
        {
            return;
        }

        currentNode = graph.getPathPoint(currentWaypointIndex);
        if (currentNode == null)
        {
            return;
        }

        if (Vector3.Distance(currentNode.transform.position, transform.position) < accuracy)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= pathLength)
            {
                return;
            }

            currentNode = graph.getPathPoint(currentWaypointIndex);
            if (currentNode == null)
            {
                return;
            }
        }

        goal = currentNode.transform;
        Vector3 lookAtGoal = new Vector3(goal.position.x, transform.position.y, goal.position.z);
        Vector3 direction = lookAtGoal - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                    Quaternion.LookRotation(direction),
                                    Time.deltaTime * rotSpeed);

        this.transform.Translate(0, 0, speed * Time.deltaTime);
    }

    void GoToWaypoint(int waypointIndex)
    {
        if (wps == null || graph == null)
        {
            return;
        }

        if (waypointIndex < 0 || waypointIndex >= wps.Length || currentNode == null)
        {
            return;
        }

        if (graph.AStar(currentNode, wps[waypointIndex]))
        {
            currentWaypointIndex = 0;
            currentNode = graph.getPathPoint(0);
        }
    }

    public void GoToHelipad()
    {
        GoToWaypoint(0);
    }

    public void GoToTwinMountains()
    {
        GoToWaypoint(8);
    }

    public void GoToBarracks()
    {
        GoToWaypoint(9);
    }

    public void GoToCommandCenter()
    {
        GoToWaypoint(0);
    }

    public void GoToOilRefineryPumps()
    {
        GoToWaypoint(10);
    }

    public void GoToTankers()
    {
        GoToWaypoint(3);
    }

    public void GoToRadar()
    {
        GoToWaypoint(11);
    }

    public void GoToCommandPost()
    {
        GoToWaypoint(12);
    }

    public void GoToMiddle()
    {
        GoToWaypoint(6);
    }
}
