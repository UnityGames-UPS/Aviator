using UnityEngine;

[RequireComponent(typeof(CurveFillerUI))]
public class CurveAnimator : MonoBehaviour
{
  [Header("Curve Points")]
  [SerializeField] private float zeroHM = 0.01f;
  [SerializeField] private float zeroWM = 0.03f;
  [SerializeField] private float topHM = 0.85f;
  [SerializeField] private float topWM = 0.76f;
  [SerializeField] private float bottomHM = 0.64f;
  [SerializeField] private float bottomWM = 0.85f;

  [Header("Flight Settings")]
  [Tooltip("Multiplier where takeoff ends and oscillation begins")]
  [SerializeField] private float takeoffEnd = 1.7f;

  [Tooltip("How many multiplier units equal one full oscillation")]
  [SerializeField] private float loopMultPerCycle = 5f; // smooth and slow

  [Tooltip("Maximum multiplier cap (for damping calculations)")]
  [SerializeField] private float crashLimit = 100f;

  [Tooltip("Initial oscillation amplitude (1 = full range)")]
  [SerializeField] private float loopAmplitude = 1f;

  [Tooltip("Amplitude decay rate per multiplier unit (0 = no decay)")]
  [SerializeField] private float amplitudeDecay = 0.015f;

  private CurveFillerUI curve;
  private bool crashed = false;
  private bool inLoop = false;

  private float lastMult = 1f;
  private float targetMult = 1f;
  private float lastPacketTime;
  private float nextPacketTime;
  private float tickInterval = 0.1f;

  private float loopPhase;
  private bool loopJustStarted = false;

  void Awake()
  {
    curve = GetComponent<CurveFillerUI>();
    ResetVisual();
  }

  internal void ResetVisual()
  {
    crashed = false;
    inLoop = false;
    loopJustStarted = false;
    loopPhase = 0f;
    lastMult = targetMult = 1f;
    curve.followCurve = true;
    curve.heightMultiplier = zeroHM;
    curve.widthMultiplier = zeroWM;
    curve.SetVerticesDirty();
  }

  internal void OnMultiplierUpdate(float newMult, float serverTickInterval)
  {
    if (crashed) return;

    tickInterval = serverTickInterval;
    lastMult = targetMult;
    targetMult = Mathf.Clamp(newMult, 1f, crashLimit);

    lastPacketTime = Time.time;
    nextPacketTime = Time.time + tickInterval;
  }

  void Update()
  {
    if (crashed) return;

    float t = 0f;
    if (nextPacketTime > lastPacketTime)
      t = Mathf.InverseLerp(lastPacketTime, nextPacketTime, Time.time);
    t = Mathf.Clamp01(t);

    float predicted = Mathf.Lerp(lastMult, targetMult, t);
    ApplyMultiplier(predicted);
  }

  private void ApplyMultiplier(float mult)
  {
    if (mult <= takeoffEnd)
    {
      // --- TAKEOFF ---
      float t = Mathf.InverseLerp(1f, takeoffEnd, mult);
      float ease = Mathf.SmoothStep(0f, 1f, t);

      curve.heightMultiplier = Mathf.Lerp(zeroHM, topHM, ease);
      curve.widthMultiplier = Mathf.Lerp(zeroWM, topWM, ease);

      inLoop = false;
      loopJustStarted = true;
    }
    else
    {
      // --- LOOP ---
      if (!inLoop)
      {
        inLoop = true;

        if (loopJustStarted)
        {
          // start perfectly at top
          loopPhase = Mathf.PI / 2f; // sin(π/2) = +1 → top
          loopJustStarted = false;
        }
      }

      // Advance phase continuously
      float loopSpeed = (1f / loopMultPerCycle) * Mathf.PI * 2f;
      loopPhase += Time.deltaTime * loopSpeed;
      if (loopPhase > Mathf.PI * 2f) loopPhase -= Mathf.PI * 2f;

      // Amplitude shrinks over multiplier
      float amplitudeFactor = Mathf.Clamp01(1f - ((mult - takeoffEnd) * amplitudeDecay));
      float currentAmplitude = loopAmplitude * amplitudeFactor;

      // Map sine: +1 = top, -1 = bottom ✅
      float sine = Mathf.Sin(loopPhase);
      float osc = (-(sine * 0.5f) + 0.5f) * currentAmplitude;

      curve.heightMultiplier = Mathf.Lerp(topHM, bottomHM, osc);
      curve.widthMultiplier = Mathf.Lerp(topWM, bottomWM, osc);
    }

    curve.SetVerticesDirty();
  }

  internal void OnCrash(float crashMult)
  {
    crashed = true;

    curve.followCurve = false;
    curve.heightMultiplier = 0;
    curve.widthMultiplier = 0;
    curve.SetVerticesDirty();
  }
}
