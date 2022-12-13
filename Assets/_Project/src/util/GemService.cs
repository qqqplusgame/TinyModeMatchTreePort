using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.U2D;
using Ease = DG.Tweening.Ease;

namespace ProjectM
{
    public class GemService
    {
        public static Entity getGemEntity(in EntityManager em, int cellHashCode)
        {
            var gemEntity = GridService.getGemEntity(em, cellHashCode);
            if (em.Exists(gemEntity))
            {
                return gemEntity;
            }
            else
            {
                return Entity.Null;
            }
        }


        public static Gem getGem(in EntityManager em, int cellHashCode)
        {
            var gemEntity = GridService.getGemEntity(em, cellHashCode);
            if (em.Exists(gemEntity) && em.HasComponent<Gem>(gemEntity))
            {
                return em.GetComponentData<Gem>(gemEntity);
            }
            else
            {
                return new Gem()
                {
                    CellHashKey = -1
                };
            }
        }

        public static Gem getGemFromEntity(in EntityManager em, Entity gemEntity)
        {
            if (em.Exists(gemEntity) && !em.HasComponent<Disabled>(gemEntity) && em.HasComponent<Gem>(gemEntity))
            {
                return em.GetComponentData<Gem>(gemEntity);
            }
            else
            {
                return new Gem()
                {
                    CellHashKey = -1
                };
            }
        }

        public static void setGem(in EntityManager em, ref GridConfiguration grid, int cellHashCode, Entity gemEntity)
        {
            var buff = GridService.getGemBuff(em);
            var gem = buff[cellHashCode];
            // if (gem.Gem != Entity.Null)
            //     em.DestroyEntity(gem.Gem);
            if (gemEntity == Entity.Null)
            {
                gem.Gem = Entity.Null;
            }
            else
            {
                gem.Gem = gemEntity;
            }

            buff[cellHashCode] = gem;
            //world.setConfigData(grid);
        }

        // public static void setGem(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
        //     int cellHashCode, Entity gemEntity)
        // {
        //     var buff = GridService.getGemBuff(em);
        //     var gem = buff[cellHashCode];
        //     // if (gem.Gem != Entity.Null)
        //     //     ecb.DestroyEntity(gem.Gem);
        //
        //     if (gemEntity == Entity.Null)
        //     {
        //         gem.Gem = ecb.CreateEntity();
        //     }
        //     else
        //     {
        //         gem.Gem = gemEntity;
        //     }
        //     
        //     buff[cellHashCode] = gem;
        //     //world.setConfigData(grid);
        // }


        public static Entity createGem(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
            int cellHashCode,
            GemTypes gemType)
        {
            return createGemOfType(em, ecb, ref grid, cellHashCode, gemType);
        }


        public static Entity createGemOfType(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
            int cellHashCode,
            GemTypes gemType)
        {
            var gameMangerEntity = GameService.GetGameManagerEntity();
            var gameManager = em.GetComponentData<GameManager>(gameMangerEntity);
            var gameAssets = em.GetComponentObject<GameAssets>(gameMangerEntity);
            var entity = em.Instantiate(gameManager.GemPrefab); //em.Instantiate();//(world, "game.Gem")[0];
#if UNITY_EDITOR
            em.SetName(entity, "Gem_" + cellHashCode.ToString("00"));
#endif
            var gem = em.GetComponentData<Gem>(entity);

            gem.GemType = gemType;
            gem.CellHashKey = cellHashCode;
            em.SetComponentData(entity, gem);
            //delay the gem instantiation to the next frame
            ecb.RemoveComponent<Disabled>(entity);

            Debug.Log("createGemOfType " + cellHashCode + " isFalling " + gem.IsFalling + " isSwapping " +
                      gem.IsSwapping);

            setGem(em, ref grid, cellHashCode, entity);

            var gemName = "";
            if (gemType == GemTypes.Blue)
            {
                gemName = "Blue";
            }
            else if (gemType == GemTypes.Green)
            {
                gemName = "Green";
            }
            else if (gemType == GemTypes.Purple)
            {
                gemName = "Purple";
            }
            else if (gemType == GemTypes.Red)
            {
                gemName = "Red";
            }
            else if (gemType == GemTypes.Silver)
            {
                gemName = "Silver";
            }
            else if (gemType == GemTypes.Yellow)
            {
                gemName = "Yellow";
            }

            var spriteName = "Gem_" + gemName;
            var spriteRenderer = em.GetComponentObject<SpriteRenderer>(entity);
            spriteRenderer.sprite = gameAssets.spriteDic[spriteName + "_Plain"];


            spriteRenderer = em.GetComponentObject<SpriteRenderer>(gem.SpriteRendererHighlightGem);
            spriteRenderer.sprite = gameAssets.spriteDic[spriteName + "_Highlighted"];


            return entity;
        }

        public static void deleteGem(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
            Entity gemEntity, ref Gem gem)
        {
            var gemHashKey = gem.CellHashKey;
            Debug.Log($"deleteGem {gemHashKey}  entity {gemEntity.Index}");
            if (em.HasComponent<MatchPossibility>(gemEntity))
            {
                ecb.RemoveComponent<MatchPossibility>(gemEntity);
            }

            //Tween.Stop(ecb, gemEntity);
            //ut.Tweens.TweenService.removeAllTweens(world, gemEntity);
            //ut.Core2D.TransformService.destroyTree(world, gemEntity, true);
            ecb.DestroyEntity(gemEntity);

            setGem(em, ref grid, gemHashKey, Entity.Null);
        }

        public static void setGemPowerUp(in EntityManager em, Entity gemEntity, ref Gem gem, GemPowerUpTypes powerUp)
        {
            gem.PowerUp = powerUp;
            if (powerUp == GemPowerUpTypes.SameColor)
            {
                setSpecialGemType(em, gemEntity, ref gem, GemTypes.ColorBomb);
            }
        }

        public static void setSpecialGemType(in EntityManager em, Entity gemEntity, ref Gem gem, GemTypes gemType)
        {
            //var pathGemSprite = "assets/sprites/Gems/";
            var gameAssets = GameService.getGameAssets(em);
            if (gemType == GemTypes.ColorBomb)
            {
                gem.GemType = gemType;
                var spriteRenderer = em.GetComponentObject<SpriteRenderer>(gemEntity);
                spriteRenderer.sprite = gameAssets.spriteDic["Gem_Colorless_Plain_Glow"];
                //spriteRenderer.sortingOrder = 9;
                //spriteRenderer.sprite = em.getEntityByName(pathGemSprite + "Gem_Colorless_Plain_Glow");
                // world.setComponentData(gemEntity, spriteRenderer);
                // let layerSorting = world.getComponentData(gemEntity, ut.Core2D.LayerSorting)
                // layerSorting.order = 9;
                // world.setComponentData(gemEntity, layerSorting);
            }
            else if (gemType == GemTypes.Egg)
            {
                gem.GemType = gemType;
                var spriteRenderer = em.GetComponentObject<SpriteRenderer>(gemEntity);
                spriteRenderer.sprite = gameAssets.spriteDic["Gem_Egg_Plain"];
                //spriteRenderer.sortingOrder = 9;
                spriteRenderer = em.GetComponentObject<SpriteRenderer>(gem.SpriteRendererHighlightGem);
                spriteRenderer.sprite = gameAssets.spriteDic["Gem_Egg_Highlighted"];

                // let spriteRenderer = world.getComponentData(gemEntity, ut.Core2D.Sprite2DRenderer)
                // spriteRenderer.sprite = world.getEntityByName(pathGemSprite + "Gem_Egg_Plain");
                // world.setComponentData(gemEntity, spriteRenderer);
                // let spriteRendererHighlight =
                //     world.getComponentData(gem.SpriteRendererHighlightGem, ut.Core2D.Sprite2DRenderer)
                // spriteRendererHighlight.sprite = world.getEntityByName(pathGemSprite + "Gem_Egg_Highlighted");
                // world.setComponentData(gem.SpriteRendererHighlightGem, spriteRendererHighlight); // 
            }
        }

        public static void addMatchedComponent(in EntityManager em, EntityCommandBuffer ecb, Entity gemEntity,
            bool isMatch)
        {
            if (!em.Exists(gemEntity))
                return;
            if (!em.HasComponent<Matched>(gemEntity))
            {
                var matched = new Matched();
                matched.CreatedPowerUp = GemPowerUpTypes.None;
                matched.IsMatch = isMatch;
                ecb.AddComponent(gemEntity, matched);
            }
        }

        public static Entity getGemEntityAtPosition(in EntityManager em, in GridConfiguration grid, int x, int y)
        {
            return getGemEntity(em, GridService.getCellHashCode(grid, x, y));
        }


        public static Gem getGemAtPosition(in EntityManager em, ref GridConfiguration grid, int x, int y)
        {
            return getGem(em, GridService.getCellHashCode(grid, x, y));
        }

        public static Entity getNeighborGem(in EntityManager em, ref GridConfiguration grid, in Gem gem, int xOffset,
            int yOffset)
        {
            var gemPosition = GridService.getPositionFromCellHashCode(grid, gem.CellHashKey);

            //ignore out of bounds neighbors
            if ((gemPosition.x == 0 && xOffset == -1)
                || (gemPosition.x == grid.Width - 1 && xOffset == 1)
                || (gemPosition.y == 0 && yOffset == -1)
                || (gemPosition.y == grid.Height - 1 && yOffset == 1))
                return Entity.Null;

            var neighborGemHashKey =
                GridService.getCellHashCode(grid, gemPosition.x + xOffset, gemPosition.y + yOffset);

            return getGemEntity(em, neighborGemHashKey);
        }

        public static bool areGemsNeighbor(ref GridConfiguration grid, ref Gem gem1, ref Gem gem2)
        {
            var gem1Position = GridService.getPositionFromCellHashCode(grid, gem1.CellHashKey);
            var gem2Position = GridService.getPositionFromCellHashCode(grid, gem2.CellHashKey);
            return math.abs(gem1Position.x - gem2Position.x) + math.abs(gem1Position.y - gem2Position.y) == 1;
        }

        public static void animateGemFall(in EntityManager em, EntityCommandBuffer ecb,
            ref GridConfiguration grid,
            Entity gemEntity, ref Gem gem, int cellHashKey,
            int fallHeight)
        {
            var transform = em.GetComponentData<LocalTransform>(gemEntity);
            animateGemFall(em, ecb, ref grid, gemEntity, ref gem, cellHashKey, fallHeight,
                transform.Position);
        }

        public static void animateGemFall(in EntityManager em, in EntityCommandBuffer ecb, 
            ref GridConfiguration grid,
            in Entity gemEntity, ref Gem gem, int cellHashKey,
            int fallHeight, in float3 startPosition)
        {
            if (fallHeight == 0)
            {
                Debug.LogWarning($"animateGemFall fallHeight is 0 for gem {gem.CellHashKey}");
                return;
            }

            //Debug.Log("animateGemFall " + cellHashKey);
            //var transform = em.GetComponentData<LocalTransform>(gemEntity);
            var targetPosition = GemService.getGemWorldPosition(ref grid, cellHashKey); // in gem);

            float fallDuration = 0;
            for (int i = 1; i <= fallHeight; i++)
            {
                fallDuration += 0.2f / i;
            }

            //TODO deal with tween
            // let gemTween = new ut.Tweens.TweenDesc;
            // gemTween.cid = ut.Core2D.TransformLocalPosition.cid;
            // gemTween.offset = 0;
            // gemTween.duration = fallDuration;
            // gemTween.func = ut.Tweens.TweenFunc.OutBounce;
            // gemTween.loop = ut.Core2D.LoopMode.Once;
            // gemTween.destroyWhenDone = false;
            // gemTween.t = 0.0;
            //
            // let tweenEntity = ut.Tweens.TweenService.addTweenVector3(world, gemEntity, transformPosition.position,
            //     targetPosition, gemTween);
            //
            // var gemCallback = new GemFallTweenEndCallback();
            // gemCallback.GemEntity = gemEntity;
            // em.AddComponentData<GemFallTweenEndCallback>(tweenEntity, gemCallback);


            // var tweenParams = new TweenParams(fallDuration, EaseType.Linear, 2, false);
            // tweenParams.EndCallBackEntity = tweenEcb.CreateEntity();
            // // tweenEcb.AddComponent(tweenParams.EndCallBackEntity,new TweenTag());
            //
            // var gemCallback = new GemFallTweenEndCallback();
            // gemCallback.GemEntity = gemEntity;
            // tweenEcb.AddComponent(tweenParams.EndCallBackEntity, gemCallback);
            //
            // Tween.Move(tweenEcb, gemEntity, startPosition, targetPosition, tweenParams);

            ecb.AddComponent(gemEntity,
                new TweenMove()
                {
                    duration = fallDuration,
                    time = 0,
                    from = startPosition,
                    to = targetPosition,
                    ease = Ease.OutBounce,
                    moveType = GemMoveType.Fall
                });

            gem.IsFalling = true;
            em.SetComponentData(gemEntity, gem);

            Debug.Log($"GemFallStart {gem.CellHashKey} {gemEntity.Index} {startPosition} {targetPosition}");
        }


        // public static void swapGems(in EntityManager em, ref GridConfiguration grid, Entity gemEntity1, ref Gem gem1,
        //     Entity gemEntity2, ref Gem gem2, EntityCommandBuffer ecb)
        // {
        //     var gem2HashKey = gem2.CellHashKey;
        //
        //     setGem(em, ref grid, gem1.CellHashKey, gemEntity2);
        //     gem2.CellHashKey = gem1.CellHashKey;
        //     //em.SetComponentData(gemEntity2, gem2);
        //     ecb.SetComponent(gemEntity2, gem2);
        //
        //     setGem(em, ref grid, gem2HashKey, gemEntity1);
        //     gem1.CellHashKey = gem2HashKey;
        //     //em.SetComponentData(gemEntity1, gem1);
        //     ecb.SetComponent(gemEntity1, gem1);
        // }

        public static void swapGems(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
            Entity gemEntity1, ref Gem gem1,
            Entity gemEntity2, ref Gem gem2)
        {
            var gem2HashKey = gem2.CellHashKey;

            setGem(em, ref grid, gem1.CellHashKey, gemEntity2);
            gem2.CellHashKey = gem1.CellHashKey;

            em.SetComponentData(gemEntity2, gem2);

            setGem(em, ref grid, gem2HashKey, gemEntity1);
            gem1.CellHashKey = gem2HashKey;
            em.SetComponentData(gemEntity1, gem1);
        }


        public static void animateGemsSwap(in EntityManager em, EntityCommandBuffer ecb, ref GridConfiguration grid,
            Entity gemEntity1,
            ref Gem gem1, Entity gemEntity2, ref Gem gem2)
        {
            animateGemSwap(em, ref grid, gemEntity1, ref gem1, ecb);
            animateGemSwap(em, ref grid, gemEntity2, ref gem2, ecb);
        }

        private static void animateGemSwap(in EntityManager em, ref GridConfiguration grid, in Entity gemEntity,
            ref Gem gem,
            EntityCommandBuffer ecb)
        {
            var transform = em.GetComponentData<LocalTransform>(gemEntity);
            var targetPosition = getGemWorldPosition(ref grid, gem);

            //todo tween
            // var gemTween = new ut.Tweens.TweenDesc;
            // gemTween.cid = ut.Core2D.TransformLocalPosition.cid;
            // gemTween.offset = 0;
            // gemTween.duration = 0.12;
            // gemTween.func = ut.Tweens.TweenFunc.Linear;
            // gemTween.loop = ut.Core2D.LoopMode.Once;
            // gemTween.destroyWhenDone = false;
            // gemTween.t = 0.0;
            //
            // let tweenEntity = ut.Tweens.TweenService.addTweenVector3(world, gemEntity, transformPosition.position,
            //     targetPosition, gemTween);

            var startPos = transform.Position;

            // var tweenParams = new TweenParams(0.12f, EaseType.Linear, 1, false);
            // tweenParams.EndCallBackEntity = ecb.CreateEntity();
            //
            // var gemCallback = new GemSwapTweenEndCallback();
            // gemCallback.GemEntity = gemEntity;
            // ecb.AddComponent(tweenParams.EndCallBackEntity, gemCallback);

            // Tween.Move(ecb, gemEntity, startPos, targetPosition, tweenParams);

            ecb.AddComponent<TweenMove>(gemEntity,
                new TweenMove()
                {
                    duration = 0.12f,
                    time = 0,
                    from = startPos,
                    to = targetPosition,
                    ease = Ease.Linear,
                    moveType = GemMoveType.Swap
                });


            Debug.Log($"animateGemSwap {gem.CellHashKey}");
            Debug.Log($"startPos {startPos} targetPosition {targetPosition}");
            gem.IsSwapping = true;
            // em.SetComponentData(gemEntity, gem);
            em.SetComponentData(gemEntity, gem);
        }

        public static float3 getGemWorldPosition(ref GridConfiguration grid, in Gem gem)
        {
            var gemPosition = GridService.getPositionFromCellHashCode(in grid, gem.CellHashKey);
            var gemWorldPosition = GridService.getGridToWorldPosition(in grid, gemPosition.x, gemPosition.y);
            return new float3(gemWorldPosition.x - 0.5f, gemWorldPosition.y + 0.5f, 0);
        }

        public static float3 getGemWorldPosition(ref GridConfiguration grid, int cellHashKey)
        {
            var gemPosition = GridService.getPositionFromCellHashCode(in grid, cellHashKey);
            var gemWorldPosition = GridService.getGridToWorldPosition(in grid, gemPosition.x, gemPosition.y);
            return new float3(gemWorldPosition.x - 0.5f, gemWorldPosition.y + 0.5f, 0);
        }


        public static Color getGemParticleColor(in Gem gem)  {
            switch (gem.GemType)
            {
                case GemTypes.Blue:
                {
                    return new Color(0f, 102f / 255f, 1f, 1f);
                }
                case GemTypes.Red:
                {
                    return new Color(236f / 255f, 23f / 255f, 40f / 255f, 1f);
                }
                case GemTypes.Green:
                {
                    return new Color(0f, 1f, 0f, 1f);
                }
                case GemTypes.Yellow:
                {
                    return new Color(1f, 198f / 255f, 0f, 1f);
                }
                case GemTypes.Silver:
                {
                    return new Color(199f / 255f, 1f, 1f, 1f);
                }
                case GemTypes.Purple:
                {
                    return new Color(205f / 255f, 39f / 255f, 255f / 255f, 1f);
                }
                default:
                {
                    return new Color(1f, 1f, 1f, 1f);
                }
            }
        }

        public static bool isMatchableType1(in Gem gem)
        {
            return gem.CellHashKey >= 0 &&
                   !gem.IsFalling &&
                   !gem.IsSwapping && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb;
        }

        public static bool isMatchableType2(in Gem gem)
        {
            return gem.CellHashKey >= 0 && gem.GemType != GemTypes.Egg && gem.GemType != GemTypes.ColorBomb;
        }
    }
}