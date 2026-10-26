using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Namsan
{
    public static class WallFinishMenu
    {
        [MenuItem("NAMSAN/Materials/Apply Wall Finish to Current Scene")]
        static void Apply()
        {
            if(EditorApplication.isPlaying) return;
            var plaster=Resources.Load<Material>("WallFinish/Namsan_Plaster");
            var paint=Resources.Load<Material>("WallFinish/Namsan_Paint");
            if(plaster==null || paint==null || !plaster.shader.isSupported || !paint.shader.isSupported)
            {
                EditorUtility.DisplayDialog("NAMSAN","Materials unavailable. Wait for import and check Console shader errors.","OK"); return;
            }
            int changed=0;
            foreach(var renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(renderer.gameObject.scene!=EditorSceneManager.GetActiveScene()) continue;
                var finish=B1WallFinish.Replacement(renderer.sharedMaterial,plaster,paint);
                if(finish==null || finish==renderer.sharedMaterial) continue;
                Undo.RecordObject(renderer,"Apply NAMSAN wall finish");
                renderer.sharedMaterial=finish;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                changed++;
            }
            if(changed>0) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("NAMSAN: updated "+changed+" wall renderers in current scene. Save the scene to keep the change. Undo is available.");
        }
    }
}
