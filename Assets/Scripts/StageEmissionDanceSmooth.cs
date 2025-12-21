using UnityEngine;

[DisallowMultipleComponent]
public class StageEmissionDanceSmooth : MonoBehaviour
{
    [Header("Targets (MANUAL)")]
    [SerializeField] private Renderer[] targets;

    [Header("Which material slot to drive (0 = first, 1 = second, ...)")]
    [SerializeField] private int materialIndex = 1;

    [Header("Emission Property")]
    [SerializeField] private string emissionColorProperty = "_EmissionColor";
    [SerializeField] private bool ensureEmissionKeyword = true;

    [Header("Base Look (single color)")]
    [SerializeField] private Color baseColor = Color.white;

    [Tooltip("If true, you DON'T choose baseColor. It will auto-pick and keep changing smoothly.")]
    [SerializeField] private bool autoBaseColors = true;

    [Tooltip("How long it takes to blend from one base color to the next (seconds). Bigger = slower.")]
    [SerializeField] private Vector2 baseColorChangeSeconds = new Vector2(4f, 10f);

    [Tooltip("How fast the base intensity breathes (cycles/sec). 0.08 = ~12.5s cycle.")]
    [SerializeField] private float pulseSpeed = 0.08f;

    [Header("Intensity (EV)")]
    [SerializeField] private float minEV = -1.5f;
    [SerializeField] private float maxEV = 1.5f;

    [Header("Smooth Bursts (TEMPO BOOST)")]
    [Tooltip("Average bursts per second PER TARGET. Example: 0.01 ≈ one burst every 100s per target.")]
    [SerializeField] private float burstRate = 0.02f;

    [SerializeField] private Vector2 burstDurationSeconds = new Vector2(1.0f, 2.8f);

    [Tooltip("How much faster everything runs during burst (smoothly). 3 = 3X faster.")]
    [SerializeField] private float burstSpeedMultiplier = 3f;

    [Header("Burst Colors (Bold)")]
    [Range(0f, 1f)][SerializeField] private float minSaturation = 0.90f;
    [Range(0f, 1f)][SerializeField] private float maxSaturation = 1.00f;
    [Range(0f, 1f)][SerializeField] private float minValue = 0.90f;
    [Range(0f, 1f)][SerializeField] private float maxValue = 1.00f;

    [Header("Desync")]
    [SerializeField] private float perTargetPulseOffset = 2.0f;

    private int _emissionId;
    private MaterialPropertyBlock _mpb;

    // Burst state per target
    private bool[] _burstActive;
    private float[] _burstStartTime;
    private float[] _burstDuration;

    // Base color transition per target (smooth, random bold colors)
    private Color[] _baseFrom;
    private Color[] _baseTo;
    private float[] _baseU;          // 0..1 progress through current base color transition
    private float[] _baseDuration;

    // Pulse phase accumulator (smoothly speedable)
    private float[] _pulseAngle;     // radians accumulator (0..inf)

    void Awake()
    {
        _emissionId = Shader.PropertyToID(emissionColorProperty);
        _mpb = new MaterialPropertyBlock();

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"{nameof(StageEmissionDanceSmooth)} on {name}: No targets assigned.", this);
            enabled = false;
            return;
        }

        int n = targets.Length;

        _burstActive = new bool[n];
        _burstStartTime = new float[n];
        _burstDuration = new float[n];

        _baseFrom = new Color[n];
        _baseTo = new Color[n];
        _baseU = new float[n];
        _baseDuration = new float[n];

        _pulseAngle = new float[n];

        for (int i = 0; i < n; i++)
        {
            // start pulse at random phase (desync)
            _pulseAngle[i] = Random.Range(0f, Mathf.PI * 2f) + Random.Range(0f, perTargetPulseOffset);

            if (autoBaseColors)
            {
                _baseFrom[i] = RandomBoldColor();
                _baseTo[i] = RandomBoldColorDifferent(_baseFrom[i]);
                _baseU[i] = Random.value; // desync
                _baseDuration[i] = Random.Range(baseColorChangeSeconds.x, baseColorChangeSeconds.y);
            }
            else
            {
                _baseFrom[i] = baseColor;
                _baseTo[i] = baseColor;
                _baseU[i] = 1f;
                _baseDuration[i] = 1f;
            }
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
    }

    void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;

        for (int i = 0; i < targets.Length; i++)
        {
            var r = targets[i];
            if (!r) continue;

            // Start a burst sometimes (smooth + random)
            if (!_burstActive[i] && burstRate > 0f && Random.value < burstRate * dt)
            {
                _burstActive[i] = true;
                _burstStartTime[i] = t;
                _burstDuration[i] = Random.Range(burstDurationSeconds.x, burstDurationSeconds.y);
            }

            // Burst envelope (0..1..0) to smoothly ramp tempo up/down
            float burstK = 0f;
            if (_burstActive[i])
            {
                float u = (t - _burstStartTime[i]) / Mathf.Max(0.0001f, _burstDuration[i]);

                if (u >= 1f)
                {
                    _burstActive[i] = false;
                }
                else
                {
                    burstK = Mathf.Sin(u * Mathf.PI); // 0->1->0
                    burstK = Smooth01(burstK);
                }
            }

            // Speed multiplier: 1 -> burstSpeedMultiplier -> 1 (smooth)
            float speedMul = Mathf.Lerp(1f, burstSpeedMultiplier, burstK);

            // ----- Base color update (auto changes always; during burst it changes faster) -----
            Color baseColorNow = baseColor;

            if (autoBaseColors)
            {
                float dur = Mathf.Max(0.0001f, _baseDuration[i]);
                _baseU[i] += (dt / dur) * speedMul;

                while (_baseU[i] >= 1f)
                {
                    _baseU[i] -= 1f;
                    _baseFrom[i] = _baseTo[i];
                    _baseTo[i] = RandomBoldColorDifferent(_baseFrom[i]);
                    _baseDuration[i] = Random.Range(baseColorChangeSeconds.x, baseColorChangeSeconds.y);
                    dur = Mathf.Max(0.0001f, _baseDuration[i]);
                }

                float kBase = Smooth01(_baseU[i]);
                baseColorNow = Color.Lerp(_baseFrom[i], _baseTo[i], kBase);
            }

            // ----- Pulse update (during burst pulses ~3x faster) -----
            float omega = (pulseSpeed * speedMul) * Mathf.PI * 2f; // radians per second
            _pulseAngle[i] += omega * dt;

            float wave = 0.5f + 0.5f * Mathf.Sin(_pulseAngle[i]); // 0..1
            float ev = Mathf.Lerp(minEV, maxEV, wave);

            // Emission
            float intensity = Mathf.Pow(2f, ev);
            Color emissive = baseColorNow * intensity;

            r.GetPropertyBlock(_mpb, materialIndex);
            _mpb.SetColor(_emissionId, emissive);
            r.SetPropertyBlock(_mpb, materialIndex);
        }
    }

    // Your exact selection logic
    Color RandomBoldColor()
    {
        float h = Random.value;
        float s = Random.Range(minSaturation, maxSaturation);
        float v = Random.Range(minValue, maxValue);
        return Color.HSVToRGB(h, s, v, true);
    }

    // Avoid “almost same” colors
    Color RandomBoldColorDifferent(Color avoid)
    {
        Color c = RandomBoldColor();
        for (int tries = 0; tries < 6; tries++)
        {
            if (ColorDistanceSq(c, avoid) > 0.12f) break;
            c = RandomBoldColor();
        }
        return c;
    }

    static float ColorDistanceSq(Color a, Color b)
    {
        float dr = a.r - b.r;
        float dg = a.g - b.g;
        float db = a.b - b.b;
        return dr * dr + dg * dg + db * db;
    }

    static float Smooth01(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * (3f - 2f * x); // smoothstep
    }
}
