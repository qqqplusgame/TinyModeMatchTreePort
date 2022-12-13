using Unity.Entities;
using UnityEngine;

namespace ProjectM.Authoring
{
    public class SpriteAnimationAuthoring : MonoBehaviour
    {
        public int CurrentFrame;
        public int TotalFrames;
        public float FrameRate;
        public float Time;
        public bool IsLooping;
        public Sprite[] Sprites;
    }
    
    public class SpriteAnimationBaker : Baker<SpriteAnimationAuthoring>
    {
        public override void Bake(SpriteAnimationAuthoring authoring)
        {
            AddComponent(new SpriteAnimation
            {
                CurrentFrame = authoring.CurrentFrame,
                TotalFrames = authoring.TotalFrames,
                FrameRate = authoring.FrameRate,
                Time = authoring.Time,
                IsLooping = authoring.IsLooping
            });
            AddComponentObject(new SpriteAnimationList()
            {
                Sprites = authoring.Sprites
            });
        }
    }
}