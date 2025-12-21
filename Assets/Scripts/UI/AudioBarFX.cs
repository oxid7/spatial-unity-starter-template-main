using UnityEngine;

public class AudioBarFX : MonoBehaviour
{
    [SerializeField] private RectTransform bar;

    [Header("Height Range")]
    [SerializeField] private float minHeight = 10f;
    [SerializeField] private float maxHeight = 150f;

    [Header("Timing")]
    [SerializeField] private float targetChangeInterval = 0.08f; // like your updateRate

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 12f; // higher = snappier

    [SerializeField] private bool isAlive = true;

    private float _targetHeight;
    private float _nextTargetTime;

    private void OnEnable()
    {
        PickNewTarget();
        _nextTargetTime = Time.time + targetChangeInterval;
    }

    private void Update()
    {
        if (!isAlive || bar == null) return;

        // Change target every interval
        if (Time.time >= _nextTargetTime)
        {
            PickNewTarget();
            _nextTargetTime = Time.time + targetChangeInterval;
        }

        // Smoothly move current height toward target height
        float current = bar.sizeDelta.y;
        float next = Mathf.Lerp(current, _targetHeight, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        bar.sizeDelta = new Vector2(bar.sizeDelta.x, next);
    }

    private void PickNewTarget()
    {
        _targetHeight = Random.Range(minHeight, maxHeight);
    }
}
