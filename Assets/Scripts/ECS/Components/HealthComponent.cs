using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class HealthAuthoring : MonoBehaviour
    {
        [SerializeField, Range(1f, 100f)] private float m_health;

        class Baker : Baker<HealthAuthoring>
        {
            public override void Bake(HealthAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Health
                {
                    maxHealth = authoring.m_health,
                    health = authoring.m_health,
                    damageResistance = 1
                });

                AddComponent<Dead>(entity);
                SetComponentEnabled<Dead>(entity, false);

                AddComponent<DefBuffTag>(entity);
                SetComponentEnabled<DefBuffTag>(entity, false);

                AddComponent<CancelBuffTag>(entity);
                SetComponentEnabled<CancelBuffTag>(entity, false);

                AddBuffer<DamageBufferElement>(entity);
            }
        }
    }

    public struct Health : IComponentData
    {
        public float maxHealth;
        public float health;
        public float damageResistance;
    }

    public struct DefBuffTag : IComponentData, IEnableableComponent
    {

    }
    public struct CancelBuffTag : IComponentData, IEnableableComponent
    {

    }
}