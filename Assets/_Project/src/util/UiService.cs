using ProjectM.UI;
using Unity.Entities;
using UnityEngine.UIElements;

namespace ProjectM
{
    public class UiService
    {
        private static MainUI _mainUi;

        public static bool IsInitialized => _mainUi != null;

        public static void SetupMainUI(in EntityManager em, MainUI ui)
        {
            var e = GameService.GetGameManagerEntity();
            if (em.Exists(e))
            {
                var gui = new MainUIComponent
                {
                    MainUI = ui
                };
                em.AddComponentData(e, gui);
            }

            _mainUi = ui;
        }

        public static void InitUiByGameState(GameStateTypes gameStateTypes)
        {
            _mainUi.InitUiStateByGameState(gameStateTypes);
        }

        public static void UpdateUi(GameUiUpdateType type)
        {
            switch (type)
            {
                case GameUiUpdateType.GameUIOnLoad:
                    _mainUi.gameUI.OnLoadGameUpdate();
                    break;
                case GameUiUpdateType.GameUI:
                    _mainUi.gameUI.UpdateUI();
                    break;
                case GameUiUpdateType.GameUIUpdateRemainingMoves:
                    _mainUi.gameUI.UpdateRemainingMoves();
                    break;
                case GameUiUpdateType.GameUIUpdateObjectiveChange:
                    _mainUi.gameUI.UpdateObjectiveChange();
                    break;
                case GameUiUpdateType.WorldMapUI:
                    _mainUi.worldMapUI.UpdateUI();
                    break;
            }
        }

        public static void SetVisualElementVisible(VisualElement element, bool visible)
        {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}