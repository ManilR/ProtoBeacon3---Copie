using Unity.Burst;
using Unity.Entities;

namespace Beacon
{
    [UpdateAfter(typeof(TargetingSystem))]
    [BurstCompile]
    public partial struct DamageSystem : ISystem
    {
        [BurstCompile] public void OnCreate(ref SystemState state) {}

        [BurstCompile] public void OnDestroy(ref SystemState state) {}

        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? true : !GameManager.instance.isPlaying)
            //     return;

            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var job = new DamageJob
            {
                Delta = deltaTime,
                ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            };
            job.ScheduleParallelByRef();
        }
    }

    [BurstCompile]
    public partial struct DamageJob : IJobEntity
    {
        public float Delta;
        public EntityCommandBuffer.ParallelWriter ECB;
        

        [BurstCompile] void Execute(DamageAspect damage, [EntityIndexInQuery]int sortKey)
        {
            damage.DealDamage(Delta, ECB, sortKey);
        }
    }
}