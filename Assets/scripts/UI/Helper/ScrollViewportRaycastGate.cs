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

  private void Awake()
  {
    if (viewportRaycastGraphic == null && transform.parent != null)
      viewportRaycastGraphic = transform.parent.GetComponent<Graphic>();
  }

  private void OnEnable()
  {
    Refresh();
  }

  internal void Refresh()
  {
    bool hasVisibleItems = HasAnyActiveImmediateChild();

    if (viewportRaycastGraphic != null)
      viewportRaycastGraphic.raycastTarget = hasVisibleItems;
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
