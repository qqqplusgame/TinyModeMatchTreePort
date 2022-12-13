using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(ReplenishGemBoardSystem))]
    public partial class FindPossibleMatchSystem : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var matchableGemCount = 0;
            Entities.ForEach((Entity entity, in Gem gem, in MatchPossibility mp) => { matchableGemCount++; }).Run();

            if (matchableGemCount > 0)
            {
                return;
            }

            var grid = GridService.getGridConfiguration(EntityManager);
            if (!grid.IsGridCreated)
            {
                return;
            }

            var foundMatch = false;

            var endSimECB = endSimECBSystem.CreateCommandBuffer();
            Entities.ForEach((ref Gem gem) =>
            {
                if (!foundMatch && GemService.isMatchableType2(gem))
                {
                    var gemPosition = GridService.getPositionFromCellHashCode(in grid, gem.CellHashKey);
                    if ((gemPosition.x + gemPosition.y) % 2 == 0)
                    {
                        var gemEntity = GemService.getGemEntity(EntityManager, gem.CellHashKey);
                        var leftGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, -1, 0);
                        var rightGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 1, 0);
                        var downGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 0, -1);
                        var upGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 0, 1);

                        if (trySwapAndMatch(ref grid, gemEntity, leftGemEntity, endSimECB))
                        {
                            Debug.Log("Found match by swapping left " + gem.CellHashKey);
                            foundMatch = true;
                        }
                        else if (trySwapAndMatch(ref grid, gemEntity, rightGemEntity, endSimECB))
                        {
                            Debug.Log("Found match by swapping right " + gem.CellHashKey);
                            foundMatch = true;
                        }
                        else if (trySwapAndMatch(ref grid, gemEntity, downGemEntity, endSimECB))
                        {
                            Debug.Log("Found match by swapping down " + gem.CellHashKey);
                            foundMatch = true;
                        }
                        else if (trySwapAndMatch(ref grid, gemEntity, upGemEntity, endSimECB))
                        {
                            Debug.Log("Found match by swapping up " + gem.CellHashKey);
                            foundMatch = true;
                        }
                    }
                }
            }).WithoutBurst().Run();

            // TODO: if (foundMatch == false), trigger blocked!
        }

        bool trySwapAndMatch(ref GridConfiguration grid, Entity gemEntity1, Entity gemEntity2, EntityCommandBuffer ecb)
        {
            var gem1 = GemService.getGemFromEntity(EntityManager, gemEntity1);
            var gem2 = GemService.getGemFromEntity(EntityManager, gemEntity2);

            if (!GemService.isMatchableType2(gem1) || !GemService.isMatchableType2(gem2))
            {
                return false;
            }

            //todo: not optimal, but works
            GemService.swapGems(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2, ref gem2);
            var matched = checkMatchPossibility(ref grid, ref gem1, ref gem2, ecb);
            GemService.swapGems(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2, ref gem2);
            return matched;
        }

        bool checkMatchPossibility(ref GridConfiguration grid, ref Gem swapedGem1, ref Gem swapedGem2,
            EntityCommandBuffer ecb)
        {
            var matchPossibilityCount = 0;
            var gemBuff = GridService.getGemBuff(EntityManager);
            for (int i = 0; i < gemBuff.Length; i++)
            {
                var gemEntity = gemBuff[i].Gem;
                if (!EntityManager.Exists(gemEntity))
                {
                    continue;
                }

                var gem = EntityManager.GetComponentData<Gem>(gemEntity);
                if (i == swapedGem1.CellHashKey)
                    gem = swapedGem1;
                else if (i == swapedGem2.CellHashKey)
                    gem = swapedGem2;

                if (GemService.isMatchableType2(gem))
                {
                    var leftGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, -1, 0);
                    var rightGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 1, 0);
                    var downGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 0, -1);
                    var upGemEntity = GemService.getNeighborGem(EntityManager, ref grid, in gem, 0, 1);
                    var leftGem = GemService.getGemFromEntity(EntityManager, leftGemEntity);
                    var rightGem = GemService.getGemFromEntity(EntityManager, rightGemEntity);
                    var downGem = GemService.getGemFromEntity(EntityManager, downGemEntity);
                    var upGem = GemService.getGemFromEntity(EntityManager, upGemEntity);

                    if (GemService.isMatchableType2(leftGem) && GemService.isMatchableType2(rightGem) &&
                        gem.GemType == leftGem.GemType &&
                        gem.GemType == rightGem.GemType)
                    {
                        matchPossibilityCount++;
                        addMatchPossibility(gemEntity, ref gem, in swapedGem1, in swapedGem2, ecb);
                        addMatchPossibility(leftGemEntity, ref leftGem, in swapedGem1, in swapedGem2, ecb);
                        addMatchPossibility(rightGemEntity, ref rightGem, in swapedGem1, in swapedGem2, ecb);
                    }
                    else if (GemService.isMatchableType2(downGem) && GemService.isMatchableType2(upGem) &&
                             gem.GemType == downGem.GemType &&
                             gem.GemType == upGem.GemType)
                    {
                        matchPossibilityCount++;
                        addMatchPossibility(gemEntity, ref gem, in swapedGem1, in swapedGem2, ecb);
                        addMatchPossibility(downGemEntity, ref downGem, in swapedGem1, in swapedGem2, ecb);
                        addMatchPossibility(upGemEntity, ref upGem, in swapedGem1, in swapedGem2, ecb);
                    }
                }
            }

            return matchPossibilityCount > 0;
        }

        void addMatchPossibility(Entity gemEntity, ref Gem gem, in Gem swapedGem1, in Gem swapedGem2,
            EntityCommandBuffer ecb)
        {
            if (!EntityManager.Exists(gemEntity))
                return;
            if (!EntityManager.HasComponent<MatchPossibility>(gemEntity))
            {
                var matchPossibility = new MatchPossibility();
                matchPossibility.NeedsSwap = (gem.CellHashKey == swapedGem1.CellHashKey ||
                                              gem.CellHashKey == swapedGem2.CellHashKey);
                matchPossibility.SwapGem1HashKey = swapedGem1.CellHashKey;
                matchPossibility.SwapGem2HashKey = swapedGem2.CellHashKey;
                //EntityManager.AddComponentData(gemEntity, matchPossibility);
                ecb.AddComponent(gemEntity, matchPossibility);
            }
        }
    }
}