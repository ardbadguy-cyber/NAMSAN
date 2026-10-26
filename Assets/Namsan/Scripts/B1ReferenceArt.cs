using System.Collections.Generic;
using UnityEngine;

namespace Namsan
{
    // Additive reference-inspired set dressing; saved scenes and source materials are untouched.
    public class B1ReferenceArt : MonoBehaviour
    {
        readonly List<Object> owned=new List<Object>();
        Transform root;
        Material iron, concrete, enamel, leather, lightSurface, wet;
        public void Apply()
        {
            if(root!=null || GameObject.Find("B1 - ten rooms and central corridor")==null) return;
            root=new GameObject("Reference art - runtime dressing").transform;
            iron=Mat(new Color(.12f,.14f,.13f),.65f,.3f);
            concrete=Mat(new Color(.37f,.36f,.31f),0,.15f);
            enamel=Mat(new Color(.19f,.22f,.20f),.35f,.33f);
            leather=Mat(new Color(.14f,.10f,.065f),0,.3f);
            wet=Mat(new Color(.075f,.085f,.07f),.15f,.7f);
            lightSurface=Mat(new Color(.8f,.76f,.61f),0,.2f);
            lightSurface.EnableKeyword("_EMISSION"); lightSurface.SetColor("_EmissionColor",new Color(1,.9f,.7f)*1.5f);
            UpgradeFloor();
            // Keep room layout and desks; replace only the generated chair appearance and room fixtures.
            var renderers=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
            foreach(var renderer in renderers)
            {
                if(renderer.name.StartsWith("Chair"))
                {
                    renderer.enabled=false;
                    var collider=renderer.GetComponent<Collider>(); if(collider!=null) collider.enabled=false;
                    if(renderer.name=="Chair seat") Chair(renderer.transform.position-Vector3.up*.45f);
                }
                if(renderer.name=="Aged fluorescent housing" || renderer.name=="Fluorescent tube")
                    if(Mathf.Abs(renderer.transform.position.z)>2) renderer.enabled=false;
            }
            foreach(var lamp in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(lamp.name!="Ceiling light") continue;
                if(Mathf.Abs(lamp.transform.position.z)>2) lamp.enabled=false;
                else { lamp.intensity=1.65f; lamp.color=new Color(.89f,.92f,.83f); }
            }
            // Large overhead duct, collared every 2 metres, offset to leave the centre lights clear.
            Pipe(new Vector3(-14.7f,2.73f,.72f),new Vector3(14.7f,2.73f,.72f),.19f,iron);
            for(int i=0;i<15;i++)
            {
                float x=-14+i*2;
                Pipe(new Vector3(x-.025f,2.73f,.72f),new Vector3(x+.025f,2.73f,.72f),.205f,enamel);
                Box("Duct suspension",new Vector3(x,2.99f,.72f),new Vector3(.035f,.36f,.035f),iron);
            }
            foreach(int side in new[]{-1,1}) for(int i=0;i<5;i++)
            {
                float x=-12+i*6;
                // Surface wiring stays alongside doors, never across their openings.
                float z=side*1.33f;
                Pipe(new Vector3(x+1.25f,.65f,z),new Vector3(x+1.25f,2.8f,z),.019f,iron);
                Box("Wall electrical junction box",new Vector3(x+1.25f,1.62f,z),new Vector3(.26f,.36f,.13f),enamel);
                Box("Junction cover",new Vector3(x+1.25f,1.62f,z-side*.075f),new Vector3(.21f,.30f,.025f),iron);
                foreach(float y in new[]{.85f,2.15f,2.65f}) Box("Conduit clamp",new Vector3(x+1.25f,y,z),new Vector3(.075f,.035f,.08f),enamel);
                Pendant(new Vector3(x,2.72f,side*4.4f),i==4 && side==-1);
                Basin(new Vector3(x-2.32f,0,side*4.5f));
            }
        }
        Material Mat(Color color,float metallic,float smooth)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            const int n=128; var pixels=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++)
            {
                float grain=Mathf.PerlinNoise(x*.7f+11,y*.7f+31);
                float patch=Mathf.PerlinNoise(x*.055f+73,y*.055f+13);
                float scrape=Mathf.PerlinNoise(x*.6f+43,y*.035f+17);
                Color worn=metallic>.3f?new Color(.23f,.17f,.10f):color*1.23f;
                Color c=Color.Lerp(color,worn,Mathf.SmoothStep(.58f,.78f,patch)*.6f);
                pixels[y*n+x]=c*(.83f+grain*.20f+Mathf.SmoothStep(.6f,.8f,scrape)*.16f);
            }
            var texture=new Texture2D(n,n,TextureFormat.RGBA32,true); texture.SetPixels(pixels); texture.Apply(); texture.wrapMode=TextureWrapMode.Repeat; owned.Add(texture);
            m.color=Color.white; m.mainTexture=texture; m.SetFloat("_Metallic",metallic); m.SetFloat("_Smoothness",smooth); owned.Add(m); return m;
        }
        GameObject Box(string name,Vector3 p,Vector3 size,Material m,bool solid=false)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube); obj.name=name; obj.transform.SetParent(root); obj.transform.position=p; obj.transform.localScale=size; obj.GetComponent<Renderer>().sharedMaterial=m;
            if(!solid) { var c=obj.GetComponent<Collider>(); c.enabled=false; Destroy(c); } return obj;
        }
        void Pipe(Vector3 a,Vector3 b,float radius,Material m)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cylinder); obj.name="Utility pipe"; obj.transform.SetParent(root); obj.transform.position=(a+b)/2; obj.transform.up=(b-a).normalized; obj.transform.localScale=new Vector3(radius*2,Vector3.Distance(a,b)/2,radius*2); obj.GetComponent<Renderer>().sharedMaterial=m;
            var c=obj.GetComponent<Collider>(); c.enabled=false; Destroy(c);
        }
        void Chair(Vector3 p)
        {
            Box("Worn upholstered seat",p+Vector3.up*.46f,new Vector3(.49f,.07f,.48f),leather,true);
            Box("Worn upholstered back",p+new Vector3(0,.86f,.23f),new Vector3(.49f,.42f,.055f),leather);
            foreach(float x in new[]{-.23f,.23f})
            {
                Pipe(p+new Vector3(x,.04f,-.20f),p+new Vector3(x,.46f,-.20f),.018f,iron);
                Pipe(p+new Vector3(x,.04f,.22f),p+new Vector3(x,1.07f,.22f),.018f,iron);
                Pipe(p+new Vector3(x,.42f,-.20f),p+new Vector3(x,.42f,.22f),.018f,iron);
            }
        }
        void Basin(Vector3 p)
        {
            Box("Concrete basin plinth",p+Vector3.up*.34f,new Vector3(.85f,.68f,1.5f),concrete,true);
            Box("Basin dark interior",p+Vector3.up*.74f,new Vector3(.68f,.05f,1.3f),wet);
            foreach(float x in new[]{-.4f,.4f}) Box("Concrete basin rim",p+new Vector3(x,.82f,0),new Vector3(.10f,.30f,1.5f),concrete,true);
            foreach(float z in new[]{-.7f,.7f}) Box("Concrete basin rim",p+new Vector3(0,.82f,z),new Vector3(.85f,.30f,.10f),concrete,true);
            Vector3 tap=p+new Vector3(-.30f,1.2f,0);
            Pipe(tap+Vector3.left*.20f,tap+Vector3.right*.15f,.025f,iron);
            Pipe(tap+Vector3.right*.15f,tap+new Vector3(.15f,-.1f,0),.025f,iron);
            Pipe(tap+new Vector3(-.12f,.06f,0),tap+new Vector3(-.12f,.14f,0),.013f,iron);
            Box("Tap handle",tap+new Vector3(-.12f,.14f,0),new Vector3(.10f,.02f,.025f),iron);
        }
        void Pendant(Vector3 p,bool shadows)
        {
            Pipe(new Vector3(p.x,3.08f,p.z),p+Vector3.up*.21f,.012f,iron);
            // Open metal shade, generated as a lathed mesh rather than a solid cone.
            const int count=24; var vertices=new Vector3[count*3]; var triangles=new int[count*12];
            float[] radii={.065f,.22f,.32f}; float[] ys={.20f,.10f,0};
            for(int ring=0;ring<3;ring++) for(int i=0;i<count;i++)
            { float a=i*Mathf.PI*2/count; vertices[ring*count+i]=new Vector3(Mathf.Cos(a)*radii[ring],ys[ring],Mathf.Sin(a)*radii[ring]); }
            int index=0;
            for(int ring=0;ring<2;ring++) for(int i=0;i<count;i++)
            {
                int a=ring*count+i,b=ring*count+(i+1)%count,c=a+count,d=b+count;
                triangles[index++]=a; triangles[index++]=b; triangles[index++]=c; triangles[index++]=b; triangles[index++]=d; triangles[index++]=c;
            }
            var mesh=new Mesh(); mesh.name="Metal pendant shade"; mesh.vertices=vertices; mesh.triangles=triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds(); owned.Add(mesh);
            var obj=new GameObject("Pendant shade"); obj.transform.SetParent(root); obj.transform.position=p; obj.AddComponent<MeshFilter>().sharedMesh=mesh; obj.AddComponent<MeshRenderer>().sharedMaterial=iron;
            Pipe(p+Vector3.down*.015f,p+Vector3.down*.005f,.27f,lightSurface);
            var lamp=new GameObject("Warm interrogation pendant"); lamp.transform.SetParent(root); lamp.transform.position=p+Vector3.down*.12f; lamp.transform.rotation=Quaternion.Euler(90,0,0);
            var light=lamp.AddComponent<Light>(); light.type=LightType.Spot; light.spotAngle=115; light.innerSpotAngle=65; light.range=8; light.intensity=4; light.color=new Color(1,.86f,.65f); light.shadows=shadows?LightShadows.Soft:LightShadows.None;
        }
        void UpgradeFloor()
        {
            const int n=256; var pixels=new Color[n*n]; var mask=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++)
            {
                float noise=Mathf.PerlinNoise(x*.06f+17,y*.06f+29);
                bool seam=x<2 || y<2;
                Color c=Color.Lerp(new Color(.21f,.22f,.19f),new Color(.37f,.35f,.28f),noise);
                pixels[y*n+x]=seam?c*.45f:c;
                float polish=seam?.08f:Mathf.Lerp(.18f,.70f,Mathf.SmoothStep(.36f,.72f,noise));
                mask[y*n+x]=new Color(0,0,0,polish);
            }
            var texture=new Texture2D(n,n,TextureFormat.RGBA32,true); texture.SetPixels(pixels); texture.Apply(); texture.wrapMode=TextureWrapMode.Repeat; texture.anisoLevel=4; owned.Add(texture);
            var gloss=new Texture2D(n,n,TextureFormat.RGBA32,true,true); gloss.SetPixels(mask); gloss.Apply(); gloss.wrapMode=TextureWrapMode.Repeat; owned.Add(gloss);
            foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(r.name!="Floor" && r.name!="Stair landing" && r.name!="B101 worn floor" && r.name!="Corridor worn floor") continue;
                var m=new Material(r.sharedMaterial); m.name="Worn tile with patchy damp sheen"; m.color=Color.white; m.mainTexture=texture;
                var size=r.transform.lossyScale; m.mainTextureScale=new Vector2(Mathf.Abs(size.x)/.65f,Mathf.Abs(size.z)/.65f);
                m.SetTexture("_MetallicGlossMap",gloss); m.EnableKeyword("_METALLICSPECGLOSSMAP"); m.SetFloat("_Smoothness",.85f); owned.Add(m); r.sharedMaterial=m;
            }
            // A local probe supports dull environmental sheen, not mirror-perfect reflections.
            var probeObject=new GameObject("Corridor local reflection"); probeObject.transform.SetParent(root); probeObject.transform.position=new Vector3(0,1.5f,0);
            var probe=probeObject.AddComponent<ReflectionProbe>(); probe.mode=UnityEngine.Rendering.ReflectionProbeMode.Realtime; probe.refreshMode=UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting; probe.size=new Vector3(32,5,4); probe.resolution=64; probe.timeSlicingMode=UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.AllFacesAtOnce; StartCoroutine(CaptureProbe(probe));
        }
        System.Collections.IEnumerator CaptureProbe(ReflectionProbe probe)
        {
            yield return null;
            if(probe!=null) probe.RenderProbe();
        }
        void OnDestroy()
        {
            if(root!=null) Destroy(root.gameObject);
            foreach(var item in owned) if(item!=null) Destroy(item);
        }
    }
}
