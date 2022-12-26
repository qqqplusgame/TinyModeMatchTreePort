using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB2SystemGroup))]
    [UpdateAfter(typeof(CheckMatchSystem))]
    [UpdateBefore(typeof(RestoreGemSwapSystem))]
    public partial class ActivateGemPowerUpSystem : SystemBase
    {
        public bool sameColorBombTriggeredThisFrame = false;
        GameECB2System gameECBSystem;

        protected override void OnCreate()
        {
            gameECBSystem = World.GetOrCreateSystemManaged<GameECB2System>();
        }

        protected override void OnUpdate()
        {
            if (GameService.getGameState(EntityManager).GameStateType != GameStateTypes.Game)
            {
                return;
            }

            var ecb = gameECBSystem.CreateCommandBuffer();
            var em = EntityManager;
            var grid = GridService.getGridConfiguration(em);

            var gameMangerEntity = GameService.GetGameManagerEntity();
            var gameManager = em.GetComponentData<GameManager>(gameMangerEntity);

            sameColorBombTriggeredThisFrame = false;

            var deltaTime = World.Time.DeltaTime;

            if (grid.FrozenGridTimer > 0)
            {
                grid.FrozenGridTimer -= deltaTime;
                GridService.setGridConfiguration(em, grid);
            }
            else
            {
                NativeHashSet<Entity> entitiesToDestroy =
                    new NativeHashSet<Entity>(grid.Width * grid.Height, Allocator.TempJob);
                activeSameColorBombAfterSwap(em, ecb, gameManager, ref grid, entitiesToDestroy);
                activateMatchedBombs(em, ecb, gameManager, ref grid, entitiesToDestroy);

                entitiesToDestroy.Dispose();
            }
        }


        void activeSameColorBombAfterSwap(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            ref GridConfiguration grid
            , in NativeHashSet<Entity> entitiesToDestroy)
        {
            var swapedGemEntities = new NativeList<Entity>(Allocator.Temp);

            var manager = em;
            Entities.ForEach((Entity entity, in Gem gem, in GemSwap swaped) =>
            {
                if (!gem.IsSwapping)
                {
                    var gridGemEntity = GemService.getGemEntity(manager, gem.CellHashKey);
                    var gridGem = GemService.getGem(manager, gem.CellHashKey);
                    if (gridGem.CellHashKey >= 0 && !gridGem.IsSwapping)
                    {
                        swapedGemEntities.Add(gridGemEntity);
                    }
                }
            }).Run();

            if (swapedGemEntities.Length != 2)
            {
                return;
            }

            var gem1 = GemService.getGemFromEntity(em, swapedGemEntities[0]);
            var gem2 = GemService.getGemFromEntity(em, swapedGemEntities[1]);
            if (gem1.PowerUp == GemPowerUpTypes.SameColor || gem2.PowerUp == GemPowerUpTypes.SameColor)
            {
                var bombGemEntity = gem1.PowerUp == GemPowerUpTypes.SameColor
                    ? swapedGemEntities[0]
                    : swapedGemEntities[1];
                var otherGemEntity = gem1.PowerUp == GemPowerUpTypes.SameColor
                    ? swapedGemEntities[1]
                    : swapedGemEntities[0];
                var bombGem = gem1.PowerUp == GemPowerUpTypes.SameColor ? gem1 : gem2;
                var otherGem = gem1.PowerUp == GemPowerUpTypes.SameColor ? gem2 : gem1;
                var bombGemPosition = GridService.getPositionFromCellHashCode(grid, bombGem.CellHashKey);

                if (otherGem.PowerUp != GemPowerUpTypes.SameColor && otherGem.GemType != GemTypes.Egg)
                {
                    if (!em.HasComponent<Matched>(otherGemEntity)) // && !entitiesToDestroy.Contains(otherGemEntity))
                    {
                        GameService.incrementMoveCounter(em);
                    }

                    sameColorBombTriggeredThisFrame = true;
                    addMatchedComponent(em, ecb, bombGemEntity, entitiesToDestroy);

                    // var bombGemSpriteLayerSorting =
                    //     em.GetComponentData(bombGemEntity, ut.Core2D.LayerSorting);
                    // bombGemSpriteLayerSorting.order = 1000;
                    // em.setComponentData(bombGemEntity, bombGemSpriteLayerSorting);
                    // var bombGemPowerUpSpriteLayerSorting =
                    //     em.getComponentData(bombGem.SameColorPowerUpVisual, ut.Core2D.LayerSorting);
                    // bombGemPowerUpSpriteLayerSorting.order = 1001;
                    // em.setComponentData(bombGem.SameColorPowerUpVisual, bombGemPowerUpSpriteLayerSorting);
                    //
                    // SoundService.play(em, "Slash");
                    AudioUtils.PlaySound(em, "Slash");
                    var gemBuff = GridService.getGemBuff(em);
                    for (int i = 0; i < gemBuff.Length; i++)
                    {
                        var gemEntity = gemBuff[i].Gem;
                        if (em.Exists(gemEntity))
                        {
                            var gem = em.GetComponentData<Gem>(gemEntity);
                            var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                            if (gem.GemType == otherGem.GemType)
                            {
                                destroyGem(em, ecb, gameManager, ref grid, gemEntity, false, entitiesToDestroy);

                                var startPosition =
                                    GridService.getGridToWorldPosition(grid, bombGemPosition.x, bombGemPosition.y);
                                var endPosition =
                                    GridService.getGridToWorldPosition(grid, gemPosition.x, gemPosition.y);
                                spawnDestroyLaserAnimation(em, ecb, gameManager, startPosition.x, startPosition.y,
                                    endPosition.x,
                                    endPosition.y);
                            }
                        }
                    }
                }
            }
            else if (gem1.PowerUp != GemPowerUpTypes.None && gem2.PowerUp != GemPowerUpTypes.None)
            {
                // If two power ups are swaped together, they are both triggered.
                addMatchedComponent(em, ecb, swapedGemEntities[0], entitiesToDestroy);
                addMatchedComponent(em, ecb, swapedGemEntities[1], entitiesToDestroy);
            }

            swapedGemEntities.Dispose();
        }

        void activateMatchedBombs(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            ref GridConfiguration grid,
            in NativeHashSet<Entity> entitiesToDestroy)
        {
            NativeHashMap<int, Gem> bombGems = new NativeHashMap<int, Gem>(grid.Width * grid.Height, Allocator.Temp);

            var manager = em;
            Entities.ForEach((Entity entity, in Gem gem, in Matched matched) =>
            {
                if (gem.PowerUp != GemPowerUpTypes.None && gem.PowerUp != GemPowerUpTypes.SameColor)
                {
                    bombGems.Add(gem.CellHashKey, GemService.getGem(manager, gem.CellHashKey));
                }
            }).Run();

            foreach (var entity in entitiesToDestroy)
            {
                if (em.HasComponent<Gem>(entity))
                {
                    var gem = em.GetComponentData<Gem>(entity);
                    if (!bombGems.ContainsKey(gem.CellHashKey) && gem.PowerUp != GemPowerUpTypes.None &&
                        gem.PowerUp != GemPowerUpTypes.SameColor)
                    {
                        bombGems.Add(gem.CellHashKey, gem);
                    }
                }
            }

            var bombTriggered = false;
            foreach (var gem in bombGems)
            {
                bombTriggered = activateBomb(em, ecb, gameManager, ref grid, gem.Value, entitiesToDestroy) ||
                                bombTriggered;
            }


            if (bombTriggered && grid.FrozenGridTimer < 0.3 &&
                !sameColorBombTriggeredThisFrame)
            {
                grid.FrozenGridTimer = 0.35f;
                GridService.setGridConfiguration(EntityManager, grid);
            }

            bombGems.Dispose();
        }

        bool activateBomb(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            ref GridConfiguration grid,
            in Gem gem,
            in NativeHashSet<Entity> entitiesToDestroy)
        {
            var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
            switch (gem.PowerUp)
            {
                case GemPowerUpTypes.Row:
                {
                    for (int i = 0; i < grid.Width; i++)
                    {
                        destroyGem(em, ecb, gameManager, ref grid,
                            GemService.getGemEntityAtPosition(em, grid, i, gemPosition.y), true, entitiesToDestroy);
                    }

                    var startPosition = GridService.getGridToWorldPosition(grid, 0, gemPosition.y);
                    var endPosition = GridService.getGridToWorldPosition(grid, grid.Width - 1, gemPosition.y);
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPosition.x, startPosition.y, endPosition.x,
                        endPosition.y);
                    //SoundService.play(EntityManager, "Slash");
                    AudioUtils.PlaySound(em, "Slash");
                    return true;
                }
                case GemPowerUpTypes.Column:
                {
                    for (int j = 0; j < grid.Height; j++)
                    {
                        destroyGem(em, ecb, gameManager, ref grid,
                            GemService.getGemEntityAtPosition(em, grid, gemPosition.x, j), true, entitiesToDestroy);
                    }

                    var startPosition = GridService.getGridToWorldPosition(grid, gemPosition.x, 0);
                    var endPosition = GridService.getGridToWorldPosition(grid, gemPosition.x, grid.Height - 1);
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPosition.x, startPosition.y, endPosition.x,
                        endPosition.y);
                    //SoundService.play(EntityManager, "Slash");
                    AudioUtils.PlaySound(em, "Slash");
                    return true;
                }
                case GemPowerUpTypes.Square:
                {
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, -1, 1),
                        true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, -1, 0),
                        true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, -1, -1),
                        true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, 0, 1), true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, 0, -1),
                        true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, 1, 1), true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, 1, 0), true,
                        entitiesToDestroy);
                    destroyGem(em, ecb, gameManager, ref grid, GemService.getNeighborGem(em, ref grid, gem, 1, -1),
                        true,
                        entitiesToDestroy);
                    var startPositionLeft =
                        GridService.getGridToWorldPosition(grid, gemPosition.x - 1, gemPosition.y + 1);
                    var endPositionLeft =
                        GridService.getGridToWorldPosition(grid, gemPosition.x - 1, gemPosition.y - 1);
                    var startPositionRight =
                        GridService.getGridToWorldPosition(grid, gemPosition.x + 1, gemPosition.y + 1);
                    var endPositionRight =
                        GridService.getGridToWorldPosition(grid, gemPosition.x + 1, gemPosition.y - 1);
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPositionLeft.x, startPositionLeft.y + 10,
                        endPositionLeft.x + 5,
                        endPositionLeft.y - 10);
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPositionRight.x, startPositionRight.y + 10,
                        endPositionRight.x - 5, endPositionRight.y - 10);
                    //SoundService.play(EntityManager, "Slash");
                    AudioUtils.PlaySound(em, "Slash");
                    return true;
                }
                case GemPowerUpTypes.DiagonalCross:
                {
                    for (int i = 0; i < grid.Width; i++)
                    {
                        destroyGem(em, ecb, gameManager, ref grid,
                            GemService.getGemEntityAtPosition(em, grid, i,
                                gemPosition.y + gemPosition.x - i), true, entitiesToDestroy);
                    }

                    for (int i = 0; i < grid.Width; i++)
                    {
                        destroyGem(em, ecb, gameManager, ref grid,
                            GemService.getGemEntityAtPosition(em, grid, i,
                                gemPosition.y + i - gemPosition.x), true, entitiesToDestroy);
                    }

                    var startPosition1 = findDiagonalEnd(ref grid, new int2(gemPosition.x, gemPosition.y),
                        new int2(-1, -1));
                    var endPosition1 = findDiagonalEnd(ref grid, new int2(gemPosition.x, gemPosition.y),
                        new int2(1, 1));
                    var startPosition2 = findDiagonalEnd(ref grid, new int2(gemPosition.x, gemPosition.y),
                        new int2(-1, 1));
                    var endPosition2 = findDiagonalEnd(ref grid, new int2(gemPosition.x, gemPosition.y),
                        new int2(1, -1));
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPosition1.x, startPosition1.y, endPosition1.x,
                        endPosition1.y);
                    spawnDestroyLineAnimation(em, ecb, gameManager, startPosition2.x, startPosition2.y, endPosition2.x,
                        endPosition2.y);
                    //SoundService.play(EntityManager, "Slash");
                    AudioUtils.PlaySound(em, "Slash");
                    return true;
                }
            }

            return false;
        }

        float2 findDiagonalEnd(ref GridConfiguration grid, int2 current, int2 direction)
        {
            //fix a bug when the gem is on the edge of the grid,animation will not be displayed
            //,casue the positon is not correct
            if ((current.x == 0 && direction.x < 0) ||
                (current.x == grid.Width - 1 && direction.x > 0) ||
                (current.y == 0 && direction.y < 0) ||
                (current.y == grid.Height - 1 && direction.y > 0))
            {
                var worldPositon = GridService.getGridToWorldPosition(grid, current.x, current.y);
                return new float2(worldPositon.x, worldPositon.y);
            }
            else
            {
                return findDiagonalEnd(ref grid, new int2(current.x + direction.x, current.y + direction.y),
                    direction);
            }
        }

        void destroyGem(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            ref GridConfiguration grid, Entity gemEntity,
            bool triggerBomb, in NativeHashSet<Entity> entitiesToDestroy)
        {
            var gem = GemService.getGemFromEntity(em, gemEntity);
            if (gem.CellHashKey >= 0 && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb)
            {
                var hadComponent = em.HasComponent<Matched>(gemEntity) || entitiesToDestroy.Contains(gemEntity);
                addMatchedComponent(em, ecb, gemEntity, entitiesToDestroy);

                if (triggerBomb && !hadComponent)
                {
                    activateBomb(em, ecb, gameManager, ref grid, gem, entitiesToDestroy);
                }
            }
        }

        void addMatchedComponent(in EntityManager em, in EntityCommandBuffer ecb, Entity gemEntity,
            in NativeHashSet<Entity> entitiesToDestroy)
        {
            if (!em.HasComponent<Matched>(gemEntity) && !entitiesToDestroy.Contains(gemEntity))
            {
                ecb.AddComponent(gemEntity, new Matched()
                {
                    CreatedPowerUp = GemPowerUpTypes.None,
                    IsMatch = false
                });
                entitiesToDestroy.Add(gemEntity);
            }
        }

        void spawnDestroyLineAnimation(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            float startPositionX,
            float startPositionY, float endPositionX,
            float endPositionY)
        {
            var entity = em.Instantiate(gameManager.DestroyLineAnimationPrefab);
            var destroyLineAnimation = em.GetComponentData<DestroyLineAnimation>(entity);
            destroyLineAnimation.StartPositionX = startPositionX;
            destroyLineAnimation.StartPositionY = startPositionY;
            destroyLineAnimation.EndPositionX = endPositionX;
            destroyLineAnimation.EndPositionY = endPositionY;
            em.SetComponentData(entity, destroyLineAnimation);
        }

        void spawnDestroyLaserAnimation(in EntityManager em, in EntityCommandBuffer ecb, in GameManager gameManager,
            float startPositionX,
            float startPositionY, float endPositionX,
            float endPositionY)
        {
            var entity = em.Instantiate(gameManager.DestroyLaserAnimationPrefab);
            var destroyLaserAnimation = em.GetComponentData<DestroyLaserAnimation>(entity);
            destroyLaserAnimation.StartPositionX = startPositionX;
            destroyLaserAnimation.StartPositionY = startPositionY;
            destroyLaserAnimation.EndPositionX = endPositionX;
            destroyLaserAnimation.EndPositionY = endPositionY;
            em.SetComponentData(entity, destroyLaserAnimation);
        }
    }
}