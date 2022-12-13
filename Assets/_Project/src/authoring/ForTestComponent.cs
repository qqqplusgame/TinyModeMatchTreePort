using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace ProjectM.Authoring
{
    public struct ForTest : IComponentData
    {
    }
    // [ConverterVersion("Tiny2D", 1)]
    public class ForTestComponent : MonoBehaviour
    {
    }
    public class MyBaker : Baker<ForTestComponent>
    {
        public override void Bake(ForTestComponent authoring)
        {
            AddComponent(new ForTest() );
        }
    }
    
    
    
}