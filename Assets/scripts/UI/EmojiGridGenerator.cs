using UnityEngine;
using UnityEngine.UI;
using AdvancedInputFieldPlugin;
using DG.Tweening;


public class EmojiGridGenerator : MonoBehaviour
{
    public Sprite[] emojiSprites;
    public GameObject emojiButtonPrefab;
    public Transform gridParent;
    public AdvancedInputField inputField;

    private bool generated = false;


    void Start()
    {
        if (generated) return;
        generated = true;

        GenerateEmojis();
        NativeKeyboardManager.TryDestroy();

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

        string tag = $"<sprite index={index}>";

        // Always append to end
        inputField.Text += tag;

        // Move caret to end
        inputField.CaretPosition = inputField.Text.Length;
        inputField.ManualSelect();
    }
}
