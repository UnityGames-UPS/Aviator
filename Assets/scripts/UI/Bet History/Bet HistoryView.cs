using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetHistoryView : MonoBehaviour
{
  [SerializeField] private TMP_Text betText;
  [SerializeField] private TMP_Text winText;
  [SerializeField] private TMP_Text createdAtText;
  [SerializeField] private TMP_Text multText;
  [SerializeField] private Image bgImage;
  [SerializeField] private Color blackColor;
  [SerializeField] private Color greenColor;

  internal void Set(BetHistory view)
  {
    if(bgImage)
      bgImage.color = blackColor;

    if (view.bet_amount != -1)
    {
      if (betText) betText.text = view.bet_amount.ToString("N2");
    }
    else
    {
      Debug.LogError("BetHist bet amount is not parsed properly");
    }

    if (view.win_amount != -1)
    {
      if (winText) 
        winText.text = view.win_amount.ToString("N2");
    }
    else
    {
      if (winText)
        winText.text = "-";
      Debug.LogError("BetHist win amount is not parsed properly");
    }

    if (view.created_at != "")
    {
      if (DateTime.TryParse(view.created_at, out var dateTime))
      {
        if (createdAtText)
          createdAtText.text = dateTime.ToString("hh:mm tt\ndd.MM.yy");
      }
      else
      {
        if (createdAtText)
          createdAtText.text = "-";
        Debug.LogError("BetHistory Date Time parse err");
      }
    }
    else
    {
      if (createdAtText)
        createdAtText.text = "-";
      Debug.LogError("BetHistory Date Time parse err");
    }

    if (view.multiplier != -1)
    {
      if (multText)
        multText.text = view.multiplier.ToString("N2");
    }
    else
    {
      if (multText)
        multText.text = "-";
      Debug.LogError("BetHist mult parse error");
    }

    if (view.win_amount > 0)
    {
      if(bgImage)
        bgImage.color = greenColor; 
    }
  }
}

