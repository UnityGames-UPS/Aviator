using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach on a ScrollRect Content object.
/// It enables/disables the parent viewport's raycastTarget based on whether
/// this content has any active immediate children.
/// </summary>
public class ScrollViewportRaycastGate : MonoBehaviour
{
  [SerializeField] private Graphic viewportRaycastGraphic;
  [SerializeField] private ScrollRect owningScrollRect;
  [SerializeField] private bool disableScrollRectWhenEmpty = true;

  private void Awake()
  {
    if (viewportRaycastGraphic == null && transform.parent != null)
      viewportRaycastGraphic = transform.parent.GetComponent<Graphic>();

    if (owningScrollRect == null)
      owningScrollRect = GetComponentInParent<ScrollRect>();
  }

  private void OnEnable()
  {
    Refresh();
  }

  public void Refresh()
  {
    bool hasVisibleItems = HasAnyActiveImmediateChild();

    if (viewportRaycastGraphic != null)
      viewportRaycastGraphic.raycastTarget = hasVisibleItems;

    if (disableScrollRectWhenEmpty && owningScrollRect != null)
      owningScrollRect.enabled = hasVisibleItems;
  }

  private bool HasAnyActiveImmediateChild()
  {
    for (int i = 0; i < transform.childCount; i++)
    {
      if (transform.GetChild(i).gameObject.activeSelf)
        return true;
    }
    return false;
  }
}
