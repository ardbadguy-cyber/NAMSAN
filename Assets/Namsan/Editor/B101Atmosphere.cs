using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Namsan
{
    // A fictional period-inspired art study. All generated resources stay inside this project.
    public static class B101Atmosphere
    {
        const string Folder="Assets/Namsan/Generated/Atmosphere";
        static Transform root;
        static Material plaster, green, metal, rust, wood, paper, black, tile, glow;
        public static void Apply(Transform sceneRoot)
        {
            if(!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Namsan/Generated","Atmosphere");
            root=new GameObject("B101 period atmosphere - editable props").transform;
            root.SetParent(sceneRoot);
            plaster=Surface("Plaster",new Color(.55f,.53f,.43f),.88f,.35f);
            green=Surface("Paint",new Color(.22f,.29f,.23f),.7f,.24f);
            metal=Surface("Iron",new Color(.19f,.22f,.20f),.5f,.25f);
            rust=Surface("Rust",new Color(.24f,.14f,.08f),.9f,.3f);
            wood=Surface("Timber",new Color(.23f,.12f,.055f),.66f,.3f);
            paper=Surface("Paper",new Color(.72f,.66f,.49f),.95f,.12f);
            black=Surface("Bakelite",new Color(.035f,.031f,.025f),.3f,.13f);
            tile=Surface("Concrete",new Color(.30f,.30f,.26f),.85f,.3f);
            glow=Surface("Tube",new Color(.73f,.77f,.59f),.4f,.03f);
            glow.EnableKeyword("_EMISSION"); glow.SetColor("_EmissionColor",new Color(.7f,.8f,.54f)*1.7f); EditorUtility.SetDirty(glow);
            // Snapshot list: added trim must not be processed a second time.
            foreach(var r in sceneRoot.GetComponentsInChildren<MeshRenderer>())
            {
                var t=r.transform; var p=t.position;
                bool nearby=p.x>=9 && p.z<1.7f;
                if(!nearby) continue;
                string name=t.name;
                if(name.Contains("wall") || name.Contains("front") || name.Contains("partition") || name.Contains("lintel"))
                {
                    r.sharedMaterial=plaster;
                    // Overlay only broad wall faces, preserving doorway openings and lintels.
                    if(t.localScale.y>2.8f)
                    {
                        var size=t.lossyScale; size.y=1.22f; size.x+=.014f; size.z+=.014f;
                        Box("Lower painted wall",new Vector3(p.x,.61f,p.z),size,green);
                        size.y=.045f; size.x+=.008f; size.z+=.008f;
                        Box("Paint division trim",new Vector3(p.x,1.24f,p.z),size,metal);
                    }
                }
                if(name=="B101 door") DecorateDoor(t);
                if(name.Contains("B101 desk") || (name=="Desk leg" && p.x>=9 && p.z< -2)) r.sharedMaterial=wood;
                if(name.StartsWith("Chair") && p.x>=9 && p.z< -2) r.sharedMaterial=wood;
            }
            Box("B101 rear plaster",new Vector3(12,1.5f,-7.385f),new Vector3(5.8f,3,.025f),plaster);
            Box("B101 rear lower paint",new Vector3(12,.61f,-7.365f),new Vector3(5.8f,1.22f,.025f),green);
            Box("B101 rear paint trim",new Vector3(12,1.24f,-7.345f),new Vector3(5.8f,.045f,.025f),metal);
            Box("B101 worn floor",new Vector3(12,.006f,-4.5f),new Vector3(5.8f,.012f,5.8f),tile);
            Box("Corridor worn floor",new Vector3(11.9f,.006f,0),new Vector3(5.9f,.012f,2.8f),tile);
            // Thin joints give a readable small-scale floor treatment without imported assets.
            for(int i=0;i<12;i++)
                Box("Floor joint",new Vector3(9.15f+i*.5f,.013f,-4.5f),new Vector3(.008f,.002f,5.8f),metal);
            for(int i=0;i<12;i++)
                Box("Floor joint",new Vector3(12,.013f,-7.35f+i*.5f),new Vector3(5.8f,.002f,.008f),metal);
            // Room skirting, clear of the doorway.
            Box("Back skirting",new Vector3(12,.08f,-7.38f),new Vector3(5.8f,.16f,.06f),metal);
            foreach(float x in new[]{9.12f,14.88f}) Box("Side skirting",new Vector3(x,.08f,-4.5f),new Vector3(.06f,.16f,5.8f),metal);
            Pipe(new Vector3(9.3f,2.85f,-7.3f),new Vector3(9.3f,2.85f,-1.8f),.045f,rust);
            Pipe(new Vector3(9.3f,2.85f,-1.8f),new Vector3(14.7f,2.85f,-1.8f),.045f,rust);
            foreach(float z in new[]{-.95f,-.72f})
            {
                Pipe(new Vector3(9,2.85f,z),new Vector3(14.85f,2.85f,z),.035f,metal);
                foreach(float x in new[]{9.5f,11.5f,13.5f}) Box("Pipe bracket",new Vector3(x,2.85f,z),new Vector3(.055f,.17f,.17f),rust);
            }
            // Recessed-looking vent on the rear wall.
            Box("Vent frame",new Vector3(13.5f,2.35f,-7.35f),new Vector3(.8f,.45f,.08f),metal);
            for(int i=0;i<7;i++) Box("Vent slot",new Vector3(13.5f,2.18f+i*.055f,-7.29f),new Vector3(.65f,.019f,.025f),black);
            Cabinet(new Vector3(9.65f,0,-6.7f));
            DeskProps(); Clock();
            foreach(var light in sceneRoot.GetComponentsInChildren<Light>())
            {
                if(light.transform.position.x<9) continue;
                light.color=new Color(1,.87f,.64f); light.intensity=light.transform.position.z< -2?1.6f:1.25f;
                // Only the principal room light casts shadows, keeping the scene inexpensive.
                light.shadows=light.transform.position.z< -2?LightShadows.Soft:LightShadows.None;
            }
            Fixture(new Vector3(12,2.99f,-4.4f)); Fixture(new Vector3(12,2.99f,0));
            RenderSettings.ambientLight=new Color(.075f,.08f,.065f);
            // Existing camera remains in the room facing its exit.
        }
        static Material Surface(string name,Color tint,float roughness,float variation)
        {
            string path=Folder+"/"+name+".mat";
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path); if(existing!=null) return existing;
            const int n=128; var texture=new Texture2D(n,n,TextureFormat.RGB24,false);
            // Fixed offsets keep procedural weathering reproducible and avoid changing Unity's Random state.
            for(int y=0;y<n;y++) for(int x=0;x<n;x++)
            {
                float broad=Mathf.PerlinNoise(x*.045f+17,y*.045f+31);
                float grain=Mathf.PerlinNoise(x*.8f+41,y*.8f+7);
                float stain=Mathf.PerlinNoise(x*.025f+5,y*.08f+11);
                float value=1-variation*(.5f*broad+.25f*grain+.25f*stain);
                texture.SetPixel(x,y,new Color(value,value,value));
            }
            texture.Apply(); string texturePath=Folder+"/"+name+".png";
            File.WriteAllBytes(texturePath,texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(texturePath);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath); importer.wrapMode=TextureWrapMode.Repeat; importer.SaveAndReimport();
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=tint;
            m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath); m.mainTextureScale=new Vector2(3,3);
            m.SetFloat("_Smoothness",1-roughness); AssetDatabase.CreateAsset(m,path); return m;
        }
        static GameObject Box(string label,Vector3 p,Vector3 size,Material mat)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=label; obj.transform.SetParent(root); obj.transform.position=p; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=mat;
            // Small decorative parts never intercept interaction rays or snag the player.
            Object.DestroyImmediate(obj.GetComponent<Collider>()); return obj;
        }
        static void Pipe(Vector3 a,Vector3 b,float radius,Material mat)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name="Exposed pipe"; obj.transform.SetParent(root); obj.transform.position=(a+b)*.5f; obj.transform.up=(b-a).normalized; obj.transform.localScale=new Vector3(radius*2,Vector3.Distance(a,b)/2,radius*2); obj.GetComponent<Renderer>().sharedMaterial=mat; Object.DestroyImmediate(obj.GetComponent<Collider>());
        }
        static void DecorateDoor(Transform door)
        {
            door.GetComponent<Renderer>().sharedMaterial=metal;
            foreach(float z in new[]{-1.59f,-1.41f})
            {
                var panel=Box("Door inspection cover",new Vector3(12,1.65f,z),new Vector3(.43f,.22f,.045f),black); panel.transform.SetParent(door,true);
                var handle=Box("Door pull",new Vector3(12.44f,1.05f,z),new Vector3(.04f,.22f,.07f),rust); handle.transform.SetParent(door,true);
            }
            foreach(float x in new[]{11.29f,12.71f}) Box("Steel door jamb",new Vector3(x,1.22f,-1.5f),new Vector3(.075f,2.44f,.28f),metal);
            Box("Number plaque",new Vector3(12,2.65f,-1.365f),new Vector3(.65f,.30f,.045f),black);
            // Existing number label is toward the corridor; move it in front of the plaque.
            var sign=GameObject.Find("Sign B101"); if(sign!=null) { sign.transform.position=new Vector3(12,2.65f,-1.334f); sign.transform.rotation=Quaternion.Euler(0,180,0); }
        }
        static void Cabinet(Vector3 p)
        {
            var body=Box("Steel filing cabinet",p+Vector3.up*.85f,new Vector3(.8f,1.7f,.65f),metal); body.AddComponent<BoxCollider>();
            for(int i=0;i<4;i++)
            {
                float y=.24f+i*.4f;
                Box("Drawer",p+new Vector3(0,y,.34f),new Vector3(.71f,.36f,.035f),green);
                Box("Drawer handle",p+new Vector3(0,y,.38f),new Vector3(.2f,.035f,.045f),black);
                Box("Label holder",p+new Vector3(0,y+.09f,.365f),new Vector3(.15f,.06f,.012f),paper);
            }
        }
        static void DeskProps()
        {
            Box("Closed case file",new Vector3(12.35f,.862f,-4.8f),new Vector3(.35f,.045f,.45f),paper);
            for(int i=0;i<4;i++) Box("Paper sheet",new Vector3(11.8f+i*.012f,.847f+i*.003f,-4.75f),new Vector3(.21f,.002f,.29f),paper);
            Box("Telephone base",new Vector3(12.55f,.89f,-5),new Vector3(.29f,.12f,.22f),black);
            Box("Telephone receiver",new Vector3(12.55f,.99f,-5),new Vector3(.36f,.065f,.09f),black);
            foreach(float x in new[]{12.4f,12.7f}) Box("Receiver earpiece",new Vector3(x,.96f,-5),new Vector3(.08f,.07f,.14f),black);
            Pipe(new Vector3(12.55f,.955f,-4.93f),new Vector3(12.55f,.968f,-4.93f),.065f,metal);
            Box("Ashtray",new Vector3(11.45f,.86f,-4.95f),new Vector3(.14f,.035f,.14f),metal);
            Box("Pencil",new Vector3(12.03f,.85f,-4.75f),new Vector3(.012f,.012f,.2f),wood);
        }
        static void Clock()
        {
            Pipe(new Vector3(12,2.35f,-7.36f),new Vector3(12,2.35f,-7.27f),.24f,black);
            Pipe(new Vector3(12,2.35f,-7.265f),new Vector3(12,2.35f,-7.25f),.21f,paper);
            Box("Clock hour hand",new Vector3(12,2.41f,-7.23f),new Vector3(.018f,.13f,.012f),black);
            Box("Clock minute hand",new Vector3(12.08f,2.35f,-7.23f),new Vector3(.17f,.012f,.012f),black);
        }
        static void Fixture(Vector3 p)
        {
            Box("Aged fluorescent housing",p,new Vector3(1.25f,.13f,.32f),metal);
            foreach(float z in new[]{-.085f,.085f}) Box("Fluorescent tube",p+new Vector3(0,-.09f,z),new Vector3(1.08f,.035f,.035f),glow);
        }
    }
}
