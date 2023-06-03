using Unity.Entities;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct DamageAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRW<DamageTimer> m_timer;
        private readonly RefRO<Damage> m_damage;
        private readonly RefRO<Target> m_target;


        private Entity targetEntity => m_target.ValueRO.Value;
        private float damage => m_damage.ValueRO.damage;
        private float atkPerSec => m_damage.ValueRO.atkPerSec;
        private bool isInRange => m_target.ValueRO.isInRange;
        private bool isEmpty => m_target.ValueRO.emptyTarget;
        private float Timer
        {
            get => m_timer.ValueRO.Value;
            set => m_timer.ValueRW.Value = value;
        }

        [BurstCompile] public void DealDamage(float detlaTime, EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if(isEmpty)
                return;

            if ((Timer += detlaTime) >= 1f / atkPerSec && isInRange)
            {
                Timer = 0;
                var curDamage = new DamageBufferElement { Value = damage };
                ecb.AppendToBuffer(sortKey, targetEntity, curDamage);
            }
        }
    }
}

