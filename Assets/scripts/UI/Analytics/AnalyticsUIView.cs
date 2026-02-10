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
  [SerializeField] private Button provablyFairButton;
  [SerializeField] private Sprite[] ProfileImages;
  private AnalyticsRecord currentRecord;

  private void Awake()
  {
    if (provablyFairButton != null)
    {
      provablyFairButton.onClick.RemoveAllListeners();
      provablyFairButton.onClick.AddListener(OnProvablyFairButtonClicked);
    }
  }

  public void Setup(AnalyticsRecord recordData)
  {
    currentRecord = recordData;
    RoundDetails roundDetails = recordData != null ? recordData.round_details : null;

    string username = recordData?.user_id ?? "";
    username = username.Length > 2 ? $"{username[0]}****{username[^1]}" : username;
    if (usernameText)
      usernameText.text = username;

    if (dateText)
    {
      string createdAt = recordData?.created_at ?? "";
      if (DateTime.TryParse(createdAt, out var dateTime))
        dateText.text = dateTime.ToString("dd.MM.yy HH:mm");
      else
        dateText.text = "";
    }

    float bet = recordData != null ? recordData.bet_amount : 0f;
    float mult = recordData != null ? recordData.multiplier : 0f;
    float win = recordData != null ? recordData.win_amount : 0f;
    float crash = roundDetails != null ? roundDetails.crashPoint : 0f;

    if (betText) betText.text = bet > 0 ? bet.ToString("N2") : "";
    if (multText) multText.text = mult > 0 ? mult.ToString("N2") + "x" : crash > 0 ? crash.ToString("N2") + "x" : "";
    if (winText) winText.text = win > 0 ? win.ToString("N2") : "";
    if (crashText) crashText.text = crash > 0 ? crash.ToString("N2") + "x" : mult > 0 ? mult.ToString("N2") + "x" : "";

    if (avatarImage)
    {
      Sprite sprite = UIManager.Instance != null ? UIManager.Instance.GetRandomProfileSprite() : null;
      if (sprite == null && ProfileImages != null && ProfileImages.Length > 0)
        sprite = ProfileImages[UnityEngine.Random.Range(0, ProfileImages.Length)];
      avatarImage.sprite = sprite;
    }
  }

  private void OnProvablyFairButtonClicked()
  {
    if (currentRecord == null)
      return;

    if (UIManager.Instance != null)
      UIManager.Instance.OpenProvablyFairPopupFromAnalytics(currentRecord);
  }
}
