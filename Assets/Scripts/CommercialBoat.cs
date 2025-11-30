using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SpatialSys.UnitySDK;

public class CommercialBoat : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private SpatialSyncedObject syncedObject;
    [SerializeField] private float speed;
    [SerializeField] private float safeDistance;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform[] waypoints;

    public int currentWaypointIndex;

    private bool isInitialized;

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Vector3 a = waypoints[i].position;
            Gizmos.DrawSphere(a, 0.25f);

            int j = (i + 1) % waypoints.Length;   // wraps last → first
            if (waypoints[j] != null)
            {
                Vector3 b = waypoints[j].position;
                b.y = a.y;
                Gizmos.DrawLine(a, b);            // now closes loop
            }
        }
    }

    private void Update()
    {
        // Only the local owner controls movement
        if (!syncedObject.isLocallyOwned) return;
        if (waypoints == null || waypoints.Length == 0) return;
        if (agent == null) return;

        // One-time setup, but only after ownership is confirmed
        if (!isInitialized)
        {
            InitializeAgent();
            isInitialized = true;
        }

        // Keep agent speed in sync with inspector var if you tweak at runtime
        agent.speed = speed;
        agent.stoppingDistance = safeDistance;

        // ----- Loop movement -----
        Vector3 targetPos = waypoints[currentWaypointIndex].position;
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);

        if (distanceToTarget <= safeDistance)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            SetDestinationToCurrentWaypoint();
            targetPos = waypoints[currentWaypointIndex].position;
        }

        // ----- Smooth rotation towards movement direction / target -----
        Vector3 lookPoint = agent.hasPath ? agent.steeringTarget : targetPos;
        Vector3 direction = lookPoint - transform.position;
        direction.y = 0f; // keep the boat level

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void InitializeAgent()
    {
        // In case you forgot to assign it in the inspector
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false; // we rotate manually

        currentWaypointIndex = Mathf.Clamp(currentWaypointIndex, 0, waypoints.Length - 1);
        SetDestinationToCurrentWaypoint();
    }

    private void SetDestinationToCurrentWaypoint()
    {
        if (waypoints[currentWaypointIndex] == null) return;

        Vector3 dest = waypoints[currentWaypointIndex].position;
        agent.SetDestination(dest);
    }
}
