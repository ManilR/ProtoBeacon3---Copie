using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class WaveAuthoring : MonoBehaviour
    {

        [SerializeField] private GameObject unitNormalPrefab;
        [SerializeField] private GameObject unitSpeedyPrefab;
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private TargetingSystem.SpatialPartitioningType spatialPartitioning;

        class Baker : Baker<WaveAuthoring>
        {
            public override void Bake(WaveAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Wave {
                    unitNormal = GetEntity(authoring.unitNormalPrefab, TransformUsageFlags.Dynamic),
                    unitSpeedy = GetEntity(authoring.unitSpeedyPrefab, TransformUsageFlags.Dynamic),
                    boss = GetEntity(authoring.bossPrefab, TransformUsageFlags.Dynamic),
                    SpatialPartitioning = authoring.spatialPartitioning
                });
            }
        }
    }

    public struct Wave : IComponentData
    {
        public Entity unitNormal;
        public Entity unitSpeedy;
        public Entity boss;
        public TargetingSystem.SpatialPartitioningType SpatialPartitioning;
    }
}