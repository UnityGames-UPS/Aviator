using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(EmojiGridGenerator))]
public class EmojiGridGeneratorEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    var generator = (EmojiGridGenerator)target;
    using (new EditorGUI.DisabledScope(generator == null))
    {
      if (GUILayout.Button("Generate Emoji Grid"))
      {
        if (generator.gridParent == null)
        {
          Debug.LogWarning("EmojiGridGenerator: gridParent is not assigned.");
          return;
        }

        Undo.RegisterFullObjectHierarchyUndo(generator.gridParent.gameObject, "Generate Emoji Grid");
        generator.GenerateEmojisInEditor();
        EditorUtility.SetDirty(generator);
        EditorSceneManager.MarkSceneDirty(generator.gameObject.scene);
      }
    }
  }
}
