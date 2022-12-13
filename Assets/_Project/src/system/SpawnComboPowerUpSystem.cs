using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB2SystemGroup))]
    [UpdateAfter(typeof(UpdateScoreSystem))]
    [UpdateBefore(typeof(DeleteMatchedGemSystem))]
    [UpdateBefore(typeof(RestoreGemSwapSystem))]
    [UpdateAfter(typeof(ActivateGemPowerUpSystem))]  //avoid activating power up that is just created
    public partial class SpawnComboPowerUpSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var grid = GridService.getGridConfiguration(EntityManager);
            if (GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            var gemSwapCount = 0;
            Entities.ForEach((Entity gemEntity, ref Matched matched, ref Gem gem, in GemSwap swap) =>
            {
                // Swaped matched gems will turn into a gem power up if a match combo was made 
                gemSwapCount++;

                var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                var leftStretch = this.calculateStretch(ref grid, gemEntity, gem,
                    new int2(gemPosition.x, gemPosition.y),
                    new int2(-1, 0));
                var rightStretch = this.calculateStretch(ref grid, gemEntity, gem,
                    new int2(gemPosition.x, gemPosition.y), new int2(1, 0));
                var upStretch = this.calculateStretch(ref grid, gemEntity, gem, new int2(gemPosition.x, gemPosition.y),
                    new int2(0, 1));
                var downStretch = this.calculateStretch(ref grid, gemEntity, gem,
                    new int2(gemPosition.x, gemPosition.y),
                    new int2(0, -1));

                var createdPowerUp = GemPowerUpTypes.None;

                // 5 in a line
                if (leftStretch + rightStretch == 4 || upStretch + downStretch == 4)
                {
                    createdPowerUp = GemPowerUpTypes.SameColor;
                }
                // L shape
                else if ((leftStretch == 2 && upStretch == 2) || (upStretch == 2 && rightStretch == 2) ||
                         (rightStretch == 2 && downStretch == 2) || (downStretch == 2 && leftStretch == 2))
                {
                    createdPowerUp = GemPowerUpTypes.Square;
                }
                // T shape
                else if (leftStretch + rightStretch + downStretch + upStretch == 4)
                {
                    createdPowerUp = GemPowerUpTypes.DiagonalCross;
                }
                // 4 in a row
                else if (leftStretch + rightStretch == 3)
                {
                    createdPowerUp = GemPowerUpTypes.Row;
                }
                // 4 in a column
                else if (upStretch + downStretch == 3)
                {
                    createdPowerUp = GemPowerUpTypes.Column;
                }

                //todo check if ref value changed 
                matched.CreatedPowerUp = createdPowerUp;
                if (createdPowerUp != GemPowerUpTypes.None)
                {
                    gemSwapCount++;
                    GemService.setGemPowerUp(EntityManager, gemEntity,ref gem, createdPowerUp);
                }
            }).WithoutBurst().Run();

            if (gemSwapCount > 0)
            {
                return;
            }

            Entities.ForEach((Entity gemEntity, ref Matched matched, ref Gem gem) =>
            {
                var isGemSwap = EntityManager.HasComponent<GemSwap>(gemEntity);
                if (!isGemSwap)
                {
                    // Falling gem cascade (not triggered by player swap) can also trigger power up creation.

                    var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                    var leftStretch = this.calculateStretch(ref grid, gemEntity, gem,
                        new int2(gemPosition.x, gemPosition.y), new int2(-1, 0));
                    var rightStretch = this.calculateStretch(ref grid, gemEntity, gem,
                        new int2(gemPosition.x, gemPosition.y), new int2(1, 0));
                    var upStretch = this.calculateStretch(ref grid, gemEntity, gem,
                        new int2(gemPosition.x, gemPosition.y), new int2(0, 1));
                    var downStretch = this.calculateStretch(ref grid, gemEntity, gem,
                        new int2(gemPosition.x, gemPosition.y), new int2(0, -1));

                    var createdPowerUp = GemPowerUpTypes.None;

                    // 5 in a line
                    if (rightStretch == 4 || upStretch == 4)
                    {
                        createdPowerUp = GemPowerUpTypes.SameColor;
                    }
                    // L shape
                    else if ((leftStretch == 2 && upStretch == 2) || (upStretch == 2 && rightStretch == 2) ||
                             (rightStretch == 2 && downStretch == 2) || (downStretch == 2 && leftStretch == 2))
                    {
                        createdPowerUp = GemPowerUpTypes.Square;
                    }
                    // T shape
                    else if (leftStretch == 1 && rightStretch == 1 && downStretch == 2)
                    {
                        createdPowerUp = GemPowerUpTypes.DiagonalCross;
                    }
                    else if (leftStretch == 1 && rightStretch == 1 && upStretch == 2)
                    {
                        createdPowerUp = GemPowerUpTypes.DiagonalCross;
                    }
                    else if (downStretch == 1 && upStretch == 1 && leftStretch == 2)
                    {
                        createdPowerUp = GemPowerUpTypes.DiagonalCross;
                    }
                    else if (downStretch == 1 && upStretch == 1 && rightStretch == 2)
                    {
                        createdPowerUp = GemPowerUpTypes.DiagonalCross;
                    }
                    // 4 in a row
                    else if (rightStretch == 3)
                    {
                        createdPowerUp = GemPowerUpTypes.Row;
                    }
                    // 4 in a column
                    else if (upStretch == 3)
                    {
                        createdPowerUp = GemPowerUpTypes.Column;
                    }

                    matched.CreatedPowerUp = createdPowerUp;
                    if (createdPowerUp != GemPowerUpTypes.None)
                    {
                        GemService.setGemPowerUp(EntityManager, gemEntity,ref gem, createdPowerUp);
                    }
                }
            }).WithoutBurst().Run();
        }

        int calculateStretch(ref GridConfiguration grid, Entity gemEntity, in Gem gem, int2 origin, int2 direction)
        {
            var count = 0;

            var currentPosition = new int2(origin.x + direction.x, origin.y + direction.y);
            for (int i = 0; i < grid.Width; i++)
            {
                var currentGem =
                    GemService.getGemAtPosition(EntityManager, ref grid, currentPosition.x, currentPosition.y);
                if (currentGem.CellHashKey >= 0 && currentGem.GemType == gem.GemType)
                {
                    currentPosition = new int2(currentPosition.x + direction.x, currentPosition.y + direction.y);
                    count++;
                }
                else
                {
                    break;
                }
            }

            return count;
        }
    }
}