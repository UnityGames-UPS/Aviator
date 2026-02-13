using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EmojiGridGenerator : MonoBehaviour
{
  [SerializeField] private TMP_SpriteAsset spriteAsset;
  [SerializeField] private bool generateOnAwake = false;
  public GameObject emojiButtonPrefab;
  public Transform gridParent;
  public TMP_InputField inputField;
  [SerializeField] private float spritePixelsPerUnit = 100f;

  private readonly Dictionary<uint, Sprite> runtimeSprites = new Dictionary<uint, Sprite>();

  void Awake()
  {
    if (generateOnAwake)
      GenerateEmojis(false);
  }

  public void GenerateEmojisInEditor()
  {
    GenerateEmojis(true);
  }

  void GenerateEmojis(bool immediateDestroy)
  {
    if (spriteAsset == null)
      spriteAsset = TMP_Settings.defaultSpriteAsset;
    if (spriteAsset == null)
    {
      Debug.LogWarning("EmojiGridGenerator: No TMP_SpriteAsset assigned or defaulted.");
      return;
    }
    if (gridParent == null)
    {
      Debug.LogWarning("EmojiGridGenerator: gridParent is not assigned.");
      return;
    }
    if (emojiButtonPrefab == null)
    {
      Debug.LogWarning("EmojiGridGenerator: emojiButtonPrefab is not assigned.");
      return;
    }

    ClearChildren(immediateDestroy);

    List<TMP_SpriteGlyph> glyphTable = spriteAsset.spriteGlyphTable;
    List<TMP_SpriteCharacter> characters = spriteAsset.spriteCharacterTable;
    if (characters == null || characters.Count == 0)
    {
      Debug.LogWarning($"EmojiGridGenerator: spriteCharacterTable empty for asset '{spriteAsset.name}'.");
      return;
    }

    int created = 0;
    int skipped = 0;
    for (int i = 0; i < characters.Count; i++)
    {
      var character = characters[i];
      TMP_SpriteGlyph glyph = character.glyph as TMP_SpriteGlyph;
      if (glyph == null && glyphTable != null)
        glyph = glyphTable.Find(g => g.index == character.glyphIndex);

      Sprite sprite = GetSpriteForGlyph(glyph);
      if (sprite == null)
      {
        skipped++;
        continue;
      }

      GameObject btn = Instantiate(emojiButtonPrefab, gridParent);
      if (btn.TryGetComponent<Image>(out var img))
        img.sprite = sprite;

      uint unicode = character.unicode;
      Button button = btn.GetComponent<Button>();
      if (button == null)
        continue;

      button.onClick.RemoveAllListeners();
      button.onClick.AddListener(() =>
      {
        AppendEmoji(unicode);
      });
      created++;
    }

    Debug.Log($"EmojiGridGenerator: Created {created} emoji buttons (skipped {skipped}).");
  }

  private void ClearChildren(bool immediateDestroy)
  {
    foreach (Transform child in gridParent)
    {
      if (immediateDestroy)
        DestroyImmediate(child.gameObject);
      else
        Destroy(child.gameObject);
    }
  }

  private Sprite GetSpriteForGlyph(TMP_SpriteGlyph glyph)
  {
    if (glyph == null)
      return null;

    if (glyph.sprite != null)
      return glyph.sprite;

    if (runtimeSprites.TryGetValue(glyph.index, out var cached))
      return cached;

    var sheet = spriteAsset != null ? spriteAsset.spriteSheet as Texture2D : null;
    if (sheet == null)
      return null;

    var rect = glyph.glyphRect;
    if (rect.width <= 0 || rect.height <= 0)
      return null;

    var sprite = Sprite.Create(
      sheet,
      new Rect(rect.x, rect.y, rect.width, rect.height),
      new Vector2(0.5f, 0.5f),
      spritePixelsPerUnit
    );

    runtimeSprites[glyph.index] = sprite;
    return sprite;
  }

  void AppendEmoji(uint unicode)
  {
    if (inputField == null) return;

    // Insert as real character
    inputField.text += char.ConvertFromUtf32((int)unicode);

    // Move caret to end
    inputField.caretPosition = inputField.text.Length;

    inputField.ActivateInputField();
  }
}
