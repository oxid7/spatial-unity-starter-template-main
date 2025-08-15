using UnityEngine;
using System.Collections;
using SpatialSys.UnitySDK;

public class PopupScaler : MonoBehaviour
{
    
    public float duration = 0.4f;
    public float scaleOvershoot = 1.1f;
    public float bounceHeight = 0.5f;
    public bool isForHand = false;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private Coroutine routine;

    void Awake()
    {
        initialScale = transform.localScale;
        initialPosition = transform.localPosition;
        if(!isForHand )
        {
            transform.localScale = Vector3.zero;
        }
       
        
    }

    private void Start()
    {
       // PopUp();
      // PopDown();
    }

    public void PopUp()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimatePopUp());
    }

    public void PopDown()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimatePopDown());
    }

    private IEnumerator AnimatePopUp()
    {
        float t = 0f;
        float halfDuration = duration / 2f;

        // Phase 1: overshoot
        while (t < halfDuration)
        {
            float progress = t / halfDuration;
            float scaleValue = Mathf.Lerp(0f, scaleOvershoot, EaseOutBack(progress));
            float yOffset = Mathf.Lerp(0f, bounceHeight, EaseOutQuad(progress));

            transform.localScale = initialScale * scaleValue;
            transform.localPosition = initialPosition + new Vector3(0, yOffset, 0);

            t += Time.deltaTime;
            yield return null;
        }

        // Phase 2: settle
        t = 0f;
        while (t < halfDuration)
        {
            float progress = t / halfDuration;
            float scaleValue = Mathf.Lerp(scaleOvershoot, 1f, EaseInQuad(progress));
            float yOffset = Mathf.Lerp(bounceHeight, 0f, EaseInQuad(progress));

            transform.localScale = initialScale * scaleValue;
            transform.localPosition = initialPosition + new Vector3(0, yOffset, 0);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = initialScale;
        transform.localPosition = initialPosition;
        routine = null;
    }

    private IEnumerator AnimatePopDown()
    {
        float t = 0f;
        Vector3 fromScale = transform.localScale;
        Vector3 toScale = Vector3.zero;

        while (t < duration)
        {
            float progress = t / duration;
            float eased = EaseInQuad(progress);
            transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, eased);
            transform.localPosition = initialPosition; // stay grounded

            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        transform.localPosition = initialPosition;
        routine = null;
    }

    // Easing functions
    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
    }

    private float EaseOutQuad(float t)
    {
        return 1 - (1 - t) * (1 - t);
    }

    private float EaseInQuad(float t)
    {
        return t * t;
    }
}
