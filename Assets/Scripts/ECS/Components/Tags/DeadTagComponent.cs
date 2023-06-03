using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class DeadAuthoring : MonoBehaviour
    {
        class Baker : Baker<DeadAuthoring>
        {
            public override void Bake(DeadAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Dead { });
            }
        }
    }

    public struct Dead : IComponentData, IEnableableComponent
    {
    }
}