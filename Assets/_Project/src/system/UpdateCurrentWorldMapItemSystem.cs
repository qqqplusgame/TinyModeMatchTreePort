using Unity.Entities;

namespace ProjectM
{
    [UpdateInGroup(typeof(PreGameECBSystemGroup),OrderFirst = true)]
    public partial class UpdateCurrentWorldMapItemSystem : SystemBase
    {
        protected override void OnUpdate()
        {
        }
    }
}