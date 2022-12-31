using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.U2D;

namespace ProjectM
{
    #region Audio

    // public struct TweenTag : IComponentData
    // {
    // }

    public enum AudioTypes
    {
        None,
        Click,
        Bomb1,
        Bomb2,
        Over
    }

    public struct AudioLibrary : IComponentData
    {
    }

    public struct AudioObject : IBufferElementData
    {
        public AudioTypes Type;
        public Entity Clip;
    }

    #endregion

    #region Game Enum

    public enum GameStateTypes
    {
        None = -1,

        //Boot = 0,
        //Loading = 1,
        //MainMenu = 2,
        Game = 3,
        GameOver = 4,
        Settings = 5,

        //Credits = 6,
        Paused = 7,
        WorldMap = 8,
        //Cutscene = 9,
        //CutsceneEnd = 10,
        //Transition = 11,
        //Languages = 12,
    }

    public enum GemPowerUpTypes
    {
        None = 0,
        Row = 1,
        Column = 2,
        Square = 3,
        DiagonalCross = 4,
        SameColor = 5,
    }

    public enum GemTypes
    {
        Blue = 0,
        Green = 1,
        Purple = 2,
        Red = 3,
        Silver = 4,
        Yellow = 5,
        Egg = 6,
        ColorBomb = 7,
    }

    public enum SkinTypes
    {
        Camp = 0,
        Farm = 1,
        City = 2,
    }

    #endregion

    public struct DestroyAfterDelay : IComponentData
    {
        public float Delay;
    }

    public struct GameManager : IComponentData
    {
        public Entity CellPrefab;
        public Entity GemPrefab;
        public Entity ExplodingGem1Prefab;
        public Entity ExplodingGem2Prefab;
        public Entity DestroyLineAnimationPrefab;
        public Entity DestroyLaserAnimationPrefab;

        public int GridWidth;
        public int GridHeight;
        public int GridCellDimension;
        public int GridDefaultPositionY;
    }

    public class GameAssets : IComponentData
    {
        public Dictionary<string, Sprite> spriteDic;

        public List<SpriteAtlas> spriteAtlasList;

        // public SpriteAtlas GemAtlas;
        // public SpriteAtlas CellAtlas;
        public Sprite GetSprite(string name)
        {
            if (spriteDic.ContainsKey(name))
            {
                return spriteDic[name];
            }
            else
            {
                Debug.LogError("Sprite not found: " + name);
                return null;
            }
        }
    }

    public struct LevelBuff : IBufferElementData
    {
        public Entity levelEntity;
    }

    public struct Cell : IComponentData
    {
        public int X;
        public int Y;
        public int Size;
    }

    public struct SpriteAnimation : IComponentData
    {
        public int CurrentFrame;
        public int TotalFrames;
        public float FrameRate;
        public float Time;
        public bool IsLooping;
    }

    public class SpriteAnimationList : IComponentData
    {
        public Sprite[] Sprites;
    }

    public struct DestroyLaserAnimation : IComponentData
    {
        public float Timer;
        public float Duration;
        public float StartPositionX;
        public float StartPositionY;
        public float EndPositionX;
        public float EndPositionY;
    }

    public struct DestroyLineAnimation : IComponentData
    {
        public float Timer;
        public float Duration;
        public float ScaleDuration;
        public float StartPositionX;
        public float StartPositionY;
        public float EndPositionX;
        public float EndPositionY;
    }

    public class ColorGradient : IComponentData
    {
        public Gradient gradient;
    }

    public struct Selected : IComponentData
    {
    }

    public struct Falling : IComponentData
    {
    }

    public struct Swapping : IComponentData
    {
    }

    public struct Gem : IComponentData
    {
        public int CellHashKey;
        public bool IsSelected;
        public bool IsFalling;
        public bool IsSwapping;
        public Entity SpriteRendererHighlightGem;
        public GemTypes GemType;
        public float HighlightAlpha;
        public bool IsPossibleMatch;
        public GemPowerUpTypes PowerUp;
        public GemPowerUpTypes CurrentPowerUpVisual;
        public Entity RowPowerUpVisual;
        public Entity ColumnPowerUpVisual;
        public Entity DiagonalPowerUpVisual;
        public Entity SquarePowerUpVisual;
        public Entity SameColorPowerUpVisual;
    }


    public struct GemFallTweenEndCallback : IComponentData
    {
        public Entity GemEntity;
    }

    public struct GemSwap : IComponentData
    {
    }

    public struct GemSwapTweenEndCallback : IComponentData
    {
        public Entity GemEntity;
    }

    public struct GridConfigurationGemEntities : IBufferElementData
    {
        public Entity Gem;
    }

    public struct GridConfigurationCellEntities : IBufferElementData
    {
        public Entity Cell;
    }

    public struct GridConfiguration : IComponentData
    {
        public float FrozenGridTimer;
        public int Width;
        public int Height;
        public int CellDimension;
        public int GridDefaultPositionY;
        public int GridOffsetPositionY;
        public bool IsGridCreated;
    }

    public struct Matched : IComponentData
    {
        public GemPowerUpTypes CreatedPowerUp;
        public bool IsMatch;
    }

    public struct MatchPossibility : IComponentData
    {
        public float HintTimer;
        public int SwapGem1HashKey;
        public int SwapGem2HashKey;
        public bool NeedsSwap;
    }

    public struct LevelConfig
    {
        public BlobArray<GemPowerUpTypes> StartPowerUps;
        public BlobArray<GemTypes> MissingGems;
    }

    public struct Level : IComponentData
    {
        public int LevelID;
        public SkinTypes Skin;
        public int MaxMoveCount;
        public BlobAssetReference<LevelConfig> LevelConfig;
    }

    public struct GameState : IComponentData
    {
        public GameStateTypes GameStateType;
        public sbyte CurrentLevelID;
        public int CurrentScore;
        public int CurrentMoveCount;
        public float Time;
        public float SurvivalTimer;
        public int LevelCount;
        public float ShowHintDelay;
        public float EnvironmentSceneWidth;
    }

    public struct GameStateChange : IComponentData
    {
    }

    public struct GameUiUpdate : IComponentData
    {
        public NativeArray<GameUiUpdateType> GameUiUpdateTypes;
    }

    public enum GameUiUpdateType
    {
        GameUIOnLoad,
        GameUI,
        GameUIUpdateRemainingMoves,
        GameUIUpdateObjectiveChange,
        GameUIUpdateSurvivalTimer,
        WorldMapUI,
    }

    public struct LevelPointObjective : IComponentData
    {
        public int ScoreObjective;
    }

    public struct LevelEggObjective : IComponentData
    {
        public int EggsInGridAtStart;
        public int EggsToSpawnOnEggCollected;
        public int CollectedEggs;
    }

    public struct LevelSurvival : IComponentData
    {
        public float MaxSurvivalTime;
        public float TimeDepleteRate;
        public float SurvivalTimer;
        public float StartTimeGainByMatch;
        public float EndTimeGainByMatch;
        public float DifficulyRampUpTime;
        public float TimeObjective;
    }
}