using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // for LayoutRebuilder

public class CrashHistoryManager : GenericObjectPool<CrashHistoryView>
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private RectTransform container;                 // assign: the Content rect (parent of items)
  [SerializeField] private HorizontalLayoutGroup layoutGroup;       // assign: on Content
  [SerializeField] private float introOffsetX = 120f;               // how far left the new crash starts from
  [SerializeField] private float slideDuration = 0.40f;
  [SerializeField] private Ease slideEase = Ease.OutCubic;
  [SerializeField] private float scaleDuration = 0.35f;
  [SerializeField] private Ease scaleEase = Ease.OutBack;

  // Left → right order (0 = left-most / newest after we rotate)
  private readonly List<CrashHistoryView> slots = new();

  protected override void Awake()
  {
    base.Awake();
    if (!container) container = ParentTransform as RectTransform;
    if (!layoutGroup) layoutGroup = ParentTransform.GetComponent<HorizontalLayoutGroup>();
  }

  internal void InitHistory(List<float> multipliers)
  {
    ReturnAllItemsToPool();
    slots.Clear();

    // ensure fixed count
    var list = new List<float>(multipliers);
    if (list.Count < socket.maxHistoryCount)
      list.AddRange(GenerateRandomCrashes(socket.maxHistoryCount - list.Count));
    else if (list.Count > socket.maxHistoryCount)
      list = list.GetRange(list.Count - socket.maxHistoryCount, socket.maxHistoryCount); // keep latest N

    // create exactly maxHistoryCount slots
    for (int i = 0; i < socket.maxHistoryCount; i++)
    {
      var v = GetFromPool();
      v.transform.SetParent(container, false);
      v.transform.SetAsLastSibling();
      v.SetValue(list[i], /*resetTransforms:*/ true);
      slots.Add(v);
    }

    // Build final layout once
    LayoutRebuilder.ForceRebuildLayoutImmediate(container);
  }

  internal IEnumerator AddCrash(float newMultiplier)
  {
    if (slots.Count == 0) yield break;

    // Capture current positions BEFORE reordering
    var beforeX = new float[slots.Count];
    for (int i = 0; i < slots.Count; i++)
      beforeX[i] = slots[i].Rect.anchoredPosition.x;

    // 1) Rotate: take last → front (we reuse the view)
    var recycled = slots[slots.Count - 1];
    slots.RemoveAt(slots.Count - 1);
    slots.Insert(0, recycled);

    // Move recycled in hierarchy to first
    recycled.transform.SetAsFirstSibling();
    // Set the new number (no transform reset here)
    recycled.SetValue(newMultiplier, false);

    // Force layout to compute TARGET positions for the new order
    LayoutRebuilder.ForceRebuildLayoutImmediate(container);

    var targetX = new float[slots.Count];
    for (int i = 0; i < slots.Count; i++)
      targetX[i] = slots[i].Rect.anchoredPosition.x;

    // 2) Disable layout so it doesn't fight our tweens
    if (layoutGroup) layoutGroup.enabled = false;

    // 3) Set initial positions for tween
    // All items that moved right by 1: index 1..end
    for (int i = slots.Count - 1; i >= 1; i--)
    {
      var r = slots[i].Rect;
      // start them from their "previous" x
      r.anchoredPosition = new Vector2(beforeX[i - 1], r.anchoredPosition.y);
      r.DOKill(); // cancel any lingering tweens
    }

    // Recycled (index 0): start from left offset + scaled up + transparent text
    var recRect = recycled.Rect;
    recRect.anchoredPosition = new Vector2(targetX[0] - introOffsetX, recRect.anchoredPosition.y);
    recycled.PrepareSpawnVisual(); // set scale>1 and alpha 0

    // 4) Tween everyone to their TARGET spots
    // Right-shifted items
    for (int i = 1; i < slots.Count; i++)
    {
      var r = slots[i].Rect;
      r.DOAnchorPosX(targetX[i], slideDuration).SetEase(slideEase);
    }

    // Recycled spawn
    var spawnSeq = DOTween.Sequence();
    spawnSeq.Append(recRect.DOAnchorPosX(targetX[0], slideDuration).SetEase(slideEase));
    spawnSeq.Join(recycled.DOScaleToOne(scaleDuration, scaleEase));
    spawnSeq.Join(recycled.DOFadeInText(slideDuration * 0.75f));

    // 5) Re-enable layout after animation and snap to final (for safety)
    yield return spawnSeq.WaitForCompletion();
    if (layoutGroup) layoutGroup.enabled = true;
    LayoutRebuilder.ForceRebuildLayoutImmediate(container);
  }

  internal List<float> GenerateRandomCrashes(int count)
  {
    var res = new List<float>(count);
    for (int i = 0; i < count; i++)
      res.Add(Random.Range(1.05f, socket.MaxMult));
    return res;
  }
}
