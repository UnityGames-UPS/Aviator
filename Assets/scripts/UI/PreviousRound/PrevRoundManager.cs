using DG.Tweening;
using TMPro;
using UnityEngine;

public class PrevRoundManager : GenericObjectPool<ParticipantView>
{
  [SerializeField] private SocketIOManager socket;
  [SerializeField] private TMP_Text crashPointText;
  [SerializeField] private Color BlueColor;
  [SerializeField] private Color PurpleColor;
  [SerializeField] private Color PinkColor;

  internal void PopulatePreviousRounds()
  {
    base.ReturnAllItemsToPool();

    LastRoundResult roundResult = socket.lastRoundResult;
    if (roundResult.crashPoint <= 2f)
      crashPointText.color = BlueColor;
    else if (roundResult.crashPoint < 10f)
      crashPointText.color = PurpleColor;
    else
      crashPointText.color = PinkColor;
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
