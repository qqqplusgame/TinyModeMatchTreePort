using DG.DeAudio;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//using Unity.Tiny.Audio;

namespace ProjectM
{
    public static class AudioUtils
    {
        public static void PlayMusic(in EntityManager em)
        {
            //todo music on check
            
            //
            var gamestate = GameService.getGameState(em);
            //var level = GameService.getCurrentLevel(em);
            var name = "";
            switch (gamestate.GameStateType)
            {
                case GameStateTypes.Paused:
                case GameStateTypes.Game:
                {
                    //name = "Music" + getMusicName(level.Skin);
                    name = "Level 0" + gamestate.CurrentLevelID;
                    break;
                }
                default:
                {
                    name = "Menu";
                    break;
                }
            }

            var clip = GetAudioClipData(em, name);
            if (clip == null)
                return;
            //Debug.Log($"PlayMusic {name}");
            var d = DeAudioManager.Play(clip);
            //d.SetRealPitch(1);
        }

        public static void StopMusic()
        {
            DeAudioManager.Stop(DeAudioGroupId.Music);
        }

        private static string getMusicName(in SkinTypes type)
        {
            switch (type)
            {
                case SkinTypes.Camp:
                    return "Camp";
                case SkinTypes.Farm:
                    return "Farm";
                case SkinTypes.City:
                default:
                    return "City";
            }
        }

        public static void PlaySound(EntityManager em, string name)
        {
            // var clipEntity = FindAudioClip(entityManager, audioType);
            // if (clipEntity == Entity.Null)
            //     return;

            // var sourceEntity = entityManager.CreateEntity();
            //
            // entityManager.AddComponentData(sourceEntity, new AudioSource()
            // {
            //     clip = clipEntity,
            //     loop = shouldLoop,
            //     volume = 1f
            // });
            //
            // entityManager.AddComponent<AudioSourceStart>(sourceEntity);

            var clip = GetAudioClipData(em, name);
            if (clip == null)
                return;
            //Debug.Log($"PlaySound {name}");
            DeAudioManager.Play(clip);
        }

        // public static void StopSound(EntityManager entityManager, AudioTypes audioType)
        // {
        //     // var clipEntity = FindAudioClip(entityManager, audioType);
        //     // if (clipEntity == Entity.Null)
        //     //     return;
        //     //
        //     // var audioSourceQuery = entityManager.CreateEntityQuery(typeof(AudioSource));
        //     //
        //     // var audioSources = audioSourceQuery.ToComponentDataArray<AudioSource>(Allocator.TempJob);
        //     // var audioSourceEntities = audioSourceQuery.ToEntityArray(Allocator.TempJob);
        //     //
        //     // for (var i = 0; i < audioSources.Length; i++)
        //     // {
        //     //     if (audioSources[i].clip != clipEntity)
        //     //         continue;
        //     //
        //     //     entityManager.AddComponent<AudioSourceStop>(audioSourceEntities[i]);
        //     // }
        //     //
        //     // audioSources.Dispose();
        //     // audioSourceEntities.Dispose();
        // }

        // private static Entity GetAudioLibrary(EntityManager entityManager)
        // {
        //     var libraryQuery = entityManager.CreateEntityQuery(typeof(AudioLibrary));
        //     return libraryQuery.GetSingletonEntity();
        // }
        //
        // private static Entity FindAudioClip(EntityManager entityManager, AudioTypes audioType)
        // {
        //     var library = GetAudioLibrary(entityManager);
        //     var audioClips = entityManager.GetBuffer<AudioObject>(library);
        //
        //     return FindAudioClip(audioClips, audioType);
        // }
        //
        // private static Entity FindAudioClip(DynamicBuffer<AudioObject> audioClips, AudioTypes audioType)
        // {
        //     var clipIndex = int.MaxValue;
        //     for (var i = 0; i < audioClips.Length; i++)
        //     {
        //         if (audioClips[i].Type == audioType)
        //         {
        //             clipIndex = i;
        //             break;
        //         }
        //     }
        //
        //     return clipIndex == int.MaxValue ? Entity.Null : audioClips[clipIndex].Clip;
        // }

        private static DeAudioClipData GetAudioClipData(in EntityManager em, string name)
        {
            var e = GameService.GetGameManagerEntity();

            if (em.HasComponent<AudioListComponent>(e))
            {
                var audioList = em.GetComponentObject<AudioListComponent>(e);

                if (audioList == null || !audioList.AudioClips.ContainsKey(name))
                {
                    return null;
                }

                return audioList.AudioClips[name];
            }
            
            return null;
        }
    }
}