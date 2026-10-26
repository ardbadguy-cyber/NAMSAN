using UnityEngine;
using UnityEngine.InputSystem;

namespace Namsan
{
    // A disposable runtime blockout: no assets or existing scenes are modified.
    public class B401Prototype : MonoBehaviour
    {
        CharacterController player;
        Camera view;
        Light flame;
        GameObject lighter, pickup, ghost, door;
        Material concrete, metal, dark;
        bool owned, lit, opened;
        float pitch, vertical;
        string prompt = "";

        void Start()
        {
            concrete = Mat(new Color(.32f, .35f, .31f));
            metal = Mat(new Color(.17f, .19f, .18f));
            dark = Mat(new Color(.035f, .03f, .03f));
            // Room: x -3..3, z -4..4. Corridor: z 4..16.
            Box("Room floor", new Vector3(0,-.1f,0), new Vector3(6,.2f,8), concrete);
            Box("Ceiling", new Vector3(0,3.1f,0), new Vector3(6,.2f,8), concrete);
            Box("Back wall", new Vector3(0,1.5f,-4), new Vector3(6,3,.2f), concrete);
            foreach (int side in new[]{-1,1})
            {
                Box("Room wall", new Vector3(side*3,1.5f,0), new Vector3(.2f,3,8), concrete);
                Box("Door surround", new Vector3(side*2,1.5f,4), new Vector3(2,3,.2f), concrete);
                Box("Corridor wall", new Vector3(side*1.5f,1.5f,10), new Vector3(.2f,3,12), concrete);
            }
            Box("Lintel",new Vector3(0,2.75f,4),new Vector3(2,.5f,.2f),concrete);
            door = Box("Door",new Vector3(0,1.25f,4),new Vector3(1.9f,2.5f,.15f),metal);
            Box("Corridor floor",new Vector3(0,-.1f,10),new Vector3(3,.2f,12),concrete);
            Box("Corridor ceiling",new Vector3(0,3.1f,10),new Vector3(3,.2f,12),concrete);
            Box("End wall",new Vector3(0,1.5f,16),new Vector3(3,3,.2f),metal);
            Box("Desk",new Vector3(0,.75f,0),new Vector3(1.8f,.15f,.8f),metal);
            foreach (float x in new[]{-.75f,.75f})
                Box("Desk leg",new Vector3(x,.35f,0),new Vector3(.08f,.7f,.65f),metal);
            pickup=Box("Lighter pickup",new Vector3(0,.9f,0),new Vector3(.09f,.14f,.035f),Mat(new Color(.65f,.6f,.42f)));
            Lamp(new Vector3(0,2.6f,0),1.4f,7);
            Lamp(new Vector3(0,2.6f,9),.45f,6);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.045f,.05f,.055f);
            var body=new GameObject("Player"); body.transform.position=new Vector3(0,.05f,-2.6f);
            player=body.AddComponent<CharacterController>(); player.height=1.8f; player.radius=.28f; player.center=new Vector3(0,.9f,0);
            var eye=new GameObject("Player Camera"); eye.transform.SetParent(body.transform,false); eye.transform.localPosition=new Vector3(0,1.65f,0);
            view=eye.AddComponent<Camera>(); view.nearClipPlane=.05f; eye.AddComponent<AudioListener>();
            lighter=Box("Held lighter",Vector3.zero,new Vector3(.055f,.1f,.025f),metal);
            lighter.transform.SetParent(eye.transform,false); lighter.transform.localPosition=new Vector3(.22f,-.2f,.4f);
            var lightObject=new GameObject("Lighter flame"); lightObject.transform.SetParent(lighter.transform,false); lightObject.transform.localPosition=new Vector3(0,.08f,0);
            flame=lightObject.AddComponent<Light>(); flame.color=new Color(1,.65f,.28f); flame.range=6; flame.intensity=2;
            lighter.SetActive(false);
            ghost=new GameObject("Ghost placeholder");
            var torso=Box("Silhouette",new Vector3(0,1,12),new Vector3(.55f,1.3f,.3f),dark); torso.transform.SetParent(ghost.transform,true);
            var head=GameObject.CreatePrimitive(PrimitiveType.Sphere); head.transform.position=new Vector3(0,1.85f,12); head.transform.localScale=Vector3.one*.32f; head.GetComponent<Renderer>().sharedMaterial=dark; head.transform.SetParent(ghost.transform,true);
            ghost.SetActive(false);
            Lock(true);
        }

        Material Mat(Color color)
        {
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color=color; return material;
        }
        GameObject Box(string label, Vector3 position, Vector3 scale, Material material)
        {
            var item=GameObject.CreatePrimitive(PrimitiveType.Cube); item.name=label; item.transform.SetParent(transform); item.transform.position=position; item.transform.localScale=scale; item.GetComponent<Renderer>().sharedMaterial=material; return item;
        }
        void Lamp(Vector3 position,float intensity,float range)
        {
            var item=new GameObject("Ceiling light"); item.transform.SetParent(transform); item.transform.position=position;
            var light=item.AddComponent<Light>(); light.type=LightType.Point; light.color=new Color(.8f,.87f,.78f); light.intensity=intensity; light.range=range;
        }
        void Lock(bool locked) { Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None; Cursor.visible=!locked; }
        void Update()
        {
            var keys=Keyboard.current; var mouse=Mouse.current;
            if(keys==null || mouse==null || player==null) return;
            if(keys.escapeKey.wasPressedThisFrame) Lock(false);
            if(Cursor.lockState!=CursorLockMode.Locked)
            {
                if(mouse.leftButton.wasPressedThisFrame) Lock(true);
                return;
            }
            Vector2 delta=mouse.delta.ReadValue()*.085f;
            player.transform.Rotate(0,delta.x,0); pitch=Mathf.Clamp(pitch-delta.y,-80,80); view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            Vector2 move=Vector2.zero;
            if(keys.wKey.isPressed) move.y++; if(keys.sKey.isPressed) move.y--;
            if(keys.dKey.isPressed) move.x++; if(keys.aKey.isPressed) move.x--;
            move=Vector2.ClampMagnitude(move,1);
            if(player.isGrounded && vertical<0) vertical=-2;
            vertical+=Physics.gravity.y*Time.deltaTime;
            player.Move((player.transform.forward*move.y*2.5f+player.transform.right*move.x*2.5f+Vector3.up*vertical)*Time.deltaTime);
            if(owned && keys.fKey.wasPressedThisFrame) { lit=!lit; flame.enabled=lit; }
            prompt="";
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,2.5f))
            {
                if(hit.collider.gameObject==pickup && !owned)
                {
                    prompt="E : Take lighter";
                    if(keys.eKey.wasPressedThisFrame) { owned=true; pickup.SetActive(false); lighter.SetActive(true); flame.enabled=false; }
                }
                if(hit.collider.gameObject==door)
                {
                    prompt=owned?"E : Open door":"Find the lighter on the desk";
                    if(owned && keys.eKey.wasPressedThisFrame) { opened=true; door.SetActive(false); }
                }
            }
            ghost.SetActive(opened && lit && Vector3.Distance(player.transform.position,new Vector3(0,0,12))>3);
        }
        void OnGUI()
        {
            GUI.Label(new Rect(20,20,700,30),"NAMSAN B4 | WASD: Move   Mouse: Look   E: Interact   F: Lighter   Esc: Release cursor");
            GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,20,25),"+");
            GUI.Label(new Rect(Screen.width/2-150,Screen.height/2+35,400,30),prompt);
            GUI.Label(new Rect(20,55,700,30),"Prototype: take the lighter, open the door, and light the corridor.");
        }
        void OnDestroy() { Lock(false); }
    }
}
