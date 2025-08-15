using SpatialSys.UnitySDK;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SheepBrain : MonoBehaviour
{
    [Header("Components")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Waypoints/Grazing")]
    public List<Transform> grasses;
    public float stoppingDistance = 1f;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTime = 10f;
    private float waitTimer = 0f;

    [Header("Animator Blend Speeds")]
    [Tooltip("Value fed into Animator 'Speed' for walking blend (your blend tree decides actual clip).")]
    public float walkSpeed = 0.5f;
    [Tooltip("Value fed into Animator 'Speed' for running blend (often 0..1).")]
    public float runSpeed = 1.0f;

    [Header("Agent Planning Speeds (planner only)")]
    public float agentWalkSpeed = 2f;
    public float agentRunningSpeed = 2.2f;   // ↑ Set higher than player speed

    [Header("Run Speed Control")]
    [Tooltip("Multiply animation playback while running to hide sliding when speeding up.")]
    public float runAnimSpeedMultiplier = 1.15f;

    [Tooltip("Scale root motion toward the agent's planned speed during RUN only.")]
    public bool speedWarpDuringRun = true;
    [Tooltip("Max upward scale applied to root motion when warping speed.")]
    public float maxSpeedWarpScale = 1.5f;

    [Tooltip("After warping, add a small NavMesh-constrained boost along desiredVelocity.")]
    public bool additiveRunBoost = true;
    [Tooltip("Cap for the extra m/s we add via agent.Move along the plan.")]
    public float maxAdditiveBoostPerSec = 2.0f;

    [Header("Turning")]
    [Tooltip("Degrees per second.")]
    public float turnSpeed = 240f;

    [Header("Runaway Settings")]
    public float playerSafeDistance = 5f;    // start running if closer than this
    public float sheepRunningDistance = 10f; // keep running until this far
    public float runCooldownDuration = 5f;   // idle after fleeing before resuming patrol

    [Header("Repath Control")]
    [Tooltip("Throttle SetDestination() calls to avoid jitter from frequent re-pathing.")]
    public float repathInterval = 0.25f;

    [Header("Debug")]
    public bool drawGizmos = true;

    // --- Internal state ---
    private bool isRunningAway = false;
    private bool isCooldownAfterRun = false;
    private float runCooldownTimer = 0f;
    private float _lastSetDestTime = -999f;

    private IAvatar localAvatar => SpatialBridge.actorService.localActor?.avatar;
    public Transform playerTransform; // optional fallback

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();

        // Root-motion drives animation; NavMeshAgent constrains position on the NavMesh
        agent.updatePosition = false; // we'll push using agent.Move()
        agent.updateRotation = false; // we rotate manually
        agent.stoppingDistance = Mathf.Max(stoppingDistance, 0.1f);
        agent.autoBraking = false;

        animator.applyRootMotion = true;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; // ensure OnAnimatorMove runs off-camera

        if (grasses != null && grasses.Count > 0 && grasses[0])
        {
            SafeSetDestination(grasses[currentWaypointIndex].position);
            TryPopup(currentWaypointIndex, up: true);
        }
    }

    void Update()
    {
        bool busy = RunAway(GetPlayerPosition());
        if (!busy)
            Patrol();
    }

    // ---------------- Patrol / Grazing ----------------
    private void Patrol()
    {
        if (grasses == null || grasses.Count == 0 || grasses[currentWaypointIndex] == null)
        {
            animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
            animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            agent.speed = 0f;
            animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
            animator.SetBool("Eating", true); // <-- your eating flag (kept)

            if (waitTimer >= waitTime)
            {
                TryPopup(currentWaypointIndex, up: false);
                isWaiting = false;
                waitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % grasses.Count;

                if (grasses[currentWaypointIndex] != null)
                {
                    SafeSetDestination(grasses[currentWaypointIndex].position);
                    TryPopup(currentWaypointIndex, up: true);
                }
            }
            return;
        }

        Vector3 target = grasses[currentWaypointIndex].position;
        float dist = transform.position.DistanceXZ(target);

        if (dist > stoppingDistance)
        {
            agent.speed = agentWalkSpeed;
            animator.SetFloat("Speed", walkSpeed, 0.2f, Time.deltaTime);
            animator.SetBool("Eating", false); // <-- stop eating when walking
            animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);
        }
        else
        {
            isWaiting = true;
            waitTimer = 0f;
            agent.ResetPath();
            animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
            animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);
        }
    }

    // ---------------- Run Away (your logic, NavMesh-constrained) ----------------
    /// <summary> Returns true when running or cooling down. </summary>
    private bool RunAway(Vector3 player)
    {
        animator.SetBool("Eating", false); // <-- ensure not eating during flee
        float distanceToPlayer = Vector3.Distance(transform.position, player);

        // 1) Cooldown phase — waiting after fleeing
        if (isCooldownAfterRun)
        {
            if (distanceToPlayer < playerSafeDistance)
            {
                // Player came back — cancel cooldown
                isCooldownAfterRun = false;
                runCooldownTimer = 0f;
                // fall through to flee
            }
            else
            {
                runCooldownTimer += Time.deltaTime;

                // Stay idle during cooldown
                agent.ResetPath();
                agent.speed = 0f;
                animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
                animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);

                if (runCooldownTimer >= runCooldownDuration)
                {
                    isRunningAway = false;
                    isCooldownAfterRun = false;
                    runCooldownTimer = 0f;

                    // Resume grazing
                    if (grasses != null && grasses.Count > 0 && grasses[currentWaypointIndex] != null)
                    {
                        SafeSetDestination(grasses[currentWaypointIndex].position);
                        animator.SetBool("Eating", false);
                        isWaiting = false;
                        waitTimer = 0f;
                    }
                }
                return true; // still busy
            }
        }

        // 2) Player too close or still fleeing — keep running
        if (distanceToPlayer < playerSafeDistance || (isRunningAway && distanceToPlayer < sheepRunningDistance))
        {
            isRunningAway = true;

            Vector3 runDirection = (transform.position - player);
            runDirection.y = 0f;
            if (runDirection.sqrMagnitude < 0.0001f) runDirection = transform.forward;
            runDirection.Normalize();

            Vector3 targetPos = transform.position + runDirection * sheepRunningDistance;

            NavMeshHit hit;
            // Prefer a straight escape if not blocked; otherwise try lateral sidesteps
            if (!NavMesh.Raycast(transform.position, targetPos, out hit, NavMesh.AllAreas) &&
                NavMesh.SamplePosition(targetPos, out hit, 3f, NavMesh.AllAreas))
            {
                SafeSetDestination(hit.position);
            }
            else
            {
                Vector3 sideStep = Vector3.Cross(runDirection, Vector3.up) * 3f;
                if (NavMesh.SamplePosition(transform.position + sideStep, out hit, 2f, NavMesh.AllAreas))
                    SafeSetDestination(hit.position);
                else if (NavMesh.SamplePosition(transform.position - sideStep, out hit, 2f, NavMesh.AllAreas))
                    SafeSetDestination(hit.position);
            }

            // Planner target speed (set higher than player speed)
            agent.speed = agentRunningSpeed;

            // Animation parameters (blend + playback up)
            animator.SetFloat("Speed", runSpeed, 0.15f, Time.deltaTime);
            animator.speed = Mathf.Lerp(animator.speed, runAnimSpeedMultiplier, 6f * Time.deltaTime);

            return true; // busy
        }

        // 3) Player is far enough — start cooldown
        if (isRunningAway && distanceToPlayer >= sheepRunningDistance)
        {
            isCooldownAfterRun = true;
            runCooldownTimer = 0f;

            agent.ResetPath();
            agent.speed = 0f;
            animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
            animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);
            return true; // busy
        }

        isRunningAway = false;
        animator.speed = Mathf.Lerp(animator.speed, 1f, 6f * Time.deltaTime);
        return false;
    }

    // ---------------- Root Motion applied via NavMeshAgent.Move (+ speed warp/boost) ----------------
    void OnAnimatorMove()
    {
        if (!animator || !agent) return;

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);

        // 1) Base root-motion step (XZ only)
        Vector3 delta = animator.deltaPosition;
        delta.y = 0f;

        // Compute current RM speed
        float rmSpeed = delta.magnitude / dt;

        // Planned speed from NavMesh (can be 0 briefly when no path)
        float plannedSpeed = agent.hasPath ? agent.desiredVelocity.magnitude : rmSpeed;

        // 2) Speed-warp RM toward planned speed when fleeing
        if (isRunningAway && speedWarpDuringRun && plannedSpeed > 0.01f && rmSpeed > 0.0001f)
        {
            float scale = Mathf.Clamp(plannedSpeed / rmSpeed, 1f, maxSpeedWarpScale); // only scale up
            delta *= scale;
            rmSpeed *= scale;
        }

        // 3) Move the AGENT on the NavMesh by the warped RM delta (respects obstacles & avoidance)
        agent.Move(delta);

        // 4) Optional extra boost along the plan to hit the planned speed exactly
        if (isRunningAway && additiveRunBoost && plannedSpeed > rmSpeed + 0.01f)
        {
            Vector3 planDir = agent.desiredVelocity;
            planDir.y = 0f;
            if (planDir.sqrMagnitude > 0.0001f)
            {
                planDir.Normalize();
                float deficit = plannedSpeed - rmSpeed;                   // m/s shortfall
                float boost = Mathf.Min(deficit, maxAdditiveBoostPerSec); // cap for natural look
                agent.Move(planDir * boost * dt);
            }
        }

        // 5) Sync transform to the agent (keeps us glued to the NavMesh)
        transform.position = agent.nextPosition;

        // 6) Rotate AFTER moving, to follow the planned path
        Vector3 faceDir = agent.desiredVelocity;
        if (faceDir.sqrMagnitude < 0.001f && agent.hasPath)
            faceDir = agent.steeringTarget - transform.position;
        if (faceDir.sqrMagnitude < 0.001f)
            faceDir = delta; // fallback to RM delta

        faceDir.y = 0f;
        if (faceDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(faceDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    // ---------------- Helpers ----------------
    private Vector3 GetPlayerPosition()
    {
        if (localAvatar != null) return localAvatar.position;
        if (playerTransform != null) return playerTransform.position;
        return transform.position + transform.forward * 9999f; // far away => no flee
    }

    private void SafeSetDestination(Vector3 worldPos)
    {
        // Throttle destination updates to avoid micro-repath jitter
        if (Time.time - _lastSetDestTime < repathInterval) return;

        if (NavMesh.SamplePosition(worldPos, out var hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            _lastSetDestTime = Time.time;
        }
    }

    private void TryPopup(int index, bool up)
    {
        if (grasses == null || index < 0 || index >= grasses.Count) return;
        var t = grasses[index];
        if (!t) return;
        var pop = t.GetComponent<PopupScaler>();
        if (!pop) return;
        if (up) pop.PopUp(); else pop.PopDown();
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || agent == null) return;

        // Desired direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, agent.desiredVelocity.normalized * 1.5f);

        // Steering target
        if (agent.hasPath)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(agent.steeringTarget, 0.15f);
            Gizmos.DrawLine(transform.position, agent.steeringTarget);
        }
    }
}

public static class Vector3Extensions
{
    public static float DistanceXZ(this Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
}
