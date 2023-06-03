using Unity.Entities;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct HealthAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRW<Health> m_health;
        private readonly DynamicBuffer<DamageBufferElement> damageBuffer;

        private float health
        {
            get => m_health.ValueRO.health;
            set => m_health.ValueRW.health = value;
        }

        private float resistance => m_health.ValueRO.damageResistance;

        [BurstCompile] public void ApplyDamage(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            foreach (var bufferElement in damageBuffer)
                health -= (bufferElement.Value /resistance);

            damageBuffer.Clear();

            if (health <= 0)
            {
                ecb.SetComponentEnabled<Dead>(sortKey, Entity, true);
                //ecb.DestroyEntity(sortKey, Entity); // A RETIRER POUR METTER UN DISABLE DE RENDERER
                //ecb.SetComponent<Dead>(sortKey, Entity, new Dead { });
            }
        }
    }
}

