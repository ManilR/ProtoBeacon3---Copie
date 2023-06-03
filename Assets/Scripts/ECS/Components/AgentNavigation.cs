using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;

namespace Beacon
{
    public class AgentNavigation : MonoBehaviour
    {
        public float navigationSpeed = 1f;
    }

    public class AgentNavigationBaker : Baker<AgentNavigation>
    {
        public override void Bake(AgentNavigation authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent<NavigationCurrentPathIndexComponent>(entity);
            AddComponent(entity, new NavigationSpeedDataComponent
            {
                speed = authoring.navigationSpeed
            });
            AddBuffer<NavigationPathBuffer>(entity);
        }
    }

    /// <summary>
    /// State values mostly for the debuging purpose
    /// </summary>
    public enum NavigateToPositionState
    {
        WaitingForJob = 0,
        InvalidFromPosition = 1,
        InvalidToPosition = 2,
        InProgress = 3,
        QuerySuccess = 4,
        QueryError = 5,
        NavigationEnd = 6,
        ReachedTargetPoint = 7
    }

    /// <summary>
    /// This component will tell us when given object end movement between to points.
    /// </summary>
    public struct NavigateReachedTargetPointComponent : IComponentData { }

    /// <summary>
    /// Information about that current navigation could not be finished. 
    /// </summary>
    public struct NavigateErrorComponent : IComponentData { }

    /// <summary>
    /// Component used for initialize navigation process.
    /// </summary>
    public struct NavigateInitializeComponent : IComponentData { }

    /// <summary>
    /// Component used together with NavigateInitializeComponent. Contains navigation target position.
    /// </summary>
    public struct NavigateToPositionComponent : IComponentData
    {
        public float3 worldPosition;
        public NavigateToPositionState state;
    }

    /// <summary>
    /// Buffer which will hold all points found by path finding system.
    /// We don't want to store whole buffer in entities chunks so set InterlaBufferCapacity(0).
    /// </summary>
    [InternalBufferCapacity(0)]
    public struct NavigationPathBuffer : IBufferElementData
    {
        public float3 worldPosition;
    }

    /// <summary>
    /// Component for movement script, telling us which path points is currently reaching.
    /// </summary>
    public struct NavigationCurrentPathIndexComponent : IComponentData
    {
        public int pathBufferIndex;
    }

    /// <summary>
    /// Information about movement speed.
    /// </summary>
    public struct NavigationSpeedDataComponent : IComponentData
    {
        public float speed;
    }

    [BurstCompile]
    public readonly struct NavigateHelper
    {
        [BurstCompile]
        public static void ClearNavigationData(ref EntityCommandBuffer ecb, ref Entity self)
        {
            ecb.RemoveComponent<NavigateReachedTargetPointComponent>(self);
            ecb.RemoveComponent<NavigateToPositionComponent>(self);
            ecb.RemoveComponent<NavigateInitializeComponent>(self);
        }

        [BurstCompile]
        public static void ClearNavigationData(ref EntityCommandBuffer.ParallelWriter ecb, int index, ref Entity self)
        {
            ecb.RemoveComponent<NavigateReachedTargetPointComponent>(index, self);
            ecb.RemoveComponent<NavigateToPositionComponent>(index, self);
            ecb.RemoveComponent<NavigateInitializeComponent>(index, self);
        }

        [BurstCompile]
        public static void NavigateToPostion(ref EntityCommandBuffer ecb, ref Entity self, ref float3 position)
        {
            ecb.AddComponent<NavigateInitializeComponent>(self);
            ecb.AddComponent<NavigateToPositionComponent>(self, new NavigateToPositionComponent
            {
                worldPosition = position
            });
        }
    }

    public readonly partial struct NavigateToPositionInitializeComponentAspect : IAspect
    {
        public readonly RefRW<NavigateToPositionComponent> navigateToPositionData;
        public readonly RefRW<LocalToWorld> transform;
        public readonly RefRO<NavigateInitializeComponent> initializeTag;
        public readonly Entity self;

        public float3 Position
        {
            get => transform.ValueRO.Position;
        }

        public float3 TargetPosition
        {
            get => navigateToPositionData.ValueRO.worldPosition;
        }

        public NavigateToPositionState NavigationStatus
        {
            set => navigateToPositionData.ValueRW.state = value;
            get => navigateToPositionData.ValueRO.state;
        }
    }
}