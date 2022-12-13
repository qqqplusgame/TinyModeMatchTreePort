using Unity.Entities;
using UnityEngine;

namespace ProjectM.Authoring
{
    public class DestroyLaserAnimationAuthoring : MonoBehaviour
    {
        public float Duration;
    }
    
    public class DestroyLaserAnimationAuthoringBaker : Baker<DestroyLaserAnimationAuthoring>
    {
        public override void Bake(DestroyLaserAnimationAuthoring authoring)
        {
            AddComponent(new DestroyLaserAnimation
            {
                Duration = authoring.Duration,
            });
        }
    }
}