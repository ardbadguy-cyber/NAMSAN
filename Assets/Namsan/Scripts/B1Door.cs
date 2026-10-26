using UnityEngine;
namespace Namsan
{
    public class B1Door : MonoBehaviour
    {
        public bool hinged;
        bool open;
        Quaternion closedRotation;
        void Awake() { closedRotation=transform.localRotation; }
        public void Toggle()
        {
            open=!open;
            // Older blockout scenes retain their original sliding behavior.
            if(!hinged) transform.localPosition+=Vector3.right*(open?1.55f:-1.55f);
        }
        void Update()
        {
            if(!hinged) return;
            var target=closedRotation*Quaternion.Euler(0,open?100:0,0);
            transform.localRotation=Quaternion.RotateTowards(transform.localRotation,target,120*Time.deltaTime);
        }
    }
}
