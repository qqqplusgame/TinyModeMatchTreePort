using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectM
{
    [UpdateInGroup(typeof(PreGameECBSystemGroup))]
    [UpdateBefore(typeof(CheckMatchSystem))]
    public partial class SwapGemSystem : SystemBase
    {
        BeginGameECBSystem endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<BeginGameECBSystem>();
        }

        protected override void OnUpdate()
        {
            if (GameService.getGameState(EntityManager).GameStateType != GameStateTypes.Game ||
                !GameService.hasRemainingMoves(EntityManager))
            {
                return;
            }

            var grid = GridService.getGridConfiguration(EntityManager);
            if (!grid.IsGridCreated)
                return;

            //check is mouse is over UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            var previouslySelectedGemHashKey = -1;
            var previouslySelectedGemPosition = new float2();
            var hoveredGemHashKey = -1;
            var isGemAnimating = false;

            var pointerWorldPosition = InputUtil.GetInputPosition();
            var pointerDown = InputUtil.GetInputDown();

            var pointerPressed = InputUtil.GetInputPressed();

            // Find the gem currently under the input pointer.
            // todo: this is a bit of a hack, we should be able to use the mouse position directly
            Entities.ForEach((ref Gem gem, in LocalTransform gemTransform) =>
            {
                if (gem.IsFalling || gem.IsSwapping)
                {
                    // if (gem.IsFalling)
                    //     Debug.Log("Gem is falling" + gem.CellHashKey);
                    // if (gem.IsSwapping)
                    //     Debug.Log("Gem is swapping" + gem.CellHashKey);
                    isGemAnimating = true;
                    return;
                }

                if (gem.IsSelected)
                {
                    previouslySelectedGemHashKey = gem.CellHashKey;
                    previouslySelectedGemPosition =
                        new float2(gemTransform.Position.x, gemTransform.Position.y);

                    if (pointerDown)
                    {
                        gem.IsSelected = false;
                    }
                }

                var gemPosition = new float2(gemTransform.Position.x, gemTransform.Position.y);
                if (math.abs(gemPosition.x - pointerWorldPosition.x) <= grid.CellDimension / 2 &&
                    math.abs(gemPosition.y - pointerWorldPosition.y) <= grid.CellDimension / 2)
                {
                    hoveredGemHashKey = gem.CellHashKey;
                }
            }).Run();

            if (isGemAnimating)
            {
                return;
            }

            EntityCommandBuffer
                endSimECB = new EntityCommandBuffer(Allocator.Temp); //endSimECBSystem.CreateCommandBuffer();


            if (pointerDown && hoveredGemHashKey != -1)
            {
                Debug.Log("hoveredGemHashKey" + hoveredGemHashKey);
                Debug.Log("previouslySelectedGemHashKey" + previouslySelectedGemHashKey);

                // Handle gem selection and two-step click match.
                var clickedGemEntity = GemService.getGemEntity(EntityManager, hoveredGemHashKey);
                var clickedGem = GemService.getGemFromEntity(EntityManager, clickedGemEntity);
                var previouslySelectedGemEntity = GemService.getGemEntity(EntityManager, previouslySelectedGemHashKey);
                var previouslySelectedGem = GemService.getGemFromEntity(EntityManager, previouslySelectedGemEntity);
                if (previouslySelectedGemHashKey != -1 && clickedGem.CellHashKey == previouslySelectedGem.CellHashKey)
                {
                    // Deselect gem if previously clicked gem is already selected.
                    previouslySelectedGem.IsSelected = false;
                    //EntityManager.SetComponentData(previouslySelectedGemEntity, previouslySelectedGem);
                    endSimECB.SetComponent(previouslySelectedGemEntity, previouslySelectedGem);
                }
                else if (previouslySelectedGemHashKey == -1 ||
                         !GemService.areGemsNeighbor(ref grid, ref clickedGem, ref previouslySelectedGem))
                {
                    Debug.Log("clickedGem.IsSelected " + clickedGem.CellHashKey);
                    clickedGem.IsSelected = true;
                    // EntityManager.SetComponentData(clickedGemEntity, clickedGem);
                    endSimECB.SetComponent(clickedGemEntity, clickedGem);
                }
                else
                {
                    swapGems(ref grid, previouslySelectedGemEntity, ref previouslySelectedGem, clickedGemEntity,
                        ref clickedGem, endSimECB);
                }
            }
            else if (pointerPressed && hoveredGemHashKey != previouslySelectedGemHashKey &&
                     previouslySelectedGemHashKey != -1)
            {
                Debug.Log($"hoveredGemHashKey = {hoveredGemHashKey}");
                // Trigger gem swap when the selected gem is dragged over another gem.
                int2 gemToSwapWithPosition = int2.zero;
                var gemPosition = GridService.getPositionFromCellHashCode(grid, previouslySelectedGemHashKey);
                var xDiff = pointerWorldPosition.x - previouslySelectedGemPosition.x;
                var yDiff = pointerWorldPosition.y - previouslySelectedGemPosition.y;
                var isHorizontalMatch = math.abs(xDiff) > math.abs(yDiff);
                var isEdgeLeap = false;
                if (isHorizontalMatch && xDiff > 0)
                {
                    gemToSwapWithPosition = new int2(gemPosition.x + 1, gemPosition.y);
                    if (gemPosition.x == grid.Width - 1)
                        isEdgeLeap = true;
                }
                else if (isHorizontalMatch && xDiff < 0)
                {
                    gemToSwapWithPosition = new int2(gemPosition.x - 1, gemPosition.y);
                    if (gemPosition.x == 0)
                        isEdgeLeap = true;
                }
                else if (!isHorizontalMatch && yDiff > 0)
                {
                    gemToSwapWithPosition = new int2(gemPosition.x, gemPosition.y + 1);
                    if (gemPosition.y == grid.Height - 1)
                        isEdgeLeap = true;
                }
                else if (!isHorizontalMatch && yDiff < 0)
                {
                    gemToSwapWithPosition = new int2(gemPosition.x, gemPosition.y - 1);
                    if (gemPosition.y == 0)
                        isEdgeLeap = true;
                }


                var previouslySelectedGemEntity = GemService.getGemEntity(EntityManager, previouslySelectedGemHashKey);
                var previouslySelectedGem = GemService.getGemFromEntity(EntityManager, previouslySelectedGemEntity);

                if (!isEdgeLeap)
                {
                    var gemToSwapWithEntity = GemService.getGemEntityAtPosition(EntityManager, grid,
                        gemToSwapWithPosition.x, gemToSwapWithPosition.y);
                    var gemToSwapWith = GemService.getGemFromEntity(EntityManager, gemToSwapWithEntity);
                    if (gemToSwapWith.CellHashKey >= 0)
                    {
                        swapGems(ref grid, previouslySelectedGemEntity, ref previouslySelectedGem, gemToSwapWithEntity,
                            ref gemToSwapWith, endSimECB);
                    }
                }


                previouslySelectedGem.IsSelected = false;
                endSimECB.SetComponent(previouslySelectedGemEntity, previouslySelectedGem);
            }

            endSimECB.Playback(EntityManager);
            endSimECB.Dispose();
        }

        void swapGems(ref GridConfiguration grid, Entity gemEntity1, ref Gem gem1, Entity gemEntity2, ref Gem gem2,
            EntityCommandBuffer ecb)
        {
            if (!EntityManager.HasComponent<GemSwap>(gemEntity1))
            {
                //EntityManager.AddComponent<GemSwap>(gemEntity1);
                ecb.AddComponent<GemSwap>(gemEntity1);
            }

            if (!EntityManager.HasComponent<GemSwap>(gemEntity2))
            {
                //EntityManager.AddComponent<GemSwap>(gemEntity2);
                ecb.AddComponent<GemSwap>(gemEntity2);
            }

            GemService.swapGems(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2, ref gem2);
            GemService.animateGemsSwap(EntityManager, ecb, ref grid, gemEntity1, ref gem1, gemEntity2, ref gem2);

            //todo SoundService.play(this.world, "GemSwapSound");
            AudioUtils.PlaySound(EntityManager, "CellSwap");
        }
    }
}