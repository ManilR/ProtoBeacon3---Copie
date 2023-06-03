using Unity.Entities;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct UnitAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<UnitTag> m_unit;
        private readonly RefRW<Health> m_health;

        //private readonly RefRO<Damage> m_damage;

        //private readonly RefRO<Target> m_target;
        //private float range => m_target.ValueRO.range;
        private float resistance
        {
            get => m_health.ValueRO.damageResistance;
            set => m_health.ValueRW.damageResistance = value;
        }



        [BurstCompile] public void CleanUnit(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            ecb.DestroyEntity(sortKey, Entity);
        }
        [BurstCompile] public void AddDefBuff(EntityCommandBuffer.ParallelWriter ecb, int sortKey) 
        {
            resistance *= 2;

            ecb.SetComponentEnabled<DefBuffTag>(sortKey, Entity, false);
        }

        [BurstCompile] public void ResetBuff(EntityCommandBuffer.ParallelWriter ecb, int sortKey) 
        {
            resistance = 1;

            ecb.SetComponentEnabled<CancelBuffTag>(sortKey, Entity, false);
        }
    }
}

