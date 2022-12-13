using Unity.Entities;
using UnityEngine;

namespace ProjectM.Authoring
{
    public class DestroyAfterDelayAuthoring : MonoBehaviour
    {
        public float Delay;
    }

    public class DestroyAfterDelayBaker : Baker<DestroyAfterDelayAuthoring>
    {
        public override void Bake(DestroyAfterDelayAuthoring authoring)
        {
            AddComponent(new DestroyAfterDelay
            {
                Delay = authoring.Delay
            });
        }
    }
}