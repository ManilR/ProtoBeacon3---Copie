using Beacon;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;


namespace Beacon
{


    public class NavAgentAuthoring : MonoBehaviour
    {

        class Baker : Baker<NavAgentAuthoring>
        {
            public override void Bake(NavAgentAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new NavAgent_Component {});

                AddComponent(entity, new NavAgent_ToBeRoutedTag { });
                SetComponentEnabled<NavAgent_ToBeRoutedTag>(entity, false);

                AddBuffer<NavAgent_Buffer>(entity);

            }
        }
    }


    public struct NavAgent_Component : IComponentData
    {
        public float3 fromLocation;
        public float3 toLocation;
        public NavMeshLocation nml_FromLocation;
        public NavMeshLocation nml_ToLocation;
        public bool routed;
    }

    public struct NavAgent_Buffer : IBufferElementData
    {
        public float3 wayPoints;
    }

    public struct NavAgent_ToBeRoutedTag : IComponentData, IEnableableComponent
    {

    }
}