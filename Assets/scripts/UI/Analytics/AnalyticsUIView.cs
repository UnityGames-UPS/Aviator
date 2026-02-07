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

  public void Setup(string username = "", string date = "", float bet = 0, float mult = 0, float win = 0, float crash = 0f)
  {
    if (usernameText && username != "") usernameText.text = username;
    string formattedDate = "";
    string formattedTime = "";
    if (DateTime.TryParse(date, out var dateTime) && date != "")
    {
      formattedDate = dateTime.ToString("dd.MM.yy");
      formattedTime = dateTime.ToString("HH:mm");
      dateText.text = formattedDate + " " + formattedTime;
    }
    if (betText && bet != 0) betText.text = bet.ToString("N2");
    if (multText && mult != 0) multText.text = mult.ToString("N2") + "x";
    if (winText && win != 0) winText.text = win.ToString("N2");
    if (crashText && crash != 0) crashText.text = crash.ToString("N2") + "x";
    if (avatarImage)
    {
      Sprite sprite = UIManager.Instance != null ? UIManager.Instance.GetRandomProfileSprite() : null;
      if (sprite == null && ProfileImages != null && ProfileImages.Length > 0)
        sprite = ProfileImages[UnityEngine.Random.Range(0, ProfileImages.Length)];
      avatarImage.sprite = sprite;
    }
  }
}
