using Unity.Entities;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial class DestroyAfterDelaySystem : SystemBase
    {
        GameECB3System endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<GameECB3System>();
        }

        protected override void OnUpdate()
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = endSimECBSystem.CreateCommandBuffer().AsParallelWriter();
            Entities
                .WithAll<DestroyAfterDelay>()
                .ForEach((Entity entity, int entityInQueryIndex, ref DestroyAfterDelay destroyAfterDelay) =>
                {
                    destroyAfterDelay.Delay -= deltaTime;
                    if (destroyAfterDelay.Delay <= 0)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    }
                })
                .ScheduleParallel();

            endSimECBSystem.AddJobHandleForProducer(Dependency);
        }
    }
}