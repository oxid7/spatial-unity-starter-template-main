using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class GameObjectEvent : UnityEvent<GameObject> { }

public class ParticleSystemEvents : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    public UnityEvent OnPlayed;
    public UnityEvent OnStopped;          // requires Stop Action = Callback
    public UnityEvent OnAllParticlesDead; // fires when IsAlive() == false
    public GameObjectEvent OnParticleCollided;
    public UnityEvent OnTriggerEnterAny;


    bool wasPlaying;
    bool wasPaused;

    void Awake() { if (!ps) ps = GetComponent<ParticleSystem>(); }

    public void Play()
    {
        ps.Play(true);
        OnPlayed?.Invoke();
        StopAllCoroutines();
        StartCoroutine(WaitUntilDead());
    }

    // Called when Stop Action is set to "Callback" in Main > Stop Action
    void OnParticleSystemStopped() => OnStopped?.Invoke();

    // Collision module: enable and check "Send Collision Messages"
    void OnParticleCollision(GameObject other) => OnParticleCollided?.Invoke(other);

    // Trigger module: configure Enter/Exit/etc. colliders
    void OnParticleTrigger()
    {
        var entered = new List<ParticleSystem.Particle>();
        int count = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, entered);
        if (count > 0) OnTriggerEnterAny?.Invoke();
    }

    void LateUpdate()
    {
        if (!ps) return;

        bool nowPlaying = ps.isPlaying;
        bool nowPaused = ps.isPaused;

        // fire only on a fresh start (not on unpause)
        if (nowPlaying && !wasPlaying && !nowPaused)
        {
            OnPlayed?.Invoke();
            StopAllCoroutines();
            StartCoroutine(WaitUntilDead());
        }

        wasPlaying = nowPlaying;
        wasPaused = nowPaused;

    }


    System.Collections.IEnumerator WaitUntilDead()
    {
        // true = include children
        while (ps.IsAlive(true)) yield return null;
        OnAllParticlesDead?.Invoke();
    }
}
