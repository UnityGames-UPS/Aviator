using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class PortraitHalfHeight : MonoBehaviour
{
  [SerializeField] private RectTransform sourceRect;

  private RectTransform selfRect;
  private float originalHeight;
  private bool initialized;

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
  }

  private void Apply()
  {
    if (sourceRect == null || selfRect == null) return;

    float w = sourceRect.rect.width;
    float h = sourceRect.rect.height;
    if (w <= 0f || h <= 0f) return;

    float targetHeight = (w < h) ? (originalHeight * 0.7f) : originalHeight;
    Vector2 size = selfRect.sizeDelta;
    if (!Mathf.Approximately(size.y, targetHeight))
    {
      size.y = targetHeight;
      selfRect.sizeDelta = size;
    }
  }
}
