using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]
    

    public class LevelPointObjectiveAuthoring : MonoBehaviour
    {
        public int ScoreObjective;
        
    }

    public class LevelPointObjectiveBaker : Baker<LevelPointObjectiveAuthoring>
    {
        public override void Bake(LevelPointObjectiveAuthoring authoring)
        {
            AddComponent(new LevelPointObjective
            {
                ScoreObjective = authoring.ScoreObjective
            });
        }
    }
}