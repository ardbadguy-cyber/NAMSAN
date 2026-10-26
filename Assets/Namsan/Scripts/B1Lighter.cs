using UnityEngine;
using UnityEngine.InputSystem;

namespace Namsan
{
    // Runtime retrofit for existing B1 scenes; no saved scene or furniture is replaced.
    public class B1Lighter : MonoBehaviour
    {
        public string Prompt { get; private set; }="";
        Camera eye;
        GameObject pickup, held, flame;
        Transform lid;
        Light flameLight;
        Material gold, dark, fire;
        AudioSource sound;
        AudioClip click;
        bool owned, open;
        float lidAngle;

        public void Initialize(Camera camera)
        {
            if(eye!=null || camera==null) return;
            eye=camera;
            gold=Material(new Color(.68f,.48f,.19f),.8f,.72f);
            dark=Material(new Color(.055f,.045f,.03f),.65f,.55f);
            fire=Material(new Color(1,.63f,.12f),0,0);
            fire.EnableKeyword("_EMISSION"); fire.SetColor("_EmissionColor",new Color(1,.36f,.045f)*3);
            pickup=new GameObject("B101 - collectible gold lighter");
            pickup.transform.position=new Vector3(11.5f,.835f,-4.55f);
            Build(pickup.transform,false);
            // A forgiving pick volume remains above the desk, without occluding other objects.
            var collider=pickup.AddComponent<BoxCollider>(); collider.center=new Vector3(0,.06f,0); collider.size=new Vector3(.15f,.14f,.09f);
            held=new GameObject("Held gold lighter"); held.transform.SetParent(eye.transform,false);
            held.transform.localPosition=new Vector3(.19f,-.19f,.38f);
            held.transform.localRotation=Quaternion.Euler(-8,-12,0);
            Build(held.transform,true); held.SetActive(false);
            sound=held.AddComponent<AudioSource>(); sound.playOnAwake=false; sound.spatialBlend=0; sound.volume=.13f;
            click=MakeClick();
        }
        Material Material(Color color,float metallic,float smoothness)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=color;
            m.SetFloat("_Metallic",metallic); m.SetFloat("_Smoothness",smoothness); return m;
        }
        void Build(Transform parent,bool functional)
        {
            Part(parent,"Gold case",new Vector3(0,.039f,0),new Vector3(.052f,.078f,.022f),gold);
            Part(parent,"Cap seam",new Vector3(0,.079f,0),new Vector3(.053f,.002f,.023f),dark);
            // Fine parallel ribs suggest an engraved luxury pocket lighter.
            for(int i=0;i<11;i++) foreach(float side in new[]{-1f,1f})
                Part(parent,"Engraved rib",new Vector3(-.023f+i*.0046f,.039f,side*.0115f),new Vector3(.0007f,.071f,.0007f),dark);
            var hinge=new GameObject("Hinged cap"); hinge.transform.SetParent(parent,false); hinge.transform.localPosition=new Vector3(-.026f,.079f,0);
            Part(hinge.transform,"Gold lid",new Vector3(.026f,.016f,0),new Vector3(.052f,.032f,.023f),gold);
            Part(parent,"Burner",new Vector3(-.006f,.084f,0),new Vector3(.012f,.012f,.011f),dark);
            Part(parent,"Ignition wheel",new Vector3(.013f,.084f,0),new Vector3(.012f,.012f,.014f),gold);
            if(!functional) return;
            lid=hinge.transform;
            flame=GameObject.CreatePrimitive(PrimitiveType.Sphere); flame.name="Lighter flame";
            flame.transform.SetParent(parent,false); flame.transform.localPosition=new Vector3(-.006f,.109f,0);
            flame.transform.localScale=new Vector3(.009f,.033f,.009f); flame.GetComponent<Renderer>().sharedMaterial=fire;
            RemoveCollider(flame); flame.SetActive(false);
            var lamp=new GameObject("Warm lighter illumination"); lamp.transform.SetParent(parent,false); lamp.transform.localPosition=new Vector3(-.006f,.12f,.025f);
            flameLight=lamp.AddComponent<Light>(); flameLight.type=LightType.Point; flameLight.color=new Color(1,.60f,.25f); flameLight.range=4; flameLight.intensity=1.5f; flameLight.shadows=LightShadows.None; flameLight.enabled=false;
        }
        void Part(Transform parent,string name,Vector3 position,Vector3 size,Material material)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(parent,false); obj.transform.localPosition=position; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=material; RemoveCollider(obj);
        }
        void RemoveCollider(GameObject obj)
        {
            var c=obj.GetComponent<Collider>(); c.enabled=false; Destroy(c);
        }
        // Called after player movement, so pickup uses this frame's camera position.
        public bool Tick(bool acceptInput)
        {
            Prompt="";
            if(eye==null) return false;
            var keys=Keyboard.current;
            bool targeted=false;
            if(acceptInput && keys!=null)
            {
                if(!owned && Physics.Raycast(eye.transform.position,eye.transform.forward,out var hit,2.1f,~0,QueryTriggerInteraction.Ignore) && hit.collider.gameObject==pickup)
                {
                    targeted=true; Prompt="E : Pick up lighter";
                    if(keys.eKey.wasPressedThisFrame)
                    {
                        owned=true; pickup.SetActive(false); held.SetActive(true); Prompt="F : Open lid / light lighter";
                    }
                }
                if(owned && keys.fKey.wasPressedThisFrame)
                {
                    open=!open;
                    sound.PlayOneShot(click,open?1f:.55f);
                }
            }
            if(owned)
            {
                lidAngle=Mathf.MoveTowards(lidAngle,open?125:0,600*Time.deltaTime);
                lid.localRotation=Quaternion.Euler(0,0,lidAngle);
                bool burning=open && lidAngle>100;
                flame.SetActive(burning); flameLight.enabled=burning;
                if(burning)
                {
                    float flicker=1+Mathf.Sin(Time.time*31)*.045f+Mathf.Sin(Time.time*47)*.025f;
                    flameLight.intensity=1.5f*flicker;
                    flame.transform.localScale=new Vector3(.009f,.033f*flicker,.009f);
                }
            }
            return targeted;
        }
        AudioClip MakeClick()
        {
            const int rate=22050; var samples=new float[rate/4];
            for(int i=0;i<samples.Length;i++)
            {
                float t=(float)i/rate;
                samples[i]=Mathf.Sin(2*Mathf.PI*2150*t)*Mathf.Exp(-t*35)*.5f+Mathf.Sin(2*Mathf.PI*3710*t)*Mathf.Exp(-t*70)*.18f;
            }
            var clip=AudioClip.Create("Synthetic metal cap click",samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
        void OnDestroy()
        {
            if(pickup!=null) Destroy(pickup);
            if(held!=null) Destroy(held);
            if(gold!=null) Destroy(gold); if(dark!=null) Destroy(dark); if(fire!=null) Destroy(fire); if(click!=null) Destroy(click);
        }
    }
}
