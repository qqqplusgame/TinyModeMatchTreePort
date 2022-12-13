using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial class UpdateDestroyLineAnimation : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = World.Time.DeltaTime;

            // Update the claw/stratch animation when using a special gem power up.
            Entities.ForEach((SpriteRenderer spriteRenderer,ColorGradient gradient, ref DestroyLineAnimation destroyLineAnimation,
                ref LocalTransform  transform
            ) =>
            {
                destroyLineAnimation.Timer += deltaTime;

                var position = transform.Position;
                position.x = destroyLineAnimation.StartPositionX;
                position.y = destroyLineAnimation.StartPositionY;
                transform.Position = position;

                var scaleRatio = math.min(1, destroyLineAnimation.Timer / destroyLineAnimation.ScaleDuration);
                var startPosition =
                    new float2(destroyLineAnimation.StartPositionX, destroyLineAnimation.StartPositionY);
                var endPosition = new float2(destroyLineAnimation.EndPositionX, destroyLineAnimation.EndPositionY);
                var startToEndDistance = math.distance(startPosition, endPosition);
                spriteRenderer.size = new Vector2(spriteRenderer.size.x, scaleRatio * startToEndDistance);

                var colorRatio = math.min(1, destroyLineAnimation.Timer / destroyLineAnimation.Duration);
                //todo set color by Gradient
                spriteRenderer.color = gradient.gradient.Evaluate(colorRatio);

                var angle = math.atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) - math.PI / 2;
                transform.Rotation = quaternion.AxisAngle(new float3(0, 0, 1), angle);
            }).WithoutBurst().Run();

            // Update the gem laser animation when using a special 5-match same color bomb power up.
            Entities.ForEach((SpriteRenderer spriteRenderer, ref DestroyLaserAnimation destroyLaserAnimation,
                ref LocalTransform  transform) =>
            {
                destroyLaserAnimation.Timer += deltaTime;

                var position = transform.Position;
                position.x = destroyLaserAnimation.StartPositionX;
                position.y = destroyLaserAnimation.StartPositionY;
                transform.Position = position;

                var startPosition = new float2(destroyLaserAnimation.StartPositionX,
                    destroyLaserAnimation.StartPositionY);
                var endPosition = new float2(destroyLaserAnimation.EndPositionX, destroyLaserAnimation.EndPositionY);
                var startToEndDistance = math.distance(startPosition, endPosition);
                spriteRenderer.size = new Vector2(startToEndDistance, spriteRenderer.size.y);

                var angle = math.atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x);
                transform.Rotation = quaternion.AxisAngle(new float3(0, 0, 1), angle);
            }).WithoutBurst().Run();
        }
    }
}