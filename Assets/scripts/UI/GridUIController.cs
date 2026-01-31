using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GridUIController : MonoBehaviour
{
  [SerializeField]
  private GridLayoutGroup grid;
  [SerializeField]
  private RectTransform BetPanel_RT;
  [SerializeField]
  private RectTransform MainParent_RT;
  [SerializeField]
  private RectTransform rect;
  [SerializeField]
  private RectTransform widthSource;
  [SerializeField]
  private float minItemWidth = 250f;
  [SerializeField]
  private float minWidthSourceWidth = 250f;
  [SerializeField]
  private float itemHeight = 120f;
  [SerializeField]
  private float verticalSpacingWhenStacked = 16f;
  float lastWidth = -1f;
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

    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = columns;

    grid.cellSize = new Vector2(
        stacked ? width : width / 2f,
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
  }
}
