using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Namsan
{
    public static class BunkerExpansion
    {
        const float Pitch=BunkerFloor.StoreyHeight;
        static Transform parent;
        static Material concrete, steel, wall;
        [MenuItem("NAMSAN/Expand Current Scene to B1-B4")]
        static void Expand()
        {
            if(EditorApplication.isPlaying) return;
            var source=GameObject.Find("B1 - ten rooms and central corridor");
            if(source==null) { EditorUtility.DisplayDialog("NAMSAN","Open your existing B1 scene first.","OK"); return; }
            if(GameObject.Find("Bunker Expansion B2-B4")!=null)
            { EditorUtility.DisplayDialog("NAMSAN","This scene is already expanded. Nothing was changed.","OK"); return; }
            if(source.transform.position.sqrMagnitude>.001f || Quaternion.Angle(source.transform.rotation,Quaternion.identity)>.1f || (source.transform.lossyScale-Vector3.one).sqrMagnitude>.001f)
            { EditorUtility.DisplayDialog("NAMSAN","B1 root must be at the original world origin, rotation and scale. No changes made.","OK"); return; }
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Undo.IncrementCurrentGroup(); int undoGroup=Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Expand bunker to four floors");
            var container=new GameObject("Bunker Expansion B2-B4"); Undo.RegisterCreatedObjectUndo(container,"Create bunker expansion");
            parent=container.transform;
            concrete=AssetDatabase.LoadAssetAtPath<Material>("Assets/Namsan/Generated/Floor.mat");
            steel=AssetDatabase.LoadAssetAtPath<Material>("Assets/Namsan/Generated/Steel.mat");
            if(concrete==null) concrete=source.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            if(steel==null) steel=concrete;
            wall=Resources.Load<Material>("WallFinish/Namsan_Plaster");
            if(wall==null) wall=concrete;
            var marker=source.GetComponent<BunkerFloor>(); if(marker==null) marker=Undo.AddComponent<BunkerFloor>(source); marker.number=1;
            // Clone before removing B1 closures; only this known static map root is copied, never the player.
            var floors=new GameObject[4]; floors[0]=source;
            for(int level=2;level<=4;level++)
            {
                var clone=Object.Instantiate(source,parent); clone.name="B"+level+" - ten rooms and central corridor";
                clone.transform.position=Vector3.down*(level-1)*Pitch; clone.GetComponent<BunkerFloor>().number=level;
                foreach(var t in clone.GetComponentsInChildren<Transform>(true)) t.name=Renumber(t.name,level);
                foreach(var label in clone.GetComponentsInChildren<TextMesh>(true)) label.text=Renumber(label.text,level);
                floors[level-1]=clone;
                FloorMaterials(clone,level);
            }
            for(int level=1;level<=4;level++)
            {
                var floor=floors[level-1]; float y=-(level-1)*Pitch;
                RemoveClosure(floor,level==2 || level==3,level==1);
                if(level==2 || level==3) Landing(1,y,level);
                // Room numbers and floor identifiers are distinct on every floor.
                Sign("B"+level,new Vector3(0,y+2.65f,1.36f),0,.12f);
            }
            Stairs(-1,0,1); Stairs(1,-Pitch,2); Stairs(-1,-Pitch*2,3);
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(source.scene);
            BunkerLayoutAudit.ValidateScene();
            Undo.CollapseUndoOperations(undoGroup); Selection.activeGameObject=container;
            EditorUtility.DisplayDialog("NAMSAN","B2-B4 and three physical staircases added to this scene. Save (Ctrl+S), then Play. Undo restores the previous layout.","OK");
        }
        static string Renumber(string text,int level) { return Regex.Replace(text,@"B1(\d{2})",m=>"B"+level+m.Groups[1].Value); }
        static void RemoveClosure(GameObject floor,bool openRight,bool undo)
        {
            foreach(var t in floor.GetComponentsInChildren<Transform>(true))
            {
                if(t==null) continue;
                bool remove=t.name=="B2 access closed" || t.name.StartsWith("Sign STAIRS / B2");
                if(openRight)
                {
                    Vector3 local=floor.transform.InverseTransformPoint(t.position);
                    bool wallAtEnd=Mathf.Abs(local.x-15)<.03f && Mathf.Abs(local.z)<.03f && t.lossyScale.z>2.8f && t.lossyScale.x<.3f;
                    remove|=t.name=="Right corridor end" || (wallAtEnd && (t.name=="Lower painted wall" || t.name=="Paint division trim"));
                }
                if(remove) { if(undo) Undo.DestroyObjectImmediate(t.gameObject); else Object.DestroyImmediate(t.gameObject); }
            }
        }
        static void FloorMaterials(GameObject floor,int level)
        {
            const string folder="Assets/Namsan/Generated/FloorMaterials";
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Namsan/Generated","FloorMaterials");
            var plaster=FloorMaterial("Namsan_Plaster",level,folder); var paint=FloorMaterial("Namsan_Paint",level,folder);
            if(plaster==null || paint==null) return;
            foreach(var renderer in floor.GetComponentsInChildren<MeshRenderer>(true))
            {
                var replacement=B1WallFinish.Replacement(renderer.sharedMaterial,plaster,paint);
                if(replacement!=null) renderer.sharedMaterial=replacement;
            }
        }
        static Material FloorMaterial(string name,int level,string folder)
        {
            string path=folder+"/"+name+"_B"+level+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path); if(material!=null) return material;
            var source=Resources.Load<Material>("WallFinish/"+name); if(source==null) return null;
            material=new Material(source); material.name=name+"_B"+level;
            material.SetFloat("_FloorHeight",-(level-1)*Pitch); material.SetFloat("_CeilingHeight",-(level-1)*Pitch+3.1f);
            AssetDatabase.CreateAsset(material,path); return material;
        }
        static GameObject Box(string name,Vector3 position,Vector3 size,Material material,bool solid=true)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(parent); obj.transform.position=position; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid) Object.DestroyImmediate(obj.GetComponent<Collider>()); return obj;
        }
        static void Landing(int side,float y,int level)
        {
            var previousWall=wall;
            wall=FloorMaterial("Namsan_Plaster",level,"Assets/Namsan/Generated/FloorMaterials") ?? wall;
            Box("Stair landing",new Vector3(side*17,y-.15f,0),new Vector3(4,.3f,3),concrete);
            Box("Landing ceiling",new Vector3(side*17,y+3.25f,0),new Vector3(4,.25f,3),wall);
            foreach(int z in new[]{-1,1}) Box("Landing side",new Vector3(side*17,y+1.5f,z*1.6f),new Vector3(4,3,.2f),wall);
            Lamp(new Vector3(side*17,y+2.6f,0));
            wall=previousWall;
        }
        static void Stairs(int side,float top,int upper)
        {
            // Two 14-step flights: .15m risers, .30m treads, 1.3m clear lane width.
            // Top: negative-Z lane, moving outward. Bottom: positive-Z lane, moving inward.
            var stairRoot=new GameObject("Stairs B"+upper+" to B"+(upper+1)); stairRoot.transform.SetParent(parent);
            var container=parent; parent=stairRoot.transform;
            var previousWall=wall;
            wall=FloorMaterial("Namsan_Plaster",upper,"Assets/Namsan/Generated/FloorMaterials") ?? wall;
            for(int i=0;i<14;i++)
            {
                float outward=19+(i+.5f)*.3f;
                float firstY=top-(i+1)*.15f;
                Box("Descending tread "+(i+1),new Vector3(side*outward,firstY-.075f,-.8f),new Vector3(.3f,.15f,1.3f),concrete);
                float inward=23.2f-(i+.5f)*.3f;
                float secondY=top-2.1f-(i+1)*.15f;
                Box("Return tread "+(i+1),new Vector3(side*inward,secondY-.075f,.8f),new Vector3(.3f,.15f,1.3f),concrete);
                if(i%3==0)
                {
                    Post(new Vector3(side*outward,firstY,-1.48f));
                    Post(new Vector3(side*inward,secondY,1.48f));
                }
            }
            Box("Turn landing",new Vector3(side*23.9f,top-2.25f,0),new Vector3(1.4f,.3f,3.2f),concrete);
            foreach(float z in new[]{-1.57f,1.57f}) Box("Stairwell side wall",new Vector3(side*21.8f,top-.65f,z),new Vector3(5.6f,7.3f,.15f),wall);
            Box("Stairwell outer wall",new Vector3(side*24.7f,top-.65f,0),new Vector3(.2f,7.3f,3.3f),wall);
            Box("Stair flight divider",new Vector3(side*21.1f,top-.65f,0),new Vector3(4,7.3f,.10f),wall);
            Box("Stairwell roof",new Vector3(side*21.85f,top+3.05f,0),new Vector3(5.7f,.2f,3.3f),wall);
            Box("Stairwell bottom",new Vector3(side*21.85f,top-4.45f,0),new Vector3(5.7f,.2f,3.3f),wall);
            Beam("Outer handrail",new Vector3(side*19,top+1,-1.48f),new Vector3(side*23.2f,top-1.1f,-1.48f),.05f);
            Beam("Return handrail",new Vector3(side*23.2f,top-1.1f,1.48f),new Vector3(side*19,top-3.2f,1.48f),.05f);
            Beam("Middle handrail",new Vector3(side*19,top+1,-.1f),new Vector3(side*23.2f,top-1.1f,-.1f),.05f);
            Beam("Lower middle handrail",new Vector3(side*23.2f,top-1.1f,.1f),new Vector3(side*19,top-3.2f,.1f),.05f);
            // Guard the non-walkable half of each floor edge, avoiding accidental drops between flights.
            Guard(side,top,.8f); Guard(side,top-4.2f,-.8f);
            Sign("B"+upper+" > B"+(upper+1),new Vector3(side*18.7f,top+2.1f,0),side<0?-90:90,.065f);
            Sign("B"+(upper+1)+" > B"+upper,new Vector3(side*18.7f,top-2.1f,0),side<0?-90:90,.065f);
            Lamp(new Vector3(side*23.8f,top+.5f,0)); Lamp(new Vector3(side*19.8f,top+2.5f,-.8f)); Lamp(new Vector3(side*19.8f,top-1.7f,.8f));
            parent=container; wall=previousWall;
        }
        static void Guard(int side,float y,float z)
        {
            Box("Landing edge safety panel",new Vector3(side*19,y+.55f,z),new Vector3(.08f,1.1f,1.3f),steel);
        }
        static void Post(Vector3 foot) { Box("Handrail post",foot+Vector3.up*.5f,new Vector3(.045f,1,.045f),steel); }
        static void Beam(string name,Vector3 a,Vector3 b,float thickness)
        {
            var obj=Box(name,(a+b)*.5f,new Vector3(thickness,Vector3.Distance(a,b),thickness),steel); obj.transform.up=(b-a).normalized;
        }
        static void Sign(string text,Vector3 position,float yaw,float size)
        {
            var obj=new GameObject("Floor guide "+text); obj.transform.SetParent(parent); obj.transform.position=position; obj.transform.rotation=Quaternion.Euler(0,yaw,0);
            var label=obj.AddComponent<TextMesh>(); label.text=text; label.anchor=TextAnchor.MiddleCenter; label.fontSize=48; label.characterSize=size; label.color=new Color(.88f,.84f,.68f);
        }
        static void Lamp(Vector3 position)
        {
            var obj=new GameObject("Stairwell lamp"); obj.transform.SetParent(parent); obj.transform.position=position;
            var light=obj.AddComponent<Light>(); light.type=LightType.Point; light.range=5; light.intensity=1.8f; light.color=new Color(.9f,.89f,.72f); light.shadows=LightShadows.None;
        }
    }
}
