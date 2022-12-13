using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    [UpdateInGroup(typeof(GameECB2SystemGroup))]
    public partial class SpawnGemExplosionSystem : SystemBase
    {
        GameECB2System ecbSystem;

        protected override void OnCreate()
        {
            ecbSystem = World.GetOrCreateSystemManaged<GameECB2System>();
        }

        protected override void OnUpdate()
        {
            if (GridService.isGridFrozen(EntityManager))
            {
                return;
            }

            var em = EntityManager;
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
            var gameMangerEntity = GameService.GetGameManagerEntity();
            var gameManager = em.GetComponentData<GameManager>(gameMangerEntity);
            //todo create effect particle
            Entities.ForEach((in Gem gemToDestroy, in Matched matched,
                in LocalTransform  gemTransformLocalPosition) =>
            {
                var particleEntity1 = em.Instantiate(gameManager.ExplodingGem1Prefab);
                initParticleSystem(em, ecb, particleEntity1, gemToDestroy, gemTransformLocalPosition);

                var particleEntity2 = em.Instantiate(gameManager.ExplodingGem2Prefab);
                initParticleSystem(em, ecb, particleEntity2, gemToDestroy, gemTransformLocalPosition);
            }).WithStructuralChanges().WithoutBurst().Run();
        }

        static void initParticleSystem(in EntityManager em, in EntityCommandBuffer ecb, in Entity explosionEntity,
            in Gem gemToDestroy, in LocalTransform  gemTransformLocalPosition)
        {
            var emitter = em.GetComponentObject<ParticleSystem>(explosionEntity);

            var mainModule = emitter.main;
            var color = mainModule.startColor;
            color.color = GemService.getGemParticleColor(gemToDestroy);
            mainModule.startColor = color;
            //var particleSpriteRenderer = em.GetComponentData(emitter.particle, ut.Core2D.Sprite2DRenderer);
            //particleSpriteRenderer.color = GemService.getGemParticleColor(this.world, gemToDestroy);
            //EntityManager.SetComponentData(emitter.particle, particleSpriteRenderer);

            var gemPosition = gemTransformLocalPosition.Position;
            var explosionTransformPosition =
                em.GetComponentData<LocalTransform>(explosionEntity);
            var position = explosionTransformPosition.Position;
            position.x = gemPosition.x;
            position.y = gemPosition.y;
            explosionTransformPosition.Position = position;
            em.SetComponentData(explosionEntity, explosionTransformPosition);
        }
    }
}