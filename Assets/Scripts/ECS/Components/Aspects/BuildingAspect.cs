using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct BuildingAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRW<SoldierSpawnTimer> m_timer;
        private readonly RefRW<Building> m_building;
        private readonly RefRW<Health> m_health;
        private readonly DynamicBuffer<SoldierBuildingBufferElement> soldierBuffer;
        private readonly DynamicBuffer<ChildBufferElement> childrenBuffer;
        private readonly RefRW<LocalTransform> m_transform;
        private readonly RefRW<LocalToWorld> m_transformLW;

        private float3 rallyPoint {
            get => m_building.ValueRO.rallyPoint;
            set => m_building.ValueRW.rallyPoint = value;
        }
        private float Timer
        {
            get => m_timer.ValueRO.Value;
            set => m_timer.ValueRW.Value = value;
        }

        private bool isDestroyed
        {
            get => m_building.ValueRO.isDestroyed;
            set => m_building.ValueRW.isDestroyed = value;
        }

        private int nbSoldier
        {
            get => m_building.ValueRO.nbSoldier;
            set => m_building.ValueRW.nbSoldier = value;
        }

        private int nbSoldierMAX
        {
            get => m_building.ValueRO.nbSoldierMAX;
            set => m_building.ValueRW.nbSoldierMAX = value;
        }

        private float health
        {
            get => m_health.ValueRO.health;
            set => m_health.ValueRW.health = value;
        }

        private float maxHealth
        {
            get => m_health.ValueRO.maxHealth;
            set => m_health.ValueRW.maxHealth = value;
        }

        private Mode mode
        {
            get => m_building.ValueRO.mode;
            set => m_building.ValueRW.mode = value;
        }

        private int lvlAttack
        {
            get => m_building.ValueRO.lvlAttack;
            set => m_building.ValueRW.lvlAttack = value;
        }

        private int lvlDefense
        {
            get => m_building.ValueRO.lvlDefense;
            set => m_building.ValueRW.lvlDefense = value;
        }

        private int lvlProduction
        {
            get => m_building.ValueRO.lvlProduction;
            set => m_building.ValueRW.lvlProduction = value;
        }
        private Entity Flag
        {
            get => m_building.ValueRO.rallyFlag;
            set => m_building.ValueRW.rallyFlag = value;
        }

        private int production => m_building.ValueRO.production;

        private float buildPrice => m_building.ValueRO.buildingPrice;
        private float spawnTime => m_building.ValueRO.soldierSpawnTime;
        private float radius => m_building.ValueRO.rallyRadius;
        private Entity soldierPrefab => m_building.ValueRO.soldierPrefab;

        private float3 position => m_transform.ValueRO.Position;

        private Unity.Mathematics.Random randomizer {
           get => m_building.ValueRO.randomizer;
            set => m_building.ValueRW.randomizer = value;
        }
        private uint seed => m_building.ValueRO.baseSeed;

        private int lastNbsoldier
        {
            get => m_building.ValueRO.lastNbSoldier;
            set => m_building.ValueRW.lastNbSoldier = value;
        }

        //private Quaternion rotation
        //{
        //    get => m_transform.ValueRO._Rotation;
        //    set => m_transform.ValueRW._Rotation = value;
        //}


        [BurstCompile] public void DestroyBuiling(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if (isDestroyed)
                return;

            isDestroyed = true;
            ecb.SetComponentEnabled<Destroyed>(sortKey, Entity, true);

            LocalTransform transform = m_transform.ValueRO;
            transform.Rotation = quaternion.RotateX(math.radians(-270));

            foreach (var child in childrenBuffer)
                ecb.AddComponent<ChildRotateComponent>(sortKey,child.Value,new ChildRotateComponent { up = false });

            ecb.SetComponent<LocalTransform>(sortKey, Entity, transform);
        }

        [BurstCompile] public void RebuildBuilding(EntityCommandBuffer.ParallelWriter ecb, int sortKey, Entity beacon, Beacon beaconComp)
        {
            if (!(isDestroyed && beaconComp.lightLevel > buildPrice))
                return;

            isDestroyed = false;

            ecb.SetComponentEnabled<Destroyed>(sortKey, Entity, false);
            ecb.SetComponentEnabled<Dead>(sortKey, Entity, false);
            ecb.SetComponentEnabled<InConstruction>(sortKey, Entity, false);
            health = maxHealth;

            LocalTransform transform = m_transform.ValueRO;
            transform.Rotation = quaternion.RotateX(math.radians(-90));
            ecb.SetComponent<LocalTransform>(sortKey, Entity, transform);

            foreach (var child in childrenBuffer)
                ecb.AddComponent<ChildRotateComponent>(sortKey, child.Value, new ChildRotateComponent { up = true });

            ecb.AppendToBuffer<LightModifBuffer>(sortKey, beacon, new LightModifBuffer { Value = -buildPrice});
        }

        [BurstCompile] public void AddSoldier(EntityCommandBuffer.ParallelWriter ecb, int sortKey, float delta)
        {
            if (isDestroyed || mode == Mode.production)
                return;
            if(mode == Mode.attack && rallyPoint.x == 0 && rallyPoint.y == 0 && rallyPoint.z == 0)
                    rallyPoint = position;

            nbSoldier = 0;
            foreach (var bufferElement in soldierBuffer)
            {
                nbSoldier++;
            }

            float3 target = position + new float3(-radius, 0, -radius);
            float2 curDir = new float2(-1f, 0f);
            if (nbSoldier < nbSoldierMAX && (Timer += delta) > spawnTime)
            {
                float3 pos = position + new float3(0f, 2.5f, -2f);
                
                //int rndPos = randomizer.NextInt(0, 3);
                //randomizer = new Unity.Mathematics.Random((uint)((seed+1) * (nbSoldier + 1)/(rallyPoint.x+rallyPoint.z) + lvlAttack));
                //switch (rndPos)
                //{
                //    case 0:
                //        pos += new float3(-2, 0, 0);
                //        target = position + new float3(-radius, 0, +radius);
                //        curDir = new float2(0f,1f);
                //        break;
                //    case 1:
                //        pos += new float3(2, 0, 0);
                //        target = position + new float3(+radius, 0, -radius);
                //        curDir = new float2(0f, -1f);
                //        break;
                //    case 2:
                //        pos += new float3(0, 0, -2);
                //        target = position + new float3(-radius, 0, -radius);
                //        curDir = new float2(-1f, 0f);
                //        break;
                //    case 3:
                //        pos += new float3(0, 0, 2);
                //        target = position + new float3(+radius, 0, +radius);
                //        curDir = new float2(1f, 0f);
                //        break;
                //}

                Entity soldier = ecb.Instantiate(sortKey, soldierPrefab);
                ecb.SetComponent(sortKey, soldier, new LocalTransform { Position = pos, Scale = 1, Rotation = quaternion.identity });
                ecb.SetComponent(sortKey, soldier, new Soldier
                {
                    refBuilding = Entity,
                    rallyPoint = position,
                    rallyRadius = radius,
                    TargetRange = radius * 2,
                    defaultTarget = target,
                    nbSoldierInUnit = nbSoldier
                });
                
                if(mode == Mode.defense)
                {
                    ecb.SetComponentEnabled<DefBuffTag>(sortKey, soldier, true);
                }
                
                ecb.SetComponent(sortKey, soldier, new Target { range = 4f});
                ecb.SetComponent(sortKey, soldier, new Movement {curDir = curDir, targetPosition = target, Speed = 4 });
                var curSoldier = new SoldierBuildingBufferElement { Value = soldier };
                ecb.AppendToBuffer(sortKey, Entity, curSoldier);

                Timer = 0;

                nbSoldier++;

            }
            if (nbSoldier > 1)// && nbSoldier != lastNbsoldier)
            {
                float3 rally = float3.zero;
                if(mode == Mode.attack)
                {
                    rally = rallyPoint;
                }
                else
                {
                    rally = position;
                }

                Entity stockE = Entity.Null;
                foreach (var bufferElement in soldierBuffer)
                {

                    if (stockE != Entity.Null)
                    {
                        ecb.SetComponent<Soldier>(sortKey, bufferElement.Value, new Soldier
                        {
                            refBuilding = Entity,
                            rallyPoint = rally,
                            rallyRadius = radius,
                            TargetRange = radius * 2,
                            defaultTarget = target,
                            nbSoldierInUnit = nbSoldier,
                            nextUnit = stockE
                        });
                    }
                    stockE = bufferElement.Value;
                }
                ecb.SetComponent<Soldier>(sortKey, soldierBuffer[0].Value, new Soldier
                {
                    refBuilding = Entity,
                    rallyPoint = rally,
                    rallyRadius = radius,
                    TargetRange = radius * 2,
                    defaultTarget = target,
                    nbSoldierInUnit = nbSoldier,
                    nextUnit = stockE
                });
            }


            lastNbsoldier = nbSoldier;
        }

        [BurstCompile]
        public void ChangeFlagPosition(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if (!isDestroyed && mode == Mode.attack)
            {
                ShowFlag(ecb, sortKey);
            }
            else
            {
                HideFlag(ecb, sortKey);

            }
            
        }
        [BurstCompile]
        public void ChangeMode(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            if (!isDestroyed)
            {


                switch (mode)
                {
                    case Mode.attack:
                        

                        foreach (var soldier in soldierBuffer)
                        {
                            ecb.SetComponentEnabled<CancelBuffTag>(sortKey, soldier.Value, true);
                        }
                        break;
                    case Mode.defense:

                        foreach (var soldier in soldierBuffer)
                        {
                            ecb.SetComponentEnabled<DefBuffTag>(sortKey, soldier.Value, true);
                        }
                        break;
                    case Mode.production:
                        foreach (var soldier in soldierBuffer)
                        {
                            ecb.DestroyEntity(sortKey, soldier.Value);
                        }
                        break;
                }
                ecb.SetComponentEnabled<ChangeModeTag>(sortKey,Entity, false);
            }

        }
        [BurstCompile]
        public void HideFlag(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {

            ecb.SetComponentEnabled<ActiveFlag>(sortKey, Flag, false);
            ecb.SetComponent<Flag>(sortKey, Flag, new Flag { building = Entity, position = new float3(0, -10, 0) });
        }
        [BurstCompile]
        public void ShowFlag(EntityCommandBuffer.ParallelWriter ecb, int sortKey)
        {
            ecb.SetComponentEnabled<ActiveFlag>(sortKey, Flag, true);
            ecb.SetComponent<Flag>(sortKey, Flag, new Flag { building = Entity, position = rallyPoint + new float3(0, 2, 0) });
        }

    }
}

