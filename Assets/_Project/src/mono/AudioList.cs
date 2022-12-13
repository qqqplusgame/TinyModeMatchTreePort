// using System.Collections;

using System;
using System.Collections.Generic;
using DG.DeAudio;
using Unity.Entities;
using UnityEngine;

namespace ProjectM
{
    public class AudioListComponent : IComponentData
    {
        public Dictionary<string, DeAudioClipData> AudioClips;
    }

    // public class AudioListInitSystem : SystemBase
    // {
    //     protected override void OnUpdate()
    //     {
    //         var em = EntityManager;
    //         var gameManagerEntity = GameService.GetGameManagerEntity();
    //     }
    // }
    public class AudioList : MonoBehaviour
    {
        public DeAudioClipData[] musicClips;
        public DeAudioClipData[] soundClips;
        
        public bool isEntityInit = false;
        private void Start()
        {
            
            //DeAudioManager.RegisterAudioClipDatas(musicClips);
            //DeAudioManager.RegisterAudioClipDatas(soundClips);
            
        }

        private void Update()
        {
            //make sure entity init when GameManager entity has inited
            if(isEntityInit)
                return;
            
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = em.CreateEntityQuery(typeof(GameManager));
            if (query.TryGetSingletonEntity<GameManager>(out var e))
            {
                var alc = new AudioListComponent()
                {
                    AudioClips = new Dictionary<string, DeAudioClipData>()
                };
                foreach (var clip in musicClips)
                {
                    alc.AudioClips.Add(clip.clip.name, clip);
                }

                foreach (var clip in soundClips)
                {
                    alc.AudioClips.Add(clip.clip.name, clip);
                }

                em.AddComponentObject(e, alc);
                
                Debug.Log("AudioList Entity Inited");
                isEntityInit = true;
                enabled = false;
            }
        }
    }
}