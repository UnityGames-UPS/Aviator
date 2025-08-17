using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CurveFillerUI))]
public class CurveAnimator : MonoBehaviour
{
  [Header("Curve Points")]
  public float zeroHM = 0.01f;
  public float zeroWM = 0.03f;
  public float topHM = 0.85f;
  public float topWM = 0.76f;
  public float bottomHM = 0.64f;
  public float bottomWM = 0.85f;

  public float initialDuration = 1.3f;
  public float loopDuration = 1.0f;

  private CurveFillerUI curve;
  private Sequence loopSequence;
  private bool started = false;

  void Awake()
  {
    curve = GetComponent<CurveFillerUI>();

    // Initialize at zero
    curve.heightMultiplier = zeroHM;
    curve.widthMultiplier = zeroWM;
    curve.SetVerticesDirty();
  }

  void Update()
  {
    if (!started && Input.GetKeyDown(KeyCode.Space))
    {
      StartAnimation();
    }

    // Plane Crash
    if (started && Input.GetKeyDown(KeyCode.K))
    {
      loopSequence?.Kill();        // keeps the value where it is
    }
  }

  void StartAnimation()
  {
    started = true;

    // First go to TOP (zero -> top)
    DOTween.To(() => curve.heightMultiplier, v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); }, topHM, initialDuration);
    DOTween.To(() => curve.widthMultiplier, v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); }, topWM, initialDuration)
           .OnComplete(StartLoop);  // when it reaches the top, start the ping/pong loop
  }

  void StartLoop()
  {
    loopSequence = DOTween.Sequence()
        .Append(DOTween.To(() => curve.heightMultiplier, v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); }, bottomHM, loopDuration))
        .Join(DOTween.To(() => curve.widthMultiplier, v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); }, bottomWM, loopDuration))
        .SetLoops(-1, LoopType.Yoyo);
  }
}
