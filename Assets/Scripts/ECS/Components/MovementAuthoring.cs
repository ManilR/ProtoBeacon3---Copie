using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Beacon
{
    public class MovementAuthoring : MonoBehaviour
    {
        [SerializeField, Range(1f, 100f)] private float speed;
        [SerializeField] private bool m_isFixed;

        class Baker : Baker<MovementAuthoring>
        {
            public override void Bake(MovementAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Movement
                {
                    curDir = new float2(1f, 1f),
                    isFixed = authoring.m_isFixed,
                    Speed = authoring.speed
                });
            }
        }

    }
    
    public struct Movement : IComponentData, IEnableableComponent
    {
        public float Speed;
        public float2 curDir;
        public float3 targetPosition;
        public bool isFixed;
    }
}