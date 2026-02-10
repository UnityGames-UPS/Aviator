using DG.Tweening;
using UnityEngine;

public class AnalyticsUIManager : GenericObjectPool<AnalyticsUIView>
{
  [SerializeField] private SocketIOManager socket;

  internal void PopulateAnalyticsUI()
  {
    base.ReturnAllItemsToPool();

    var records = socket.analyticsData.payload.analyticsRecords;

    if (records == null || records.Count == 0)
    {
      Debug.LogWarning("⚠️ No analytics records to display.");
      return;
    }

    for (int i = 0; i < records.Count; i++)
    {
      var recordData = records[i];
      var recordUI = base.GetFromPool();
      recordUI.Setup(recordData);
      recordUI.transform.SetSiblingIndex(i);

      recordUI.transform.localScale = Vector3.one * 0.95f;
      recordUI.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
  }
}
