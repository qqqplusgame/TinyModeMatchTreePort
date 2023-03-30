using Unity.Entities;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class GameSystemGroup : ComponentSystemGroup
    {
    }


    [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
    public partial class PreGameECBSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(PreGameECBSystemGroup))]
    public partial class GameECBSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(GameECBSystemGroup))]
    public partial class GameECB2SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(GameECB2SystemGroup))]
    public partial class GameECB3SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateAfter(typeof(GameECB3SystemGroup))]
    [UpdateBefore(typeof(ParentSystem))]
    public partial class GameECB4SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameECBSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(GameECB2System))]
    public partial class BeginGameECBSystem : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECBSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(GameECB2System))]
    public partial class GameECBSystem : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB2SystemGroup), OrderLast = true)]
    public partial class GameECB2System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB3SystemGroup), OrderLast = true)]
    public partial class GameECB3System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB4SystemGroup), OrderLast = true)]
    public partial class GameECB4System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }
}