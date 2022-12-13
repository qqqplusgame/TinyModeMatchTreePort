using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;


namespace ProjectM.Authoring
{
    public class ForTest2DAni : IComponentData
    {
        public Animator animator;
    }

    public class ForTest2DAniAuthoring : MonoBehaviour
    {
        public Animator animator;
    }

    public class ForTest2DBaker : Baker<ForTest2DAniAuthoring>
    {
        public override void Bake(ForTest2DAniAuthoring authoring)
        {
            AddComponentObject(new ForTest2DAni
            {
                animator = authoring.animator
            });
        }
    }

   
}