using UnityEngine;
using UnityEditor;
using TMPro;

public class EmojiFixer
{
    [MenuItem("Tools/Fix Emoji Bearings")]
    static void FixBearings()
    {
        TMP_SpriteAsset spriteAsset = Selection.activeObject as TMP_SpriteAsset;

        if (spriteAsset == null)
        {
            Debug.LogError("Select a TMP Sprite Asset first.");
            return;
        }

        foreach (var glyph in spriteAsset.spriteGlyphTable)
        {
            var metrics = glyph.metrics;

            metrics.horizontalBearingX = 0;
            metrics.horizontalBearingY = 56f;

            glyph.metrics = metrics;
        }

        EditorUtility.SetDirty(spriteAsset);
        AssetDatabase.SaveAssets();

        Debug.Log("Emoji bearings updated successfully.");
    }
}
