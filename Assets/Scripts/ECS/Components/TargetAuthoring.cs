using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace Beacon
{
    public class TargetAuthoring : MonoBehaviour
    {
        [SerializeField, Range(1f, 200f)] private float m_range;
        class Baker : Baker<TargetAuthoring>
        {
            public override void Bake(TargetAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Target {
                    range = authoring.m_range,
                    isInRange = false
                });
            }
        }
    }

    public struct Target : IComponentData
    {
        public bool emptyTarget;
        public Entity Value;
        public float3 Position;
        public float range;
        public bool isTargetBeacon;
        public bool isInRange;
    }
}