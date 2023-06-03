using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class EnemyAuthoring : MonoBehaviour
    {
        class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Enemy{ });
            }
        }
    }

    public struct Enemy : IComponentData
    {
    }
}