using System.IO;
using UnityEditor;
using UnityEngine;

namespace Namsan
{
    // Fictional period-inspired geometry; the supplied photograph is a shape reference only.
    public static class B101Atmosphere
    {
        const string Folder="Assets/Namsan/Generated/Atmosphere";
        static Transform root;
        static Material plaster, green, metal, rust, wood, paper, black, tile, glow;
        public static void Apply(Transform sceneRoot)
        {
            if(!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Namsan/Generated","Atmosphere");
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
            root=new GameObject("B1 full floor atmosphere").transform; root.SetParent(sceneRoot);
            // These helpers use B101-local coordinates with x=12 as their origin.
            root.position=new Vector3(12,0,0);
            foreach(var renderer in sceneRoot.GetComponentsInChildren<MeshRenderer>())
            {
                var t=renderer.transform; string name=t.name;
                if(name.Contains("wall") || name.Contains("partition") || name.Contains("front") || name.Contains("lintel") || name.Contains("ceiling") || name=="Ceiling")
                {
                    renderer.sharedMaterial=plaster;
                    if(t.lossyScale.y>2.8f)
                    {
                        var size=t.lossyScale; size.y=1.22f; size.x+=.014f; size.z+=.014f;
                        Box("Lower painted wall",new Vector3(t.position.x,.61f,t.position.z),size,green);
                        size.y=.045f; size.x+=.008f; size.z+=.008f;
                        Box("Paint division trim",new Vector3(t.position.x,1.24f,t.position.z),size,metal);
                    }
                }
                if(name=="Floor" || name=="Stair landing") renderer.sharedMaterial=tile;
                if(name.Contains("desk") || name.StartsWith("Chair")) renderer.sharedMaterial=wood;
            }
            // Replace all ten prototype leaves with true openings and hinged leaves.
            foreach(var oldDoor in sceneRoot.GetComponentsInChildren<B1Door>()) Object.DestroyImmediate(oldDoor.gameObject);
            foreach(var light in sceneRoot.GetComponentsInChildren<Light>())
            {
                light.color=new Color(1,.87f,.64f); light.intensity=Mathf.Abs(light.transform.position.z)>2?1.6f:1.25f;
                light.shadows=light.transform.position.x>9 && light.transform.position.z< -2?LightShadows.Soft:LightShadows.None;
            }
            // Remove the old solid light boxes so they don't cover the new tube fixtures.
            foreach(var t in sceneRoot.GetComponentsInChildren<Transform>())
                if(t.name=="Fluorescent fixture") Object.DestroyImmediate(t.gameObject);
            Transform overall=root;
            foreach(int side in new[]{-1,1}) for(int i=0;i<5;i++)
            {
                float x=-12+6*i; string room="B"+(side==1?110-i:105-i);
                var group=new GameObject(room+" atmosphere"); group.transform.SetParent(overall);
                group.transform.position=new Vector3(x,0,0); group.transform.rotation=Quaternion.Euler(0,side==1?180:0,0); root=group.transform;
                Room(room);
                // Label orientation and position are in world space so both corridor sides read correctly.
                var sign=GameObject.Find("Sign "+room);
                if(sign!=null)
                {
                    sign.transform.position=new Vector3(x,2.65f,side*1.325f);
                    sign.transform.rotation=Quaternion.Euler(0,side==1?0:180,0);
                    var text=sign.GetComponent<TextMesh>(); text.characterSize=.045f;
                }
            }
            root=overall;
            // Consistent corridor pipes, brackets, fixtures, and floor throughout the floor.
            foreach(float z in new[]{-.95f,-.72f})
            {
                Pipe(new Vector3(-14.8f,2.85f,z),new Vector3(14.8f,2.85f,z),.035f,metal);
                for(int x=-14;x<=14;x+=2) Box("Pipe bracket",new Vector3(x,2.85f,z),new Vector3(.055f,.17f,.17f),rust);
            }
            for(int i=0;i<5;i++) Fixture(new Vector3(-12+6*i,2.99f,0));
            RenderSettings.ambientLight=new Color(.075f,.08f,.065f);
        }
        static void Room(string room)
        {
            // Coordinates below are canonical B101; room root handles translation and the opposite row.
            for(int i=0;i<12;i++)
            {
                Box("Floor joint",new Vector3(9.15f+i*.5f,.006f,-4.5f),new Vector3(.008f,.012f,5.8f),metal);
                Box("Floor joint",new Vector3(12,.006f,-7.35f+i*.5f),new Vector3(5.8f,.012f,.008f),metal);
            }
            Box("Back skirting",new Vector3(12,.08f,-7.38f),new Vector3(5.8f,.16f,.06f),metal);
            foreach(float x in new[]{9.12f,14.88f}) Box("Side skirting",new Vector3(x,.08f,-4.5f),new Vector3(.06f,.16f,5.8f),metal);
            Pipe(new Vector3(9.3f,2.85f,-7.3f),new Vector3(9.3f,2.85f,-1.8f),.045f,rust);
            Pipe(new Vector3(9.3f,2.85f,-1.8f),new Vector3(14.7f,2.85f,-1.8f),.045f,rust);
            Box("Vent frame",new Vector3(13.5f,2.35f,-7.35f),new Vector3(.8f,.45f,.08f),metal);
            for(int i=0;i<7;i++) Box("Vent slot",new Vector3(13.5f,2.18f+i*.055f,-7.29f),new Vector3(.65f,.019f,.025f),black);
            Cabinet(new Vector3(9.65f,0,-6.7f)); DeskProps(); Clock(); Fixture(new Vector3(12,2.99f,-4.4f));
            Door(room);
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
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=label; obj.transform.SetParent(root,false); obj.transform.localPosition=p-new Vector3(12,0,0); obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=mat;
            Object.DestroyImmediate(obj.GetComponent<Collider>()); return obj;
        }
        static void Pipe(Vector3 a,Vector3 b,float radius,Material mat)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name="Pipe or round fitting"; obj.transform.SetParent(root,false); obj.transform.localPosition=(a+b)*.5f-new Vector3(12,0,0); obj.transform.localRotation=Quaternion.FromToRotation(Vector3.up,(b-a).normalized); obj.transform.localScale=new Vector3(radius*2,Vector3.Distance(a,b)/2,radius*2); obj.GetComponent<Renderer>().sharedMaterial=mat; Object.DestroyImmediate(obj.GetComponent<Collider>());
        }
        static void Door(string room)
        {
            var roomRoot=root;
            foreach(float x in new[]{11.29f,12.71f}) Box("Steel door jamb",new Vector3(x,1.22f,-1.5f),new Vector3(.075f,2.44f,.28f),metal);
            Box("Steel header",new Vector3(12,2.45f,-1.5f),new Vector3(1.5f,.10f,.28f),metal);
            Box("Number plaque",new Vector3(12,2.65f,-1.35f),new Vector3(.72f,.30f,.035f),black);
            var pivot=new GameObject(room+" hinged iron door"); pivot.transform.SetParent(root,false); pivot.transform.localPosition=new Vector3(-.65f,0,-1.5f);
            pivot.AddComponent<B1Door>().hinged=true;
            // In leaf space x=0 is the hinge. Preserve the true barred aperture: no solid panel behind it.
            root=pivot.transform;
            Panel("Lower iron plate",new Vector3(.65f,.76f,0),new Vector3(1.3f,1.52f,.10f));
            Panel("Upper iron plate",new Vector3(.65f,2.17f,0),new Vector3(1.3f,.46f,.10f));
            Panel("Left window cheek",new Vector3(.165f,1.73f,0),new Vector3(.33f,.42f,.10f));
            Panel("Right window cheek",new Vector3(1.135f,1.73f,0),new Vector3(.33f,.42f,.10f));
            foreach(float z in new[]{-.065f,.065f})
            {
                Leaf("Window frame top",new Vector3(.65f,1.96f,z),new Vector3(.76f,.065f,.035f),metal);
                Leaf("Window frame bottom",new Vector3(.65f,1.50f,z),new Vector3(.76f,.065f,.035f),metal);
                foreach(float x in new[]{.29f,1.01f}) Leaf("Window frame side",new Vector3(x,1.73f,z),new Vector3(.065f,.46f,.035f),metal);
                // Bolt carriage, keepers and pull match the photograph's silhouette.
                Leaf("Bolt carriage",new Vector3(.85f,.81f,z*1.5f),new Vector3(.64f,.10f,.08f),rust);
                Leaf("Bolt bar",new Vector3(.85f,.85f,z*2),new Vector3(.61f,.045f,.045f),metal);
                foreach(float x in new[]{.61f,.83f,1.09f}) Leaf("Bolt keeper",new Vector3(x,.84f,z*2),new Vector3(.055f,.12f,.08f),metal);
                Leaf("Bolt grip",new Vector3(.59f,.74f,z*2.2f),new Vector3(.035f,.19f,.035f),metal);
                Leaf("Pull backplate",new Vector3(1.11f,.60f,z*1.3f),new Vector3(.09f,.28f,.025f),rust);
                Leaf("Iron pull",new Vector3(1.11f,.60f,z*2),new Vector3(.035f,.19f,.045f),black);
                // Raised rivets across edges and the viewing-window frame.
                for(int i=0;i<9;i++) foreach(float x in new[]{.07f,1.23f}) Rivet(new Vector3(x,.15f+i*.26f,z));
                for(int i=0;i<6;i++) foreach(float y in new[]{.08f,2.32f}) Rivet(new Vector3(.15f+i*.20f,y,z));
                foreach(float x in new[]{.29f,.65f,1.01f}) foreach(float y in new[]{1.5f,1.96f}) Rivet(new Vector3(x,y,z*1.5f));
            }
            for(int i=0;i<4;i++)
            {
                var bar=Leaf("Window vertical bar",new Vector3(.42f+i*.155f,1.73f,0),new Vector3(.035f,.42f,.045f),metal);
                bar.AddComponent<BoxCollider>();
            }
            foreach(float y in new[]{.25f,1.2f,2.15f}) Leaf("Hinge",new Vector3(.025f,y,0),new Vector3(.09f,.18f,.16f),rust);
            root=roomRoot;
        }
        static GameObject Leaf(string name,Vector3 pos,Vector3 size,Material material)
        {
            return Box(name,pos+new Vector3(12,0,0),size,material);
        }
        static void Panel(string name,Vector3 pos,Vector3 size)
        {
            Leaf(name,pos,size,metal).AddComponent<BoxCollider>();
        }
        static void Rivet(Vector3 pos)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Sphere); obj.name="Iron rivet"; obj.transform.SetParent(root,false); obj.transform.localPosition=pos; obj.transform.localScale=new Vector3(.023f,.023f,.012f); obj.GetComponent<Renderer>().sharedMaterial=metal; Object.DestroyImmediate(obj.GetComponent<Collider>());
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
