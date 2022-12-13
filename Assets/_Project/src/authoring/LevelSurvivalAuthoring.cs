using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]


    public class LevelSurvivalAuthoring : MonoBehaviour
    {
        public float MaxSurvivalTime;
        public float TimeDepleteRate;
        public float StartTimeGainByMatch;
        public float EndTimeGainByMatch;
        public float DifficulyRampUpTime;
        public float TimeObjective;
        public Gradient color;
    }

    public class LevelSurvivalBaker : Baker<LevelSurvivalAuthoring>
    {
        public override void Bake(LevelSurvivalAuthoring authoring)
        {
            AddComponent(new LevelSurvival
            {
                MaxSurvivalTime = authoring.MaxSurvivalTime,
                TimeDepleteRate = authoring.TimeDepleteRate,
                SurvivalTimer = 0,
                StartTimeGainByMatch = authoring.StartTimeGainByMatch,
                EndTimeGainByMatch = authoring.EndTimeGainByMatch,
                DifficulyRampUpTime = authoring.DifficulyRampUpTime,
                TimeObjective = authoring.TimeObjective,
            });
        }
    }
}