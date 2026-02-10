using UnityEngine;
using UnityEngine.UI;
using AdvancedInputFieldPlugin;
using TMPro;

public class EmojiGridGenerator : MonoBehaviour
{
  public Sprite[] emojiSprites;
  public GameObject emojiButtonPrefab;
  public Transform gridParent;
  public TMP_InputField inputField;

  private bool generated = false;

  // Must match the baseUnicode used in your editor script
  private const int BASE_UNICODE = 0xE000;

  void Start()
  {
    if (generated) return;
    generated = true;

    GenerateEmojis();
  }

  void GenerateEmojis()
  {
    foreach (Transform child in gridParent)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < emojiSprites.Length; i++)
    {
      int index = i;

      GameObject btn = Instantiate(emojiButtonPrefab, gridParent);
      btn.GetComponent<Image>().sprite = emojiSprites[index];

      Button button = btn.GetComponent<Button>();
      button.onClick.RemoveAllListeners();
      button.onClick.AddListener(() =>
      {
        AppendEmoji(index);
      });
    }
  }

  void AppendEmoji(int index)
  {
    if (inputField == null) return;

    // Convert index to unicode character
    char emojiChar = (char)(BASE_UNICODE + index);

    // Insert as real character
    inputField.text += emojiChar;

    // Move caret to end
    inputField.caretPosition = inputField.text.Length;

    inputField.ActivateInputField();
  }
}
