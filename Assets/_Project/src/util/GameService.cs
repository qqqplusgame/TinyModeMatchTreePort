using System;
using ProjectM.UI;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectM
{
    public class GameService
    {
        private static Entity gameStateEntity;
        private static Entity gameMangerEntity;

        public static void init(in EntityManager em, Entity gmEntity)
        {
            gameStateEntity = em.CreateEntity();
            gameMangerEntity = gmEntity;
#if UNITY_EDITOR
            em.SetName(gameStateEntity, "GameService");
#endif
            //test GameState Data
            em.AddComponentData(gameStateEntity, new GameState()
            {
                CurrentLevelID = 1,
                CurrentMoveCount = 0,
                CurrentScore = 0,
                EnvironmentSceneWidth = 100.0f,
                GameStateType = GameStateTypes.WorldMap,
                LevelCount = 1,
                ShowHintDelay = 1,
                SurvivalTimer = 0f,
                Time = 0
            });
            UpdateGameStateChange(em);
        }

        public static void UpdateGameStateChange(in EntityManager em)
        {
            var e = em.CreateEntity();
            em.AddComponent<GameStateChange>(e);
        }

        public static Entity GetGameManagerEntity()
        {
            return gameMangerEntity;
        }

        public static GameState getGameState(in EntityManager em)
        {
            if (!em.Exists(gameStateEntity))
            {
                return new GameState()
                {
                    GameStateType = GameStateTypes.None
                };
            }

            return em.GetComponentData<GameState>(gameStateEntity);
        }

        public static void setGameState(in EntityManager em, in GameState gameState)
        {
            if (!em.Exists(gameStateEntity))
            {
                return;
            }

            em.SetComponentData(gameStateEntity, gameState);
        }

        public static void changeGameStateTypeWithUpdate(in EntityManager em, GameStateTypes gameStateType)
        {
            var gameState = getGameState(em);
            gameState.GameStateType = gameStateType;
            setGameState(em, gameState);
            UpdateGameStateChange(em);
        }


        public static GameAssets getGameAssets(in EntityManager em)
        {
            return em.GetComponentData<GameAssets>(gameMangerEntity);
        }

        public static Entity getCurrentLevelEntity(in EntityManager em)
        {
            var currentLevelID = getGameState(em).CurrentLevelID;
            if (currentLevelID > 0)
            {
                var levels = em.GetBuffer<LevelBuff>(gameMangerEntity);
                if (levels.Length > currentLevelID-1)
                {
                    return levels[currentLevelID-1].levelEntity;
                }
            }

            return Entity.Null;
            // var currentLevelID = getGameState(em).CurrentLevelID;
            // if (currentLevelID <= 0)
            // {
            //     return Entity.Null;
            // }
            //
            // return getLevelEntity(em, currentLevelID);
        }

        public static Level getCurrentLevel(in EntityManager em)
        {
            // var levelEntity = getCurrentLevelEntity(em);
            // if (!em.Exists(levelEntity))
            // {
            //     return new Level()
            //     {
            //         LevelID = 0
            //     };
            // }
            // return em.GetComponentData<Level>(levelEntity);

            return getLevel(em, getGameState(em).CurrentLevelID);
        }


        public static Entity getLevelEntity(in EntityManager em, int levelID)
        {
            if (levelID > 0)
            {
                var levels = em.GetBuffer<LevelBuff>(gameMangerEntity);
                if (levels.Length > levelID-1)
                {
                    return levels[levelID-1].levelEntity;
                }
            }

            return Entity.Null;
            //return em.GetEntityQueryMask("Level" + levelID);
        }


        public static Level getLevel(in EntityManager em, int levelID)
        {
            if (levelID > 0)
            {
                var levels = em.GetBuffer<LevelBuff>(gameMangerEntity);
                if (levels.Length > levelID - 1)
                {
                    return em.GetComponentData<Level>(levels[levelID - 1].levelEntity);
                }
            }

            //no level found
            return new Level()
            {
                LevelID = 0
            };
        }

        public static void incrementMoveCounter(in EntityManager em)
        {
            var gameState = getGameState(em);
            gameState.CurrentMoveCount++;
            em.SetComponentData(gameStateEntity, gameState);
            UiService.UpdateUi(GameUiUpdateType.GameUIUpdateRemainingMoves);
        }

        // static updateRemainingMovesLabel(world: ut.World) {
        //     let currentLevel = this.getCurrentLevel(world);
        //     let maxMoveCount = currentLevel == null ? 0 : currentLevel.MaxMoveCount;
        //
        //     let gameUI = world.getComponentData(world.getEntityByName("GameUI"), game.GameUI);
        //     let labelRemainingMoves = world.getComponentData(gameUI.LabelRemainingMoves, ut.Text.Text2DRenderer);
        //
        //     let strRemainingMoveCount = "";
        //     if (maxMoveCount > 0) {
        //         let remainingMoveCount = maxMoveCount - this.getGameState(world).CurrentMoveCount;
        //         strRemainingMoveCount = String(remainingMoveCount);
        //     }
        //
        //     labelRemainingMoves.text = strRemainingMoveCount;
        //     world.setComponentData(gameUI.LabelRemainingMoves, labelRemainingMoves);
        // }
        //
        public static bool hasRemainingMoves(in EntityManager em)
        {
            var maxMoveCount = getCurrentLevel(em).MaxMoveCount;
            if (maxMoveCount <= 0)
            {
                return true;
            }

            var remainingMoveCount = maxMoveCount - getGameState(em).CurrentMoveCount;
            return remainingMoveCount > 0;
        }

        public static bool isNearDeath(in EntityManager em)
        {
            var maxMoveCount = getCurrentLevel(em).MaxMoveCount;
            if (maxMoveCount > 0)
            {
                var remainingMoveCount = maxMoveCount - getGameState(em).CurrentMoveCount;
                return remainingMoveCount <= 3;
            }

            if (em.HasComponent<LevelSurvival>(getCurrentLevelEntity(em)))
            {
                var levelSurvival =
                    em.GetComponentData<LevelSurvival>(getCurrentLevelEntity(em));
                var survivalRatio = levelSurvival.SurvivalTimer / levelSurvival.MaxSurvivalTime;
                return survivalRatio < 0.2;
            }

            return false;
        }

        public static bool isObjectiveCompleted(in EntityManager em)
        {
            if (em.HasComponent<LevelPointObjective>(getCurrentLevelEntity(em)))
            {
                var levelPointObjective = em.GetComponentData<LevelPointObjective>(getCurrentLevelEntity(em));
                return getGameState(em).CurrentScore >= levelPointObjective.ScoreObjective;
            }

            if (em.HasComponent<LevelPointObjective>(getCurrentLevelEntity(em)))
            {
                var levelEggObjective = em.GetComponentData<LevelEggObjective>(getCurrentLevelEntity(em));
                return levelEggObjective.CollectedEggs >=
                       levelEggObjective.EggsInGridAtStart + levelEggObjective.EggsToSpawnOnEggCollected;
            }

            if (em.HasComponent<LevelSurvival>(getCurrentLevelEntity(em)))
            {
                var levelSurvival = em.GetComponentData<LevelSurvival>(getCurrentLevelEntity(em));
                return getGameState(em).Time >= levelSurvival.TimeObjective;
            }

            return false;
        }

        public static string formatTime(float second)
        {
            var result = "";
            int hours = (int)math.floor(second / 3600f);
            int minutes = (int)math.floor((second % 3600f) / 60f);
            int seconds = (int)math.floor(second % 60f);

            if (hours > 0)
            {
                result += "" + hours + ":" + (minutes < 10 ? "0" : "");
            }

            result += "" + minutes + ":" + (seconds < 10 ? "0" : "");
            result += "" + seconds;
            return result;
        }

        public static string formatNumber(int value)
        {
            return value.ToString().Replace(@"/\B(?=(\d{3})+(?!\d))/g", " ");
        }

        public static void unloadLevel(in EntityManager em, in EntityCommandBuffer ecb)
        {
            //     SoundService.playMusic(world);
            AudioUtils.PlayMusic(em);

            //
            //     let grid = GridService.getGridConfiguration(world);
            //     game.GridService.clear(world, grid);
            //

            var grid = GridService.getGridConfiguration(em);
            GridService.clear(em, ecb, ref grid);


            //     world.forEach([ut.Entity, game.ScrollingObject],
            //         (entity, scrollingObject) => {
            //             ut.Tweens.TweenService.removeAllTweens(world, entity);
            //             ut.Core2D.TransformService.destroyTree(world, entity, true);
            //         });
            //
            //     world.forEach([ut.Entity, game.Helicopter],
            //         (entity, helicopter) => {
            //             ut.Tweens.TweenService.removeAllTweens(world, entity);
            //             ut.Core2D.TransformService.destroyTree(world, entity, true);
            //         });
            //
            //     ut.Tweens.TweenService.removeAllTweensInWorld(world);
            //
            //     ut.EntityGroup.destroyAll(world, "game.Gem");
            //     ut.EntityGroup.destroyAll(world, "game.Cell");
            //
            //     let gameUI = world.getEntityByName("GameUI");
            //     if (world.exists(gameUI)) {
            //         ut.Core2D.TransformService.destroyTree(world, gameUI);
            //         ut.EntityGroup.destroyAll(world, "game.GameUI");
            //     }
            //
            //     let pauseMenu = world.getEntityByName("PauseMenu");
            //     if (world.exists(pauseMenu)) {
            //         ut.Core2D.TransformService.destroyTree(world, pauseMenu);
            //         ut.EntityGroup.destroyAll(world, "game.PauseMenu");
            //     }
            //
            //     let gameOverMenu = world.getEntityByName("GameOverMenu");
            //     if (world.exists(gameOverMenu)) {
            //         ut.Core2D.TransformService.destroyTree(world, gameOverMenu);
            //         ut.EntityGroup.destroyAll(world, "game.GameOverMenu");
            //     }
            //
            //     ut.EntityGroup.destroyAll(world, "game.GameScene");
            //     ut.EntityGroup.destroyAll(world, "game.Dinosaur");
            //     ut.EntityGroup.destroyAll(world, "game.SurvivalModeTimeline");
            //     ut.EntityGroup.destroyAll(world, "game.BackgroundNearDeathWarning");
            //
            //     ut.EntityGroup.destroyAll(world, "game.TutorialHighlight");
            //     ut.EntityGroup.destroyAll(world, "game.TutorialMatchPointer");
            //     ut.EntityGroup.destroyAll(world, "game.TutorialEggPointer");
            //     ut.EntityGroup.destroyAll(world, "game.TutorialSurvivalPointer");
        }

        /**
         * Utility method to enable and disable entities.
         */
        public static void setEntityEnabled(in EntityManager em, EntityCommandBuffer ecb, Entity entity, bool enabled)
        {
            bool hasDisabledComponent = em.HasComponent<Disabled>(entity);
            if (enabled && hasDisabledComponent)
            {
                ecb.RemoveComponent<Disabled>(entity);
            }
            else if (!enabled && !hasDisabledComponent)
            {
                ecb.AddComponent<Disabled>(entity);
            }
        }


        //--------------------

        public static MainUI getMainUI(in EntityManager em)
        {
            if (gameMangerEntity == Entity.Null)
                return null;
            var e = em.GetComponentObject<MainUIComponent>(gameMangerEntity);
            if (e != null)
                return e.MainUI;
            return null;
        }
    }
}