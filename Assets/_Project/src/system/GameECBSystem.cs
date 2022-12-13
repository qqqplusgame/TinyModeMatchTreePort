using Unity.Entities;
using Unity.Transforms;

namespace ProjectM
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class GameSystemGroup : ComponentSystemGroup
    {
    }


    [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
    public class PreGameECBSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    public class GameECBSystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    public class GameECB2SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    public class GameECB3SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameSystemGroup))]
    [UpdateBefore(typeof(ParentSystem))]
    public class GameECB4SystemGroup : ComponentSystemGroup
    {
    }

    [UpdateInGroup(typeof(GameECBSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(GameECB2System))]
    public class BeginGameECBSystem : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECBSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(GameECB2System))]
    public class GameECBSystem : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB2SystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(GameECB3System))]
    public class GameECB2System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB3SystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(GameECB3System))]
    public class GameECB3System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }

    [UpdateInGroup(typeof(GameECB4SystemGroup), OrderLast = true)]
    public class GameECB4System : EntityCommandBufferSystem
    {
        // This class is intentionally empty. There is generally no
        // reason to put any code in an EntityCommandBufferSystem.
    }
}