using Unity.Entities;
using UnityEngine;

namespace Beacon
{
    public class LightLevelHUDAuthoring : MonoBehaviour
    {
        class Baker : Baker<LightLevelHUDAuthoring>
        {
            public override void Bake(LightLevelHUDAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new LightLevelHUD { });
            }
        }
    }

    public struct LightLevelHUD : IComponentData
    {
    }
}