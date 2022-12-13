using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECBSystemGroup))]
    public partial class CreateNewGemBoardSystem : SystemBase
    {
        private Random random;
        GameECBSystem endSimECBSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            random = new Random();
            random.InitState();
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECBSystem>();
        }

        protected override void OnUpdate()
        {
            var em = EntityManager;
            if (GameService.getGameState(em).GameStateType != GameStateTypes.Game)
            {
                return;
            }

            //create grid

            var grid = GridService.getGridConfiguration(em);

            //null grid check
            if (grid.CellDimension <= 0)
                return;

            if (grid.IsGridCreated)
                return;
            EntityCommandBuffer ecb = endSimECBSystem.CreateCommandBuffer();

            if (em.HasComponent<LevelSurvival>(GameService.getCurrentLevelEntity(em)))
            {
                grid.GridOffsetPositionY = -7;
            }
            else
            {
                grid.GridOffsetPositionY = 0;
            }

            GridService.createGridCells(em, ecb, ref grid);

            var gemEntities = createBoard(grid, ecb);
            placeStartEggs(gemEntities, grid);
            placeStartPowerUps(gemEntities);

            grid.IsGridCreated = true;

            GridService.setGridConfiguration(em, grid);

            gemEntities.Dispose();

            AudioUtils.PlayMusic(em);
        }

        public NativeList<Entity> createBoard(GridConfiguration grid, EntityCommandBuffer ecb)
        {
            var createdGems = new NativeList<Entity>(Allocator.Persistent);
            var currentLevel = GameService.getCurrentLevel(EntityManager);
            //random.InitState(100000U);
            for (int i = 0; i < grid.Width; i++)
            {
                for (int j = 0; j < grid.Height; j++)
                {
                    //need to be optimized
                    NativeList<int> possibleGemTypes = new NativeList<int>(Allocator.Temp)
                    {
                        0, 1, 2, 3, 4, 5
                    };

                    ref var array = ref currentLevel.LevelConfig.Value.MissingGems;
                    for (int k = 0; k < array.Length; k++)
                    {
                        var missingGem = array[k];
                        int indexToRemove = possibleGemTypes.IndexOf((int)missingGem);
                        possibleGemTypes.RemoveAt(indexToRemove);
                    }


                    var leftGem = GemService.getGemAtPosition(EntityManager, ref grid, i - 1, j);
                    var secondLeftGem = GemService.getGemAtPosition(EntityManager, ref grid, i - 2, j);
                    if (leftGem.CellHashKey >= 0 && secondLeftGem.CellHashKey >= 0 &&
                        leftGem.GemType == secondLeftGem.GemType)
                    {
                        removePossibleGemType(ref possibleGemTypes, leftGem.GemType);
                    }

                    var downGem = GemService.getGemAtPosition(EntityManager, ref grid, i, j - 1);
                    var secondDownGem = GemService.getGemAtPosition(EntityManager, ref grid, i, j - 2);
                    if (downGem.CellHashKey >= 0 && secondDownGem.CellHashKey >= 0 &&
                        downGem.GemType == secondDownGem.GemType)
                    {
                        removePossibleGemType(ref possibleGemTypes, downGem.GemType);
                    }

                    var gemType = possibleGemTypes[random.NextInt(0, possibleGemTypes.Length)];
                    var gemEntity = GemService.createGemOfType(EntityManager, ecb, ref grid,
                        GridService.getCellHashCode(grid, i, j), (GemTypes)gemType);
                    createdGems.Add(gemEntity);

                    possibleGemTypes.Dispose();
                }
            }

            return createdGems;
        }


        void removePossibleGemType(ref NativeList<int> possibleGemTypes, GemTypes gemType)
        {
            for (int i = 0; i < possibleGemTypes.Length; i++)
            {
                if (possibleGemTypes[i] == (int)gemType)
                {
                    possibleGemTypes.RemoveAt(i);
                    i--;
                }
            }
        }

        void placeStartEggs(NativeList<Entity> gemEntities, GridConfiguration grid)
        {
            NativeList<int> gemXPositions = new NativeList<int>(Allocator.Temp); //{1, 2, 3, 4, 5, 6, 7};
            for (int i = 0; i < 7; i++)
            {
                gemXPositions.Add(i + 1);
            }

            var levelEntity = GameService.getCurrentLevelEntity(EntityManager);
            if (EntityManager.HasComponent<LevelEggObjective>(levelEntity))
            {
                var levelEggObjective = EntityManager.GetComponentData<LevelEggObjective>(levelEntity);
                var eggCountAtStart = levelEggObjective.EggsInGridAtStart;
                for (int i = 0; i < eggCountAtStart; i++)
                {
                    int randomIndex =
                        random.NextInt(0, gemXPositions.Length); //Math.floor(Math.random() * gemXPositions.length);
                    int randomGemXPosition = gemXPositions[randomIndex];
                    gemXPositions.RemoveAt(randomIndex);

                    // Entity gemEntity; // = new ut.Entity();
                    for (int j = 0; j < gemEntities.Length; j++)
                    {
                        var currentGemEntity = gemEntities[j];
                        var currentGem = EntityManager.GetComponentData<Gem>(currentGemEntity);
                        var gemPosition = GridService.getPositionFromCellHashCode(grid, currentGem.CellHashKey);
                        if (gemPosition.y == grid.Height - 1 && gemPosition.x == randomGemXPosition)
                        {
                            var gemEntity = gemEntities[j];
                            GemService.setSpecialGemType(EntityManager, currentGemEntity, ref currentGem,
                                GemTypes.Egg);
                            EntityManager.SetComponentData(gemEntity, currentGem);
                            //todo need to check working or not?!
                            gemEntities.RemoveAt(j);
                            break;
                        }
                    }
                }
            }

            gemXPositions.Dispose();
        }

        void placeStartPowerUps(NativeList<Entity> gemEntities)
        {
            var level = GameService.getCurrentLevel(EntityManager);
            ref var powerUpsToPlace = ref level.LevelConfig.Value.StartPowerUps;
            for (int i = 0; i < powerUpsToPlace.Length; i++)
            {
                if (gemEntities.Length == 0)
                    break;
                int randomGemIndex = random.NextInt(0, gemEntities.Length);
                var gemEntity = gemEntities[randomGemIndex];
                var gem = EntityManager.GetComponentData<Gem>(gemEntity);
                GemService.setGemPowerUp(EntityManager, gemEntity, ref gem, powerUpsToPlace[i]);
                EntityManager.SetComponentData(gemEntity, gem);

                gemEntities.RemoveAt(randomGemIndex);
            }
        }
    }
}