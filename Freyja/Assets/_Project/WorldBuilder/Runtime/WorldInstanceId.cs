using UnityEngine;
namespace Freyja.WorldBuilder
{
    public sealed class WorldInstanceId:MonoBehaviour
    {
        public string worldId,recordId,kind;
        public int version=1;
        public bool managed=true;
    }
}
