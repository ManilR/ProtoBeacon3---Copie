using Unity.Entities;
using Unity.Burst;

namespace Beacon
{
    [BurstCompile]
    public readonly partial struct SoldierAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<Soldier> m_soldier;

        private Entity building => m_soldier.ValueRO.refBuilding;



    }
}

