using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantView : MonoBehaviour
{
  [Header("UI")]
  [SerializeField] private Image avatarImage;
  [SerializeField] private TMP_Text usernameText;
  [SerializeField] private TMP_Text betText;
  [SerializeField] private TMP_Text cashoutMultText;
  [SerializeField] private TMP_Text cashoutWinText;
  [SerializeField] private Image cashoutBgImage;
  [SerializeField] private Color blackColor;
  [SerializeField] private Color greenColor;
  [SerializeField] private Sprite[] ProfileImages;

  internal string BetId;
  internal string UserId;

  public void Set(Participant p)
  {
    BetId = p.betId;
    UserId = p.userId;
    cashoutBgImage.color = blackColor;
    if (UserId != "" && p.userId != null)
    {
      string userId = p.userId;
      string username = userId.Length > 2 ? $"{userId[0]}****{userId[^1]}" : userId;
      usernameText.text = username;
    }
    if (p.betAmount > 0)
    {
      betText.text = p.betAmount.ToString("F2");
    }
    cashoutMultText.text = "";
    cashoutWinText.text = "";
    avatarImage.sprite = ProfileImages[Random.Range(0, ProfileImages.Length)];
  }

  internal void MarkCashedOut(double multiplier, double winAmount)
  {
    cashoutMultText.text = multiplier.ToString("F2") + "x";
    cashoutWinText.text = winAmount.ToString("F2");
    cashoutBgImage.color = greenColor;
  }
}
