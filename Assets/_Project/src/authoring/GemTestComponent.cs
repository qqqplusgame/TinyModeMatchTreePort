using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

namespace ProjectM.Authoring
{
    public struct GemTest : IComponentData
    {
        public Entity prefab;
        public int Dimension;
    }

    public class AssetTest : IComponentData
    {
        public List<SpriteAtlas> spriteAtlas;
    }

    public class GemTestComponent : MonoBehaviour
    {
        public GameObject GemTestPrefab;
        public SpriteAtlas[] AtlasRefList;
        public int GridCellDimension;
    }

    public class GemTestBaker : Baker<GemTestComponent>
    {
        public override void Bake(GemTestComponent authoring)
        {
            var gemTest = new GemTest
            {
                prefab = GetEntity(authoring.GemTestPrefab),
                Dimension = authoring.GridCellDimension
            };
            AddComponent(gemTest);
            var assetTest = new AssetTest
            {
                spriteAtlas = new List<SpriteAtlas>(authoring.AtlasRefList)
            };
            AddComponentObject(assetTest);
        }
    }

    public partial class GemTestSystem : SystemBase
    {
        public enum GemName
        {
            Gem_Red_Plain,
            Gem_Purple_Plain,
            Gem_Blue_Plain,
            Gem_Green_Plain,
            Gem_Silver_Plain,
            Gem_Yellow_Plain,
            Gem_Egg_Plain,
        }
        protected override void OnUpdate()
        {
            var offsetX = 20;
            var offsetY = 20;
            var em = EntityManager;
            string[] gemNames = System.Enum.GetNames(typeof(GemName));
            Entities.ForEach((Entity entity, in GemTest gemTest, in AssetTest assetTest) =>
            {
                Debug.Log(
                    $"Entity: {entity} GemTest: {gemTest.prefab} {gemTest.Dimension} AssetTest: {assetTest.spriteAtlas.Count}");
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var e = em.Instantiate(gemTest.prefab);
                        
                        em.SetComponentData(e,LocalTransform.FromPosition(new float3(offsetX + i * gemTest.Dimension, offsetY + j * gemTest.Dimension, 0)));
                        // var ta = em.GetAspect<TransformAspect>(e);
                        //
                        // ta.LocalPosition = new Vector3(offsetX + i * gemTest.Dimension, offsetY + j * gemTest.Dimension, 0);

                        var sr = em.GetComponentObject<SpriteRenderer>(e);
                        var spriteName = gemNames[Random.Range(0, gemNames.Length)];
                        var sp = assetTest.spriteAtlas[0].GetSprite(spriteName);
                        if(sp != null)
                            sr.sprite = sp;
                        else
                            Debug.LogError($"Sprite {spriteName} not found");
                        
                    }
                }

                //run one time
                Enabled = false;
            }).WithoutBurst().WithStructuralChanges().Run();
        }
    }
}