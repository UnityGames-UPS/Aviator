using UnityEngine;
using UnityEditor;
using TMPro;

public class AutoAssignEmojiUnicode
{
    [MenuItem("Tools/Assign Private Unicode To Sprite Asset")]
    static void AssignUnicode()
    {
        TMP_SpriteAsset spriteAsset = Selection.activeObject as TMP_SpriteAsset;

        if (spriteAsset == null)
        {
            Debug.LogError("Select a TMP Sprite Asset first.");
            return;
        }

        int baseUnicode = 0xE000; // Private Use Area start

        for (int i = 0; i < spriteAsset.spriteCharacterTable.Count; i++)
        {
            spriteAsset.spriteCharacterTable[i].unicode = (uint)(baseUnicode + i);
        }

        EditorUtility.SetDirty(spriteAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("Unicode assigned successfully!");
    }
}
