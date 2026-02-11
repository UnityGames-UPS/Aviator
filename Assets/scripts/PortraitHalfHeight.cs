using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PortraitHalfHeight : MonoBehaviour
{
  [SerializeField] private RectTransform sourceRect;
  [Header("Portrait Overrides (Plane)")]
  [SerializeField] private RectTransform targetPlaneRect;
  [SerializeField] private Vector3 portraitPlaneLocalPosition;
  [SerializeField] private Vector2 portraitPlaneSizeDelta;

  private RectTransform selfRect;
  private float originalHeight;
  private Vector3 originalPlaneLocalPosition;
  private Vector2 originalPlaneSizeDelta;
  private bool initialized;
  private bool planeInitialized;

  void Awake()
  {
    selfRect = GetComponent<RectTransform>();
    CacheOriginalHeight();
  }

  void OnEnable()
  {
    CacheOriginalHeight();
    Apply();
  }

  void LateUpdate()
  {
    Apply();
  }

  private void CacheOriginalHeight()
  {
    if (selfRect == null) return;
    if (!initialized)
    {
      originalHeight = selfRect.sizeDelta.y;
      initialized = true;
    }

    if (!planeInitialized && targetPlaneRect != null)
    {
      originalPlaneLocalPosition = targetPlaneRect.localPosition;
      originalPlaneSizeDelta = targetPlaneRect.sizeDelta;
      planeInitialized = true;
    }
  }

  private void Apply()
  {
    if (sourceRect == null || selfRect == null) return;

    float w = sourceRect.rect.width;
    float h = sourceRect.rect.height;
    if (w <= 0f || h <= 0f) return;

    float targetHeight = (w < h) ? (originalHeight * 0.5f) : originalHeight;
    Vector2 size = selfRect.sizeDelta;
    if (!Mathf.Approximately(size.y, targetHeight))
    {
      size.y = targetHeight;
      selfRect.sizeDelta = size;
    }

    if (targetPlaneRect == null || !planeInitialized) return;

    bool isPortrait = w < h;
    Vector3 targetPos = isPortrait ? portraitPlaneLocalPosition : originalPlaneLocalPosition;
    Vector2 targetSize = isPortrait ? portraitPlaneSizeDelta : originalPlaneSizeDelta;

    if (targetPlaneRect.localPosition != targetPos)
      targetPlaneRect.localPosition = targetPos;

    if (targetPlaneRect.sizeDelta != targetSize)
      targetPlaneRect.sizeDelta = targetSize;
  }
}
