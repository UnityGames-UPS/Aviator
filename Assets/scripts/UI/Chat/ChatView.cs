using TMPro;
using UnityEngine;

public class ChatView : MonoBehaviour
{
  [SerializeField] private TMP_Text chatMessage;

  internal void SetMessage(string userId, string message, string myUserId)
  {
    Debug.Log(userId);
    Debug.Log(myUserId);
    string displayId = userId[0] + "****" + userId[^1];
    if (myUserId == userId)
    {
      Debug.Log("Blue");
      chatMessage.text = "<color=blue>" + displayId + "</color> :" +message;
    }
    else
    {
      Debug.Log("Purple");
      chatMessage.text = "<color=purple>" + displayId + "</color> :" +message; 
    }
  }
}
