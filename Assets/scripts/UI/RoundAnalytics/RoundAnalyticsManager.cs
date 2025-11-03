using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RoundAnalyticsManager : GenericObjectPool<AnalyticsUIView>
{
  [SerializeField] private SocketIOManager socket;

  internal void PopulateRoundAnalytics()
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
      // Debug.Log(recordData.round_details.crashPoint);
      // Debug.Log(recordData.created_at);
      recordUI.Setup(mult: recordData.round_details.crashPoint, date: recordData.created_at);
      
      recordUI.transform.localScale = Vector3.one * 0.95f;
      recordUI.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
  }
}
