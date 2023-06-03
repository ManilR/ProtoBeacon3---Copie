using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Beacon
{
    public partial struct BeaconSystem : ISystem
    {
        private Entity lightHUD;
        private Entity beaconEntity;
        private Beacon beacon;
        private EntityQuery beaconQuery;
        private EntityQuery lightHUDQuery;
        private bool lightInited;

        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<Beacon>();
            beaconQuery = state.EntityManager.CreateEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LightLevelHUD>();
            lightHUDQuery = state.EntityManager.CreateEntityQuery(builder);

            lightInited = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? true : !GameManager.instance.isPlaying)
            //     return;

            if (!lightInited)
                lightInited = lightHUDQuery.TryGetSingletonEntity<LightLevelHUD>(out lightHUD);

            if (state.EntityManager.Exists(lightHUD) && beaconQuery.TryGetSingleton<Beacon>(out beacon))
            {
                if (beaconEntity == Entity.Null)
                    beaconQuery.TryGetSingletonEntity<Beacon>(out beaconEntity);

                if (beaconEntity != Entity.Null)
                {
                    if (state.EntityManager.HasComponent<UpdateLight>(beaconEntity))
                    {
                        TextMesh textMesh = state.EntityManager.GetComponentObject<TextMesh>(lightHUD);
                        EventManager.Instance.Raise(new LightLevelChangedEvent()
                        {
                            lightLevel = beacon.lightLevel
                        });
                        textMesh.text = beacon.lightLevel.ToString();
                        state.EntityManager.SetComponentEnabled<UpdateLight>(beaconEntity, false);
                    }
                }
            }

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var job = new UpdateLightJob { ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() };
            job.ScheduleParallelByRef();
        }


    }
    public partial struct UpdateLightJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute(BeaconAspect beacon, [EntityIndexInQuery] int sortKey)
        {
            beacon.UpdateLightLevel(ECB, sortKey);
        }
    }




}