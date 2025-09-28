using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class YachtWaypointMover : MonoBehaviour
{
    [Header("Path")]
    public Transform[] waypoints;
    public Transform[] waitingWaypoints;
    [Tooltip("Distance considered 'arrived' at a waypoint (XZ only).")]
    public float arriveThreshold = 0.5f;

    [Header("Motion")]
    [Tooltip("Units per second.")]
    public float moveSpeed = 5f;
    [Tooltip("Degrees per second.")]
    public float turnSpeed = 90f;
    [Tooltip("Optional pause at waiting waypoints (seconds).")]
    public float waitAtWaypoint = 0f;

    [Header("Path Mode")]
    public bool loop = true;
    public bool pingPong = false;

    [Header("Misc")]
    [Tooltip("If set, overrides initial Y with this water height.")]
    public bool useFixedWaterHeight = false;
    public float waterY = 0f;
    public bool drawGizmos = true;

    [Header("Events")]
    public UnityEvent onBoatArrival;

    int _index = 0;
    int _dir = 1;
    float _fixedY;
    float _waitTimer = 0f;
    bool _isWaiting = false;

    void Start()
    {
        _fixedY = useFixedWaterHeight ? waterY : transform.position.y;

        var p = transform.position;
        transform.position = new Vector3(p.x, _fixedY, p.z);

        if (waypoints != null && waypoints.Length > 0 && _index >= waypoints.Length)
            _index = 0;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 target = waypoints[_index].position;
        target.y = _fixedY;

        Vector3 toTarget = target - transform.position;
        toTarget.y = 0f;
        float sqrDist = toTarget.sqrMagnitude;

        // ✅ Handle arrival & waiting first
        if (sqrDist <= arriveThreshold * arriveThreshold)
        {
            // Start waiting only once at this waypoint
            if (!_isWaiting && waitAtWaypoint > 0f && waitingWaypoints.Contains(waypoints[_index]))
            {
                _waitTimer = waitAtWaypoint;
                _isWaiting = true;

                // 🚨 Trigger UnityEvent
                onBoatArrival?.Invoke();
            }

            // If currently waiting, count down and stop moving/rotating
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.deltaTime;
                return;
            }

            // Done waiting → reset and advance
            if (_isWaiting)
            {
                _isWaiting = false;
                AdvanceIndex();
                return;
            }

            // If not a waiting waypoint → advance immediately
            AdvanceIndex();
            return;
        }

        // ✅ Rotate toward target
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                turnSpeed * Time.deltaTime
            );
        }

        // ✅ Move forward
        Vector3 forwardFlat = transform.forward; forwardFlat.y = 0f;
        if (forwardFlat.sqrMagnitude > 0.0001f)
        {
            forwardFlat.Normalize();
            Vector3 next = transform.position + forwardFlat * (moveSpeed * Time.deltaTime);
            next.y = _fixedY;
            transform.position = next;
        }
        else
        {
            Vector3 next = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            next.y = _fixedY;
            transform.position = next;
        }

        // Hard-lock Y every frame
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
                else _index = waypoints.Length - 1;
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

        if (waitingWaypoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var w in waitingWaypoints)
            {
                if (w != null)
                    Gizmos.DrawWireSphere(w.position, 0.6f);
            }
        }
    }
}
