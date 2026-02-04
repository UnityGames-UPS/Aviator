using DG.Tweening;
using TMPro;
using UnityEngine;

public class PrevRoundManager : GenericObjectPool<ParticipantView>
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private TMP_Text crashPointText;
  [SerializeField] private Color PurpleColor;

  internal void PopulatePreviousRounds()
  {
    base.ReturnAllItemsToPool();

    LastRoundResult roundResult = socket.lastRoundResult;
    if (roundResult.crashPoint <= 3.8f)
    {
      crashPointText.color = Color.blue;
    }
    else
    {
      crashPointText.color = PurpleColor;
    }
    crashPointText.text = roundResult.crashPoint.ToString("N2") + "x";
    if (roundResult.participants.Count > 0)
    {
      for (int i = 0; i < roundResult.participants.Count; i++)
      {
        var p = roundResult.participants[i];
        var item = base.GetFromPool();
        item.Set(p);
        if (p.cashedOut)
        {
          item.MarkCashedOut(p.multiplier, p.winAmount); 
        }
        item.transform.SetSiblingIndex(i);
        item.transform.localScale = Vector3.one * 0.95f;
        item.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
      }
    }
  }
}
