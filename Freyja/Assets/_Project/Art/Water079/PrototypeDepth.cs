using UnityEngine;

namespace Freyja.Environment
{
    [RequireComponent(typeof(Camera))]
    public sealed class PrototypeDepth : MonoBehaviour
    {
        private void OnEnable() => GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth | DepthTextureMode.DepthNormals;
    }
}
