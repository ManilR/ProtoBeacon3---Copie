using Unity.Entities;
using UnityEngine;

namespace Beacon
{

    public class ClickableAuthoring : MonoBehaviour
    {
        class Baker : Baker<ClickableAuthoring>
        {
            public override void Bake(ClickableAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new Clickable { });
            }
        }
    }
    
    public struct Clickable : IComponentData
    {
    }

}
