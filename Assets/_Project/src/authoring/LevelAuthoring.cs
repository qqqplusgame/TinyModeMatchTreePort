using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]
    public class LevelAuthoring : MonoBehaviour
    {
        public int LevelID;
        public SkinTypes Skin;
        public int MaxMoveCount;
        public GemPowerUpTypes[] StartPowerUps;
        public GemTypes[] MissingGems;
        
       
    }
    
    public class LevelBaker : Baker<LevelAuthoring>
    {
        public override void Bake(LevelAuthoring authoring)
        {
            AddComponent(new Level
            {
                LevelID = authoring.LevelID,
                Skin = authoring.Skin,
                MaxMoveCount = authoring.MaxMoveCount,
                LevelConfig = CreateLevelConfigBlob(authoring.StartPowerUps, authoring.MissingGems, Allocator.Persistent)
            } );
        }
        
        public static BlobAssetReference<LevelConfig> CreateLevelConfigBlob(GemPowerUpTypes[] startPowerUps,
            GemTypes[] missingGems, Allocator allocator)
        {
            using (var blob = new BlobBuilder(Allocator.TempJob))
            {
                ref var level = ref blob.ConstructRoot<LevelConfig>();

                var array1 = blob.Allocate(ref level.StartPowerUps, startPowerUps.Length);
                for (int i = 0; i < startPowerUps.Length; i++)
                {
                    array1[i] = startPowerUps[i];
                }

                var array2 = blob.Allocate(ref level.MissingGems, missingGems.Length);
                for (int i = 0; i < missingGems.Length; i++)
                {
                    array2[i] = missingGems[i];
                }

                return blob.CreateBlobAssetReference<LevelConfig>(allocator);
            }
        }
    }

}