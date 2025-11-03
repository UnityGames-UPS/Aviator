using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantManager : GenericObjectPool<ParticipantView>
{
  [SerializeField] private TMP_Text TotalBetsText;
  [SerializeField] private TMP_Text TotalWinText;
  [SerializeField] private Image GreenFillerImage;
  [SerializeField] private int totalBetsCount;
  [SerializeField] private int cashedOutCount;
  [SerializeField] private float totalWinAmount;

  // Lookups
  private Dictionary<string, ParticipantView> _rowsByBetId = new();
  private Dictionary<string, Participant> _participantByBetId = new();

  protected override void Awake()
  {
    base.Awake();
    GreenFillerImage.fillAmount = 0;
  }

  // ---- ROUND START ----
  internal void PopulateFromRoundStart(RoundStartData data)
  {
    // Full refresh
    ClearAll();

    totalBetsCount = 0;
    cashedOutCount = 0;
    totalWinAmount = 0;
    GreenFillerImage.fillAmount = 0f;
    TotalWinText.text = totalWinAmount.ToString("F2");

    if (data?.participants == null || data?.participants.Count == 0) return;

    TotalBetsText.text = totalBetsCount.ToString() + "/" + totalBetsCount.ToString();

    foreach (var p in data.participants)
    {
      totalBetsCount++;
      TotalBetsText.text = totalBetsCount.ToString() + "/" + totalBetsCount.ToString();
      AddOrUpdateRow(p);
    }
  }

  // ---- ADD BET ----
  internal void OnAddBet(Participant p)
  {
    if (p == null || string.IsNullOrEmpty(p.betId))
    {
      Debug.LogError("Participant invalid in OnAddBet");
      return;
    }

    totalBetsCount++;
    TotalBetsText.text = totalBetsCount.ToString() + "/" + totalBetsCount.ToString();
    AddOrUpdateRow(p);
  }


  // ---- REMOVE BET ----
  internal void OnRemoveBet(string betId)
  {
    if (string.IsNullOrEmpty(betId)) return;

    totalBetsCount = Mathf.Max(0, totalBetsCount - 1);
    TotalBetsText.text = totalBetsCount.ToString() + "/" + totalBetsCount.ToString();

    RemoveRow(betId);
  }


  // ---- USER CASHOUT ----
  // betId, winAmount, multiplier
  internal void OnUserCashout(string betId, float winAmount, float multiplier)
  {
    if (string.IsNullOrEmpty(betId)) return;

    if (_rowsByBetId.TryGetValue(betId, out var row))
    {
      row.MarkCashedOut(multiplier, winAmount);

      totalWinAmount += winAmount;
      TotalWinText.text = totalWinAmount.ToString("F2");

      cashedOutCount++;
      TotalBetsText.text = (totalBetsCount - cashedOutCount).ToString() + "/" + totalBetsCount.ToString();

      float targetFill = 0f;
      if (totalBetsCount > 0)
        targetFill = (float)cashedOutCount / totalBetsCount;

      DOTween.Kill(GreenFillerImage);
      GreenFillerImage
          .DOFillAmount(targetFill, 0.3f)
          .SetEase(Ease.OutQuad)
          .SetId(GreenFillerImage);
    }
  }


  // ---- Helpers ----
  private void AddOrUpdateRow(Participant p)
  {
    _participantByBetId[p.betId] = p;

    if (_rowsByBetId.TryGetValue(p.betId, out var existing))
    {
      existing.Set(p);
      return;
    }

    var item = base.GetFromPool();
    item.Set(p);
    item.transform.localScale = Vector3.one * 0.95f;
    item.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    _rowsByBetId[p.betId] = item;

    // Optional custom sorting can be done here by reordering item.transform.SetSiblingIndex(...)
  }

  private void RemoveRow(string betId)
  {
    if (_rowsByBetId.TryGetValue(betId, out var item))
    {
      item.Set(new Participant());
      base.ReturnToPool(item);
      _rowsByBetId.Remove(betId);
    }
    _participantByBetId.Remove(betId);
  }

  private void ClearAll()
  {
    base.ReturnAllItemsToPool();
    _rowsByBetId.Clear();
    _participantByBetId.Clear();
  }
}

