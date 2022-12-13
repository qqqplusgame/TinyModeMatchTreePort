using Unity.Entities;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameSystemGroup), OrderFirst = true)]
    public partial class UpdateGameStateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!UiService.IsInitialized)
                return;
            var em = EntityManager;
            // Entities.WithAll<GameStateChange>().ForEach((Entity e) =>
            // {
            //     var gameState = GameService.getGameState(em);
            //     GameStateLoadingService.SetGameState(em, gameState.GameStateType);
            //     em.DestroyEntity(e);
            // }).WithoutBurst().WithStructuralChanges().Run();
            var cmdBuffer = World.GetOrCreateSystemManaged<BeginGameECBSystem>().CreateCommandBuffer();
            foreach (var (change, e) in SystemAPI.Query<GameStateChange>().WithEntityAccess())
            {
                var gameState = GameService.getGameState(em);
                GameStateLoadingService.SetGameState(em, cmdBuffer, gameState.GameStateType);
                cmdBuffer.DestroyEntity(e);
                Debug.Log($"update game state {gameState.GameStateType}");
            }
        }
    }
}