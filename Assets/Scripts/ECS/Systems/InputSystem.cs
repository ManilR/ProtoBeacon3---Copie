using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEditor;
using Unity.Transforms;
using Unity.Mathematics;

namespace Beacon
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct InputSystem : ISystem
    {
        EntityQuery clickedEntitiesQuery;
        
        [BurstCompile] public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Clicked>();
            clickedEntitiesQuery = state.GetEntityQuery(builder);
        }
        
        [BurstCompile] public void OnDestroy(ref SystemState state) {}
        
        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? false : GameManager.instance.isPlaying)
            // {
            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var input in SystemAPI.Query<DynamicBuffer<PlayerClickInput>>())
            {
                foreach (var positionInput in input)
                {
                    if (physicsWorld.CastRay(positionInput.Value, out var hit))
                    {
                        var hitEntity = physicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                        if (positionInput.clicktype == Clicktype.left)
                        {
                            ecb.RemoveComponent<Clicked>(clickedEntitiesQuery);

                            
                            if (state.EntityManager.HasComponent<Clickable>(hitEntity))
                                ecb.AddComponent<Clicked>(hitEntity);
                        }
                        if (positionInput.clicktype == Clicktype.right)
                        {
                            Entity building;
                            if (SystemAPI.TryGetSingletonEntity<Clicked>(out building))
                            {

                                Building buildingData = state.EntityManager.GetComponentData<Building>(building);
                                if(!buildingData.isDestroyed && buildingData.mode == Mode.attack)
                                {
                                    if (state.EntityManager.HasComponent<Building>(hitEntity))
                                    {
                                        float4 buildingPos = state.EntityManager.GetComponentData<LocalToWorld>(hitEntity).Value.c3;
                                        buildingData.rallyPoint = new float3(buildingPos.x, buildingPos.y, buildingPos.z);
                                    }
                                    else if (!state.EntityManager.HasComponent<Beacon>(hitEntity))
                                    {
                                        //Debug.Log(hitEntity.Index.ToString());
                                        buildingData.rallyPoint = hit.Position;
                                    }
                                    


                                    state.EntityManager.SetComponentData(building, buildingData);
                                }
                                
                            }
                        }

                    }
                }
                input.Clear();
            }
            // }
        }
    }
}