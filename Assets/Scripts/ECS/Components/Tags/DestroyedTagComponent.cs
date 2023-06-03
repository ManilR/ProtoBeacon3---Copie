using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class DestroyedAuthoring : MonoBehaviour
    {
        class Baker : Baker<DestroyedAuthoring>
        {
            public override void Bake(DestroyedAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Destroyed { });
            }
        }
    }

    public struct Destroyed : IComponentData, IEnableableComponent
    {
    }

    public struct InConstruction : IComponentData, IEnableableComponent
    {
    }
}