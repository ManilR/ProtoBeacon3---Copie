using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Beacon
{
    [BurstCompile]
    public partial struct BuildingSystem : ISystem
    {
        private Entity buildingToBuild;
        private EntityQuery buildingQuery;
        private EntityQuery buildingToRebuildQuery;
        private BufferTypeHandle<SoldierBuildingBufferElement> bufferHandle;
        ComponentTypeHandle<BuildingAspect> buildingAspectHandle;

        private ComponentLookup<Soldier> SoldierLookUp;

        private Entity beacon;
        private EntityQuery beaconQuery;

        
        [BurstCompile] public void OnCreate(ref SystemState state)
        {

            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building>();
            builder.WithNone<Dead, Destroyed>();
            buildingQuery = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Building, Destroyed>();
            builder.WithNone<Dead>();
            buildingToRebuildQuery = state.GetEntityQuery(builder);

            bufferHandle = SystemAPI.GetBufferTypeHandle<SoldierBuildingBufferElement>();

            SoldierLookUp = state.GetComponentLookup<Soldier>();

            buildingToBuild = Entity.Null;

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Beacon>();
            beaconQuery = state.EntityManager.CreateEntityQuery(builder);
        }


        [BurstCompile] public void OnDestroy(ref SystemState state) {}

        
        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? true : !GameManager.instance.isPlaying)
            //     return;

            SoldierLookUp.Update(ref state);

            var jobClean = new BuildingCleanJob
            {
                soldierLookup = SoldierLookUp
            };
            jobClean.Run();

            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var job = new BuildingDestroyJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job.ScheduleParallelByRef();

            var job2 = new BuildingSoldierJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(), delta = deltaTime };
            job2.ScheduleParallelByRef();

            if (beaconQuery.TryGetSingletonEntity<Beacon>(out beacon))
            {
                var job3 = new ReBuildJob
                {
                    ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    m_beaconEntity = beacon,
                    m_beaconComp = beaconQuery.GetSingleton<Beacon>()
                };
                job3.ScheduleParallelByRef();
            }

            var job4 = new RotateChildJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job4.ScheduleParallelByRef();

            var job5 = new RallyFlagJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job5.ScheduleParallelByRef();

            var job6 = new ChangeModeJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job6.ScheduleParallelByRef();
        }

    }
    

    [BurstCompile]
    public partial struct BuildingDestroyJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile] void Execute(BuildingAspect building, Dead dead, [EntityIndexInQuery] int sortKey)
        {
            building.DestroyBuiling(ECB, sortKey);

        }


    }

    [BurstCompile]
    public partial struct ReBuildJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public Entity m_beaconEntity;
        public Beacon m_beaconComp;

        [BurstCompile] void Execute(BuildingAspect building, InConstruction construction, [EntityIndexInQuery] int sortKey)
        {
            building.RebuildBuilding(ECB, sortKey, m_beaconEntity, m_beaconComp);
        }
    }

    [BurstCompile]
    public partial struct BuildingCleanJob : IJobEntity
    {
        public ComponentLookup<Soldier> soldierLookup;
        [BurstCompile] void Execute(DynamicBuffer<SoldierBuildingBufferElement> soldierBuffer)
        {
            for(int i = 0; i < soldierBuffer.Length; i++)
            {
                if (!soldierLookup.HasComponent(soldierBuffer[i].Value))
                    soldierBuffer.RemoveAt(i);
            }
        }
    }

    [BurstCompile]
    public partial struct ChangeModeJob : IJobEntity
    {
        public float delta;
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        void Execute(BuildingAspect building,ChangeModeTag tag ,[EntityIndexInQuery] int sortKey)
        {
            building.ChangeMode(ECB, sortKey);
        }
    }

    [BurstCompile]
    public partial struct BuildingSoldierJob : IJobEntity
    {
        public float delta;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        [BurstCompile] void Execute(BuildingAspect building, [EntityIndexInQuery] int sortKey)
        {
            building.AddSoldier(ECB,sortKey, delta);
            building.HideFlag(ECB, sortKey);
        }
    }
    [BurstCompile]
    public partial struct RotateChildJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile] void Execute(Entity e,ref LocalToWorld localTW, in ChildRotateComponent rotate, [EntityIndexInQuery] int sortKey)
        {
            LocalToWorld transform = localTW;
            
            if (!rotate.up)
            {
                transform.Value.c1.z = -50;
                transform.Value.c2.y = 50;
            }
            else
            {
                transform.Value.c1.z = 50;
                transform.Value.c2.y = -50;
            }
            localTW = transform;

            ECB.RemoveComponent<ChildRotateComponent>(sortKey,e);
        }
    }


    [BurstCompile]
    public partial struct RallyFlagJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        [BurstCompile]
        void Execute(BuildingAspect building,Clicked c ,[EntityIndexInQuery] int sortKey)
        {
            building.ChangeFlagPosition(ECB, sortKey);
        }
    }
}