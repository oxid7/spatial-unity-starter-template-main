using UnityEngine;
using SpatialSys.UnitySDK;
using TMPro;

/// <summary>
/// Drives a looping yacht animation deterministically using Spatial's global network time.
/// - Uses SampleAnimation() on the detected clip.
/// - Requires the yacht model to be a child of an anchor GameObject for world placement.
/// </summary>
[RequireComponent(typeof(Animator))]
public class YachtAnimationControllerAuto : MonoBehaviour
{
    [Tooltip("Name of the animation clip (must loop).")]
    public string animationClipName = "YachtLoop";

    [Tooltip("Optional text field for debugging.")]
    public TextMeshProUGUI timeT;
    public bool animateOnPhysics = false;

    public float playbackSpeed = 1f;

    private Animator animator;
    private AnimationClip targetClip;
    private float loopDuration;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // Find the clip directly by name
        RuntimeAnimatorController ctrl = animator.runtimeAnimatorController;
        if (ctrl != null && ctrl.animationClips != null)
        {
            foreach (AnimationClip clip in ctrl.animationClips)
            {
                if (clip.name == animationClipName)
                {
                    targetClip = clip;
                    loopDuration = clip.length;
                    break;
                }
            }
        }

        if (targetClip == null)
        {
            Debug.LogError($"[YachtAnimationControllerAuto] Could not find clip '{animationClipName}' in Animator.");
        }

        // Disable Animator playback — we control pose manually
        animator.enabled = false;
    }

    void Update()
    {


        if (targetClip == null || loopDuration <= 0f) return;

        if (animateOnPhysics == true) return;

        // Get the global synchronized time from Spatial
        double netTime = SpatialBridge.networkingService.networkTime;

        // Compute time within the loop
        float localTime = (float)(netTime * playbackSpeed % loopDuration);

        // Apply the animation pose for this exact time
        targetClip.SampleAnimation(gameObject, localTime);

        // Debug info
        /*
        if (timeT != null)
        {
            timeT.text = $"Clip Time: {localTime:F2}/{loopDuration:F2}\nNetTime: {netTime:F2}";
        }*/
    }

    private void FixedUpdate()
    {

       
    }

    private void LateUpdate()
    {
        if (targetClip == null || loopDuration <= 0f) return;

        if (animateOnPhysics == false) return;

        // Get the global synchronized time from Spatial
        double netTime = SpatialBridge.networkingService.networkTime;

        // Compute time within the loop
        float localTime = (float)(netTime % loopDuration);

        // Apply the animation pose for this exact time
        targetClip.SampleAnimation(gameObject, localTime);

        // Debug info
        /*
        if (timeT != null)
        {
            timeT.text = $"Clip Time: {localTime:F2}/{loopDuration:F2}\nNetTime: {netTime:F2}";
        }*/
    }
}
