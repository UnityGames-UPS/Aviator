using System.Collections;
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
  [SerializeField] private ScrollRect scrollRect;

  private void Awake()
  {
    if (viewportRaycastGraphic == null && transform.parent != null)
      viewportRaycastGraphic = transform.parent.GetComponent<Graphic>();
    if (scrollRect == null)
      scrollRect = GetComponentInParent<ScrollRect>();
  }

  internal IEnumerator Refresh()
  {
    yield return new WaitForSecondsRealtime(0.1f);
    bool hasVisibleItems = HasAnyActiveImmediateChild();

    Debug.Log("Refreshing ScrollViewportRaycastGate hasVisibleItems: " + hasVisibleItems);
    if (viewportRaycastGraphic != null)
      viewportRaycastGraphic.raycastTarget = hasVisibleItems;


    if (scrollRect != null)
      scrollRect.enabled = hasVisibleItems;
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
