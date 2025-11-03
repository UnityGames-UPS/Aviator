using UnityEngine;
using DG.Tweening;

public class CurveAnimator : MonoBehaviour
{

  [SerializeField] private SocketIOManager socket;
  [SerializeField] private CurveFillerUI curve;

  [SerializeField] private float loopDuration = 2f;

  [Header("Curve Points")]
  [SerializeField] private float zeroHM = 0.01f;
  [SerializeField] private float zeroWM = 0.03f;

  [SerializeField] private float topHM = 0.85f;
  [SerializeField] private float topWM = 0.76f;

  [SerializeField] private float bottomHM = 0.64f;
  [SerializeField] private float bottomWM = 0.85f;

  [SerializeField] private float crashXoffset = 2000;
  [SerializeField] private float crashYoffset;
  [SerializeField] private float slowCrashDuration = 4f;
  [SerializeField] private float fastCrashDuration = 2f;
  [SerializeField] private float fastCrashTakeoffOffset = 0.3f;


  private Sequence loopSequence;
  private Sequence initialSequence;
  private float predictedFlightMult;
  private Tween takeOffTween;
  private Tween loopTween;

  void Awake()
  {
    ResetVisual();
  }

  internal void ResetVisual()
  {
    takeOffTween?.Kill();
    loopTween?.Kill();
    curve.enabled = true;
    curve.followCurve = true;
    curve.heightMultiplier = zeroHM;
    curve.widthMultiplier = zeroWM;
    curve.SetVerticesDirty();
  }

  internal void StartTakeoff()
  {
    ResetVisual();
  }

  internal void StartFlyingAnimation()
  {
    ResetVisual();
    // Initial move (zero -> top)
    initialSequence = DOTween.Sequence()
      .Append(DOTween.To(() => curve.heightMultiplier,
        v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); },
        topHM, socket.takeOffDuration)
        .SetEase(Ease.OutSine))
      .Join(DOTween.To(() => curve.widthMultiplier,
        v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); },
        topWM, socket.takeOffDuration)
        .SetEase(Ease.OutSine))
      .OnComplete(() =>
      {
        // small blend delay before loop
        DOVirtual.DelayedCall(0.1f, StartLoop);
      });
  }

  void StartLoop()
  {
    loopSequence = DOTween.Sequence()
      .Append(DOTween.To(() => curve.heightMultiplier,
        v => { curve.heightMultiplier = v; curve.SetVerticesDirty(); },
        bottomHM, loopDuration)
        .SetEase(Ease.InOutSine))
      .Join(DOTween.To(() => curve.widthMultiplier,
        v => { curve.widthMultiplier = v; curve.SetVerticesDirty(); },
        bottomWM, loopDuration)
        .SetEase(Ease.InOutSine))
      .SetLoops(-1, LoopType.Yoyo);
  }

  internal void OnCrash()
  {
    initialSequence?.Kill();
    loopSequence?.Kill();
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

    float CrashDuration = predictedFlightMult > socket.takeOffDuration - fastCrashTakeoffOffset ? slowCrashDuration : fastCrashDuration;

    Plane.DOAnchorPos(new Vector2(crashX, crashY), CrashDuration);
  }
}

