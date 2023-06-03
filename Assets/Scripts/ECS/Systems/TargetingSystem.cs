using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace Beacon
{
    [BurstCompile]
    public partial struct TargetingSystem : ISystem
    {
        public enum SpatialPartitioningType
        {
            None,
            Simple,
            KDTree,
        }

        static NativeArray<ProfilerMarker> s_ProfilerMarkers;

        EntityQuery m_TargetQueryAllies;
        EntityQuery m_TargetQueryEnemies;
        EntityQuery m_QueryKDQueryAllies;
        EntityQuery m_QueryKDQueryEnemies;

        ComponentTypeHandle<Target> targetHandle;
        ComponentTypeHandle<Movement> movementHandle;
        ComponentTypeHandle<LocalTransform> localTransformHandle;

        public void OnCreate(ref SystemState state)
        {
            s_ProfilerMarkers = new NativeArray<ProfilerMarker>(3, Allocator.Persistent);
            s_ProfilerMarkers[0] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.None);
            s_ProfilerMarkers[1] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.Simple);
            s_ProfilerMarkers[2] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.KDTree);

            targetHandle = SystemAPI.GetComponentTypeHandle<Target>();
            movementHandle = SystemAPI.GetComponentTypeHandle<Movement>();
            localTransformHandle = SystemAPI.GetComponentTypeHandle<LocalTransform>();

            var builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform, Ally>();
            builder.WithNone<Dead>();
            m_TargetQueryAllies = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform, Enemy>();
            builder.WithNone<Dead>();
            m_TargetQueryEnemies = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform, Target, Enemy>();
            builder.WithNone<Dead>();
            m_QueryKDQueryEnemies = state.GetEntityQuery(builder);

            builder = new EntityQueryBuilder(Allocator.Temp);
            builder.WithAll<LocalTransform, Target, Ally>();
            builder.WithNone<Dead>();
            m_QueryKDQueryAllies = state.GetEntityQuery(builder);

            state.RequireForUpdate<Wave>();
        }

        public void OnDestroy(ref SystemState state)
        {
            s_ProfilerMarkers.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            // if (GameManager.instance == null ? false : GameManager.instance.isPlaying)
            // {
                targetHandle.Update(ref state);
                movementHandle.Update(ref state);
                localTransformHandle.Update(ref state);

                var spatialPartitioningType = SystemAPI.GetSingleton<Wave>().SpatialPartitioning;

                using var profileMarker = s_ProfilerMarkers[(int)spatialPartitioningType].Auto();

                var targetEntitiesAllies = m_TargetQueryAllies.ToEntityArray(state.WorldUpdateAllocator);
                var targetTransformsAllies = m_TargetQueryAllies.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

                var targetEntitiesEnemies = m_TargetQueryEnemies.ToEntityArray(state.WorldUpdateAllocator);
                var targetTransformsEnemies = m_TargetQueryEnemies.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

                Entity m_beacon = SystemAPI.GetSingletonEntity<Beacon>();

                m_TargetQueryAllies.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
                if (m_TargetQueryAllies.CalculateEntityCount() > 0)
                {
                    //ENEMIES TARGET CALCUL
                    var positions = CollectionHelper.CreateNativeArray<PositionAndIndex>(targetTransformsAllies.Length,
                                    state.WorldUpdateAllocator);

                    for (int i = 0; i < positions.Length; i += 1)
                    {
                        positions[i] = new PositionAndIndex
                        {
                            Index = i,
                            Position = targetTransformsAllies[i].Position.xz
                        };
                    }

                    state.Dependency = positions.SortJob(new AxisXComparer()).Schedule(state.Dependency);

                    var simple = new SimplePartitioning
                    {
                        TargetEntities = targetEntitiesAllies,
                        TargetsTransforms = targetTransformsAllies,
                        Positions = positions,
                        TargetHandle = targetHandle,
                        LocalTransformHandle = localTransformHandle,
                        beacon = m_beacon
                    };
                    state.Dependency = simple.ScheduleParallel(m_QueryKDQueryEnemies, state.Dependency);
                    #region OLD SWITCH
                    //state.Dependency = simple.ScheduleParallel(state.Dependency);




                    //switch (spatialPartitioningType)
                    //{

                    //    case SpatialPartitioningType.None:
                    //        {
                    //            var noPartitioning = new NoPartitioning
                    //            { TargetEntities = targetEntitiesAllies, TargetTransforms = targetTransformsAllies };
                    //            state.Dependency = noPartitioning.ScheduleParallel(state.Dependency);
                    //            break;
                    //        }
                    //    case SpatialPartitioningType.Simple:
                    //        {
                    //            var positions = CollectionHelper.CreateNativeArray<PositionAndIndex>(targetTransformsAllies.Length,
                    //                state.WorldUpdateAllocator);

                    //            for (int i = 0; i < positions.Length; i += 1)
                    //            {
                    //                positions[i] = new PositionAndIndex
                    //                {
                    //                    Index = i,
                    //                    Position = targetTransformsAllies[i].Position.xz
                    //                };
                    //            }

                    //            state.Dependency = positions.SortJob(new AxisXComparer()).Schedule(state.Dependency);

                    //            var simple = new SimplePartitioning { TargetEntities = targetEntitiesAllies, TargetsTransforms = targetTransformsAllies, Positions = positions };
                    //            state.Dependency = simple.ScheduleParallel(state.Dependency);
                    //            break;
                    //        }
                    //    case SpatialPartitioningType.KDTree:
                    //        {
                    //            var tree = new KDTree(targetEntitiesAllies.Length, Allocator.TempJob, 64);

                    //            // init KD tree
                    //            for (int i = 0; i < targetEntitiesAllies.Length; i += 1)
                    //            {
                    //                // NOTE - the first parameter is ignored, only the index matters
                    //                tree.AddEntry(i, targetTransformsAllies[i].Position);
                    //            }

                    //            state.Dependency = tree.BuildTree(targetEntitiesAllies.Length, state.Dependency);

                    //            var queryKdTree = new QueryKDTree
                    //            {
                    //                Tree = tree,
                    //                TargetEntities = targetEntitiesAllies,
                    //                TargetsTransforms = targetTransformsAllies,
                    //                Scratch = default,
                    //                TargetHandle = targetHandle,
                    //                MovementHandle = movementHandle,
                    //                LocalTransformHandle = localTransformHandle
                    //            };
                    //            state.Dependency = queryKdTree.ScheduleParallel(m_QueryKDQuery, state.Dependency);

                    //            state.Dependency.Complete();
                    //            tree.Dispose();
                    //            break;
                    //        }
                    //}
                    #endregion
                }
                else
                {
                    var reset = new ResetTargetJob { TargetHandle = targetHandle };
                    state.Dependency = reset.ScheduleParallel(m_QueryKDQueryEnemies, state.Dependency);
                }
                if (m_TargetQueryEnemies.CalculateEntityCount() > 500)
                {
                    //ALLIES TARGET CALCUL
                    var tree = new KDTree(targetEntitiesEnemies.Length, Allocator.TempJob, 64);

                    // init KD tree
                    for (int i = 0; i < targetEntitiesEnemies.Length; i += 1)
                    {
                        // NOTE - the first parameter is ignored, only the index matters
                        tree.AddEntry(i, targetTransformsEnemies[i].Position);
                    }

                    state.Dependency = tree.BuildTree(targetEntitiesEnemies.Length, state.Dependency);

                    var queryKdTree = new QueryKDTree
                    {
                        Tree = tree,
                        TargetEntities = targetEntitiesEnemies,
                        TargetsTransforms = targetTransformsEnemies,
                        Scratch = default,
                        TargetHandle = targetHandle,
                        MovementHandle = movementHandle,
                        LocalTransformHandle = localTransformHandle,
                        isAlly = true
                    };
                    state.Dependency = queryKdTree.ScheduleParallel(m_QueryKDQueryAllies, state.Dependency);

                    state.Dependency.Complete();
                    tree.Dispose();
                }
                else if (m_TargetQueryEnemies.CalculateEntityCount() > 0)
                {
                    var positions = CollectionHelper.CreateNativeArray<PositionAndIndex>(targetTransformsEnemies.Length, state.WorldUpdateAllocator);

                    for (int i = 0; i < positions.Length; i += 1)
                    {
                        positions[i] = new PositionAndIndex
                        {
                            Index = i,
                            Position = targetTransformsEnemies[i].Position.xz
                        };
                    }

                    state.Dependency = positions.SortJob(new AxisXComparer()).Schedule(state.Dependency);

                    var simple = new SimplePartitioning
                    {
                        TargetEntities = targetEntitiesEnemies,
                        TargetsTransforms = targetTransformsEnemies,
                        Positions = positions,
                        TargetHandle = targetHandle,
                        LocalTransformHandle = localTransformHandle,
                        beacon = m_beacon
                    };
                    state.Dependency = simple.ScheduleParallel(m_QueryKDQueryAllies, state.Dependency);
                }
                else
                {
                    var reset = new ResetTargetJob { TargetHandle = targetHandle };
                    state.Dependency = reset.ScheduleParallel(m_QueryKDQueryAllies, state.Dependency);
                }


                state.Dependency.Complete();
            // }
        }
    }

    public partial struct QueryKDTree : IJobChunk
    {
        [ReadOnly] public NativeArray<Entity> TargetEntities;
        [ReadOnly] public NativeArray<LocalTransform> TargetsTransforms;
        public PerThreadWorkingMemory Scratch;
        public KDTree Tree;
        
    
        public ComponentTypeHandle<Target> TargetHandle;
        public ComponentTypeHandle<Movement> MovementHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformHandle;

        public bool isAlly;
        
    
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
           
            var targets = chunk.GetNativeArray(ref TargetHandle);
            var movements = chunk.GetNativeArray(ref MovementHandle);
            var transforms = chunk.GetNativeArray(ref LocalTransformHandle);
            float range = float.MaxValue;

            
            for (int i = 0; i < chunk.Count; i++)
            {
                if (!Scratch.Neighbours.IsCreated)
                    Scratch.Neighbours = new NativePriorityHeap<KDTree.Neighbour>(1, Allocator.Temp);

                Scratch.Neighbours.Clear();
                Tree.GetEntriesInRangeWithHeap(unfilteredChunkIndex, transforms[i].Position, range, ref Scratch.Neighbours);

                if(Scratch.Neighbours.Count > 0)
                {
                    var nearest = Scratch.Neighbours.Peek().index;
                    targets[i] = new Target { Value = TargetEntities[nearest], Position = TargetsTransforms[nearest].Position, range = targets[i].range };
                }
                else
                {
                    targets[i] = new Target { emptyTarget = true, range = targets[i].range };
                }
            }
        }
    }
    
    public partial struct SimplePartitioning : IJobChunk
    {
        [ReadOnly] public NativeArray<Entity> TargetEntities;
        [ReadOnly] public NativeArray<LocalTransform> TargetsTransforms;
        [ReadOnly] public NativeArray<PositionAndIndex> Positions;
        [ReadOnly] public Entity beacon;

        public ComponentTypeHandle<Target> TargetHandle;
        public ComponentTypeHandle<LocalTransform> LocalTransformHandle;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var targets = chunk.GetNativeArray(ref TargetHandle);
            var translations = chunk.GetNativeArray(ref LocalTransformHandle);

            for(int i = 0; i< chunk.Count; i++)
            {
                var ownpos = new PositionAndIndex { Position = translations[i].Position.xz };
                var index = Positions.BinarySearch(ownpos, new AxisXComparer());
                if (index < 0) index = ~index;
                if (index >= Positions.Length) index = Positions.Length - 1;

                var closestDistSq = math.distancesq(ownpos.Position, Positions[index].Position);
                var closestEntity = index;

                Search(index + 1, Positions.Length, +1, ref closestDistSq, ref closestEntity, ownpos);
                Search(index - 1, -1, -1, ref closestDistSq, ref closestEntity, ownpos);


                bool isBeacon = false;
                if (TargetEntities[Positions[closestEntity].Index] == beacon)
                    isBeacon = true;

                targets[i] = new Target { Value = TargetEntities[Positions[closestEntity].Index], Position = TargetsTransforms[Positions[closestEntity].Index].Position, range = targets[i].range, isTargetBeacon = isBeacon };
            }
        }

        void Search(int startIndex, int endIndex, int step, ref float closestDistSqRef, ref int closestEntityRef, PositionAndIndex ownpos)
        {
            for (int i = startIndex; i != endIndex; i += step)
            {
                var xdiff = ownpos.Position.x - Positions[i].Position.x;
                xdiff *= xdiff;

                if (xdiff > closestDistSqRef) break;

                var distSq = math.distancesq(Positions[i].Position, ownpos.Position);

                if (distSq < closestDistSqRef)
                {
                    closestDistSqRef = distSq;
                    closestEntityRef = i;
                }
            }
        }
    }

    public partial struct ResetTargetJob : IJobChunk
    {
        public ComponentTypeHandle<Target> TargetHandle;
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var targets = chunk.GetNativeArray(ref TargetHandle);
            for (int i = 0; i < chunk.Count; i++)
            {
                targets[i] = new Target { emptyTarget = true, range = targets[i].range };
            }

        }
    }

    public partial struct NoPartitioning : IJobEntity
    {
        [ReadOnly] public NativeArray<LocalTransform> TargetTransforms;

        [ReadOnly] public NativeArray<Entity> TargetEntities;

        public void Execute(ref Target target, in LocalTransform translation)
        {
            var closestDistSq = float.MaxValue;
            var closestEntity = Entity.Null;

            for (int i = 0; i < TargetTransforms.Length; i += 1)
            {
                var distSq = math.distancesq(TargetTransforms[i].Position, translation.Position);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestEntity = TargetEntities[i];
                }
            }
            target.Value = closestEntity;
        }
    }

    public struct AxisXComparer : IComparer<PositionAndIndex>
    {
        public int Compare(PositionAndIndex a, PositionAndIndex b)
        {
            return a.Position.x.CompareTo(b.Position.x);
        }
    }

    public struct PositionAndIndex
    {
        public int Index;
        public float2 Position;
    }

    public struct PerThreadWorkingMemory
    {
        [NativeDisableContainerSafetyRestriction]
        public NativePriorityHeap<KDTree.Neighbour> Neighbours;
    }
}