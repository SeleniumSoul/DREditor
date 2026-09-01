using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DREditor.Core
{
    [CreateAssetMenu(fileName = "New Area Data", menuName = "DREditor/AreaData")]
    public class AreaData : ScriptableObject
    {
        [SerializeField] private string areaName;
        [SerializeField] private AreaState areaType;
        [SerializeField] private AssetReference sceneReference;

        public string AreaName => areaName;
        public AreaState AreaType => areaType;
        public AssetReference SceneReference => sceneReference;
    }
}
