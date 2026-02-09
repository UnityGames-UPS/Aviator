using UnityEngine;
public class ProvablyFairSettingsManager : MonoBehaviour
{
  [Header("Layout Switch")]
  [SerializeField] private RectTransform widthSource;
  [SerializeField] private float minWidthForLandscape = 800f;
  [SerializeField] private GameObject portraitPanel;
  [SerializeField] private GameObject landscapePanel;

  private float lastWidth = -1f;
  private bool lastLandscape;

  private void Awake()
  {
    UpdateOrientation(force: true);
  }

  private void LateUpdate()
  {
    UpdateOrientation(force: false);
  }

  private void UpdateOrientation(bool force)
  {
    if (widthSource == null || portraitPanel == null || landscapePanel == null)
      return;

    float width = widthSource.rect.width;
    if (!force && Mathf.Abs(width - lastWidth) < 0.5f)
      return;

    bool isLandscape = width >= minWidthForLandscape;
    if (!force && isLandscape == lastLandscape)
    {
      lastWidth = width;
      return;
    }

    lastWidth = width;
    lastLandscape = isLandscape;

    landscapePanel.SetActive(isLandscape);
    portraitPanel.SetActive(!isLandscape);
  }
}
