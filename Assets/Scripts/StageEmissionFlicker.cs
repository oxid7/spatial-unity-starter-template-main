using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class StageEmissionFlicker : MonoBehaviour
{
    [Header("Targets (MANUAL)")]
    [SerializeField] private Renderer[] targets;

    [Header("Which material slot to drive (0 = first, 1 = second, ...)")]
    [SerializeField] private int materialIndex = 1;

    [Header("Emission Property")]
    [SerializeField] private string emissionColorProperty = "_EmissionColor";
    [SerializeField] private bool ensureEmissionKeyword = true;

    [Header("Timing")]
    [SerializeField] private Vector2 offHoldSeconds = new Vector2(0.10f, 0.40f);
    [SerializeField] private Vector2 onHoldSeconds = new Vector2(0.10f, 0.35f);
    [Range(0f, 1f)][SerializeField] private float burstChance = 0.35f;

    [Header("Fade (Smoothness)")]
    [SerializeField] private Vector2 fadeUpSeconds = new Vector2(0.15f, 0.45f);
    [SerializeField] private Vector2 fadeDownSeconds = new Vector2(0.15f, 0.45f);

    [Header("Burst")]
    [SerializeField] private Vector2Int burstFlashes = new Vector2Int(3, 10);
    [SerializeField] private Vector2 burstFlashHoldSeconds = new Vector2(0.02f, 0.06f);
    [SerializeField] private Vector2 burstFadeSeconds = new Vector2(0.03f, 0.08f);

    [Header("Color (Bold)")]
    [Range(0f, 1f)][SerializeField] private float minSaturation = 0.85f;
    [Range(0f, 1f)][SerializeField] private float maxSaturation = 1.00f;
    [Range(0f, 1f)][SerializeField] private float minValue = 0.85f;
    [Range(0f, 1f)][SerializeField] private float maxValue = 1.00f;

    [Header("Intensity (EV style)")]
    [SerializeField] private float offEV = -10f;
    [SerializeField] private Vector2 onEVRange = new Vector2(1.5f, 2.5f);

    private int _emissionId;
    private MaterialPropertyBlock _mpb;
    private Coroutine _loop;

    // current state (so fades are continuous, not snapping)
    private Color _currentColor = Color.black;
    private float _currentEV = -10f;

    void Awake()
    {
        _emissionId = Shader.PropertyToID(emissionColorProperty);
        _mpb = new MaterialPropertyBlock();

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"{nameof(StageEmissionFlicker)} on {name}: No targets assigned.", this);
            enabled = false;
            return;
        }

        if (ensureEmissionKeyword)
        {
            foreach (var r in targets)
            {
                if (!r) continue;
                var mats = r.sharedMaterials;
                if (mats == null || materialIndex < 0 || materialIndex >= mats.Length) continue;

                var mat = mats[materialIndex];
                if (mat) mat.EnableKeyword("_EMISSION");
            }
        }

        // start from "off"
        ApplyEmission(offEV, Color.black);
    }

    void OnEnable() => StartFlicker();
    void OnDisable() => StopFlicker();

    public void StartFlicker()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = StartCoroutine(FlickerLoop());
    }

    public void StopFlicker(bool turnOff = true)
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;

        if (turnOff)
            ApplyEmission(offEV, Color.black);
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            bool doBurst = Random.value < burstChance;

            if (doBurst)
            {
                int flashes = Random.Range(burstFlashes.x, burstFlashes.y + 1);

                for (int i = 0; i < flashes; i++)
                {
                    // Fade up fast, hold, fade down fast (still smooth)
                    float onEV = Random.Range(onEVRange.x, onEVRange.y);
                    Color onColor = RandomBoldColor();
                    float f = Random.Range(burstFadeSeconds.x, burstFadeSeconds.y);

                    yield return FadeTo(onEV, onColor, f);
                    yield return new WaitForSeconds(Random.Range(burstFlashHoldSeconds.x, burstFlashHoldSeconds.y));
                    yield return FadeTo(offEV, Color.black, f);
                    yield return new WaitForSeconds(Random.Range(burstFlashHoldSeconds.x, burstFlashHoldSeconds.y));
                }
            }
            else
            {
                // OFF (fade down), hold
                yield return FadeTo(offEV, Color.black, Random.Range(fadeDownSeconds.x, fadeDownSeconds.y));
                yield return new WaitForSeconds(Random.Range(offHoldSeconds.x, offHoldSeconds.y));

                // ON (fade up), hold
                float onEV = Random.Range(onEVRange.x, onEVRange.y);
                Color onColor = RandomBoldColor();

                yield return FadeTo(onEV, onColor, Random.Range(fadeUpSeconds.x, fadeUpSeconds.y));
                yield return new WaitForSeconds(Random.Range(onHoldSeconds.x, onHoldSeconds.y)); // FIXED
            }
        }
    }

    IEnumerator FadeTo(float targetEV, Color targetColor, float duration)
    {
        // If duration is tiny, just snap
        if (duration <= 0.0001f)
        {
            ApplyEmission(targetEV, targetColor);
            yield break;
        }

        float startEV = _currentEV;
        Color startColor = _currentColor;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            float ev = Mathf.Lerp(startEV, targetEV, k);
            Color col = Color.Lerp(startColor, targetColor, k);

            ApplyEmission(ev, col);
            yield return null;
        }

        ApplyEmission(targetEV, targetColor);
    }

    void ApplyEmission(float ev, Color baseColor)
    {
        _currentEV = ev;
        _currentColor = baseColor;

        float intensity = Mathf.Pow(2f, ev);
        Color emissive = baseColor * intensity;

        for (int i = 0; i < targets.Length; i++)
        {
            var r = targets[i];
            if (!r) continue;

            r.GetPropertyBlock(_mpb, materialIndex);
            _mpb.SetColor(_emissionId, emissive);
            r.SetPropertyBlock(_mpb, materialIndex);
        }
    }

    Color RandomBoldColor()
    {
        float h = Random.value;
        float s = Random.Range(minSaturation, maxSaturation);
        float v = Random.Range(minValue, maxValue);
        return Color.HSVToRGB(h, s, v, true);
    }
}
