using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrashHistoryPopupManager : GenericObjectPool<CrashHistoryView>
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private CrashHistoryManager sourceHistory;
  [SerializeField] private GridLayoutGroup gridLayoutGroup;
  [SerializeField] private Button openButton;
  [SerializeField] private Button closeButton;
  [SerializeField] private Button backgroundButton;

  private readonly List<CrashHistoryView> items = new();

  protected override void Awake()
  {
    base.Awake();
    if (!gridLayoutGroup && ParentTransform != null)
      gridLayoutGroup = ParentTransform.GetComponent<GridLayoutGroup>();

    if (openButton != null)
      openButton.onClick.AddListener(Open);
    if (closeButton != null)
      closeButton.onClick.AddListener(Close);
    if (backgroundButton != null)
      backgroundButton.onClick.AddListener(Close);
  }

  private void OnEnable()
  {
    RefreshHistory();
  }

  private void OnDisable()
  {
    if (openButton != null)
      openButton.onClick.RemoveListener(Open);
    if (closeButton != null)
      closeButton.onClick.RemoveListener(Close);
    if (backgroundButton != null)
      backgroundButton.onClick.RemoveListener(Close);
  }

  internal void Open()
  {
    ParentTransform.parent.gameObject.SetActive(true);
  }

  internal void Close()
  {
    ParentTransform.parent.gameObject.SetActive(false);
  }

  internal void RefreshHistory()
  {
    if (sourceHistory != null)
    {
      InitHistory(sourceHistory.GetDisplayedRounds());
      return;
    }

    if (socket != null)
      InitHistory(socket.crashHistoryRounds);
  }

  internal void InitHistory(List<CrashHistoryRoundData> rounds)
  {
    ReturnAllItemsToPool();
    items.Clear();

    if (ParentTransform == null)
      return;

    if (rounds == null)
      return;

    int maxCount = socket != null ? socket.maxHistoryCount : rounds.Count;
    if (maxCount <= 0)
      return;

    int startIndex = Mathf.Max(0, rounds.Count - maxCount);
    for (int i = startIndex; i < rounds.Count; i++)
    {
      var view = GetFromPool();
      view.transform.SetAsLastSibling();
      view.SetData(rounds[i], true);
      items.Add(view);
    }

    LayoutRebuilder.ForceRebuildLayoutImmediate(ParentTransform as RectTransform);
  }

  internal void AddCrash(CrashHistoryRoundData newRound)
  {
    if (newRound == null)
      return;

    if (sourceHistory != null)
    {
      RefreshHistory();
      return;
    }

    if (ParentTransform == null)
      return;

    int maxCount = socket != null ? socket.maxHistoryCount : items.Count + 1;
    if (maxCount <= 0)
      return;

    CrashHistoryView view;
    if (items.Count >= maxCount && items.Count > 0)
    {
      view = items[items.Count - 1];
      items.RemoveAt(items.Count - 1);
    }
    else
    {
      view = GetFromPool();
    }

    items.Insert(0, view);
    view.transform.SetAsFirstSibling();
    view.SetData(newRound, true);

    LayoutRebuilder.ForceRebuildLayoutImmediate(ParentTransform as RectTransform);
  }
}
