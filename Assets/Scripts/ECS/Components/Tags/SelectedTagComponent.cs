using Unity.Entities;
using UnityEngine;

namespace Beacon
{

    public class SelectedAuthoring : MonoBehaviour
    {
        class Baker : Baker<SelectedAuthoring>
        {
            public override void Bake(SelectedAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Selected { });
            }
        }
    }

    public struct Selected : IComponentData
    {
    }

}
