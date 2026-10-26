using UnityEngine;

namespace Namsan
{
    // Uses authored, persistent assets. No texture generation and no saved-material mutation during Play.
    public class B1WallFinish : MonoBehaviour
    {
        bool applied;
        public void Apply()
        {
            if(applied) return;
            applied=true;
            var plaster=Resources.Load<Material>("WallFinish/Namsan_Plaster");
            var paint=Resources.Load<Material>("WallFinish/Namsan_Paint");
            if(plaster==null || paint==null || !plaster.shader.isSupported || !paint.shader.isSupported)
            {
                Debug.LogWarning("NAMSAN wall materials are missing or unsupported. Existing walls were preserved. Check Console shader errors.",this);
                return;
            }
            foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var finish=Replacement(renderer.sharedMaterial,plaster,paint);
                if(finish!=null) renderer.sharedMaterial=finish;
            }
        }
        public static Material Replacement(Material source,Material plaster,Material paint)
        {
            if(source==null) return null;
            string name=source.name;
            if(name=="Paint" || name.StartsWith("Detailed paint:") || name=="Namsan_Paint") return paint;
            if(name=="Plaster" || name=="Wall" || name.StartsWith("Detailed plaster:") || name=="Namsan_Plaster") return plaster;
            return null; // Do not replace user-imported or renamed custom finishes.
        }
    }
}
