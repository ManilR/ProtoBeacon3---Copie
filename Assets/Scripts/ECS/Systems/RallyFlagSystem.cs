using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Beacon
{
    [BurstCompile]
    public partial struct RallyFlagSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }
        [BurstCompile] public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? false : GameManager.instance.isPlaying)
            // {
                
            var job = new FlagJob { };
            job.ScheduleParallelByRef();
            // }
        }
    }

    [BurstCompile]
    public partial struct FlagJob : IJobEntity
    {

        void Execute(ref LocalToWorld transform, Flag flag)//, in ActiveFlag active)
        {
            transform.Value.c3 = new float4( flag.position, 1);
        }
    }

    
}