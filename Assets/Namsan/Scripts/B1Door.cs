using UnityEngine;
namespace Namsan
{
    public class B1Door : MonoBehaviour
    {
        bool open;
        public void Toggle()
        {
            open=!open;
            // Slide within the facade so the leaf never blocks the corridor.
            transform.localPosition+=Vector3.right*(open?1.55f:-1.55f);
        }
    }
}
