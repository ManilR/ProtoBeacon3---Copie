using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Beacon
{
    public class GameStateAuthoring : MonoBehaviour
    {
        

        class Baker : Baker<GameStateAuthoring>
        {
            public override void Bake(GameStateAuthoring authoring)
            {
                Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new GameStateComponent
                {
                    isPlaying = false
                });
            }
        }

    }

    public struct GameStateComponent : IComponentData 
    {
        public bool isPlaying;
    }
}