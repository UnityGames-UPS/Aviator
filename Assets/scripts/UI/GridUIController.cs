using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GridUIController : MonoBehaviour
{
  [SerializeField]
  private GridLayoutGroup grid;
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

  void GamePlayPanelMove()
  {
    float width = widthSource.rect.width;

    if (Mathf.Abs(width - lastWidth) < 0.5f)
      return;

    lastWidth = width;

    bool stacked = width < minItemWidth * 2f;
    int columns = stacked ? 1 : 2;
    int rows = Mathf.CeilToInt(grid.transform.childCount / (float)columns);
    if (columns == 2)
    {
      MainParent_LE.preferredHeight = 704f;
    }
    else
    {
      MainParent_LE.preferredHeight = 1000f;
    }

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
