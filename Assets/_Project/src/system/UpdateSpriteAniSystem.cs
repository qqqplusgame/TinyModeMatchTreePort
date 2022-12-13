using ProjectM;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    /// <summary>
    /// update the sprite animation,
    /// todo: split this into two systems, one for calculating the animation frame and one for updating the sprite
    /// </summary>
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial class UpdateSpriteAniSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.Time.DeltaTime;

            // Update the claw/stratch animation when using a special gem power up.
            Entities.ForEach((SpriteRenderer spriteRenderer, ref SpriteAnimation spriteAni,
                in SpriteAnimationList spriteAniList
            ) =>
            {
                if (spriteAniList.Sprites.Length == 0 ||
                    (spriteAni.CurrentFrame >= spriteAni.TotalFrames - 1 && spriteAni.IsLooping == false))
                {
                    return;
                }

                var ani = spriteAniList.Sprites[spriteAni.CurrentFrame];
                spriteRenderer.sprite = ani;
                spriteAni.Time += deltaTime;
                if (spriteAni.Time >= spriteAni.FrameRate)
                {
                    spriteAni.Time -= spriteAni.FrameRate;
                    spriteAni.CurrentFrame = (spriteAni.CurrentFrame + 1) % spriteAni.TotalFrames;
                }
            }).WithoutBurst().Run();
        }
    }
}