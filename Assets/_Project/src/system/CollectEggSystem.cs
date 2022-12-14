using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB3SystemGroup))]
    [UpdateAfter(typeof(ActivateGemPowerUpSystem))]
    public partial class CollectEggSystem : SystemBase
    {
        GameECB3System endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECB3System>();
        }

        protected override void OnUpdate()
        {
            var em = EntityManager;
            if (GameService.getGameState(em).GameStateType != GameStateTypes.Game)
            {
                return;
            }

            var levelEntity = GameService.getCurrentLevelEntity(em);
            if (!em.HasComponent<LevelEggObjective>(levelEntity))
            {
                return;
            }

            var levelEggObjective = em.GetComponentData<LevelEggObjective>(levelEntity);

            int matchedCount = 0;
            Entities.ForEach((Entity entity, in Gem gem, in Matched matched) => { matchedCount++; }).Run();

            int gemSwapCount = 0;
            Entities.ForEach((Entity entity, in Gem gem, in GemSwap gemSwap) => { gemSwapCount++; }).Run();

            var grid = GridService.getGridConfiguration(em);
            int collectedEggCount = 0;

            var ecb = endSimECBSystem.CreateCommandBuffer();
            if (matchedCount == 0 && gemSwapCount == 0)
            {
                Entities.ForEach((Entity entity, in Gem gem, in LocalTransform  transformPosition) =>
                {
                    var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);
                    if (gem.GemType == GemTypes.Egg && gemPosition.y == 0 && !gem.IsFalling && !gem.IsSwapping)
                    {
                        GemService.addMatchedComponent(em, ecb, entity, false);

                        levelEggObjective.CollectedEggs++;
                        ecb.SetComponent(levelEntity, levelEggObjective);

                        //todo Spawn gain egg currency particle
                        {
                            //var gameUI = this.world.getComponentData(this.world.getEntityByName("GameUI"), game.GameUI);
                            // var destinationPosition =
                            //     ut.Core2D.TransformService.computeWorldPosition(this.world, gameUI.ImageObjectiveEgg);
                            //
                            // let collectedEggEntity = ut.EntityGroup.instantiate(this.world, "game.CollectedEgg")[0];
                            //
                            // let collectedEggCurrency =
                            //     this.world.getComponentData(collectedEggEntity, game.CollectedCurrency);
                            // collectedEggCurrency.StartPosition = transformPosition.position;
                            // collectedEggCurrency.MidPosition = new Vector3(-100, 0, 0);
                            // collectedEggCurrency.EndPosition = destinationPosition;
                            // collectedEggCurrency.StartDelay = collectedEggCount * 0.2;
                            // this.world.setComponentData(collectedEggEntity, collectedEggCurrency);
                            //
                            // let collectedEggTransformPosition =
                            //     this.world.getComponentData(collectedEggEntity, ut.Core2D.TransformLocalPosition);
                            // collectedEggTransformPosition.position = transformPosition.position;
                            // this.world.setComponentData(collectedEggEntity, collectedEggTransformPosition);
                        }

                        collectedEggCount++;
                    }
                }).Run();
                
                if (collectedEggCount > 0)
                {
                    UiService.UpdateUi(GameUiUpdateType.GameUIUpdateObjectiveChange);
                }
            }
        }
    }
}