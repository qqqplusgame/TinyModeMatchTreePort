﻿using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectM.UI
{
    public class PauseMenu : MonoBehaviour
    {
        UIDocument uiDocument;
        VisualElement mPanel;
        Button soundBtn;
        Button musicBtn;
        Button quitBtn;
        Button resumeBtn;

        public void Show()
        {
            mPanel.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            mPanel.style.display = DisplayStyle.None;
        }

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            mPanel = uiDocument.rootVisualElement.Q("PauseMenu");
            soundBtn = mPanel.Q<Button>("ButtonSound");
            musicBtn = mPanel.Q<Button>("ButtonMusic");
            quitBtn = mPanel.Q<Button>("ButtonQuit");
            resumeBtn = mPanel.Q<Button>("ButtonResume");
        }

        private void OnEnable()
        {
            soundBtn.RegisterCallback<ClickEvent>(SoundBtnClick);
            musicBtn.RegisterCallback<ClickEvent>(MusicBtnClick);
            quitBtn.RegisterCallback<ClickEvent>(QuitBtnClick);
            resumeBtn.RegisterCallback<ClickEvent>(ResumeBtnClick);
        }

        private void OnDisable()
        {
            soundBtn.UnregisterCallback<ClickEvent>(SoundBtnClick);
            musicBtn.UnregisterCallback<ClickEvent>(MusicBtnClick);
            quitBtn.UnregisterCallback<ClickEvent>(QuitBtnClick);
            resumeBtn.UnregisterCallback<ClickEvent>(ResumeBtnClick);
        }

        void SoundBtnClick(ClickEvent evt)
        {
            soundBtn.ToggleInClassList("sound-off");
            if (soundBtn.ClassListContains("sound-off"))
            {
                Debug.Log("Sound off");
                AudioUtils.MuteSFX(true);
            }
            else
            {
                Debug.Log("Sound on");
                AudioUtils.MuteSFX(false);
            }
        }

        void MusicBtnClick(ClickEvent evt)
        {
            
            musicBtn.ToggleInClassList("sound-off");
            if (musicBtn.ClassListContains("sound-off"))
            {
                Debug.Log("Music off");
                AudioUtils.MuteMusic(true);
            }
            else
            {
                Debug.Log("Music on");
                AudioUtils.MuteMusic(false);
            }
        }

        void QuitBtnClick(ClickEvent evt)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var gameState = GameService.getGameState(em);
            if (gameState.GameStateType == GameStateTypes.Game)
            {
                GameService.changeGameStateTypeWithUpdate(em, GameStateTypes.WorldMap);
                //GameStateLoadingService.SetGameState(em, GameStateTypes.WorldMap);
            }
            else
            {
               
                Application.Quit();
            }
        }

        void ResumeBtnClick(ClickEvent evt)
        {
            Hide();
        }
    }
}