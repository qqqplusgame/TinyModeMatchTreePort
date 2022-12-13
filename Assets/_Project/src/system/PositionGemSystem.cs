using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial class PositionGemSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var grid = GridService.getGridConfiguration(EntityManager);
            //todo need fix may not work  
            Entities.WithNone<Disabled>().ForEach((ref LocalTransform  transformLocalPosition, in Gem gem) =>
            {
                if (!gem.IsFalling && !gem.IsSwapping)
                {
                    transformLocalPosition.Position = GemService.getGemWorldPosition(ref grid, in gem);
                }
            }).Run();
        }
    }
}