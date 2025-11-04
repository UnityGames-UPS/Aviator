using TMPro;
using UnityEngine;

public class ChatView : MonoBehaviour
{
  [SerializeField] private TMP_Text chatMessage;

  internal void SetMessage(string username, string message)
  {
    username = username[0] + "****" + username[^1];
    chatMessage.text = $"<color=grey>{username}</color>: {message}";
  }
}
