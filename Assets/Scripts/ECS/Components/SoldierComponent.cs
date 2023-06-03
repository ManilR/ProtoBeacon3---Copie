using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Beacon
{
    public class SoldierAuthoring : MonoBehaviour
    {
        [SerializeField, Range(1f, 200f)] private float m_targetRange;
        
        class Baker : Baker<SoldierAuthoring>
        {
            public override void Bake(SoldierAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Soldier
                {
                    TargetRange = authoring.m_targetRange,
                    rallyRadius = 10f,

                });
                AddComponent(entity, new PatrolTimer { value = 0 });
                AddComponent(entity, new PatrolRandom { value = Unity.Mathematics.Random.CreateFromIndex(1234) });

            }
        }

    }
    public enum SoldierDir { up,down,left,right}
    public struct Soldier : IComponentData, IEnableableComponent
    {
        public Entity refBuilding;
        public float3 rallyPoint;
        public float rallyRadius;
        public float TargetRange;
        public SoldierDir currentDir;

        public float3 defaultTarget;

        public int nbSoldierInUnit;
        public Entity nextUnit;
        public float3 nextUnitPos;
        public SoldierDir nextUnitDir; 
    }
    public struct PatrolTimer : IComponentData
    {
        public float value;
    }
    public struct PatrolRandom : IComponentData
    {
        public Unity.Mathematics.Random value;
    }



}