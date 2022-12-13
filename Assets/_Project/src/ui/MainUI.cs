using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectM.UI
{
    public class MainUI : MonoBehaviour
    {
        private UIDocument uiDocument;
        public GameUI gameUI;
        public WorldMapUI worldMapUI;
        public PauseMenu pauseMenu;
        public GameOverMenu gameOverMenu;

        void Awake()
        {
            // The UXML is already instantiated by the UIDocument component
            uiDocument = GetComponent<UIDocument>();

            gameUI = GetComponent<GameUI>();
            worldMapUI = GetComponent<WorldMapUI>();
            pauseMenu = GetComponent<PauseMenu>();
            gameOverMenu = GetComponent<GameOverMenu>();

            //pauseButton = uiDocument.rootVisualElement.Q<Button>("ButtonPause");

            //pauseButton.RegisterCallback<ClickEvent>(PauseButtonClick);
        }

        void Start()
        {
            //hide all UI
            gameUI?.SetVisible(false);
            worldMapUI?.SetVisible(false);
            pauseMenu?.Hide();
            gameOverMenu?.Hide();

            //add gameuicomponet to ecs
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            UiService.SetupMainUI(em, this);
        }

        public void InitUiStateByGameState(GameStateTypes gameStateTypes)
        {
            switch (gameStateTypes)
            {
                case GameStateTypes.Game:
                    gameUI?.SetVisible(true);
                    worldMapUI?.SetVisible(false);
                    pauseMenu?.Hide();
                    gameOverMenu?.Hide();
                    
                    break;
                case GameStateTypes.Paused:
                case GameStateTypes.Settings:
                    pauseMenu?.Show();
                    break;
                case GameStateTypes.GameOver:
                    gameOverMenu?.Show();
                    break;
                case GameStateTypes.WorldMap:
                    gameUI?.SetVisible(false);
                    worldMapUI?.SetVisible(true);
                    pauseMenu?.Hide();
                    gameOverMenu?.Hide();
                    
                    break;
                case GameStateTypes.None:
                    default:
                    gameUI?.SetVisible(false);
                    worldMapUI?.SetVisible(false);
                    pauseMenu?.Hide();
                    gameOverMenu?.Hide();
                    break;

            }
        }
    }


    public class MainUIComponent : IComponentData
    {
        public MainUI MainUI;
    }
}