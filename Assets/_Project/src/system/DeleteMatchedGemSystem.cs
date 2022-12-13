using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB3SystemGroup))]
    [UpdateAfter(typeof(CollectEggSystem))]
    [UpdateBefore(typeof(RestoreGemSwapSystem))]
    public partial class DeleteMatchedGemSystem : SystemBase
    {
        GameECB3System endSimECBSystem;
        GameECB4System gameECBSystem;

        protected override void OnCreate()
        {
            //make delete happened before next simulation begin
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECB3System>();
            gameECBSystem = World.GetOrCreateSystemManaged<GameECB4System>();
        }

        protected override void OnUpdate()
        {
            var grid = GridService.getGridConfiguration(EntityManager);
            if (GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp); //endSimECBSystem.CreateCommandBuffer();
            
            var em = EntityManager;
            var matchedGemCount = 0;
            Entities.ForEach((Entity entity, ref Gem gemToDestroy, in Matched matched) =>
            {
                // Do not destroy gem if it was just turned into a power up from a match combo.

                if (matched.CreatedPowerUp == GemPowerUpTypes.None)
                {
                    GemService.deleteGem(em, ecb, ref grid, entity, ref gemToDestroy);
                }
                else
                {
                    ecb.RemoveComponent<Matched>(entity);
                }

                matchedGemCount++;
            }).Run();

            ecb.Playback(em);
            ecb.Dispose();

            if (matchedGemCount <= 0)
            {
                return;
            }

            ecb = new EntityCommandBuffer(Allocator.Temp);
            // SoundService.play(this.world, "MatchSound");
            AudioUtils.PlaySound(em,"CellMatched");
            //
            // Make existing gems fall to fill the gap left by the destroyed gems.    
            for (int i = 0; i < grid.Width; i++)
            {
                int fallOffset = 0;
                for (int j = 0; j < grid.Height; j++)
                {
                    var gemEntity = GemService.getGemEntityAtPosition(em, grid, i, j);
                    var gem = GemService.getGemFromEntity(em, gemEntity);
                    //todo check if gem is null , maybe try another way better
                    if (gem.CellHashKey < 0)
                    {
                        fallOffset++;
                    }
                    else
                    {
                        if (em.HasComponent<MatchPossibility>(gemEntity))
                        {
                            ecb.RemoveComponent<MatchPossibility>(gemEntity);
                        }

                        var lastCellHashKey = gem.CellHashKey;
                        var currentCellHashKey = GridService.getCellHashCode(grid, i, j - fallOffset);
                        if (currentCellHashKey != lastCellHashKey)
                        {
                            GemService.setGem(em, ref grid, lastCellHashKey, Entity.Null);
                            gem.CellHashKey = currentCellHashKey;
                            em.SetComponentData(gemEntity, gem);
                            GemService.setGem(em, ref grid, currentCellHashKey, gemEntity);
                            //Debug.Log($"DMGS animateGemFall {currentCellHashKey} {World.Time.ElapsedTime} ");
                            GemService.animateGemFall(em, ecb,  ref grid, gemEntity, ref gem,
                                currentCellHashKey, fallOffset);
                        }
                    }
                }
            }


            Entities.ForEach((Entity entity, in Gem gem, in GemSwap swap) =>
            {
                Debug.Log($"RemoveComponent<GemSwap> {gem.CellHashKey}");
                ecb.RemoveComponent<GemSwap>(entity);
            }).Run();


            ecb.Playback(em);

            ecb.Dispose();
        }
    }
}