// PerParticlePlayedEvents.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ParticlePlayEntry
{
    [Tooltip("For readability in the Inspector")]
    public string label;

    [Tooltip("Target ParticleSystem (root or sub-emitter)")]
    public ParticleSystem system;

    [Header("Events")]
    public UnityEvent OnPlayed;   // Fires once when this PS actually starts emitting
    public UnityEvent OnStopped;  // Requires Main → Stop Action = Callback on this PS

    // --- runtime state ---
    [HideInInspector] public bool prevEmitting = false;
    [HideInInspector] public int prevCount = 0;
    [HideInInspector] public bool hasPlayed = false;
}

public class PerParticlePlayedEvents : MonoBehaviour
{
    [SerializeField, Tooltip("Assign your root system (auto-assigned if left empty).")]
    private ParticleSystem root;

    [Tooltip("One row per system you want events for. Use the context menu to auto-populate.")]
    public List<ParticlePlayEntry> entries = new List<ParticlePlayEntry>();

    void Awake()
    {
        if (!root) root = GetComponent<ParticleSystem>();
        if (entries.Count == 0) RebuildFromRoot();
        AttachStopRelays(); // to support per-system OnStopped
        // IMPORTANT: do NOT sample current states here. We want Play On Awake to still trigger.
    }

    void LateUpdate()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            var ps = e.system;
            if (!ps) continue;

            // Snapshot current signals
            bool nowEmitting = ps.isEmitting;   // true only on frames emission is active
            int nowCount = ps.particleCount;

            // Detect the first real start:
            // - first particle appears (0 -> >0), OR
            // - emission just began AND there is at least one particle this frame
            bool started =
                (!e.prevEmitting && nowEmitting && nowCount > 0) ||
                (e.prevCount == 0 && nowCount > 0);

            if (!e.hasPlayed && started)
            {
                e.hasPlayed = true;   // fire once per play cycle
                e.OnPlayed?.Invoke();
            }

            // Update history for next frame
            e.prevEmitting = nowEmitting;
            e.prevCount = nowCount;

            // Reset for next Play() once the system is fully dead (no children traversal here)
            // Use IsAlive(true) to include children of this PS (if any in hierarchy).
            if (e.hasPlayed && !ps.IsAlive(true))
            {
                e.hasPlayed = false;
                e.prevEmitting = false;
                e.prevCount = 0;
            }
        }
    }

    // -------------------- Setup Helpers --------------------

    [ContextMenu("Rebuild From Root (include sub-emitters)")]
    public void RebuildFromRoot()
    {
        var found = FindAllSystems(root);
        entries.Clear();
        foreach (var ps in found)
        {
            entries.Add(new ParticlePlayEntry
            {
                label = ps ? ps.name : "(null)",
                system = ps
            });
        }
    }

    private HashSet<ParticleSystem> FindAllSystems(ParticleSystem start)
    {
        var set = new HashSet<ParticleSystem>();
        if (!start)
        {
            // Fallback: collect from hierarchy if no root specified
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
                set.Add(ps);
            return set;
        }

        // 1) Add all ParticleSystems in the hierarchy under this GameObject
        foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
            set.Add(ps);

        // 2) Walk sub-emitters graph to catch linked systems outside the hierarchy
        var q = new Queue<ParticleSystem>();
        q.Enqueue(start);

        while (q.Count > 0)
        {
            var ps = q.Dequeue();
            if (!ps || !set.Add(ps)) continue;

            var se = ps.subEmitters;
            for (int i = 0; i < se.subEmittersCount; i++)
            {
                var sub = se.GetSubEmitterSystem(i);
                if (sub && !set.Contains(sub))
                    q.Enqueue(sub);
            }
        }

        return set;
    }

    private void AttachStopRelays()
    {
        // Ensure each PS has a small relay component to forward OnParticleSystemStopped()
        foreach (var e in entries)
        {
            var ps = e.system;
            if (!ps) continue;

            var relay = ps.GetComponent<PSStopRelay>();
            if (!relay) relay = ps.gameObject.AddComponent<PSStopRelay>();
            relay.Bind(e);
        }
    }

    // Small helper that forwards the built-in stop message per-system.
    private class PSStopRelay : MonoBehaviour
    {
        private ParticlePlayEntry entry;

        public void Bind(ParticlePlayEntry e) => entry = e;

        // Unity calls this when Main → Stop Action = Callback for THIS ParticleSystem
        void OnParticleSystemStopped()
        {
            entry?.OnStopped?.Invoke();
        }
    }
}
