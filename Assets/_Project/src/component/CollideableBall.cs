using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Tiny2D
{
    //[GenerateAuthoringComponent]
    public struct CollideableBall : IComponentData
    {
        // Add fields to your component here. Remember that:
        //
        // * A component itself is for storing data and doesn't 'do' anything.
        //
        // * To act on the data, you will need a System.
        //
        // * Data in a component must be blittable, which means a component can
        //   only contain fields which are primitive types or other blittable
        //   structs; they cannot contain references to classes.
        //
        // * You should focus on the data structure that makes the most sense
        //   for runtime use here. Authoring Components will be used for 
        //   authoring the data in the Editor.
        //public Entity CollisionEntity { get; set; }
        public int BallType;
        public bool CollideWithSameBall;
        //public int[] lists;
    }

    public class CollideableBallAuthoring : MonoBehaviour
    {
        public int BallType;
        public bool CollideWithSameBall;
    }

    public class CollideableBallBaker : Baker<CollideableBallAuthoring>
    {
        public override void Bake(CollideableBallAuthoring authoring)
        {
            AddComponent(new CollideableBall
            {
                BallType = authoring.BallType,
                CollideWithSameBall = authoring.CollideWithSameBall
            } );
        }
    }
}