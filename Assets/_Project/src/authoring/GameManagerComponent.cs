using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using UnityEngine.U2D;

namespace ProjectM.Authoring
{
    // [Serializable]
    // public class LevelConvertData
    // {
    //     public int LevelID;
    //     public SkinTypes Skin;
    //     public int MaxMoveCount;
    //     public GemPowerUpTypes[] StartPowerUps;
    //     public GemTypes[] MissingGems;
    // }

    // [ConverterVersion("Tiny2D", 1)]
    public class GameManagerComponent : MonoBehaviour
    {
        public GameObject[] Levels;
        public GameObject CellPrefab;
        public GameObject GemPrefab;
        public GameObject ExplodingGem1Prefab;
        public GameObject ExplodingGem2Prefab;
        public GameObject DestroyLineAnimationPrefab;
        public GameObject DestroyLaserAnimationPrefab;

        public ParticleSystem.MinMaxCurve Curve;

        public Sprite[] SpriteRefList;


        public int GridWidth;
        public int GridHeight;
        public int GridCellDimension;
        public int GridDefaultPositionY;
    }

    public class GameManagerBaker : Baker<GameManagerComponent>
    {
        public override void Bake(GameManagerComponent authoring)
        {
            var gameManager = new GameManager();

            if (authoring.CellPrefab != null)
                gameManager.CellPrefab = GetEntity(authoring.CellPrefab);
            if (authoring.GemPrefab != null)
                gameManager.GemPrefab = GetEntity(authoring.GemPrefab);
            if (authoring.ExplodingGem1Prefab != null)
                gameManager.ExplodingGem1Prefab = GetEntity(authoring.ExplodingGem1Prefab);
            if (authoring.ExplodingGem2Prefab != null)
                gameManager.ExplodingGem2Prefab = GetEntity(authoring.ExplodingGem2Prefab);
            if (authoring.DestroyLineAnimationPrefab != null)
                gameManager.DestroyLineAnimationPrefab = GetEntity(authoring.DestroyLineAnimationPrefab);
            if (authoring.DestroyLaserAnimationPrefab != null)
                gameManager.DestroyLaserAnimationPrefab = GetEntity(authoring.DestroyLaserAnimationPrefab);
            

            gameManager.GridWidth = authoring.GridWidth;
            gameManager.GridHeight = authoring.GridHeight;
            gameManager.GridCellDimension = authoring.GridCellDimension;
            gameManager.GridDefaultPositionY = authoring.GridDefaultPositionY;

            AddComponent(gameManager);

            var gameAssets = new GameAssets();

            gameAssets.spriteDic = new Dictionary<string, Sprite>();

            foreach (var sprite in authoring.SpriteRefList)
            {
                gameAssets.spriteDic.Add(sprite.name, sprite);
            }

            AddComponentObject(gameAssets);


            if (authoring.Levels == null)
                return;

            var buffer = AddBuffer<LevelBuff>();

            foreach (var level in authoring.Levels)
            {
                buffer.Add(new LevelBuff()
                {
                    levelEntity = GetEntity(level)
                });
            }
        }
    }
}