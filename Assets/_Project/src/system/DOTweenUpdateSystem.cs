using DG.Tweening;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    public enum GemMoveType
    {
        Swap,
        Fall,
    }
    public struct TweenMove : IComponentData
    {
        public float duration;
        public float time;
        public float3 from;
        public float3 to;
        public Ease ease;
        public GemMoveType moveType;
    }
    
    [UpdateInGroup(typeof(PreGameECBSystemGroup))]
    public partial class DOTweenUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (trans, gem, tweem, entity) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRW<Gem>, RefRW<TweenMove>>().WithNone<Disabled>()
                         .WithEntityAccess())
            {
                tweem.ValueRW.time += deltaTime;
                var progress = tweem.ValueRW.time / tweem.ValueRW.duration;

                var from = tweem.ValueRW.from;
                var to = tweem.ValueRW.to;
                var value = DOVirtual.EasedValue(new Vector3(from.x, from.y, from.z),
                    new Vector3(to.x, to.y, to.z)
                    , progress, tweem.ValueRW.ease);

                trans.ValueRW.Position = new float3(value.x, value.y, value.z);


                if (progress >= 1.0f)
                {
                    ecb.RemoveComponent<TweenMove>(entity);
                    if (tweem.ValueRW.moveType == GemMoveType.Fall && gem.ValueRW.IsFalling)
                    {
                        gem.ValueRW.IsFalling = false;
                    }
                    else if (tweem.ValueRW.moveType == GemMoveType.Swap && gem.ValueRW.IsSwapping)
                    {
                        gem.ValueRW.IsSwapping = false;
                    }
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}