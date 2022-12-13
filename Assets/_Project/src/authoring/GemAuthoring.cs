using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]


    public class GemAuthoring : MonoBehaviour
    {
        public bool IsPossibleMatch;
        public GameObject SpriteRendererHighlightGem;
        public GameObject RowPowerUpVisual;
        public GameObject ColumnPowerUpVisual;
        public GameObject DiagonalPowerUpVisual;
        public GameObject SquarePowerUpVisual;
        public GameObject SameColorPowerUpVisual;
        
    }

    public class GemBaker : Baker<GemAuthoring>
    {
        public override void Bake(GemAuthoring authoring)
        {
            AddComponent(new Gem
            {
                CellHashKey = -1,
                IsPossibleMatch = authoring.IsPossibleMatch,
                SpriteRendererHighlightGem = GetEntity(authoring.SpriteRendererHighlightGem),
                RowPowerUpVisual = GetEntity(authoring.RowPowerUpVisual),
                ColumnPowerUpVisual = GetEntity(authoring.ColumnPowerUpVisual),
                DiagonalPowerUpVisual = GetEntity(authoring.DiagonalPowerUpVisual),
                SquarePowerUpVisual = GetEntity(authoring.SquarePowerUpVisual),
                SameColorPowerUpVisual = GetEntity(authoring.SameColorPowerUpVisual),
                
            });
            AddComponent<Disabled>();
        }
    }
}