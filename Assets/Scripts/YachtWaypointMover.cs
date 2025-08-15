using UnityEngine;

public class YachtWaypointMover : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    [Tooltip("Distance considered 'arrived' at a waypoint (XZ only).")]
    public float arriveThreshold = 0.5f;

    [Header("Motion")]
    [Tooltip("Units per second.")]
    public float moveSpeed = 5f;
    [Tooltip("Degrees per second.")]
    public float turnSpeed = 90f;
    [Tooltip("Optional pause at each waypoint (seconds).")]
    public float waitAtWaypoint = 0f;

    [Header("Path Mode")]
    public bool loop = true;
    public bool pingPong = false;

    [Header("Misc")]
    [Tooltip("If set, overrides initial Y with this water height.")]
    public bool useFixedWaterHeight = false;
    public float waterY = 0f;
    [Tooltip("Draw gizmos for the path in the Scene view.")]
    public bool drawGizmos = true;

    int _index = 0;
    int _dir = 1; // used for ping-pong
    float _fixedY;
    float _waitTimer = 0f;

    void Start()
    {
        // Lock the yacht to a fixed Y at start (or a provided water height)
        _fixedY = useFixedWaterHeight ? waterY : transform.position.y;

        // Snap to fixed Y immediately
        var p = transform.position;
        transform.position = new Vector3(p.x, _fixedY, p.z);

        // Start at the closest waypoint if none assigned, otherwise keep given index
        if (waypoints != null && waypoints.Length > 0 && _index >= waypoints.Length)
            _index = 0;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Current target (with Y locked)
        Vector3 target = waypoints[_index].position;
        target.y = _fixedY;

        // Compute flat direction on XZ
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        float sqrDist = toTarget.sqrMagnitude;

        // Smoothly rotate toward target using a proper angular speed
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                turnSpeed * Time.deltaTime
            );
        }

        // Move forward at constant speed in current forward direction
        // (gives nice boat-like arcs while turning)
        Vector3 forwardFlat = transform.forward; forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude > 0.0001f)
        {
            forwardFlat.Normalize();
            Vector3 next = transform.position + forwardFlat * (moveSpeed * Time.deltaTime);
            next.y = _fixedY; // lock Y
            transform.position = next;
        }
        else
        {
            // Fallback: if somehow forward is invalid, move directly towards target
            Vector3 next = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            next.y = _fixedY;
            transform.position = next;
        }

        // Arrival check (XZ plane)
        if (sqrDist <= arriveThreshold * arriveThreshold)
        {
            if (_waitTimer <= 0f && waitAtWaypoint > 0f)
            {
                _waitTimer = waitAtWaypoint;
            }

            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.deltaTime;
                return;
            }

            AdvanceIndex();
        }

        // Hard-lock Y every frame (in case something else nudges it)
        var pos = transform.position;
        transform.position = new Vector3(pos.x, _fixedY, pos.z);
    }

    void AdvanceIndex()
    {
        if (pingPong && waypoints.Length > 1)
        {
            _index += _dir;
            if (_index >= waypoints.Length)
            {
                _index = waypoints.Length - 2;
                _dir = -1;
            }
            else if (_index < 0)
            {
                _index = 1;
                _dir = 1;
            }
        }
        else
        {
            _index++;
            if (_index >= waypoints.Length)
            {
                if (loop) _index = 0;
                else _index = waypoints.Length - 1; // stay on last
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || waypoints == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Vector3 a = waypoints[i].position;
            a.y = useFixedWaterHeight ? waterY : (Application.isPlaying ? _fixedY : a.y);
            Gizmos.DrawSphere(a, 0.25f);

            int j = (i + 1) % waypoints.Length;
            if (i < waypoints.Length - 1 || loop)
            {
                if (waypoints[j] != null)
                {
                    Vector3 b = waypoints[j].position;
                    b.y = a.y;
                    Gizmos.DrawLine(a, b);
                }
            }
        }
    }
}
