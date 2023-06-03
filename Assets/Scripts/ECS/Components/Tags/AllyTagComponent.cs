using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class AllyAuthoring : MonoBehaviour
    {
        class Baker : Baker<AllyAuthoring>
        {
            public override void Bake(AllyAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Ally { });
            }
        }
    }

    public struct Ally : IComponentData
    {
    }
}