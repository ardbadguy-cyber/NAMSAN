using System.Collections.Generic;
using UnityEngine;

namespace Namsan
{
    // Runtime surface upgrade for existing scenes; saved materials and scene edits stay untouched.
    public class B1WallFinish : MonoBehaviour
    {
        readonly List<Object> resources=new List<Object>();
        readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        Texture2D plasterColor, plasterNormal, paintColor, paintNormal;

        public void Apply()
        {
            if(plasterColor!=null) return;
            MakeSurface(false,out plasterColor,out plasterNormal);
            MakeSurface(true,out paintColor,out paintNormal);
            foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                var source=renderer.sharedMaterial;
                if(source==null) continue;
                bool paint=source.name=="Paint";
                if(!paint && source.name!="Plaster") continue;
                // Use physical dimensions rather than stretching one texture across a whole wall.
                Vector3 size=renderer.transform.lossyScale;
                float width=Mathf.Max(Mathf.Abs(size.x),Mathf.Abs(size.z));
                float height=Mathf.Abs(size.y);
                if(height<.4f) { width=Mathf.Abs(size.x); height=Mathf.Abs(size.z); }
                Vector2 tiling=new Vector2(Mathf.Max(.1f,width/2),Mathf.Max(.1f,height/2));
                string key=(paint?"paint":"plaster")+":"+tiling.x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+":"+tiling.y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture);
                if(!materials.TryGetValue(key,out var material))
                {
                    material=new Material(source); material.name="Detailed "+key;
                    material.color=Color.white; material.mainTexture=paint?paintColor:plasterColor;
                    material.mainTextureScale=tiling;
                    material.SetTexture("_BumpMap",paint?paintNormal:plasterNormal);
                    material.SetFloat("_BumpScale",paint?.32f:.45f);
                    material.EnableKeyword("_NORMALMAP");
                    material.SetFloat("_Smoothness",paint?.22f:.06f);
                    material.SetFloat("_Metallic",0);
                    materials.Add(key,material); resources.Add(material);
                }
                renderer.sharedMaterial=material;
            }
        }
        void MakeSurface(bool paint,out Texture2D color,out Texture2D normal)
        {
            const int n=512;
            var colors=new Color[n*n]; var heights=new float[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++)
            {
                float u=(float)x/n, v=(float)y/n;
                // Periodic sampling keeps adjacent tiles seamless.
                float broad=Noise(u,v,3,17);
                float grain=Noise(u,v,90,31);
                float medium=Noise(u,v,19,57);
                float cracks=Noise(u,v,7,93);
                float crack=1-Mathf.SmoothStep(.003f,.012f,Mathf.Abs(cracks-.5f));
                float chip=paint?Mathf.SmoothStep(.58f,.68f,medium*.65f+broad*.35f):0;
                float streaks=Noise(u,v,35,131,2);
                float damp=Mathf.SmoothStep(.40f,.80f,broad)*.6f+Mathf.SmoothStep(.46f,.72f,streaks)*.4f;
                Color baseColor=paint?new Color(.29f,.31f,.27f):new Color(.60f,.58f,.51f);
                Color exposed=new Color(.48f,.46f,.39f);
                Color c=Color.Lerp(baseColor,exposed,chip);
                c*=1-damp*.36f-crack*.22f;
                c*=.94f+grain*.12f;
                colors[y*n+x]=new Color(c.r,c.g,c.b,1);
                heights[y*n+x]=grain*.12f+medium*.07f-chip*.18f-crack*.13f;
            }
            color=new Texture2D(n,n,TextureFormat.RGBA32,true,false); color.name=paint?"Weathered green paint":"Aged mineral plaster";
            color.wrapMode=TextureWrapMode.Repeat; color.anisoLevel=4; color.SetPixels(colors); color.Apply(true,false);
            var normals=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++)
            {
                float dx=heights[y*n+(x+1)%n]-heights[y*n+(x+n-1)%n];
                float dy=heights[((y+1)%n)*n+x]-heights[((y+n-1)%n)*n+x];
                Vector3 direction=new Vector3(-dx*5,-dy*5,1).normalized;
                normals[y*n+x]=new Color(direction.x*.5f+.5f,direction.y*.5f+.5f,direction.z*.5f+.5f,1);
            }
            normal=new Texture2D(n,n,TextureFormat.RGBA32,true,true); normal.name="Fine surface relief";
            normal.wrapMode=TextureWrapMode.Repeat; normal.anisoLevel=4; normal.SetPixels(normals); normal.Apply(true,false);
            resources.Add(color); resources.Add(normal);
        }
        static float Noise(float u,float v,float frequency,float seed,float verticalFrequency=-1)
        {
            float fy=verticalFrequency>0?verticalFrequency:frequency;
            float a=Mathf.PerlinNoise(u*frequency+seed,v*fy+seed);
            float b=Mathf.PerlinNoise((u-1)*frequency+seed,v*fy+seed);
            float c=Mathf.PerlinNoise(u*frequency+seed,(v-1)*fy+seed);
            float d=Mathf.PerlinNoise((u-1)*frequency+seed,(v-1)*fy+seed);
            float sx=u*u*(3-2*u), sy=v*v*(3-2*v);
            return Mathf.Lerp(Mathf.Lerp(a,b,sx),Mathf.Lerp(c,d,sx),sy);
        }
        void OnDestroy() { foreach(var resource in resources) if(resource!=null) Destroy(resource); }
    }
}
