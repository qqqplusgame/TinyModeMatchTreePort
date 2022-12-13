using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB3SystemGroup))]
    //[UpdateAfter(typeof(ActivateGemPowerUpSystem))]
    [UpdateBefore(typeof(DeleteMatchedGemSystem))]
    public partial class UpdateScoreSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (GameService.getGameState(EntityManager).GameStateType != GameStateTypes.Game ||
                GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            int matchedGemCount = 0;
            var gemPositionSum = new float2();
            Entities.ForEach(
                (Entity entity, in LocalTransform transform, in Gem gemToDestroy, in Matched matched) =>
                {
                    matchedGemCount++;
                    gemPositionSum.x += transform.Position.x;
                    gemPositionSum.y += transform.Position.y;
                }).Run();

            if (matchedGemCount <= 0)
            {
                return;
            }

            // Update current score.
            var gameState = GameService.getGameState(EntityManager);
            var scoreUnitCount = math.max(1, matchedGemCount - 2);
            var scoreGain = scoreUnitCount * 25;
            gameState.CurrentScore += scoreGain;
            GameService.setGameState(EntityManager, gameState);


            // Spawn floating score gain label.
            gemPositionSum.x = gemPositionSum.x / matchedGemCount;
            gemPositionSum.y = gemPositionSum.y / matchedGemCount;
            //spawnScoreGainLabel(gemPositionSum, scoreUnitCount, scoreGain);

            // Update survival mode timer.
            var levelEntity = GameService.getCurrentLevelEntity(EntityManager);
            if (EntityManager.HasComponent<LevelSurvival>(levelEntity))
            {
                var levelSurvival = SystemAPI.GetComponent<LevelSurvival>(levelEntity);

                var difficultyRatio = math.min(1, gameState.Time / levelSurvival.DifficulyRampUpTime);
                var timeGain = levelSurvival.EndTimeGainByMatch + (1 - difficultyRatio) *
                    (levelSurvival.StartTimeGainByMatch - levelSurvival.EndTimeGainByMatch);
                levelSurvival.SurvivalTimer += scoreUnitCount * timeGain;
                levelSurvival.SurvivalTimer =
                    math.max(0, math.min(levelSurvival.MaxSurvivalTime, levelSurvival.SurvivalTimer));
                SystemAPI.SetComponent(levelEntity, levelSurvival);
            }

            UiService.UpdateUi(GameUiUpdateType.GameUIUpdateObjectiveChange);
        }

        // void spawnScoreGainLabel(float2 position, int scoreUnitCount, int scoreGain)
        // {
        //    
        // var scoreGainEntity = ut.EntityGroup.instantiate(this.world, "game.ScoreGainLabel")[0];
        // let scoreGainTransform = this.world.getComponentData(scoreGainEntity, ut.Core2D.TransformLocalPosition);
        // scoreGainTransform.position.x = position.x;
        // scoreGainTransform.position.y = position.y + 26;
        // this.world.setComponentData(scoreGainEntity, scoreGainTransform);
        //
        // let label = this.world.getComponentData(scoreGainEntity, ut.Text.Text2DRenderer);
        // label.text = String(scoreGain);
        // this.world.setComponentData(scoreGainEntity, label);
        //
        // let transformScale = this.world.getComponentData(scoreGainEntity, ut.Core2D.TransformLocalScale);
        // let scale = Math.min(4, 1.8 + (scoreUnitCount - 1) * 0.12);
        // transformScale.scale = new Vector3(scale, scale, 1);
        // this.world.setComponentData(scoreGainEntity, transformScale);
        // }
    }
}