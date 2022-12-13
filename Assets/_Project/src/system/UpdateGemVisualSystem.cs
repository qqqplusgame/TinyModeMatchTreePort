using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(TransformSystemGroup), OrderFirst = true)]
    public partial class UpdateGemVisualSystem: SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimECBSystem;

        protected override void OnCreate()
        {
            endSimECBSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = endSimECBSystem.CreateCommandBuffer();
            var showHintDelay = GameService.getGameState(EntityManager).ShowHintDelay;
            var deltaTime = World.Time.DeltaTime;
            var em = EntityManager;
            Entities.ForEach((Entity gemEntity, ref Gem gem) =>
            {
                // updateHighlightAlpha(gemEntity, ref gem, showHintDelay, deltaTime, endSimECB);
                float highlightAlpha = 0;
                if (gem.IsSelected && gem.GemType != GemTypes.ColorBomb)
                {
                    // Highlight gem if it's selected.
                    highlightAlpha = 1;
                }
                else if (SystemAPI.HasComponent<MatchPossibility>(gemEntity))
                {
                    // Update the gem highlight opacity animation for match hint.
                    var matchPossibility = SystemAPI.GetComponent<MatchPossibility>(gemEntity);
                    var timer = matchPossibility.HintTimer;
                    timer += deltaTime;
                    matchPossibility.HintTimer = timer;
                    ecb.SetComponent(gemEntity, matchPossibility);

                    if (timer > showHintDelay)
                    {
                        highlightAlpha = 1 - ((math.cos(6 * (timer - showHintDelay)) + 1) / 2);
                    }
                }

                if (highlightAlpha != gem.HighlightAlpha)
                {
                    gem.HighlightAlpha = highlightAlpha;

                    var highlightSpriteRenderer =
                        em.GetComponentObject<SpriteRenderer>(gem.SpriteRendererHighlightGem);
                    var color = highlightSpriteRenderer.color;
                    color.a = highlightAlpha;
                    highlightSpriteRenderer.color = color;
                    GameService.setEntityEnabled(em, ecb, gem.SpriteRendererHighlightGem, true);
                    //EntityManager.set(gem.SpriteRendererHighlightGem, highlightSpriteRenderer);
                }
                // updatePowerUpVisual(ref gem, endSimECB);
                if (gem.CurrentPowerUpVisual != gem.PowerUp)
                {
                    gem.CurrentPowerUpVisual = gem.PowerUp;

                    GameService.setEntityEnabled(em, ecb, gem.RowPowerUpVisual,
                        gem.PowerUp == GemPowerUpTypes.Row);
                    GameService.setEntityEnabled(em, ecb, gem.ColumnPowerUpVisual,
                        gem.PowerUp == GemPowerUpTypes.Column);
                    GameService.setEntityEnabled(em, ecb, gem.SquarePowerUpVisual,
                        gem.PowerUp == GemPowerUpTypes.Square);
                    GameService.setEntityEnabled(em, ecb, gem.DiagonalPowerUpVisual,
                        gem.PowerUp == GemPowerUpTypes.DiagonalCross);
                    GameService.setEntityEnabled(em, ecb, gem.SameColorPowerUpVisual,
                        gem.PowerUp == GemPowerUpTypes.SameColor);
                }
            }).WithoutBurst().Run();
        }

        // void updateHighlightAlpha(Entity gemEntity, ref Gem gem, float showHintDelay, float deltaTime,
        //     EntityCommandBuffer ecb)
        // {
        //     float highlightAlpha = 0;
        //     if (gem.IsSelected && gem.GemType != GemTypes.ColorBomb)
        //     {
        //         // Highlight gem if it's selected.
        //         highlightAlpha = 1;
        //     }
        //     else if (EntityManager.HasComponent<MatchPossibility>(gemEntity))
        //     {
        //         // Update the gem highlight opacity animation for match hint.
        //         var matchPossibility = GetComponent<MatchPossibility>(gemEntity);
        //         var timer = matchPossibility.HintTimer;
        //         timer += deltaTime;
        //         matchPossibility.HintTimer = timer;
        //         ecb.SetComponent(gemEntity, matchPossibility);
        //
        //         if (timer > showHintDelay)
        //         {
        //             highlightAlpha = 1 - ((math.cos(6 * (timer - showHintDelay)) + 1) / 2);
        //         }
        //     }
        //
        //     if (highlightAlpha != gem.HighlightAlpha)
        //     {
        //         gem.HighlightAlpha = highlightAlpha;
        //
        //         var highlightSpriteRenderer =
        //             EntityManager.GetComponentObject<SpriteRenderer>(gem.SpriteRendererHighlightGem);
        //         var color = highlightSpriteRenderer.color;
        //         color.a = highlightAlpha;
        //         highlightSpriteRenderer.color = color;
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.SpriteRendererHighlightGem, true);
        //         //EntityManager.set(gem.SpriteRendererHighlightGem, highlightSpriteRenderer);
        //     }
        // }
        //
        // void updatePowerUpVisual(ref Gem gem, EntityCommandBuffer ecb)
        // {
        //     if (gem.CurrentPowerUpVisual != gem.PowerUp)
        //     {
        //         gem.CurrentPowerUpVisual = gem.PowerUp;
        //
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.RowPowerUpVisual,
        //             gem.PowerUp == GemPowerUpTypes.Row);
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.ColumnPowerUpVisual,
        //             gem.PowerUp == GemPowerUpTypes.Column);
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.SquarePowerUpVisual,
        //             gem.PowerUp == GemPowerUpTypes.Square);
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.DiagonalPowerUpVisual,
        //             gem.PowerUp == GemPowerUpTypes.DiagonalCross);
        //         GameService.setEntityEnabled(EntityManager, ecb, gem.SameColorPowerUpVisual,
        //             gem.PowerUp == GemPowerUpTypes.SameColor);
        //     }
        // }
    }
}