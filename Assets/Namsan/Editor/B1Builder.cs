using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace Namsan
{
    public static class B1Builder
    {
        static Transform root;
        static Material wall, floor, steel, wood, white;
        [MenuItem("NAMSAN/Create B1 Floor")]
        public static void Create() { CreateLayout(false); }

        [MenuItem("NAMSAN/Create B101 Atmosphere")]
        public static void CreateAtmosphere() { CreateLayout(true); }

        static void CreateLayout(bool atmosphere)
        {
            if(EditorApplication.isPlaying) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            string folder="Assets/Namsan/Generated";
            if(!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Namsan","Generated");
            // Reuse stored materials on repeated generation.
            wall=MaterialAt(folder+"/Wall.mat",new Color(.43f,.49f,.40f));
            floor=MaterialAt(folder+"/Floor.mat",new Color(.25f,.26f,.24f));
            steel=MaterialAt(folder+"/Steel.mat",new Color(.16f,.20f,.19f));
            wood=MaterialAt(folder+"/Wood.mat",new Color(.26f,.16f,.09f));
            white=MaterialAt(folder+"/Sign.mat",new Color(.85f,.82f,.68f));
            root=new GameObject("B1 - ten rooms and central corridor").transform;
            // Plan view: x left to right, +z is the upper row. Corridor width 3m.
            Box("Floor",new Vector3(0,-.15f,0),new Vector3(31,.3f,15),floor);
            Box("Ceiling",new Vector3(0,3.25f,0),new Vector3(31,.25f,15),wall);
            foreach(int side in new[]{-1,1})
            {
                Box("Outer wall",new Vector3(0,1.5f,side*7.5f),new Vector3(31,3,.2f),wall);
                for(int i=0;i<=5;i++) Box("Room partition",new Vector3(-15+i*6,1.5f,side*4.5f),new Vector3(.2f,3,6),wall);
                for(int i=0;i<5;i++)
                {
                    float x=-12+i*6; string room="B"+(side==1?110-i:105-i);
                    var group=new GameObject(room); group.transform.SetParent(root);
                    // 1.4m doorway centered on each 6m facade.
                    Box(room+" front left",new Vector3(x-1.85f,1.5f,side*1.5f),new Vector3(2.3f,3,.2f),wall);
                    Box(room+" front right",new Vector3(x+1.85f,1.5f,side*1.5f),new Vector3(2.3f,3,.2f),wall);
                    Box(room+" lintel",new Vector3(x,2.7f,side*1.5f),new Vector3(1.4f,.6f,.2f),wall);
                    var door=Box(room+" door",new Vector3(x,1.2f,side*1.5f),new Vector3(1.3f,2.4f,.12f),steel);
                    door.AddComponent<B1Door>(); door.transform.SetParent(group.transform,true);
                    Sign(room,new Vector3(x,2.65f,side*1.36f),side==1?0:180);
                    Lamp(new Vector3(x,2.85f,side*4.4f),1.7f,7);
                    Table(room+" desk",new Vector3(x,0,side*4.8f));
                    Chair(new Vector3(x,0,side*3.7f));
                    if(room=="B101") Chair(new Vector3(x,0,side*5.8f));
                }
            }
            Box("Right corridor end",new Vector3(15,1.5f,0),new Vector3(.2f,3,3),wall);
            for(int i=0;i<5;i++)
            {
                float x=-12+i*6;
                Box("Fluorescent fixture",new Vector3(x,3,0),new Vector3(1.2f,.12f,.3f),white);
                Lamp(new Vector3(x,2.85f,0),2,5);
            }
            // Left stair entrance only: B2 is deliberately outside this build.
            Box("Stair landing",new Vector3(-17,-.15f,0),new Vector3(4,.3f,3),floor);
            Box("Landing ceiling",new Vector3(-17,3.25f,0),new Vector3(4,.25f,3),wall);
            foreach(int side in new[]{-1,1}) Box("Landing side",new Vector3(-17,1.5f,side*1.5f),new Vector3(4,3,.2f),wall);
            Box("B2 access closed",new Vector3(-19,1.5f,0),new Vector3(.2f,3,3),steel);
            Sign("STAIRS / B2\nNOT BUILT",new Vector3(-18.85f,1.8f,0),-90);
            Lamp(new Vector3(-17,2.8f,0),1.5f,5);
            RenderSettings.ambientMode=AmbientMode.Flat; RenderSettings.ambientLight=new Color(.12f,.13f,.12f);
            var player=new GameObject("Player - starts inside B101"); player.transform.position=new Vector3(12,.05f,-3);
            var controller=player.AddComponent<CharacterController>(); controller.height=1.8f; controller.radius=.28f; controller.center=new Vector3(0,.9f,0); controller.stepOffset=.25f;
            var cameraObject=new GameObject("Main Camera"); cameraObject.tag="MainCamera"; cameraObject.transform.SetParent(player.transform,false); cameraObject.transform.localPosition=new Vector3(0,1.65f,0);
            var camera=cameraObject.AddComponent<Camera>(); camera.nearClipPlane=.05f; camera.farClipPlane=120; cameraObject.AddComponent<AudioListener>();
            player.AddComponent<B1Explorer>().eye=camera;
            if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
            if(atmosphere) B101Atmosphere.Apply(root);
            string path=AssetDatabase.GenerateUniqueAssetPath(atmosphere ? "Assets/Scenes/B101_Atmosphere.unity" : "Assets/Scenes/B1_Layout.unity");
            EditorSceneManager.SaveScene(scene,path); AssetDatabase.SaveAssets();
            Selection.activeGameObject=player; SceneView.lastActiveSceneView?.Frame(new Bounds(Vector3.zero,new Vector3(38,5,16)),false);
            EditorUtility.DisplayDialog("NAMSAN B1",atmosphere ? "B101 atmosphere study created. Press Play. E opens doors." : "B1 created. Press Play. E opens doors. Other floors are not included.","OK");
        }
        static Material MaterialAt(string path,Color color)
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m!=null) return m;
            m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=color; AssetDatabase.CreateAsset(m,path); return m;
        }
        static GameObject Box(string name,Vector3 pos,Vector3 size,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(root); obj.transform.position=pos; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=material; return obj;
        }
        static void Sign(string text,Vector3 pos,float yaw)
        {
            var obj=new GameObject("Sign "+text); obj.transform.SetParent(root); obj.transform.position=pos; obj.transform.rotation=Quaternion.Euler(0,yaw,0);
            var label=obj.AddComponent<TextMesh>(); label.text=text; label.anchor=TextAnchor.MiddleCenter; label.alignment=TextAlignment.Center; label.characterSize=.08f; label.fontSize=48; label.color=new Color(.9f,.85f,.65f);
        }
        static void Lamp(Vector3 pos,float intensity,float range)
        {
            var obj=new GameObject("Ceiling light"); obj.transform.SetParent(root); obj.transform.position=pos;
            var light=obj.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(.92f,.93f,.79f); light.range=range; light.intensity=intensity; light.shadows=LightShadows.None;
        }
        static void Table(string name,Vector3 pos)
        {
            Box(name,pos+Vector3.up*.78f,new Vector3(1.6f,.12f,.8f),wood);
            foreach(float x in new[]{-.65f,.65f}) Box("Desk leg",pos+new Vector3(x,.36f,0),new Vector3(.08f,.72f,.65f),steel);
        }
        static void Chair(Vector3 pos)
        {
            Box("Chair seat",pos+Vector3.up*.45f,new Vector3(.5f,.1f,.5f),wood);
            Box("Chair back",pos+new Vector3(0,.85f,.22f),new Vector3(.5f,.7f,.08f),wood);
            foreach(float x in new[]{-.2f,.2f}) foreach(float z in new[]{-.2f,.2f}) Box("Chair leg",pos+new Vector3(x,.2f,z),new Vector3(.06f,.4f,.06f),steel);
        }
    }
}
