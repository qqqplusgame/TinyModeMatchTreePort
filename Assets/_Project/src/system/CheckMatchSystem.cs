using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECBSystemGroup))]
    public partial class CheckMatchSystem : SystemBase
    {
        GameECBSystem endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECBSystem>();
        }

        protected override void OnUpdate()
        {
            var grid = GridService.getGridConfiguration(EntityManager);

            if (!grid.IsGridCreated)
                return;


            if (GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            /*
             * check if all gems 's animation is finished ,if not wait for it to finish
             */
            var hasAnimedGem = false;
            foreach (var gem in SystemAPI.Query<RefRO<Gem>>().WithNone<Disabled>())
            {
                if (gem.ValueRO.IsFalling || gem.ValueRO.IsSwapping)
                {
                    hasAnimedGem = true;
                    break;
                }
            }
           
            if (hasAnimedGem)
                return;
            
            
            EntityCommandBuffer endSimECB = endSimECBSystem.CreateCommandBuffer();
            var successfulGemSwap = false;
            var em = EntityManager;
            Entities.WithNone<Disabled>().ForEach((Entity gemEntity, in Gem gem) =>
            {
                if (GemService.isMatchableType1(gem))
                {
                    var leftGemEntity = GemService.getNeighborGem(em, ref grid, in gem, -1, 0);
                    var rightGemEntity = GemService.getNeighborGem(em, ref grid, in gem, 1, 0);
                    var downGemEntity = GemService.getNeighborGem(em, ref grid, in gem, 0, -1);
                    var upGemEntity = GemService.getNeighborGem(em, ref grid, in gem, 0, 1);
                    var leftGem = GemService.getGemFromEntity(em, leftGemEntity);
                    var rightGem = GemService.getGemFromEntity(em, rightGemEntity);
                    var downGem = GemService.getGemFromEntity(em, downGemEntity);
                    var upGem = GemService.getGemFromEntity(em, upGemEntity);

                    if (GemService.isMatchableType1(leftGem) && GemService.isMatchableType1(rightGem) &&
                        gem.GemType == leftGem.GemType &&
                        gem.GemType == rightGem.GemType)
                    {
                        GemService.addMatchedComponent(em, endSimECB, gemEntity, true);
                        GemService.addMatchedComponent(em, endSimECB, leftGemEntity, true);
                        GemService.addMatchedComponent(em, endSimECB, rightGemEntity, true);

                        if (em.HasComponent<GemSwap>(gemEntity) ||
                            em.HasComponent<GemSwap>(leftGemEntity) ||
                            em.HasComponent<GemSwap>(rightGemEntity))
                        {
                            successfulGemSwap = true;
                            Debug.Log(
                                $"Successful Gem Swap {leftGem.CellHashKey}  {gem.CellHashKey}  {rightGem.CellHashKey}");
                        }
                    }
                    else if (GemService.isMatchableType1(downGem) && GemService.isMatchableType1(upGem) &&
                             gem.GemType == downGem.GemType &&
                             gem.GemType == upGem.GemType)
                    {
                        GemService.addMatchedComponent(em, endSimECB, gemEntity, true);
                        GemService.addMatchedComponent(em, endSimECB, downGemEntity, true);
                        GemService.addMatchedComponent(em, endSimECB, upGemEntity, true);

                        if (em.HasComponent<GemSwap>(gemEntity) ||
                            em.HasComponent<GemSwap>(downGemEntity) ||
                            em.HasComponent<GemSwap>(upGemEntity))
                        {
                            successfulGemSwap = true;
                            Debug.Log(
                                $"Successful Gem Swap {downGem.CellHashKey} {gem.CellHashKey}  {upGem.CellHashKey}");
                        }
                    }
                }
            }).Run();


            if (successfulGemSwap)
            {
                GameService.incrementMoveCounter(EntityManager);
            }
        }
    }
}