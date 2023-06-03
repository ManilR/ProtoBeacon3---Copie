using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Beacon
{
    public enum Mode {attack, defense, production};

    public class BuildingAuthoring : MonoBehaviour
    {
        [Header("Building Data")]
        [SerializeField] private GameObject m_RallyFlag;
        [SerializeField] private int ID;
        [SerializeField, Range(0f, 100f)] private float health;
        [SerializeField, Range(1f, 100f)] private float max_health;
        [SerializeField, Range(1f, 100f)] private float m_buildingPrice;
        [SerializeField] private List<GameObject> m_children;
        [SerializeField, Range(1, 100)] private int m_production;

        [Header("Building Statistics")]
        [SerializeField] private Mode m_mode;
        [SerializeField, Range(1, 11)] private int m_lvlAttack;
        [SerializeField, Range(1, 11)] private int m_lvlDefense;
        [SerializeField, Range(1, 11)] private int m_lvlProduction;

        [Header("Soldiers Data")]
        [SerializeField] private GameObject m_prefab;
        [SerializeField, Range(1, 20)] private int m_nbSoldiers;
        [SerializeField, Range(1f, 20f)] private float spawnTime;
        [SerializeField, Range(1f, 50f)] private float m_radius;

        class Baker : Baker<BuildingAuthoring>
        {
            public override void Bake(BuildingAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                System.Random rnd = new System.Random();
                uint seed = (uint)rnd.Next(1, 100000);
                AddComponent(entity, new Building {
                    isDestroyed = false,
                    nbSoldierMAX = authoring.m_nbSoldiers, 
                    nbSoldier = 0,
                    soldierPrefab = GetEntity(authoring.m_prefab, TransformUsageFlags.Dynamic),
                    soldierSpawnTime = authoring.spawnTime,
                    buildingPrice =authoring.m_buildingPrice,
                    rallyRadius = authoring.m_radius,
                    ID = authoring.ID,
                    mode = authoring.m_mode,
                    lvlAttack = authoring.m_lvlAttack,
                    lvlDefense = authoring.m_lvlDefense,
                    lvlProduction = authoring.m_lvlProduction,
                    production = authoring.m_production,
                    rallyFlag = GetEntity(authoring.m_RallyFlag, TransformUsageFlags.Dynamic),
                    baseSeed = seed,
                    randomizer = new Unity.Mathematics.Random((uint)rnd.Next(1, 100000))
                });

                AddComponent(entity, new Health
                {
                    maxHealth = authoring.max_health,
                    health = authoring.health
                });

                AddBuffer<DamageBufferElement>(entity);

                AddComponent<Dead>(entity);
                SetComponentEnabled<Dead>(entity, false);

                AddComponent<Destroyed>(entity);
                SetComponentEnabled<Destroyed>(entity, false);

                AddComponent<InConstruction>(entity);
                SetComponentEnabled<InConstruction>(entity, false);

                AddComponent<ChangeModeTag>(entity);
                SetComponentEnabled<ChangeModeTag>(entity, false);

                AddComponent<SoldierSpawnTimer>(entity);
                
                AddBuffer<SoldierBuildingBufferElement>(entity);

                AddBuffer<ChildBufferElement>(entity);

                ChildBufferElement curChild;
                foreach (GameObject go in authoring.m_children)
                {
                    curChild = new ChildBufferElement { Value = GetEntity(go, TransformUsageFlags.Dynamic) };
                    AppendToBuffer<ChildBufferElement>(entity, curChild);
                }
            }
        }
    }


    public struct Building : IComponentData, IEnableableComponent
    {
        public bool isDestroyed;
        public int nbSoldierMAX;
        public int nbSoldier;
        public int lastNbSoldier;
        public float soldierSpawnTime;
        public Entity soldierPrefab;
        public float buildingPrice;
        public float rallyRadius;
        public Entity rallyFlag;
        public float3 rallyPoint;
        public int ID;
        public int lvlAttack;
        public int lvlDefense;
        public int lvlProduction;
        public int production;
        public Mode mode;
        public Unity.Mathematics.Random randomizer;
        public uint baseSeed;
        

    }

    public struct SoldierSpawnTimer : IComponentData
    {
        public float Value;
    }

    public struct ChildBufferElement : IBufferElementData
    {
        public Entity Value;
    }

    public struct ChildRotateComponent : IComponentData
    {
        public bool up;
    }

    public struct SelectedTagComponent : IComponentData, IEnableableComponent
    {

    }
    public struct ChangeModeTag : IComponentData, IEnableableComponent
    {

    }

}