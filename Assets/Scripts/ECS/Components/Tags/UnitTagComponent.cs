using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class UnitTagAuthoring : MonoBehaviour
    {
        class Baker : Baker<UnitTagAuthoring>
        {
            public override void Bake(UnitTagAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new UnitTag { });
            }
        }
    }

    public struct UnitTag : IComponentData
    {
    }
}