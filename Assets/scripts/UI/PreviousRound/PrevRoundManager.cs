using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PrevRoundManager : GenericObjectPool<ParticipantItem>
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private TMP_Text crashPointText;
  [SerializeField] private Color PurpleColor;

  internal void PopulatePreviousRounds()
  {
    base.ReturnAllItemsToPool();

    LastRoundResult roundResult = socket.lastRoundResult;
    if (roundResult.crashPoint < 2)
    {
      crashPointText.color = Color.blue;
    }
    else
    {
      crashPointText.color = PurpleColor;
    }
    crashPointText.text = roundResult.crashPoint.ToString("F2");
    if (roundResult.participants.Count > 0)
    {
      foreach (var p in roundResult.participants)
      {
        var item = base.GetFromPool();
        item.Set(p);
        item.transform.localScale = Vector3.one * 0.95f;
        item.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
      }
    }
  }
}
