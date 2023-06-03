using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AI;

namespace Beacon
{
    [BurstCompile]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial struct MovementSystem : ISystem
    {
        EntityQuery soldierQuery;
        EntityQuery agentQuery;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Soldier>();
            soldierQuery = state.EntityManager.CreateEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<NavigationSpeedDataComponent, Movement>();
            agentQuery = state.EntityManager.CreateEntityQuery(builder);
        }
        [BurstCompile] public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? false : GameManager.instance.isPlaying)
            // {

            //GameStateComponent gameState;
            //if (SystemAPI.TryGetSingleton<GameStateComponent>(out gameState) && gameState.isPlaying)
            //{



            NativeArray<Entity> soldierEntities = soldierQuery.ToEntityArray(Allocator.Temp);
            foreach (Entity e in soldierEntities)
            {

                Soldier soldierComp = state.EntityManager.GetComponentData<Soldier>(e);
                if (state.EntityManager.Exists(soldierComp.nextUnit))
                {
                    float4 pos = state.EntityManager.GetComponentData<LocalToWorld>(soldierComp.nextUnit).Value.c3;
                    soldierComp.nextUnitPos = new float3(pos.x, pos.y, pos.z);
                    soldierComp.nextUnitDir = state.EntityManager.GetComponentData<Soldier>(soldierComp.nextUnit).currentDir;
                    state.EntityManager.SetComponentData<Soldier>(e, soldierComp);

                }
                //AgentBody agentB = state.EntityManager.GetComponentData<AgentBody>(e);
                //agentB.Destination = new float3(1, 1, 80);
                //agentB.IsStopped = false;
                //state.EntityManager.SetComponentData<AgentBody>(e, agentB);


            }

            var deltaTime = SystemAPI.Time.DeltaTime;
            var job = new MovementEnemyJob { Delta = deltaTime };
            job.ScheduleParallelByRef();


            //var random = Random.CreateFromIndex(1234);



            var job2 = new MovementSoldierJob { Delta = deltaTime };
            JobHandle test = new JobHandle();
            JobHandle handle = job2.ScheduleParallelByRef(test);
            handle.Complete();

            var endSimulationBufferSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var commandBuffer = endSimulationBufferSystem.CreateCommandBuffer(state.WorldUnmanaged);
            var agentEntities = agentQuery.ToEntityArray(Allocator.TempJob);
            var agentMovementData = agentQuery.ToComponentDataArray<Movement>(Allocator.TempJob);

            for (var i = 0; i < agentEntities.Length; i++)
            {
                var agentEntity = agentEntities[i];
                var pos = agentMovementData[i].targetPosition;
                
            }


            //var job3 = new NavMeshJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            //job3.ScheduleParallelByRef();
            // }
            //}
        }
    }



    [BurstCompile]
    public partial struct MovementEnemyJob : IJobEntity
    {
        public float Delta;


        void Execute(ref LocalTransform transform, Enemy enemy, in Movement movement, ref Target target)
        {

            if (!movement.isFixed && target.Value != Entity.Null)
            {
                float distance = math.distance(new float2(target.Position.x, target.Position.z), new float2(transform.Position.x, transform.Position.z));
                if (target.isTargetBeacon)
                    distance /= 2;

                if (distance > target.range)
                {
                    float3 direction = math.normalize(target.Position - transform.Position);
                    transform.Position.xz += direction.xz * Delta * movement.Speed;
                    target.isInRange = false;
                }
                else
                {
                    target.isInRange = true;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct MovementSoldierJob : IJobEntity
    {
        public float Delta;
        //public Random random;

        [BurstCompile]
        void Execute(ref LocalTransform transform, ref Movement movement, ref Target target, ref PatrolTimer timer, ref Soldier soldier,ref AgentBody agentBody ,in PatrolRandom random)
        {
            float2 soldierPos = new float2(transform.Position.x, transform.Position.z);
            float2 targetPos = new float2(target.Position.x, target.Position.z);
            float2 rallyPos = new float2(soldier.rallyPoint.x, soldier.rallyPoint.z);
            float2 movTargPos = new float2(movement.targetPosition.x, movement.targetPosition.z);

            float distanceTargetRally = math.distance(targetPos, rallyPos); //DISTANCE CENTRE/ENEMY : SI L'ENEMI EST DANS LA ZONE
            float distanceTargetSoldier = math.distance(targetPos, soldierPos); //DISTANCE SOLDIER/ENEMY : SI L'ENEMI EST DANS LA RANGE D'ATTAQUE
            float distanceRallySoldier = math.distance(soldierPos, rallyPos); //DISTANCE CENTRE/SOLDIER : SI LE SOLDIER EST DANS LA ZONE

            float speedModifier = 1f;

            if (!movement.isFixed && distanceTargetRally < soldier.TargetRange && !target.emptyTarget) // GO ON TARGET
            {
                if (distanceTargetSoldier > target.range)
                {
                    movement.targetPosition = target.Position;
                    //movement.curDir = new float2(0f, 0f);
                    //movement.Value * Delta;
                }
            }
            #region PATROL
            else //PATROL
            {
                if ((movement.targetPosition.x == 0 && movement.targetPosition.z == 0) || (movement.targetPosition.x == rallyPos.x && movement.targetPosition.z == rallyPos.y))
                {

                    movement.targetPosition = soldier.defaultTarget;
                }
                if (math.distance(soldierPos, movTargPos) < 0.01f || timer.value > 5 || (movTargPos.x == rallyPos.x && movTargPos.y == rallyPos.y && distanceRallySoldier < soldier.rallyRadius))
                {

                    if ((movement.curDir.x == 1 && movement.curDir.y == 1) || (movement.curDir.x == 1 && movement.curDir.y == 0))
                    {
                        movement.curDir = new float2(0f, -1f);
                        movement.targetPosition = new float3(rallyPos.x + soldier.rallyRadius, 0, rallyPos.y - soldier.rallyRadius);
                        soldier.currentDir = SoldierDir.down;
                    }
                    else if (movement.curDir.x == 0 && movement.curDir.y == -1)
                    {
                        movement.curDir = new float2(-1f, 0f);
                        movement.targetPosition = new float3(rallyPos.x - soldier.rallyRadius, 0, rallyPos.y - soldier.rallyRadius);
                        soldier.currentDir = SoldierDir.left;
                    }
                    else if (movement.curDir.x == -1 && movement.curDir.y == 0)
                    {
                        movement.curDir = new float2(0f, 1f);
                        movement.targetPosition = new float3(rallyPos.x - soldier.rallyRadius, 0, rallyPos.y + soldier.rallyRadius);
                        soldier.currentDir = SoldierDir.up;
                    }
                    else if (movement.curDir.x == 0 && movement.curDir.y == 1)
                    {
                        movement.curDir = new float2(1f, 0f);
                        movement.targetPosition = new float3(rallyPos.x + soldier.rallyRadius, 0, rallyPos.y + soldier.rallyRadius);
                        soldier.currentDir = SoldierDir.right;
                    }


                    timer.value = 0;
                }
                else
                {
                    timer.value += Delta;
                }
                if (distanceRallySoldier > soldier.rallyRadius)
                {
                    movement.targetPosition = new float3(rallyPos.x, 0, rallyPos.y);
                    //movement.curDir = new float2(0f, 0f);
                }
                //SPEED CALCUL : modify speed to organize patrol
                if (soldier.nbSoldierInUnit > 1)
                {
                    float distanceTotal = 4 * math.sqrt(2) * soldier.rallyRadius;
                    float distanceInterSoldier = distanceTotal / soldier.nbSoldierInUnit;
                    #region CALCUL NEXT UNIT DISTANCE
                    float distanceNextUnit = 0;
                    if (soldier.currentDir == soldier.nextUnitDir)
                    {
                        distanceNextUnit = math.distance(transform.Position, soldier.nextUnitPos);
                    }
                    else
                    {
                        int nbSides = numberSidesDistance(soldier.currentDir, soldier.nextUnitDir);
                        distanceNextUnit += (nbSides - 1) * distanceTotal / 4;
                        distanceNextUnit += distanceUnitNextCorner(soldierPos, rallyPos, soldier.rallyRadius, soldier.currentDir);
                        distanceNextUnit += distanceUnitPreviousCorner(new float2(soldier.nextUnitPos.x, soldier.nextUnitPos.z), rallyPos, soldier.rallyRadius, soldier.nextUnitDir);
                    }

                    #endregion
                    if (distanceNextUnit > distanceInterSoldier + 1)
                    {
                        speedModifier *= 2;
                    }
                    else if (distanceNextUnit < distanceInterSoldier - 1)
                    {
                        speedModifier *= 0.5f;
                    }
                }

            }
            #endregion

            if (distanceTargetSoldier > target.range)
                target.isInRange = false;
            else
                target.isInRange = true;


            //movement
            //agentBody.IsStopped = !(!movement.isFixed || !target.isInRange);
            agentBody.SetDestination(movement.targetPosition);
            //agentBody.Destination = movement.targetPosition;
            
            //if (!movement.isFixed && !target.isInRange)
            //{
            //    //float3 direction = math.normalize(movement.targetPosition - transform.Position);
            //    //transform.Position.xz += direction.xz * Delta * movement.Speed * speedModifier;

            //}
        }

        int numberSidesDistance(SoldierDir dirA, SoldierDir dirB)
        {
            switch (dirA)
            {
                case SoldierDir.up:
                    switch (dirB)
                    {
                        case SoldierDir.right:
                            return 1;
                        case SoldierDir.down:
                            return 2;
                        case SoldierDir.left:
                            return 3;
                    }
                    break;
                case SoldierDir.down:
                    switch (dirB)
                    {
                        case SoldierDir.left:
                            return 1;
                        case SoldierDir.up:
                            return 2;
                        case SoldierDir.right:
                            return 3;
                    }
                    break;
                case SoldierDir.left:
                    switch (dirB)
                    {
                        case SoldierDir.up:
                            return 1;
                        case SoldierDir.right:
                            return 2;
                        case SoldierDir.down:
                            return 3;
                    }
                    break;
                case SoldierDir.right:
                    switch (dirB)
                    {
                        case SoldierDir.down:
                            return 1;
                        case SoldierDir.left:
                            return 2;
                        case SoldierDir.up:
                            return 3;
                    }
                    break;
            }
            return 0;
        }

        float distanceUnitNextCorner(float2 soldierPos, float2 rallyPoint, float radius, SoldierDir soldierDir)
        {
            float2 cornerPos = rallyPoint;
            switch (soldierDir)
            {
                case SoldierDir.up:
                    cornerPos += new float2(-radius, +radius);
                    break;
                case SoldierDir.right:
                    cornerPos += new float2(+radius, +radius);
                    break;
                case SoldierDir.down:
                    cornerPos += new float2(+radius, -radius);
                    break;
                case SoldierDir.left:
                    cornerPos += new float2(-radius, -radius);
                    break;
            }
            return math.distance(soldierPos, cornerPos);
        }
        float distanceUnitPreviousCorner(float2 soldierPos, float2 rallyPoint, float radius, SoldierDir soldierDir)
        {
            float2 cornerPos = rallyPoint;
            switch (soldierDir)
            {
                case SoldierDir.up:
                    cornerPos += new float2(-radius, -radius);
                    break;
                case SoldierDir.right:
                    cornerPos += new float2(-radius, +radius);
                    break;
                case SoldierDir.down:
                    cornerPos += new float2(+radius, +radius);
                    break;
                case SoldierDir.left:
                    cornerPos += new float2(+radius, -radius);
                    break;
            }
            return math.distance(soldierPos, cornerPos);


        }
    }

    //[BurstCompile]
    //public partial struct NavMeshJob : IJobEntity
    //{
    //    public EntityCommandBuffer.ParallelWriter ECB;
    //    void Execute(ref UnitAspect unit)
    //    {

    //    }
    //}
}