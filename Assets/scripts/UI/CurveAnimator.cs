using UnityEngine;
using DG.Tweening;

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

  [SerializeField] private float initialDuration = 4f;
  [SerializeField] private float loopDuration = 2f;

  private CurveFillerUI curve;
  private Sequence loopSequence;
  private Sequence initialSequence;
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
      loopSequence?.Kill();
      initialSequence?.Kill();
      started = false;
    }
  }

  void StartAnimation()
  {
    started = true;

    // Initial move (zero -> top)
    initialSequence = DOTween.Sequence()
      .Append(DOTween.To(() => curve.heightMultiplier, v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); }, topHM, initialDuration))
      .Join(DOTween.To(() => curve.widthMultiplier, v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); }, topWM, initialDuration));
    initialSequence.OnComplete(StartLoop);
  }

  void StartLoop()
  {
    loopSequence = DOTween.Sequence()
        .Append(DOTween.To(() => curve.heightMultiplier, v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); }, bottomHM, loopDuration))
        .Join(DOTween.To(() => curve.widthMultiplier, v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); }, bottomWM, loopDuration))
        .SetLoops(-1, LoopType.Yoyo);
  }
}
