using Unity.Entities;

namespace ProjectM
{
    public class GameStateLoadingService
    {
        public static void SetGameState(in EntityManager em, in EntityCommandBuffer ecb, GameStateTypes gameStateType)
        {
            var gameState = GameService.getGameState(em);
            gameState.GameStateType = gameStateType;
            GameService.setGameState(em, gameState);

            //UiService.UpdateUiByGameState(gameStateType);

            switch (gameStateType)
            {
                // case game.GameStateTypes.Loading: {
                //     this.loadLoadingScreen(world);
                //     break;
                // }
                // case game.GameStateTypes.Cutscene: {
                //     this.loadCutscene(world);
                //     break;
                // }
                // case game.GameStateTypes.CutsceneEnd: {
                //     this.loadEndCutscene(world);
                //     break;
                // }
                // case game.GameStateTypes.MainMenu: {
                //     this.loadMainMenu(world);
                //     break;
                // }
                case GameStateTypes.WorldMap:
                {
                    loadWorldMap(em, ecb);
                    break;
                }
                case GameStateTypes.Game:
                {
                    //init game state
                    gameState.CurrentMoveCount = 0;
                    gameState.CurrentScore = 0;
                    gameState.Time = 0;
                    gameState.SurvivalTimer = 0;
                    GameService.setGameState(em, gameState);


                    loadGame(em);
                    break;
                }
                case GameStateTypes.Settings:
                {
                    loadSettings(em);
                    break;
                }
                case GameStateTypes.Paused:
                {
                    loadPause(em);
                    break;
                }
                // case game.GameStateTypes.Credits: {
                //     this.loadCredits(world);
                //     break;
                // }
                // case game.GameStateTypes.Languages: {
                //     this.loadLanguages(world);
                //     break;
                // }
                case GameStateTypes.GameOver:
                {
                    loadGameOver(em);
                    break;
                }
            }
        }


        private static void loadWorldMap(in EntityManager em, in EntityCommandBuffer ecb)
        {
            if (GridService.getGridConfiguration(em).IsGridCreated)
            {
                GameService.unloadLevel(em, ecb);
            }

            UiService.InitUiByGameState(GameStateTypes.WorldMap);
            UiService.UpdateUi(GameUiUpdateType.WorldMapUI);
            AudioUtils.PlayMusic(em);
        }

        private static void loadGame(in EntityManager em)
        {
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);

            if (em.HasComponent<LevelSurvival>(levelEntity))
            {
                var levelSurvival = em.GetComponentData<LevelSurvival>(levelEntity);
                levelSurvival.SurvivalTimer = levelSurvival.MaxSurvivalTime;
                em.SetComponentData(levelEntity, levelSurvival);
            }

            //throw new System.NotImplementedException();
            UiService.InitUiByGameState(GameStateTypes.Game);
            UiService.UpdateUi(GameUiUpdateType.GameUIOnLoad);
            UiService.UpdateUi(GameUiUpdateType.GameUI);
            AudioUtils.PlayMusic(em);
        }

        private static void loadSettings(in EntityManager em)
        {
            UiService.InitUiByGameState(GameStateTypes.Settings);
        }

        private static void loadPause(in EntityManager em)
        {
            UiService.InitUiByGameState(GameStateTypes.Paused);
        }

        private static void loadGameOver(in EntityManager em)
        {
            UiService.InitUiByGameState(GameStateTypes.GameOver);
        }
    }
}