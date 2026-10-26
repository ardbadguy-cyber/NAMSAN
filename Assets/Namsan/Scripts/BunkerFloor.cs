using UnityEngine;
namespace Namsan
{
    // Saved marker for floor-specific lighting and material heights.
    public class BunkerFloor : MonoBehaviour
    {
        public const float StoreyHeight=4.2f;
        [Range(1,4)] public int number=1;
    }
}
