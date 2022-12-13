using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]


    public class LevelEggObjectiveAuthoring : MonoBehaviour
    {
        public int EggsInGridAtStart;
        public int EggsToSpawnOnEggCollected;
    }

    public class LevelEggObjectiveBaker : Baker<LevelEggObjectiveAuthoring>
    {
        public override void Bake(LevelEggObjectiveAuthoring authoring)
        {
            AddComponent(new LevelEggObjective
            {
                EggsInGridAtStart = authoring.EggsInGridAtStart,
                EggsToSpawnOnEggCollected = authoring.EggsToSpawnOnEggCollected,
                CollectedEggs = 0,
            });
        }
    }
}