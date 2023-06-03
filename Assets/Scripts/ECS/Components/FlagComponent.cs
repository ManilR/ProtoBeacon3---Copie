using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Beacon
{
    public class FlagAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject m_building;

        class Baker : Baker<FlagAuthoring>
        {
            
            public override void Bake(FlagAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Flag
                {
                    building = GetEntity(authoring.m_building, TransformUsageFlags.Dynamic),
                    position = new float3(0, 0, 0)
                });

                AddComponent<ActiveFlag>(entity);
                SetComponentEnabled<ActiveFlag>(entity, false);

            }
        }
    }

    public struct Flag : IComponentData
    {
        public Entity building;
        public float3 position;
    }
    public struct ActiveFlag : IComponentData, IEnableableComponent
    {
        
    }
}