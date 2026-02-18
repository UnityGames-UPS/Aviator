using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GridUIController : MonoBehaviour
{
  [SerializeField]
  private GridLayoutGroup grid;
  private float referenceScreenHeight = 2340f;
  private float referencePreferredHeight = 700f;
  private int lastScreenHeight;
  [SerializeField]
  private GameObject LeftAutobetOptions;
  [SerializeField]
  private GameObject RightAutobetOptions;
  [SerializeField]
  private RectTransform LeftAutobet_transform;
  [SerializeField]
  private RectTransform RightAutobet_transform;
  [SerializeField]
  private RectTransform LeftMainBet_transform;
  [SerializeField]
  private RectTransform RightMainBet_transform;
  [SerializeField]
  private RectTransform LeftInBet_transform;
  [SerializeField]
  private RectTransform RightInBet_transform;
  [SerializeField]
  private LayoutElement MainParent_LE;
  [SerializeField]
  private RectTransform BetPanel_RT;
  [SerializeField]
  private RectTransform MainParent_RT;
  [SerializeField]
  private RectTransform rect;
  [SerializeField]
  private RectTransform widthSource;
  [SerializeField]
  private CurveManager curveManager;
  [SerializeField]
  private float minItemWidth = 250f;
  [SerializeField]
  private float minWidthSourceWidth = 250f;
  [SerializeField]
  private float itemHeight = 120f;
  [SerializeField]
  private float verticalSpacingWhenStacked = 16f;
  [Header("Portrait Overrides (Plane)")]
  [SerializeField]
  private RectTransform targetPlaneRect;
  [SerializeField]
  private Vector3 portraitPlaneLocalPosition;
  [SerializeField]
  private Vector2 portraitPlaneSizeDelta;
  private Vector3 originalPlaneLocalPosition;
  private Vector2 originalPlaneSizeDelta;
  private bool planeInitialized;
  float lastWidth = -1f;
  bool lastStacked;
  void Awake()
  {
    CachePlaneDefaults();
  }
  void LateUpdate()
  {
    GamePlayPanelMove();
  }

  internal void CheckAndFixLeftAutoBet(bool isActive)
  {
    if (lastStacked && isActive)
    {
      LeftAutobet_transform.SetParent(LeftMainBet_transform);
      LeftAutobet_transform.SetSiblingIndex(LeftMainBet_transform.childCount - 2);
      grid.cellSize = new Vector2(grid.cellSize.x, 300);
      itemHeight = 300;
      LeftAutobet_transform.anchorMin = new Vector2(0.5f, 0f);
      LeftAutobet_transform.anchorMax = new Vector2(0.5f, 0f);

      // Pivot center
      LeftAutobet_transform.pivot = new Vector2(0.5f, 0.5f);

      // Position
      LeftAutobet_transform.anchoredPosition = new Vector2(0f, 21.69f);
      Canvas.ForceUpdateCanvases();
    }
    else
    {
      LeftAutobet_transform.SetParent(LeftInBet_transform);
      if (!RightAutobetOptions.activeSelf)
      {
        grid.cellSize = new Vector2(grid.cellSize.x, 260);
        itemHeight = 260;
      }
    }
  }

  internal void CheckAndFixRightAutoBet(bool isActive)
  {
    if (lastStacked && isActive)
    {
      RightAutobet_transform.SetParent(RightMainBet_transform);
      RightAutobet_transform.SetSiblingIndex(RightMainBet_transform.childCount - 2);
      grid.cellSize = new Vector2(grid.cellSize.x, 300);
      itemHeight = 300;
      RightAutobet_transform.anchorMin = new Vector2(0.5f, 0f);
      RightAutobet_transform.anchorMax = new Vector2(0.5f, 0f);

      // Pivot center
      RightAutobet_transform.pivot = new Vector2(0.5f, 0.5f);

      // Position
      RightAutobet_transform.anchoredPosition = new Vector2(0f, 21.69f);
      Canvas.ForceUpdateCanvases();
    }
    else
    {
      RightAutobet_transform.SetParent(RightInBet_transform);
      if (!LeftAutobetOptions.activeSelf)
      {
        grid.cellSize = new Vector2(grid.cellSize.x, 260);
        itemHeight = 260;
      }
    }
  }
  void OnRectTransformDimensionsChange()
  {
    if (Screen.height != lastScreenHeight)
    {
      UpdatePreferredHeight();
    }
  }
  void UpdatePreferredHeight()
  {
    float referenceHeight = 2340f;
    float referencePreferred = 680f;

    float targetDeviceHeight = 3049f;   // Replace with actual Debug.Log value
    float targetPreferred = 875f;

    float slope = (targetPreferred - referencePreferred) /
                  (targetDeviceHeight - referenceHeight);

    float newPreferredHeight =
        referencePreferred +
        (Screen.height - referenceHeight) * slope;

    MainParent_LE.preferredHeight = newPreferredHeight;

    LayoutRebuilder.ForceRebuildLayoutImmediate(
        MainParent_LE.GetComponent<RectTransform>()
    );
  }

  void GamePlayPanelMove()
  {
    float width = widthSource.rect.width;

    if (Mathf.Abs(width - lastWidth) < 0.5f)
      return;

    lastWidth = width;

    bool stacked = width < minItemWidth * 2f;
    int columns = stacked ? 1 : 2;
    int rows = Mathf.CeilToInt(grid.transform.childCount / (float)columns);
    CheckAndFixLeftAutoBet(LeftAutobetOptions.activeSelf);
    CheckAndFixRightAutoBet(RightAutobetOptions.activeSelf);
    // if (columns == 2)
    // {
    //   MainParent_LE.preferredHeight = 704f;
    // }
    // else
    // {
    //   MainParent_LE.preferredHeight = 1000f;
    // }

    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = columns;

    grid.cellSize = new Vector2(
        stacked ? width - 10f : (width / 2f) - 10f,
        itemHeight
    );

    grid.spacing = new Vector2(
        grid.spacing.x,
        stacked ? verticalSpacingWhenStacked : 0f
    );

    float totalHeight =
        rows * itemHeight +
        (rows - 1) * grid.spacing.y +
        grid.padding.top +
        grid.padding.bottom;

    // 🚨 THIS is the key line
    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    float widthForBetPanel = MainParent_RT.rect.width;
    // Debug.Log(widthForBetPanel);
    if (widthForBetPanel < minWidthSourceWidth)
    {
      BetPanel_RT.SetParent(widthSource);
    }
    else
    {
      BetPanel_RT.SetParent(MainParent_RT);
      BetPanel_RT.SetAsFirstSibling();
    }

    LayoutRebuilder.MarkLayoutForRebuild(rect);

    if (stacked != lastStacked)
    {
      lastStacked = stacked;
      if (curveManager != null)
        curveManager.SetPortraitMode(stacked);
    }

    ApplyPlaneSizing(stacked);
  }

  private void CachePlaneDefaults()
  {
    if (!planeInitialized && targetPlaneRect != null)
    {
      originalPlaneLocalPosition = targetPlaneRect.localPosition;
      originalPlaneSizeDelta = targetPlaneRect.sizeDelta;
      planeInitialized = true;
    }
  }

  private void ApplyPlaneSizing(bool isPortrait)
  {
    CachePlaneDefaults();
    if (targetPlaneRect == null || !planeInitialized)
      return;

    Vector3 targetPos = isPortrait ? portraitPlaneLocalPosition : originalPlaneLocalPosition;
    Vector2 targetSize = isPortrait ? portraitPlaneSizeDelta : originalPlaneSizeDelta;

    if (targetPlaneRect.localPosition != targetPos)
      targetPlaneRect.localPosition = targetPos;

    if (targetPlaneRect.sizeDelta != targetSize)
      targetPlaneRect.sizeDelta = targetSize;
  }
}
