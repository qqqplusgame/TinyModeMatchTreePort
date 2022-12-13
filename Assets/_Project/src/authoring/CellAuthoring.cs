using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;

namespace ProjectM.Authoring
{
    // [ConverterVersion("Tiny2D", 1)]

    public class CellAuthoring : MonoBehaviour
    {
        public int X;
        public int Y;
        public int Size;
        
    }
    
    public class CellBaker : Baker<CellAuthoring>
    {
        public override void Bake(CellAuthoring authoring)
        {
            AddComponent(new Cell
            {
                X = authoring.X,
                Y = authoring.Y,
                Size = authoring.Size
            } );
        }
    }


    
}