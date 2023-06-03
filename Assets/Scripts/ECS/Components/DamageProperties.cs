using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class DamageAuthoring : MonoBehaviour
    {
        [Header("Damage Data")]
        [SerializeField, Range(1f, 100f)] private float m_damage;
        [SerializeField, Range(1f, 100f)] private float m_atkPerSec;

        class Baker : Baker<DamageAuthoring>
        {
            public override void Bake(DamageAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Damage {
                    damage = authoring.m_damage,
                    atkPerSec = authoring.m_atkPerSec
                });

                AddComponent<DamageTimer>(entity);
            }
        }

    }


    public struct Damage : IComponentData
    {
        public float damage;
        public float atkPerSec;
    }

    public struct DamageTimer : IComponentData
    {
        public float Value;
    }
}