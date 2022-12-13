using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectM.UI
{
    public class GameOverMenu : MonoBehaviour
    {
        UIDocument uiDocument;
        VisualElement mPanel;
        private Label labelGameOver;
        private Label labelGameOverShadow;
        private VisualElement leftArmSuccess;
        private VisualElement rightArmSuccess;
        private VisualElement leftArmFail;
        private VisualElement rightArmFail;

        private Label scoreValueLabel;
        private Label timeLabel;
        private Label timeValueLabel;
        private Button quitBtn;

        public void Show()
        {
            mPanel.style.display = DisplayStyle.Flex;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            var isObjectiveComplete = GameService.isObjectiveCompleted(em);
            var isSurvivalMode = em.HasComponent<LevelSurvival>(GameService.getCurrentLevelEntity(em));
            var gameOverTextID = isObjectiveComplete ? "Success" : "Game Over";
            labelGameOver.text = gameOverTextID;
            labelGameOverShadow.text = gameOverTextID;
            leftArmSuccess.style.display = isObjectiveComplete ? DisplayStyle.Flex : DisplayStyle.None;
            rightArmSuccess.style.display = isObjectiveComplete ? DisplayStyle.Flex : DisplayStyle.None;
            leftArmFail.style.display = isObjectiveComplete ? DisplayStyle.None : DisplayStyle.Flex;
            rightArmFail.style.display = isObjectiveComplete ? DisplayStyle.None : DisplayStyle.Flex;

            scoreValueLabel.text = GameService.formatNumber(gameState.CurrentScore) + " pts";

            timeLabel.text = isSurvivalMode ? "Time" : " ";

            timeValueLabel.text = isSurvivalMode ? GameService.formatTime(gameState.Time) : "";
        }

        public void Hide()
        {
            mPanel.style.display = DisplayStyle.None;
        }

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            mPanel = uiDocument.rootVisualElement.Q("GameOverMenu");
            labelGameOver = mPanel.Q<Label>("CustomLabelGameOver");
            labelGameOverShadow = mPanel.Q<Label>("CustomLabelGameOverShadow");
            leftArmSuccess = mPanel.Q("ImageLeftArmSuccess");
            rightArmSuccess = mPanel.Q("ImageRightArmSuccess");
            leftArmFail = mPanel.Q("ImageLeftArmFail");
            rightArmFail = mPanel.Q("ImageRightArmFail");

            scoreValueLabel = mPanel.Q<Label>("CustomLabelScoreValue");
            timeLabel = mPanel.Q<Label>("CustomLabelTime");
            timeValueLabel = mPanel.Q<Label>("CustomLabelTimeValue");
            // musicBtn = mPanel.Q<Button>("ButtonMusic");
            quitBtn = mPanel.Q<Button>("ButtonQuit");
            // resumeBtn = mPanel.Q<Button>("ButtonResume");
        }

        private void OnEnable()
        {
            // soundBtn.RegisterCallback<ClickEvent>(SoundBtnClick);
            // musicBtn.RegisterCallback<ClickEvent>(MusicBtnClick);
            quitBtn.RegisterCallback<ClickEvent>(QuitBtnClick);
            // resumeBtn.RegisterCallback<ClickEvent>(ResumeBtnClick);
        }

        private void QuitBtnClick(ClickEvent evt)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            GameService.changeGameStateTypeWithUpdate(em, GameStateTypes.WorldMap);
            //GameStateLoadingService.SetGameState(em, GameStateTypes.WorldMap);
        }

        private void OnDisable()
        {
            // soundBtn.UnregisterCallback<ClickEvent>(SoundBtnClick);
            // musicBtn.UnregisterCallback<ClickEvent>(MusicBtnClick);
            quitBtn.UnregisterCallback<ClickEvent>(QuitBtnClick);
            // resumeBtn.UnregisterCallback<ClickEvent>(ResumeBtnClick);
        }
    }
}