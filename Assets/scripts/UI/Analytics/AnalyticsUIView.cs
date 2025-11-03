using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnalyticsUIView : MonoBehaviour
{
  [SerializeField] private Image avatarImage;
  [SerializeField] private TMP_Text usernameText;
  [SerializeField] private TMP_Text dateText;
  [SerializeField] private TMP_Text betText;
  [SerializeField] private TMP_Text winText;
  [SerializeField] private TMP_Text multText;
  [SerializeField] private TMP_Text crashText;
  [SerializeField] private Sprite[] ProfileImages;

  public void Setup(string username, string date, int bet, float mult, float win, float crash)
  {
    if (usernameText) usernameText.text = username;
    string formattedDate = "";
    if (DateTime.TryParse(date, out var dateTime))
    {
      formattedDate = dateTime.ToString("dd.MM.yy");
      dateText.text = formattedDate;
    }
    if (betText) betText.text = bet.ToString("N0");
    if (multText) multText.text = mult.ToString("0.00") + "x";
    if (winText) winText.text = win.ToString("N0");
    if (crashText) crashText.text = crash.ToString("0.00") + "x";
    if (avatarImage) avatarImage.sprite = ProfileImages[UnityEngine.Random.Range(0, ProfileImages.Length)];
  }
}
