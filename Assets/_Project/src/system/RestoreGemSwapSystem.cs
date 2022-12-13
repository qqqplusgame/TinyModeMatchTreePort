using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB4SystemGroup))]
    [UpdateAfter(typeof(DeleteMatchedGemSystem))]
    public partial class RestoreGemSwapSystem : SystemBase
    {
        GameECB4System endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECB4System>();
        }

        protected override void OnUpdate()
        {
            if (GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            EntityCommandBuffer ecb = endSimECBSystem.CreateCommandBuffer();

            int gem1HashKey = -1;
            int gem2HashKey = -1;
            
            // Find if there are 2 gems to swap back.
            Entities.ForEach((Entity entity, in Gem gem, in GemSwap gemSwap) =>
            {
                if (gem.IsSwapping)
                {
                    return;
                }

                if (gem1HashKey == -1)
                {
                    gem1HashKey = gem.CellHashKey;
                }
                else if (gem2HashKey == -1)
                {
                    gem2HashKey = gem.CellHashKey;
                }
            }).Run();

            // Swap gems.
            if (gem1HashKey != -1 && gem2HashKey != -1)
            {
                var grid = GridService.getGridConfiguration(EntityManager);
                var gemEntity1 = GemService.getGemEntity(EntityManager, gem1HashKey);
                var gem1 = GemService.getGem(EntityManager, gem1HashKey);
                var gemEntity2 = GemService.getGemEntity(EntityManager, gem2HashKey);
                var gem2 = GemService.getGem(EntityManager, gem2HashKey);

                ecb.RemoveComponent<GemSwap>(gemEntity1);
                ecb.RemoveComponent<GemSwap>(gemEntity2);
                Debug.Log("Swapping back gems " + gem1HashKey + " and " + gem2HashKey);
                GemService.swapGems(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2, ref gem2);
                GemService.animateGemsSwap(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2,
                    ref gem2);
            }
        }
    }
}