using Unity.Entities;
using UnityEngine;

namespace ProjectM.Authoring
{
    public class DestroyLineAnimationAuthoring : MonoBehaviour
    {
        public float Duration;
        public float ScaleDuration;
        public Gradient ColorGradient;
    }

    public class DestroyLineAnimationAuthoringBaker : Baker<DestroyLineAnimationAuthoring>
    {
        public override void Bake(DestroyLineAnimationAuthoring authoring)
        {
            AddComponent(new DestroyLineAnimation
            {
                Duration = authoring.Duration,
                ScaleDuration = authoring.ScaleDuration,
            });
            AddComponentObject(new ColorGradient()
            {
                gradient = authoring.ColorGradient
            });
        }
    }
}