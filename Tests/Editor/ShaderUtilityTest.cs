using NUnit.Framework;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using UnityEditor.Polybrush;
using UnityEditor;

namespace UnityEngine.Polybrush.EditorTests
{
    /// <summary>
    /// Will test all shader utility functions
    /// won't test:
    /// </summary>
    public class ShaderUtilityTest
    {
        private static Texture GetTextureFromPackagesByName(string name)
        {
            string[] allGUIDTextures = AssetDatabase.FindAssets(name + " t: Texture", new string[1] { "Packages" });
            
            if (allGUIDTextures.Length > 0)
            {
                return (Texture)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allGUIDTextures[0]), typeof(Texture));
            }
            return null;
        }

        private static void TestMeshAttributeContainer(AttributeLayoutContainer meshAttributes, Material mat)
        {
            Assert.IsNotNull(meshAttributes);
            Assert.IsTrue(meshAttributes.shader == mat.shader);
            //test textures preview and default values
            for (int i = 0; i < meshAttributes.attributes.Length; i++)
            {
                AttributeLayout attributeLayout = meshAttributes.attributes[i];

                //preview
                Texture attributeTexture = mat.GetTexture(attributeLayout.propertyTarget);
                Assert.IsTrue(attributeTexture == attributeLayout.previewTexture);
                //default
                Assert.IsTrue(Enum.IsDefined(typeof(MeshChannel), attributeLayout.channel));
                Assert.IsTrue(attributeLayout.min == 0);
                Assert.IsTrue(attributeLayout.max == 1);
                //if the index is part of the enum ComponentIndex
                Assert.IsTrue(Enum.IsDefined(typeof(ComponentIndex), attributeLayout.index));
            }
        }

        private Material CreateMeshAttributeMat()
        {
            //create a new material, using a polybrush shader
            Material mat = new Material(Shader.Find("Polybrush/Standard Texture Blend"));
            //fill it with polybrush example textures
            Texture[] allTextures = new Texture[4] { GetTextureFromPackagesByName("Dirt"), GetTextureFromPackagesByName("Grass"), GetTextureFromPackagesByName("Ground"), GetTextureFromPackagesByName("Sand") };

            //Dirt Grass Ground Sand
            string[] allTexturesNames = mat.GetTexturePropertyNames();
            allTexturesNames = allTexturesNames.OrderBy(PadNumbers).ToArray();
            for (int index = 0; index < Mathf.Min(allTexturesNames.Length, allTextures.Length); index++)
            {
                mat.SetTexture(allTexturesNames[index], allTextures[index]);
            }
            //ensure that the json metadata exists
            PolyEditorUtilityTest.CreateShaderMetadataTest(mat.shader.name);
            return mat;
        }

        private object PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }

        private static void CheckChannels(MeshChannel[] channels)
        {
            //the function should returns 3 channels > Color/UV3/UV4
            Assert.IsNotNull(channels);
            Assert.IsTrue(channels.Length == 3);
            Assert.IsTrue(channels[0] == MeshChannel.Color);
            Assert.IsTrue(channels[1] == MeshChannel.UV3);
            Assert.IsTrue(channels[2] == MeshChannel.UV4);
        }
    }
}
