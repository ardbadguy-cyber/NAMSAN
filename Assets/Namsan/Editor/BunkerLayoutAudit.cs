using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Namsan
{
    public static class BunkerLayoutAudit
    {
        [MenuItem("NAMSAN/Validate Four Floor Layout")]
        public static void ValidateScene()
        {
            Physics.SyncTransforms();
            int errors=0, samples=0, floors=0;
            foreach(var marker in Object.FindObjectsByType<BunkerFloor>(FindObjectsSortMode.None))
            {
                if(marker.gameObject.scene!=EditorSceneManager.GetActiveScene()) continue;
                floors++;
                var labels=new HashSet<string>();
                foreach(var text in marker.GetComponentsInChildren<TextMesh>()) labels.Add(text.text);
                for(int room=1;room<=10;room++)
                {
                    string expected="B"+marker.number+room.ToString("D2");
                    if(!labels.Contains(expected)) { Debug.LogWarning("NAMSAN: missing room label "+expected,marker); errors++; }
                }
                float right=(marker.number==2 || marker.number==3)?18.5f:14;
                for(float x=-18.5f;x<=right;x+=.5f)
                    Check(new Vector3(x,marker.transform.position.y,0),"B"+marker.number+" corridor",ref samples,ref errors);
            }
            for(int upper=1;upper<=3;upper++)
            {
                int side=upper==2?1:-1; float top=-(upper-1)*4.2f;
                for(int i=0;i<14;i++)
                {
                    Check(new Vector3(side*(19+(i+.5f)*.3f),top-(i+1)*.15f,-.8f),"B"+upper+" first flight",ref samples,ref errors);
                    Check(new Vector3(side*(23.2f-(i+.5f)*.3f),top-2.1f-(i+1)*.15f,.8f),"B"+upper+" return flight",ref samples,ref errors);
                }
                foreach(float z in new[]{-.8f,0,.8f}) Check(new Vector3(side*23.9f,top-2.1f,z),"B"+upper+" turn landing",ref samples,ref errors);
            }
            if(floors!=4) { Debug.LogWarning("NAMSAN: expected 4 floor markers, found "+floors); errors++; }
            Debug.Log("NAMSAN geometry audit: "+floors+" floors; "+samples+" body-clearance samples; "+errors+" warnings. This is not a CharacterController traversal test.");
        }
        static void Check(Vector3 foot,string location,ref int samples,ref int errors)
        {
            samples++;
            // Skip the bottom 25cm so a nearby higher tread doesn't count as a torso obstacle.
            // This checks static torso/head clearance; actual stepping still requires Play testing.
            Vector3 a=foot+Vector3.up*.52f,b=foot+Vector3.up*1.53f;
            var hits=Physics.OverlapCapsule(a,b,.27f,~0,QueryTriggerInteraction.Ignore);
            foreach(var hit in hits)
            {
                if(hit.gameObject.scene!=EditorSceneManager.GetActiveScene()) continue;
                if(hit.GetComponentInParent<B1Explorer>()!=null) continue;
                Debug.LogWarning("NAMSAN clearance: "+location+" blocked by "+hit.name+" at "+foot,hit);
                errors++; break;
            }
        }
    }
}
