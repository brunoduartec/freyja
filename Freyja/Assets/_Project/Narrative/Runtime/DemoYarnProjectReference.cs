using UnityEngine;
using Yarn.Unity;

namespace Freyja.Narrative
{
    [CreateAssetMenu(menuName = "Freyja/Narrative/Demo Yarn Project Reference")]
    public sealed class DemoYarnProjectReference : ScriptableObject
    {
        [SerializeField] private YarnProject project;
        public YarnProject Project => project;
        public void SetProject(YarnProject value) => project = value;
    }
}
