using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectM
{
    public class GridService
    {
        private static Entity gridConfigEntity;
        private static Entity gameMangerEntity;

        private static readonly SharedStatic<Entity> sharedGridConfigEntity =
            SharedStatic<Entity>.GetOrCreate<GridService>();

        public static void init(in EntityManager em, Entity gmEntity)
        {
            gridConfigEntity = em.CreateEntity();
            sharedGridConfigEntity.Data = gridConfigEntity;
            gameMangerEntity = gmEntity;
#if UNITY_EDITOR
            em.SetName(gridConfigEntity, "GridService");
#endif
            var gameManager = em.GetComponentData<GameManager>(gameMangerEntity);

            em.AddComponentData(gridConfigEntity, new GridConfiguration()
            {
                IsGridCreated = false,
                CellDimension = gameManager.GridCellDimension,
                FrozenGridTimer = 0,
                GridDefaultPositionY = gameManager.GridDefaultPositionY,
                GridOffsetPositionY = 0,
                Height = gameManager.GridHeight,
                Width = gameManager.GridWidth
            });
            em.AddBuffer<GridConfigurationCellEntities>(gridConfigEntity);
            em.AddBuffer<GridConfigurationGemEntities>(gridConfigEntity);
        }


        public static GridConfiguration getGridConfiguration(in EntityManager em)
        {
            if (!em.Exists(gridConfigEntity))
                return new GridConfiguration()
                {
                    CellDimension = -1
                };
            return em.GetComponentData<GridConfiguration>(gridConfigEntity);
        }

        public static void setGridConfiguration(in EntityManager em, GridConfiguration grid)
        {
            Assert.IsTrue(em.Exists(gridConfigEntity));
            em.SetComponentData(gridConfigEntity, grid);
        }

        public static void clear(in EntityManager em, in EntityCommandBuffer ecb, ref GridConfiguration grid)
        {
            grid.IsGridCreated = false;


            //grid.CellEntities = new Array(grid.Width * grid.Height);

            var cellBuff = em.GetBuffer<GridConfigurationCellEntities>(gridConfigEntity);

            foreach (var one in cellBuff)
            {
                if (one.Cell != Entity.Null)
                {
                    ecb.DestroyEntity(one.Cell);
                }
            }


            cellBuff.Clear();

            for (int i = 0; i < grid.Width * grid.Height; i++)
            {
                cellBuff.Add(new GridConfigurationCellEntities()
                {
                    Cell = Entity.Null
                });
#if UNITY_EDITOR
                em.SetName(cellBuff[i].Cell, "Cell_" + i);
#endif
            }

            var gemBuff = em.GetBuffer<GridConfigurationGemEntities>(gridConfigEntity);
            foreach (var one in gemBuff)
            {
                if (one.Gem != Entity.Null)
                {
                    ecb.DestroyEntity(one.Gem);
                }
            }

            gemBuff.Clear();

            for (int i = 0; i < grid.Width * grid.Height; i++)
            {
                gemBuff.Add(new GridConfigurationGemEntities()
                {
                    Gem = Entity.Null
                });
#if UNITY_EDITOR
                em.SetName(gemBuff[i].Gem, "Gem_" + i);
#endif
            }


            em.SetComponentData(gridConfigEntity, grid);
        }

        public static bool isGridFrozen(in EntityManager em)
        {
            if (getGridConfiguration(em).IsGridCreated)
            {
                return getGridConfiguration(em).FrozenGridTimer > 0;
            }

            return true;
        }

        public static Entity getGemEntity(in EntityManager em, int cellHashCode)
        {
            var buff = em.GetBuffer<GridConfigurationGemEntities>(sharedGridConfigEntity.Data);
            if (cellHashCode >= 0 && cellHashCode < buff.Length)
                return buff[cellHashCode].Gem;
            return Entity.Null;
        }

        public static DynamicBuffer<GridConfigurationGemEntities> getGemBuff(in EntityManager em)
        {
            return em.GetBuffer<GridConfigurationGemEntities>(sharedGridConfigEntity.Data);
        }

        // public static DynamicBuffer<GridConfigurationGemEntities> getGemBuff(in EntityManager em,EntityCommandBuffer ecb)
        // {
        //     //var b = em.GetBuffer<GridConfigurationGemEntities>(gridConfigEntity);
        //     //var a = ecb.SetBuffer<GridConfigurationGemEntities>(gridConfigEntity);
        //     //a.a
        //     ecb.SetBuffer<>()
        //     return ecb.SetBuffer<GridConfigurationGemEntities>(gridConfigEntity);
        // }

        public static Cell getCellAt(in EntityManager em, ref GridConfiguration grid, int x, int y)
        {
            var hashCode = getCellHashCode(in grid, x, y);

            var cellBuff = em.GetBuffer<GridConfigurationCellEntities>(gridConfigEntity);

            if (cellBuff.Length > hashCode && em.Exists(cellBuff[hashCode].Cell))
            {
                return em.GetComponentData<Cell>(cellBuff[hashCode].Cell);
            }
            else
                return new Cell()
                {
                    Size = -1
                };
        }

        public static int getCellHashCode(in GridConfiguration grid, int x, int y)
        {
            //origin code has a bug, cause typescript has not check array index out of range 
            //return x * (grid.Width + 1) + y;
            return y * grid.Width + x;
        }

        public static int2 getPositionFromCellHashCode(in GridConfiguration grid, int hashCode)
        {
            return new int2(hashCode % grid.Width, hashCode / grid.Width);
            // var width = grid.Width + 1;
            // return new int2((int) math.floor(hashCode / width), hashCode % width);
        }

        public static float3 getCellWorldPosition(in GridConfiguration grid, Cell cell)
        {
            return getGridToWorldPosition(in grid, cell.X, cell.Y);
        }

        public static float3 getGridToWorldPosition(in GridConfiguration grid, int x, int y)
        {
            var cellSize = grid.CellDimension;
            var gridWidth = cellSize * grid.Width;
            var gridHeight = cellSize * grid.Height;
            var position = new float3(
                x * cellSize - gridWidth / 2 + cellSize / 2,
                grid.GridDefaultPositionY + grid.GridOffsetPositionY + y * cellSize - gridHeight / 2 + cellSize / 2,
                0);

            return position;
        }

        public static void createGridCells(in EntityManager em, in EntityCommandBuffer ecb, ref GridConfiguration grid)
        {
            clear(em, ecb, ref grid);

            for (int i = 0; i < grid.Width; i++)
            {
                for (int j = 0; j < grid.Height; j++)
                {
                    var cell = getCellAt(em, ref grid, i, j);
                    if (cell.Size == -1)
                    {
                        var entity = createCell(ref grid, em, i, j);
                        cell = em.GetComponentData<Cell>(entity);

                        var transform = em.GetComponentData<LocalTransform>(entity);
                        var position = getCellWorldPosition(in grid, cell);
                        transform.Position.x = position.x;
                        transform.Position.y = position.y;
                        em.SetComponentData(entity, transform);


                        //背景渲染调整
                        var gameAssets = em.GetComponentObject<GameAssets>(gameMangerEntity);


                        // let spriteRendererOptions = world.getComponentData(entity, ut.Core2D.Sprite2DRendererOptions);
                        // spriteRendererOptions.size = new Vector2(cell.Size, cell.Size);
                        // world.setComponentData(entity, spriteRendererOptions);

                        var spriteName = "Center";
                        if (i == 0 && j == 0)
                        {
                            spriteName = "BottomLeft";
                        }
                        else if (i == grid.Width - 1 && j == 0)
                        {
                            spriteName = "BottomRight";
                        }
                        else if (i == grid.Width - 1 && j == grid.Height - 1)
                        {
                            spriteName = "TopRight";
                        }
                        else if (i == 0 && j == grid.Height - 1)
                        {
                            spriteName = "TopLeft";
                        }
                        else if (j == 0)
                        {
                            spriteName = "Bottom";
                        }
                        else if (i == grid.Width - 1)
                        {
                            spriteName = "Right";
                        }

                        var spriteRenderer = em.GetComponentObject<SpriteRenderer>(entity);

                        spriteRenderer.sprite = gameAssets.spriteDic[spriteName];


                        // let path = "assets/sprites/Cells/" + spriteName;
                        // let spriteRenderer = world.getComponentData(entity, ut.Core2D.Sprite2DRenderer);
                        // spriteRenderer.sprite = world.getEntityByName(path);
                        // world.setComponentData(entity, spriteRenderer);
                    }
                }
            }
            //em.SetComponentData(gridConfigEntity,grid);
        }

        public static Entity createCell(ref GridConfiguration grid, in EntityManager em, int x, int y)
        {
            var gameManager = em.GetComponentData<GameManager>(gameMangerEntity);

            var cellEntity = em.Instantiate(gameManager.CellPrefab);
            var cellBuff = em.GetBuffer<GridConfigurationCellEntities>(gridConfigEntity);

#if UNITY_EDITOR
            em.SetName(cellEntity, "Cell_" + x + "_" + y);
#endif

            if (em.HasComponent<Cell>(cellEntity))
            {
                var cell = em.GetComponentData<Cell>(cellEntity);
                cell.X = x;
                cell.Y = y;
                em.SetComponentData(cellEntity, cell);
            }
            else
            {
                em.AddComponentData(cellEntity, new Cell()
                {
                    X = x,
                    Y = y
                });
            }

            cellBuff.ElementAt(getCellHashCode(in grid, x, y)).Cell = cellEntity;

            return cellEntity;
        }
    }
}