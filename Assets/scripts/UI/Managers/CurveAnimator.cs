using UnityEngine;
using DG.Tweening;

public class CurveAnimator : MonoBehaviour
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private CurveFillerUI curve;

  [Header("Crash Settings")]
  [SerializeField] private float crashXoffset = 2000;
  [SerializeField] private float crashYoffset;
  [SerializeField] private float slowCrashDuration = 4f;
  [SerializeField] private float fastCrashDuration = 2f;
  [SerializeField] private float fastCrashTakeoffOffset = 0.3f;

  [Header("Curve Points")]
  [SerializeField] private float zeroHM = 0.01f;
  [SerializeField] private float zeroWM = 0.03f;

  [SerializeField] private float topHM = 0.85f;
  [SerializeField] private float topWM = 0.76f;

  [SerializeField] private float bottomHM = 0.64f;
  [SerializeField] private float bottomWM = 0.85f;

  [Header("Flight Settings")]
  [Tooltip("How many multiplier units equal one full oscillation")]
  [SerializeField] private float loopMultPerCycle = 4f;

  [Tooltip("Maximum multiplier cap (for damping calculations)")]
  [SerializeField] private float crashLimit = 100f;

  [Tooltip("Initial oscillation amplitude (1 = full range)")]
  [SerializeField] private float loopAmplitude = 1f;

  [Tooltip("Amplitude decay rate per multiplier unit (0 = no decay)")]
  [SerializeField] private float amplitudeDecay = 0.009f;

  private bool inLoop = false;
  private float lastMult = 1f;
  private float targetMult = 1f;
  private float lastPacketTime;
  private float nextPacketTime;
  private float tickInterval = 0.1f;
  private float predictedFlightMult;
  private float loopPhase;
  private bool loopJustStarted = false;

  void Awake()
  {
    ResetVisual();
  }

  internal void ResetVisual()
  {
    inLoop = false;
    loopJustStarted = false;
    loopPhase = 0f;
    lastMult = targetMult = 1f;
    curve.enabled = true;
    curve.followCurve = true;
    curve.heightMultiplier = zeroHM;
    curve.widthMultiplier = zeroWM;
    curve.SetVerticesDirty();
  }

  internal void OnMultiplierUpdate(float newMult, float serverTickInterval)
  {
    tickInterval = serverTickInterval;
    lastMult = targetMult;
    targetMult = Mathf.Clamp(newMult, 1f, crashLimit);

    lastPacketTime = Time.time;
    nextPacketTime = Time.time + tickInterval;
  }

  void Update()
  {
    if (socket.CurrentState != SocketIOManager.AviatorState.TickerStart) return;

    float t = 0f;
    if (nextPacketTime > lastPacketTime)
      t = Mathf.InverseLerp(lastPacketTime, nextPacketTime, Time.time);
    t = Mathf.Clamp01(t);

    predictedFlightMult = Mathf.Lerp(lastMult, targetMult, t);
    ApplyMultiplier(predictedFlightMult);
  }

  private void ApplyMultiplier(float mult)
  {
    if (mult <= socket.takeoffEnd)
    {
      // --- TAKEOFF ---
      float t = Mathf.InverseLerp(1f, socket.takeoffEnd, mult);
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
      float amplitudeFactor = Mathf.Clamp01(1f - ((mult - socket.takeoffEnd) * amplitudeDecay));
      float currentAmplitude = loopAmplitude * amplitudeFactor;

      // Map sine: +1 = top, -1 = bottom ✅
      float sine = Mathf.Sin(loopPhase);
      float osc = (-(sine * 0.5f) + 0.5f) * currentAmplitude;

      curve.heightMultiplier = Mathf.Lerp(topHM, bottomHM, osc);
      curve.widthMultiplier = Mathf.Lerp(topWM, bottomWM, osc);
    }

    curve.SetVerticesDirty();
  }

  internal void OnCrash()
  {
    curve.followCurve = false;
    curve.heightMultiplier = 0;
    curve.widthMultiplier = 0;
    curve.SetVerticesDirty();
    curve.enabled = false;

    RectTransform Plane = curve.PlaneParent;

    if (Plane == null)
    {
      Debug.LogError("Plane ref not found");
      return;
    }

    crashYoffset = Random.Range(150f, 250f);

    float crashX = Plane.anchoredPosition.x + crashXoffset;
    float crashY = Plane.anchoredPosition.y + crashYoffset;

    float CrashDuration = predictedFlightMult > socket.takeoffEnd - fastCrashTakeoffOffset ? slowCrashDuration : fastCrashDuration;

    Plane.DOAnchorPos(new Vector2(crashX, crashY), CrashDuration);
  }
}
