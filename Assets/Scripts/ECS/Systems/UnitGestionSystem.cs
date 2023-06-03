using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Beacon
{

    [BurstCompile] public partial struct UnitGestionSystem : ISystem
    {
        //EntityQuery soldierQueryDefBuff;

        //EntityQuery soldierQueryResetBuff;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //var builder = new EntityQueryBuilder(Allocator.Temp);
            //builder.WithAll<Soldier, DefBuffTag>();
            //soldierQueryDefBuff = state.EntityManager.CreateEntityQuery(builder);

            //builder = new EntityQueryBuilder(Allocator.Temp);
            //builder.WithAll<Soldier, CancelBuffTag>();
            //soldierQueryResetBuff = state.EntityManager.CreateEntityQuery(builder);
        }
        [BurstCompile] public void OnDestroy(ref SystemState state) {}

        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? false : GameManager.instance.isPlaying)
            // {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var job = new UnitCleanJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job.ScheduleParallelByRef();

            var job2 = new DefBuffJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job2.ScheduleParallelByRef();

            var job3 = new CancelBuffJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job3.ScheduleParallelByRef();
            // }
        }
    }

    [BurstCompile] public partial struct UnitCleanJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        void Execute(UnitAspect unit, Dead dead, [EntityIndexInQuery] int sortKey)
        {
            unit.CleanUnit(ECB, sortKey);
        }
    }

    [BurstCompile]
    public partial struct DefBuffJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        void Execute(UnitAspect unit, DefBuffTag tag, [EntityIndexInQuery] int sortKey)
        {
            unit.AddDefBuff(ECB, sortKey);
        }
    }
    [BurstCompile]
    public partial struct CancelBuffJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        void Execute(UnitAspect unit, CancelBuffTag tag, [EntityIndexInQuery] int sortKey)
        {
            unit.ResetBuff(ECB, sortKey);
        }
    }
}