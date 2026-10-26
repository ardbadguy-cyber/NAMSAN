using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Namsan
{
    public static class PrototypeMenu
    {
        [MenuItem("NAMSAN/Create B401 Prototype")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("B401 Prototype").AddComponent<B401Prototype>();
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scenes/B401_Playable.unity");
            EditorSceneManager.SaveScene(scene, path);
            Selection.activeGameObject = GameObject.Find("B401 Prototype");
            EditorUtility.DisplayDialog("NAMSAN", "Scene created. Press Play to explore B401.", "OK");
        }
    }
}
