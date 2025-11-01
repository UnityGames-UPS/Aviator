using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatView : MonoBehaviour
{
  [SerializeField] private TMP_Text chatMessage;

  internal void SetMessage(string username, string message)
  {
    chatMessage.text = $"<color=grey>{username}</color>: {message}";
  }
}
