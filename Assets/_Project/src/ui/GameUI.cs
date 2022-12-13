using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectM.UI
{
    public class GameUI : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement uiRoot;
        private Button pauseButton;

        private VisualElement ImageObjectiveCompleteGlow;
        private VisualElement ImageObjectivePoint;
        private VisualElement ImageObjectivePointIncomplete;
        private VisualElement ImageObjectiveEgg;
        private VisualElement ImageObjectiveEggIncomplete;
        private VisualElement ImageObjectiveSurvival;
        private VisualElement ImageObjectiveSurvivalIncomplete;
        private VisualElement ImageMoves;
        private VisualElement ImageNoMovesWarning;
        private Label LabelObjective;
        private Label LabelTime;
        private Label LabelScore;
        private Label LabelRemainingMoves;

        private int LastCollectedEggCount;
        private bool LastIsObjectiveComplete;


        [SerializeField] PauseMenu pauseMenu;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            pauseMenu = GetComponent<PauseMenu>();
            uiRoot = uiDocument.rootVisualElement.Q<VisualElement>("GameUI");
            pauseButton = uiRoot.Q<Button>("ButtonPause");
            ImageObjectiveCompleteGlow = uiRoot.Q<VisualElement>("ImageObjectiveCompleteGlow");
            ImageObjectivePoint = uiRoot.Q<VisualElement>("ImagePoint");
            ImageObjectivePointIncomplete = uiRoot.Q<VisualElement>("ImagePointIncomplete");
            ImageObjectiveEgg = uiRoot.Q<VisualElement>("ImageEgg");
            ImageObjectiveEggIncomplete = uiRoot.Q<VisualElement>("ImageEggIncomplete");
            ImageObjectiveSurvival = uiRoot.Q<VisualElement>("ImageSurvival");
            ImageObjectiveSurvivalIncomplete = uiRoot.Q<VisualElement>("ImageSurvivalIncomplete");
            ImageMoves = uiRoot.Q<VisualElement>("ImageMoves");
            ImageNoMovesWarning = uiRoot.Q<VisualElement>("ImageNoMovesWarning");
            LabelObjective = uiRoot.Q<Label>("CustomLabelObjective");
            LabelTime = uiRoot.Q<Label>("CustomLabelTime");
            LabelScore = uiRoot.Q<Label>("CustomLabelScoreValue");
            LabelRemainingMoves = uiRoot.Q<Label>("CustomLabelRemainingMoves");
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            // The UXML is already instantiated by the UIDocument component
            pauseButton.RegisterCallback<ClickEvent>(PauseButtonClick);
        }

        public void SetVisible(bool visible)
        {
            uiRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDisable()
        {
            pauseButton.UnregisterCallback<ClickEvent>(PauseButtonClick);
        }

        private void PauseButtonClick(ClickEvent evt)
        {
            pauseMenu?.Show();
        }

        public void OnLoadGameUpdate()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);


            var isPointObjective = em.HasComponent<LevelPointObjective>(levelEntity);
            var isEggObjective = em.HasComponent<LevelEggObjective>(levelEntity);
            var isSurvivalObjective = em.HasComponent<LevelSurvival>(levelEntity);


            UiService.SetVisualElementVisible(ImageObjectivePoint, isPointObjective);
            UiService.SetVisualElementVisible(ImageObjectiveEgg, isEggObjective);
            UiService.SetVisualElementVisible(ImageObjectiveSurvival, isSurvivalObjective);

            UiService.SetVisualElementVisible(ImageMoves, !isSurvivalObjective);
            UiService.SetVisualElementVisible(ImageNoMovesWarning, !isSurvivalObjective);

            LabelTime.text = "";
            LabelScore.text = "0";
            if (isPointObjective)
            {
                var pointObjective = em.GetComponentData<LevelPointObjective>(levelEntity);
                LabelObjective.text = GameService.formatNumber(pointObjective.ScoreObjective);
            }
            else if (isSurvivalObjective)
            {
                var survivalObjective = em.GetComponentData<LevelSurvival>(levelEntity);
                LabelObjective.text = GameService.formatTime(survivalObjective.TimeObjective);
                // ut.EntityGroup.instantiate(world, "game.SurvivalModeTimeline");
            }


            //update RemainingMovesLabel
            UpdateRemainingMoves();
        }

        public void UpdateUI()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);
            var level = em.GetComponentData<Level>(levelEntity);

            var isPointObjective = em.HasComponent<LevelPointObjective>(levelEntity);
            var isEggObjective = em.HasComponent<LevelEggObjective>(levelEntity);
            var isSurvivalObjective = em.HasComponent<LevelSurvival>(levelEntity);


            //var collectedEggs = levelEggObjective.CollectedEggs;
            // let heldEgg = 0;
            // world.forEach([game.CollectedCurrency], (collectedCurrency) => { heldEgg++; });
            //heldEgg == 0 &&
            var isObjectiveComplete = GameService.isObjectiveCompleted(em);

            UiService.SetVisualElementVisible(ImageObjectiveCompleteGlow, isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectivePoint, isPointObjective && isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectiveEgg, isEggObjective && isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectiveSurvival, isSurvivalObjective && isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectivePointIncomplete, isPointObjective && !isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectiveEggIncomplete, isEggObjective && !isObjectiveComplete);
            UiService.SetVisualElementVisible(ImageObjectiveSurvivalIncomplete,
                isSurvivalObjective && !isObjectiveComplete);

            //todo animation when objective complete
            // if (isObjectiveComplete) {
            //     let objectiveGlowTransformRotation = world.getComponentData(gameUI.ImageObjectiveCompleteGlow, ut.Core2D.TransformLocalRotation);
            //     objectiveGlowTransformRotation.rotation.setFromAxisAngle(new Vector3(0, 0, 1), gameState.Time);
            //     world.setComponentData(gameUI.ImageObjectiveCompleteGlow, objectiveGlowTransformRotation);
            //
            //     if (!gameUI.LastIsObjectiveComplete) {
            //         gameUI.LastIsObjectiveComplete = true;
            //         world.setComponentData(gameUIEntity, gameUI);
            //         this.punchScale(world, gameUI.LabelObjective, 1.35);
            //         this.punchScale(world, gameUI.ImageObjectivePoint, 1.1);
            //         this.punchScale(world, gameUI.ImageObjectiveEgg, 1.1);
            //         this.punchScale(world, gameUI.ImageObjectiveSurvival, 1.1);
            //     }
            // }

            if (isSurvivalObjective)
            {
                LabelTime.text = GameService.formatTime(gameState.Time);
            }
            else if (isEggObjective)
            {
                var eggObjective = em.GetComponentData<LevelEggObjective>(levelEntity);
                var totalToCollect = eggObjective.EggsInGridAtStart + eggObjective.EggsToSpawnOnEggCollected;

                var collectedEgg = eggObjective.CollectedEggs; // - heldEgg;
                if (collectedEgg > LastCollectedEggCount)
                {
                    LastCollectedEggCount = collectedEgg;

                    //this.punchScale(world, gameUI.LabelObjective, 1.35);
                    //this.punchScale(world, gameUI.ImageObjectiveEggIncomplete, 1.1);
                }

                var remainingEggCount = collectedEgg + "/" + totalToCollect;
                if (remainingEggCount != LabelObjective.text)
                {
                    LabelObjective.text = remainingEggCount;
                }
            }
        }

        public void UpdateRemainingMoves()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var level = GameService.getCurrentLevel(em);
            var maxMoveCount = level.MaxMoveCount;
            var strRemainingMoves = "";
            if (maxMoveCount > 0)
            {
                var remainingMoveCount = maxMoveCount - gameState.CurrentMoveCount;
                strRemainingMoves = remainingMoveCount.ToString();
            }

            LabelRemainingMoves.text = strRemainingMoves;
        }

        public void UpdateObjectiveChange()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);
            var level = em.GetComponentData<Level>(levelEntity);

            //var isPointObjective = em.HasComponent<LevelPointObjective>(levelEntity);
            //var isEggObjective = em.HasComponent<LevelEggObjective>(levelEntity);
            var isSurvivalObjective = em.HasComponent<LevelSurvival>(levelEntity);

            LabelScore.text = GameService.formatNumber(gameState.CurrentScore);
            if (isSurvivalObjective)
            {
                var survivalObjective = em.GetComponentData<LevelSurvival>(levelEntity);
                LabelTime.text = GameService.formatTime(survivalObjective.SurvivalTimer);
            }
        }
    }
}