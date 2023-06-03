using Unity.Burst;
using Unity.Entities;

namespace Beacon
{
    [BurstCompile]
    //[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    [UpdateAfter(typeof(DamageSystem))]
    public partial struct ApplyDamageSystem : ISystem
    {
        [BurstCompile] public void OnCreate(ref SystemState state) {}
        [BurstCompile] public void OnDestroy(ref SystemState state) {}

        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? true : !GameManager.instance.isPlaying)
            //     return;

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var job = new ApplyDamageJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job.ScheduleParallelByRef();

            var job2 = new ApplyLightDamageJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job2.ScheduleParallelByRef();
        }
    }

    [BurstCompile]
    public partial struct ApplyDamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        
        [BurstCompile] void Execute(HealthAspect health, [EntityIndexInQuery] int sortKey)
        {
            health.ApplyDamage(ECB, sortKey);
        }
    }

    [BurstCompile]
    public partial struct ApplyLightDamageJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        void Execute(BeaconAspect beacon, [EntityIndexInQuery] int sortKey)
        {
            beacon.ApplyLightDamage(ECB, sortKey);
        }
    }
}