using Unity.Entities;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct BeaconAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRW<Beacon> m_beacon;
        private readonly DynamicBuffer<LightModifBuffer> lightModifBuffer;
        private readonly DynamicBuffer<DamageBufferElement> damageBuffer;


        private float MAX_LIGHT => m_beacon.ValueRO.MAX_LIGHT_LEVEL;
        private float lightLevel
        {
            get => m_beacon.ValueRO.lightLevel;
            set => m_beacon.ValueRW.lightLevel = value;
        }



        [BurstCompile] public void UpdateLightLevel(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if(lightModifBuffer.Capacity > 0) 
                ecb.SetComponentEnabled<UpdateLight>(sortKey, Entity, true);

            foreach (var bufferElement in lightModifBuffer)
                lightLevel += bufferElement.Value;

            

            lightModifBuffer.Clear();
        }

        [BurstCompile]
        public void ApplyLightDamage(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if (damageBuffer.Capacity > 0)
                ecb.SetComponentEnabled<UpdateLight>(sortKey, Entity, true);

            foreach (var bufferElement in damageBuffer)
                lightLevel -= bufferElement.Value;

            damageBuffer.Clear();
        }
    }
}

