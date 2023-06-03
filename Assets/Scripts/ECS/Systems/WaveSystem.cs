using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

namespace Beacon
{
    [BurstCompile]
    public partial struct WaveSystem : ISystem
    {
        [BurstCompile] public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Wave>();
        }

        [BurstCompile] public void OnDestroy(ref SystemState state){}

        [BurstCompile] public void OnUpdate(ref SystemState state)
        {
            var waveUnitNormal = SystemAPI.GetSingleton<Wave>().unitNormal;
            var waveUnitSpeedy = SystemAPI.GetSingleton<Wave>().unitSpeedy;
            var waveBoss = SystemAPI.GetSingleton<Wave>().boss;

            foreach (var entity in SystemAPI.Query<DynamicBuffer<WaveData>>())
            {
                foreach (var wave in entity) {
                    if (wave.ennemyType == ennemyType.boss)
                        SpawnBoss(ref state, waveBoss, wave.position);
                    else if (wave.ennemyType == ennemyType.normal)
                        SpawnWave(ref state, waveUnitNormal, wave.count, wave.position);
                    else if (wave.ennemyType == ennemyType.speedy)
                        SpawnWave(ref state, waveUnitSpeedy, wave.count, wave.position);
                }
                entity.Clear();
            }
        }

        [BurstCompile] void SpawnWave(ref SystemState state, Entity unit, int count, Vector3 position)
        {
            var units = state.EntityManager.Instantiate(unit, count, Allocator.Temp);
            
            for (int i = 0; i < units.Length; i += 1)
            {
                Vector3 newPos = new Vector3(Random.Range(-10.0f, 10.0f) + position.x, position.y, Random.Range(-10.0f, 10.0f) + position.z);
                state.EntityManager.SetComponentData(units[i], new LocalTransform { Position = newPos, Scale = 1, Rotation = quaternion.identity });
            }
                
        }

        [BurstCompile] void SpawnBoss(ref SystemState state, Entity boss, Vector3 position)
        {
            var units = state.EntityManager.Instantiate(boss, 1, Allocator.Temp);

            for (int i = 0; i < units.Length; i += 1)
            {
                Vector3 newPos = new Vector3(Random.Range(-10.0f, 10.0f) + position.x, position.y, Random.Range(-10.0f, 10.0f) + position.z);
                state.EntityManager.SetComponentData(units[i], new LocalTransform { Position = newPos, Scale = 1, Rotation = quaternion.identity });
            }

        }
    }
}