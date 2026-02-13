using UnityEngine;
using DG.Tweening;

public class CurveManager : MonoBehaviour
{

  [SerializeField] private CurveFillerUI curve;

  [SerializeField] private float loopDuration = 2f;

  [Header("Curve Points")]
  [SerializeField] private float zeroHM = 0.01f;
  [SerializeField] private float zeroWM = 0.03f;

  [SerializeField] private float topHM = 0.85f;
  [SerializeField] private float topWM = 0.76f;

  [SerializeField] private float bottomHM = 0.64f;
  [SerializeField] private float bottomWM = 0.85f;

  [Header("Portrait Curve Points")]
  [SerializeField] private float topHMPortrait = 0.75f;
  [SerializeField] private float topWMPortrait = 0.76f;
  [SerializeField] private float bottomHMPortrait = 0.5f;
  [SerializeField] private float bottomWMPortrait = 0.85f;

  [Space(10)]
  [SerializeField] private float crashXoffset = 2000;
  [SerializeField] private float crashYoffset;

  [Space(10)]
  [SerializeField] private float slowCrashDuration = 2f;
  [SerializeField] private float fastCrashDuration = 1f;

  [Space(10)]
  [SerializeField] private float takeoffDurationSeconds = 9f;

  internal bool Flying;
  private bool animationToggle = true;
  private bool isPortrait;
  private float takeoffElapsed;
  private float loopElapsed;
  private bool loopGoingDown = true;
  private bool loopAllowed;
  private float currentMult = 1f;
  [SerializeField] private float takeoffHeightTimeExponent = 1.5f;
  [SerializeField] private float takeoffEndMultiplier = 2f;
  [Header("Debug")]
  [SerializeField] private bool debugToggle = false;
  [SerializeField] private KeyCode debugStartKey = KeyCode.T;
  [SerializeField] private KeyCode debugStopKey = KeyCode.Y;
  [SerializeField] private float debugTakeoffDuration = 2f;

  private enum AnimState
  {
    Idle,
    Takeoff,
    Loop
  }

  private AnimState state = AnimState.Idle;

  void Awake()
  {
    ResetVisual();
  }

  void Update()
  {
    if (debugToggle)
      HandleDebugInput();

    if (!animationToggle || !curve.enabled)
      return;

    switch (state)
    {
      case AnimState.Takeoff:
        UpdateTakeoff();
        break;
      case AnimState.Loop:
        UpdateLoop();
        break;
    }
  }

  internal void ResetVisual()
  {
    if(animationToggle)
      curve.enabled = true;
    curve.followCurve = true;
    curve.heightMultiplier = zeroHM;
    curve.widthMultiplier = zeroWM;
    curve.SetVerticesDirty();
    state = AnimState.Idle;
    takeoffElapsed = 0f;
    loopElapsed = 0f;
    loopGoingDown = true;
    loopAllowed = false;
    currentMult = 1f;
  }

  internal void StartFlyingAnimation()
  {
    Debug.Log("Starting flying animation");
    ResetVisual();

    Flying = true;
    state = AnimState.Takeoff;
    takeoffElapsed = 0f;
    loopAllowed = false;
    currentMult = 1f;
  }

  internal void NotifyMultiplier(float mult)
  {
    currentMult = Mathf.Max(1f, mult);
    if (state == AnimState.Takeoff && currentMult >= takeoffEndMultiplier)
      loopAllowed = true;
  }

  void StartLoop()
  {
    state = AnimState.Loop;
    loopElapsed = 0f;
    loopGoingDown = true;
  }

  internal void OnCrash(float crashMult)
  {
    Flying = false;
    state = AnimState.Idle;
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

    float finalMult = crashMult > 0f ? crashMult : currentMult;
    float CrashDuration = finalMult >= takeoffEndMultiplier ? slowCrashDuration : fastCrashDuration;

    Plane.DOAnchorPos(new Vector2(crashX, crashY), CrashDuration);
  }

  internal void AnimationToggle(bool toggle)
  {
    animationToggle = toggle;
    curve.enabled = toggle;
    curve.PlaneParent.gameObject.SetActive(toggle);
  }

  internal void SetPortraitMode(bool portrait)
  {
    if (isPortrait == portrait)
      return;

    isPortrait = portrait;
  }

  void UpdateTakeoff()
  {
    float duration = Mathf.Max(0.0001f, GetTakeoffDuration());
    takeoffElapsed = Mathf.Min(duration, takeoffElapsed + Time.deltaTime);

    float t = Mathf.Min(1f, takeoffElapsed / duration);
    float widthEased = EaseOutSine(t);
    float heightTime = Mathf.Pow(t, Mathf.Max(0.01f, takeoffHeightTimeExponent));
    float heightEased = EaseOutSine(heightTime);

    float topH = isPortrait ? topHMPortrait : topHM;
    float topW = isPortrait ? topWMPortrait : topWM;
    curve.heightMultiplier = Mathf.Lerp(zeroHM, topH, heightEased);
    curve.widthMultiplier = Mathf.Lerp(zeroWM, topW, widthEased);
    curve.SetVerticesDirty();

    if (takeoffElapsed >= duration && loopAllowed)
      StartLoop();
  }

  void UpdateLoop()
  {
    float duration = Mathf.Max(0.0001f, loopDuration);
    loopElapsed += Time.deltaTime;

    if (loopElapsed >= duration)
    {
      loopElapsed -= duration;
      loopGoingDown = !loopGoingDown;
    }

    float t = loopElapsed / duration;
    float eased = EaseInOutSine(t);

    float topH = isPortrait ? topHMPortrait : topHM;
    float bottomH = isPortrait ? bottomHMPortrait : bottomHM;
    float topW = isPortrait ? topWMPortrait : topWM;
    float bottomW = isPortrait ? bottomWMPortrait : bottomWM;

    float fromH = loopGoingDown ? topH : bottomH;
    float toH = loopGoingDown ? bottomH : topH;
    float fromW = loopGoingDown ? topW : bottomW;
    float toW = loopGoingDown ? bottomW : topW;

    curve.heightMultiplier = Mathf.Lerp(fromH, toH, eased);
    curve.widthMultiplier = Mathf.Lerp(fromW, toW, eased);
    curve.SetVerticesDirty();
  }

  static float EaseOutSine(float t)
  {
    return Mathf.Sin(t * Mathf.PI * 0.5f);
  }

  static float EaseInOutSine(float t)
  {
    return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
  }

  float GetTakeoffDuration()
  {
    if (debugToggle)
      return debugTakeoffDuration;
    return takeoffDurationSeconds;
  }

  void HandleDebugInput()
  {
    if (Input.GetKeyDown(debugStartKey))
      StartFlyingAnimation();

    if (Input.GetKeyDown(debugStopKey))
      OnCrash(currentMult);
  }

}
