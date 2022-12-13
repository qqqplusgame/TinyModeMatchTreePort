using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB4SystemGroup))]
    [UpdateAfter(typeof(DeleteMatchedGemSystem))]
    [UpdateAfter(typeof(CreateNewGemBoardSystem))]
    public partial class ReplenishGemBoardSystem : SystemBase
    {
        private Random r;


        GameECB4System endSimECBSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            r = new Random();
            //r.InitState();

            endSimECBSystem = World.GetOrCreateSystemManaged<GameECB4System>();
        }

        protected override void OnUpdate()
        {
            if (GameService.getGameState(EntityManager).GameStateType != GameStateTypes.Game ||
                GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            r.InitState(1234U);
            EntityCommandBuffer ecb = endSimECBSystem.CreateCommandBuffer();
            NativeList<int> possibleGemTypes = new NativeList<int>(Allocator.Temp)
            {
                0, 1, 2, 3, 4, 5
            };

            var currentLevel = GameService.getCurrentLevel(EntityManager);
            foreach (var missingGem in currentLevel.LevelConfig.Value.MissingGems.ToArray())
            {
                var indexToRemove = possibleGemTypes.IndexOf((int)missingGem);
                possibleGemTypes.RemoveAt(indexToRemove);
            }


            var grid = GridService.getGridConfiguration(EntityManager);
            for (int i = 0; i < grid.Width; i++)
            {
                int fallOffset = 0;
                for (int j = 0; j < grid.Height; j++)
                {
                    var existingGemEntity = GemService.getGemEntityAtPosition(EntityManager, grid, i, j);

                    if (existingGemEntity == Entity.Null)
                    {
                        var cellHashKey = GridService.getCellHashCode(grid, i, j);
                        GemTypes gemType = (GemTypes)possibleGemTypes[r.NextInt(0, possibleGemTypes.Length)];
                        var newGemEntity = GemService.createGem(EntityManager, ecb, ref grid,
                            cellHashKey, gemType);
                        var newGem = EntityManager.GetComponentData<Gem>(newGemEntity);
                        var spawnYPosition = grid.Height + fallOffset;
                        var transformLocalPosition =
                            EntityManager.GetComponentData<LocalTransform>(newGemEntity);
                        var newPosition = GridService.getGridToWorldPosition(grid, i, spawnYPosition);
                        transformLocalPosition.Position.x = newPosition.x;
                        transformLocalPosition.Position.y = newPosition.y;
                        ecb.SetComponent(newGemEntity, transformLocalPosition);

                        //Debug.Log($"Replenish animateGemFall {World.Time.ElapsedTime} ");
                        GemService.animateGemFall(EntityManager, ecb, ref grid, newGemEntity, ref newGem,
                            cellHashKey, spawnYPosition - j, transformLocalPosition.Position);

                        fallOffset++;
                    }
                }
            }

            possibleGemTypes.Dispose();
        }
    }
}