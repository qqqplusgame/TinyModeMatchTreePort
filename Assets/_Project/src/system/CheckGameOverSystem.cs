using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(FindPossibleMatchSystem))]
    public partial class CheckGameOverSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;
            var gameState = GameService.getGameState(em);

            if (gameState.GameStateType != GameStateTypes.Game ||
                GridService.isGridFrozen(em))
            {
                return;
            }

            //count total gaming time
            gameState.Time += SystemAPI.Time.DeltaTime;
            GameService.setGameState(em, gameState);


            var isGridAnimating = false;

            Entities.ForEach((in Gem gem) =>
            {
                if (gem.IsFalling || gem.IsSwapping)
                {
                    isGridAnimating = true;
                }
            }).Run();

            if (isGridAnimating)
                return;

            var levelEntity = GameService.getCurrentLevelEntity(em);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            // Trigger game over in limited move count modes.
            if (!GameService.hasRemainingMoves(em))
            {
                GameStateLoadingService.SetGameState(em, ecb, GameStateTypes.GameOver);
            }
            // Trigger game over in survival mode.
            else if (em.HasComponent<LevelSurvival>(levelEntity))
            {
                var levelSurvival = em.GetComponentData<LevelSurvival>(levelEntity);

                levelSurvival.SurvivalTimer -= levelSurvival.TimeDepleteRate * SystemAPI.Time.DeltaTime;
                levelSurvival.SurvivalTimer =
                    math.max(0, math.min(levelSurvival.MaxSurvivalTime, levelSurvival.SurvivalTimer));
                em.SetComponentData(levelEntity, levelSurvival);

                UiService.UpdateUi(GameUiUpdateType.GameUIUpdateSurvivalTimer);
                if (levelSurvival.SurvivalTimer <= 0)
                {
                    GameStateLoadingService.SetGameState(em, ecb, GameStateTypes.GameOver);
                }
            }

            ecb.Playback(em);
        }
    }
}