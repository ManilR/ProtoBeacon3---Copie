using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class BeaconAuthoring : MonoBehaviour
    {
        [Header("Light Data")]
        [SerializeField, Range(1f, 2000f)] private float m_maxLight;
        [SerializeField, Range(-1f, 2000f)] private float m_startLight;

        class Baker : Baker<BeaconAuthoring>
        {
            public override void Bake(BeaconAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Beacon {
                    MAX_LIGHT_LEVEL = authoring.m_maxLight,
                    lightLevel = authoring.m_startLight
                });
                AddComponent(entity, new Ally { });
                AddComponent(entity, new UpdateLight { });
                SetComponentEnabled<UpdateLight>(entity, false);

                AddBuffer<LightModifBuffer>(entity);

                AddComponent(entity, new Dead { });
                SetComponentEnabled<Dead>(entity, false);

                AddBuffer<DamageBufferElement>(entity);
            }
        }
    }

    public struct Beacon : IComponentData
    {
        public float MAX_LIGHT_LEVEL;
        public float lightLevel;
    }

    public struct LightModifBuffer : IBufferElementData
    {
        public float Value;
    }

    public struct UpdateLight : IComponentData, IEnableableComponent
    {
    }
}