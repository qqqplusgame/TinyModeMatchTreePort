using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectM.UI
{
    public class WorldMapUI : MonoBehaviour
    {
        private UIDocument uiDocument;
        private VisualElement uiRoot;

        private bool isCreated;
        private int currentLevelId;
        private Button buttonLeft;
        private Button buttonRight;
        private VisualElement ground;
        private VisualElement groundTransition;
        private VisualElement sky;
        private VisualElement skyTransition;
        private VisualElement labelLevelTitle;
        private Label labelLevelInfoLine1;
        private Label labelLevelInfoLine2;
        private int lastLevelId;
        private Label labelBestScore;
        private VisualElement imagePointObjective;
        private VisualElement imageEggObjective;
        private VisualElement imageSurvivalObjective;
        private int lastBeatenLevelId;

        private VisualElement imagePreview;

        [SerializeField] PauseMenu pauseMenu;

        private Button playBtn;
        private Button pauseBtn;
        
        private bool isInTransAni;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            pauseMenu = GetComponent<PauseMenu>();
            uiRoot = uiDocument.rootVisualElement.Q<VisualElement>("WorldMap");

            buttonLeft = uiRoot.Q<Button>("ButtonLeft");
            buttonRight = uiRoot.Q<Button>("ButtonRight");
            ground = uiRoot.Q<VisualElement>("ImageGround");
            groundTransition = uiRoot.Q<VisualElement>("ImageGroundTransition");
            sky = uiRoot.Q<VisualElement>("ImageSky");
            skyTransition = uiRoot.Q<VisualElement>("ImageSkyTransition");
            labelLevelTitle = uiRoot.Q<VisualElement>("LabelLevelTitle");
            labelLevelInfoLine1 = uiRoot.Q<Label>("LabelLevelInfoLine1");
            labelLevelInfoLine2 = uiRoot.Q<Label>("LabelLevelInfoLine2");
            labelBestScore = uiRoot.Q<Label>("LabelBestScore");
            imagePointObjective = uiRoot.Q<VisualElement>("ImagePointsObjective");
            imageEggObjective = uiRoot.Q<VisualElement>("ImageEggsObjective");
            imageSurvivalObjective = uiRoot.Q<VisualElement>("ImageSurvivalObjective");

            imagePreview = uiRoot.Q<VisualElement>("ImagePreview");


            playBtn = uiRoot.Q<Button>("ButtonPlay");
            pauseBtn = uiRoot.Q<Button>("ButtonSettings");
            

            isInTransAni = false;
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            buttonLeft.RegisterCallback<ClickEvent>(BtnLeftClick);
            buttonRight.RegisterCallback<ClickEvent>(BtnRightClick);
            playBtn.RegisterCallback<ClickEvent>(PlayBtnClick);
            pauseBtn.RegisterCallback<ClickEvent>(PauseButtonClick);


            skyTransition.RegisterCallback<TransitionEndEvent>(OnSkyTransitionEnd);
        }

        public void SetVisible(bool visible)
        {
            uiRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDisable()
        {
            buttonLeft.UnregisterCallback<ClickEvent>(BtnLeftClick);
            buttonRight.UnregisterCallback<ClickEvent>(BtnRightClick);
            playBtn.UnregisterCallback<ClickEvent>(PlayBtnClick);
            pauseBtn.UnregisterCallback<ClickEvent>(PauseButtonClick);


            skyTransition.UnregisterCallback<TransitionEndEvent>(OnSkyTransitionEnd);
        }

        private void OnSkyTransitionEnd(TransitionEndEvent evt)
        {
            Debug.Log("OnSkyTransitionEnd");
            isInTransAni = false;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);
            var level = em.GetComponentData<Level>(levelEntity);

            sky.ClearClassList();
            sky.AddToClassList(getSkyClassName(level.Skin));

            ground.ClearClassList();
            ground.AddToClassList(getGroundClassName(level.Skin));
            skyTransition.ClearClassList();
            //skyTransition.RemoveFromClassList("opt-in");
            skyTransition.AddToClassList("opt-zero");
            groundTransition.ClearClassList();
            //groundTransition.RemoveFromClassList("opt-in");
            groundTransition.AddToClassList("opt-zero");
        }

        private void BtnLeftClick(ClickEvent evt)
        {
            LeftRightClick(-1);
        }

        private void BtnRightClick(ClickEvent evt)
        {
            LeftRightClick(1);
        }

        private void LeftRightClick(int increment)
        {
            // let worldMapEntity = world.getEntityByName("WorldMap");
            // let worldMap = world.getComponentData(worldMapEntity, game.WorldMap);
            var levelIndex = currentLevelId;
            levelIndex += increment;
            levelIndex = math.min(3, math.max(1, levelIndex));
            currentLevelId = levelIndex;
            // world.setComponentData(worldMapEntity, worldMap);
            //
            // SoundService.play(world, "GenericClickSound");
            if (lastLevelId != currentLevelId)
            {
                UpdateWorldMapItem(true);
                lastLevelId = currentLevelId;
            }
        }

        private void PauseButtonClick(ClickEvent evt)
        {
            pauseMenu?.Show();
        }

        private void PlayBtnClick(ClickEvent evt)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            GameService.changeGameStateTypeWithUpdate(em, GameStateTypes.Game);
            //GameStateLoadingService.SetGameState(em, GameStateTypes.Game);

            // let currentLevelIndex = worldMap.CurrentLevelIndex;
            // UserDataService.setSelectedWorldMapIndex(currentLevelIndex);
            //
            // GameStateLoadingService.transitionToGameStateWithScaledHole(world, game.GameStateTypes.Game, new Vector2(0, 0));
            //
            // SoundService.play(world, "GameStartSound");
        }

        public void UpdateUI()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var levelID = gameState.CurrentLevelID;
            var levelEntity = GameService.getLevelEntity(em, levelID);
            var level = em.GetComponentData<Level>(levelEntity);

            currentLevelId = gameState.CurrentLevelID;
            lastBeatenLevelId = UserDataService.getLastBeatenLevelID();
            UpdateWorldMapItem(false);
            lastLevelId = currentLevelId;
        }

        private string getSkyClassName(SkinTypes skin)
        {
            switch (skin)
            {
                case SkinTypes.Farm:
                    return "farm-sky";
                case SkinTypes.Camp:
                    return "camp-sky";
                case SkinTypes.City:
                    return "city-sky";
                default:
                    return "city-sky";
            }
        }

        private string getGroundClassName(SkinTypes skin)
        {
            switch (skin)
            {
                case SkinTypes.Farm:
                    return "farm-ground";
                case SkinTypes.Camp:
                    return "camp-ground";
                case SkinTypes.City:
                    return "city-ground";
                default:
                    return "city-ground";
            }
        }

        /// <summary>
        /// update the world map item,assumes that the current level index is set
        /// and after this call the last level index is set
        /// </summary>
        /// <param name="transAni"></param>
        public void UpdateWorldMapItem(bool transAni)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;


            var gameState = GameService.getGameState(em);
            gameState.CurrentLevelID = (sbyte)currentLevelId;
            GameService.setGameState(em, gameState);

            var levelID = currentLevelId;


            var levelEntity = GameService.getLevelEntity(em, levelID);
            var level = em.GetComponentData<Level>(levelEntity);

            imagePreview.ClearClassList();
            labelLevelTitle.ClearClassList();


            if (transAni)
            {
                if (isInTransAni)
                {
                    
                    skyTransition.ClearClassList();
                    //skyTransition.RemoveFromClassList("opt-in");
                    //skyTransition.AddToClassList("opt-zero");
                    groundTransition.ClearClassList();
                    //groundTransition.RemoveFromClassList("opt-in");
                   // groundTransition.AddToClassList("opt-zero");
                }

                isInTransAni = true;
                skyTransition.AddToClassList(getSkyClassName(level.Skin));
                skyTransition.AddToClassList("opt-in");
                skyTransition.RemoveFromClassList("opt-zero");
                groundTransition.AddToClassList(getGroundClassName(level.Skin));
                groundTransition.AddToClassList("opt-in");
                groundTransition.RemoveFromClassList("opt-zero");
            }
            else
            {
                sky.ClearClassList();
                sky.AddToClassList(getSkyClassName(level.Skin));

                ground.ClearClassList();
                ground.AddToClassList(getGroundClassName(level.Skin));

                skyTransition.ClearClassList();
                skyTransition.AddToClassList("opt-zero");

                groundTransition.ClearClassList();
                groundTransition.AddToClassList("opt-zero");
            }

            switch (level.Skin)
            {
                case SkinTypes.Farm:
                    imagePreview.AddToClassList("level-farm");
                    labelLevelTitle.AddToClassList("label-level-farm");
                    break;
                case SkinTypes.Camp:
                    imagePreview.AddToClassList("level-camp");
                    labelLevelTitle.AddToClassList("label-level-camp");
                    break;
                case SkinTypes.City:
                    imagePreview.AddToClassList("level-city");
                    labelLevelTitle.AddToClassList("label-level-city");
                    break;
            }

            UiService.SetVisualElementVisible(buttonLeft, currentLevelId > 1);
            UiService.SetVisualElementVisible(buttonRight, currentLevelId < 3);

            var isPointObjective = em.HasComponent<LevelPointObjective>(levelEntity);
            var isEggObjective = em.HasComponent<LevelEggObjective>(levelEntity);
            var isSurvivalObjective = em.HasComponent<LevelSurvival>(levelEntity);

            labelLevelInfoLine1.text = "";
            labelLevelInfoLine2.text = "";
            var strObjective = "";
            if (isPointObjective)
            {
                var pointObjective = em.GetComponentData<LevelPointObjective>(levelEntity);
                strObjective = $"Reach a score of {pointObjective.ScoreObjective} pts in {level.MaxMoveCount} moves.";
            }
            else if (isEggObjective)
            {
                var eggObjective = em.GetComponentData<LevelEggObjective>(levelEntity);
                var eggCount = eggObjective.EggsInGridAtStart + eggObjective.EggsToSpawnOnEggCollected;
                strObjective = $"Collect {eggCount} eggs in {level.MaxMoveCount} moves.";
            }
            else if (isSurvivalObjective)
            {
                var survivalObjective = em.GetComponentData<LevelSurvival>(levelEntity);
                strObjective = $"Survive for {survivalObjective.TimeObjective} minutes.";
            }

            var words = strObjective.Split(' ');
            foreach (var word in words)
            {
                if (labelLevelInfoLine1.text.Length + word.Length <= 25)
                {
                    labelLevelInfoLine1.text += word + " ";
                }
                else
                {
                    labelLevelInfoLine2.text += word + " ";
                }
            }

            UiService.SetVisualElementVisible(imagePointObjective, isPointObjective);
            UiService.SetVisualElementVisible(imageEggObjective, isEggObjective);
            UiService.SetVisualElementVisible(imageSurvivalObjective, isSurvivalObjective);

            // Update best score.
            // let bestScore = game.UserDataService.getBestScore(currentLevelIndex + 1);
            // let labelBestScore = this.world.getComponentData(worldMap.LabelBestScore, ut.Text.Text2DRenderer);
            // if (bestScore == 0) {
            //     labelBestScore.text = "---";
            // }
            // else {
            //     labelBestScore.text = game.GameService.formatNumber(bestScore) + " pts";
            // }
            // this.world.setComponentData(worldMap.LabelBestScore, labelBestScore);

            //lastLevelIndex = currentLevelIndex;

            //worldMap.LastLanguageID = LocalizationService.getLanguageID(this.world);
        }
    }
}