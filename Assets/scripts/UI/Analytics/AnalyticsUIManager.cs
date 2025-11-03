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

      string userId = recordData.user_id;
      string username = userId.Length > 2 ? $"{userId[0]}****{userId[^1]}" : userId;

      recordUI.Setup(username, recordData.created_at, recordData.bet_amount, recordData.multiplier, recordData.win_amount, recordData.round_details.crashPoint);

      recordUI.transform.localScale = Vector3.one * 0.95f;
      recordUI.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
  }
}
