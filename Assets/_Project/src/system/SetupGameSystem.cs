using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    //[UpdateBefore(typeof(CopyInitialTransformFromGameObjectSystem))]
    public partial struct SetupGameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            Debug.Log("SetupGameSystem");

            var em = state.EntityManager;
            if (SystemAPI.TryGetSingletonEntity<GameManager>(out var e))
            {
                Debug.Log("GetSingletonEntity");
                
                GameService.init(em, e);

                GridService.init(em, e);

                
                
                state.Enabled = false;
            }
            // var e = SystemAPI.GetSingletonEntity<GameManager>();
            // if (SystemAPI.Exists(e))
            // {
            //     GameService.init(World.DefaultGameObjectInjectionWorld.EntityManager, e);
            //
            //     GridService.init(World.DefaultGameObjectInjectionWorld.EntityManager, e);
            // }

           
        }
    }
}