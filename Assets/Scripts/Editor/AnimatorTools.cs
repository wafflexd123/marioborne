using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorTools : MonoBehaviour
{
    [MenuItem("Animator Tools/Save Blendtree as .asset ^&b")]
    static void CreateBlendtree()
    {
        if (Selection.activeObject is BlendTree)
        {
            AssetDatabase.CreateAsset(Instantiate(Selection.activeObject), AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(Selection.activeObject) + ".asset"));
            Debug.Log("Created BlendTree");
        }
        else
        {
            if (Selection.activeObject != null)
                Debug.LogWarning($"Selected object is a {Selection.activeObject.GetType()} (name: {Selection.activeObject.name}), must be a BlendTree");
            else
                Debug.LogWarning("Selected object is null");
        }
    }
}