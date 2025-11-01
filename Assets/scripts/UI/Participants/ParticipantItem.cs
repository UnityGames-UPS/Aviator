using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantItem : MonoBehaviour
{
  [Header("UI")]
  [SerializeField] private Image  avatarImage;
  [SerializeField] private TMP_Text usernameText;
  [SerializeField] private TMP_Text betText;
  [SerializeField] private TMP_Text cashoutMultText;
  [SerializeField] private TMP_Text cashoutWinText;
  [SerializeField] private Image cashoutBgImage;      // optional, for user, etc.
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
    usernameText.text = string.IsNullOrEmpty(p.username) ? "—" : p.username;
    betText.text = p.betAmount.ToString("F2");
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
