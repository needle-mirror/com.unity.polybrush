using NUnit.Framework;
using System.Linq;
using System;
using UnityEditor.Polybrush;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

namespace UnityEngine.Polybrush.EditorTests
{
    /// <summary>
    /// Will test all shader utility functions
    /// </summary>
    public class ShaderMetaDataTests
    {
        const string k_PathToShaderWithMeta = "Hidden/Polybrush/Tests/ShaderWithMeta";
        const string k_PathToShaderWithNoMeta = "Hidden/Polybrush/Tests/ShaderWithNoMeta";
        const string k_InvalidPathToShader = "Hidden/Polybrush/Tests/InvalidName";

        string k_PathToShaderWithOldMetaFormat = "Hidden/Polybrush/Tests/ShaderWithOldMetaFormat";
        string k_FilePathToShaderWithOldMetaFormat = TestsUtility.testsRootDirectory + "/Resources/TestShaderWithOldMetaFormat.shader_";
        string k_FilePathToShaderWithOldMetaFormatPBS = TestsUtility.testsRootDirectory + "/Resources/TestShaderWithOldMetaFormat.pbs.json";
        string k_DestFilePathToShaderWithOldMetaFormat = Application.dataPath + "/Shader/TestShaderWithOldMetaFormat.shader";
        string k_DestFilePathToShaderWithOldMetaFormatPBS = Application.dataPath + "/Shader/TestShaderWithOldMetaFormat.pbs.json";

        string k_AssetShaderFolder = Application.dataPath + "/Shader";

        private static Texture GetTextureFromPackagesByName(string name)
        {
            string[] allGUIDTextures = AssetDatabase.FindAssets(name + " t: Texture", new string[1] { "Packages" });
            
            if (allGUIDTextures.Length > 0)
            {
                return (Texture)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allGUIDTextures[0]), typeof(Texture));
            }
            return null;
        }

#pragma warning disable 0618
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
#pragma warning restore 0618
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

        [Test]
        public void IsValidShader_ArgumentNull_ThrowException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ShaderMetaDataUtility.IsValidShader(null);
            });
        }

        [Test]
        public void IsValidShader_ArgumentShaderFromBuiltinResources_ReturnsFalse()
        {
            Shader s = Shader.Find("Standard");

            Assume.That(s, Is.Not.Null);

            Assert.IsFalse(ShaderMetaDataUtility.IsValidShader(s));
        }


        [Test]
        public void LoadShaderMetaData_ArgumentNull_ThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => ShaderMetaDataUtility.LoadShaderMetaData(null));
        }

        [Test]
        public void LoadShaderMetaData_ValidShaderWithMeta_ReturnsAttributes()
        {
            Shader shaderWithMeta = Shader.Find(k_PathToShaderWithMeta);
            Assume.That(shaderWithMeta != null);

            AttributeLayoutContainer attributes = ShaderMetaDataUtility.LoadShaderMetaData(shaderWithMeta);

            Assert.IsNotNull(attributes);

            Assert.IsNotNull(attributes.attributes);
            Assert.GreaterOrEqual(attributes.attributes.Length, 2);
        }

        [Test]
        public void LoadShaderMetaData_ValidShaderWithoutMeta_ReturnsAttributes()
        {
            Shader shaderWithMeta = Shader.Find(k_PathToShaderWithNoMeta);
            Assume.That(shaderWithMeta != null);

            AttributeLayoutContainer attributes = ShaderMetaDataUtility.LoadShaderMetaData(shaderWithMeta);

            Assert.IsNotNull(attributes);
            Assert.IsNull(attributes.attributes);
        }

        [Test]
        public void ConvertMetaDataToNewFormat_ArgumentValid_NoException()
        {
            if (!Directory.Exists(k_AssetShaderFolder))
            {
                Directory.CreateDirectory(k_AssetShaderFolder);
            }

            // Setup environment for this specific tests.
            FileUtil.CopyFileOrDirectory(k_FilePathToShaderWithOldMetaFormat, k_DestFilePathToShaderWithOldMetaFormat);
            FileUtil.CopyFileOrDirectory(k_FilePathToShaderWithOldMetaFormatPBS, k_DestFilePathToShaderWithOldMetaFormatPBS);
            AssetDatabase.Refresh();

            Shader shader = Shader.Find(k_PathToShaderWithOldMetaFormat);
            Assume.That(shader != null);

#pragma warning disable 0618
            // We should find a .pbs.json file for the shader.
            Assert.IsFalse(String.IsNullOrEmpty(ShaderMetaDataUtility.FindPolybrushMetaDataForShader(shader)));

            // Convert the data from .pbs.json to meta data.
            ShaderMetaDataUtility.ConvertMetaDataToNewFormat(shader);

            // After conversion, we shouldn't find a .pbs.json file for the shader.
            Assert.IsTrue(String.IsNullOrEmpty(ShaderMetaDataUtility.FindPolybrushMetaDataForShader(shader)));
#pragma warning restore 0618
            // Check tha
            AttributeLayoutContainer attributes = ShaderMetaDataUtility.LoadShaderMetaData(shader);
            Assert.IsNotNull(attributes);
            Assert.IsNotNull(attributes.attributes);

            // Clean up
            FileUtil.DeleteFileOrDirectory(k_DestFilePathToShaderWithOldMetaFormat);
            FileUtil.DeleteFileOrDirectory(k_DestFilePathToShaderWithOldMetaFormat + ".meta");
            FileUtil.DeleteFileOrDirectory(k_DestFilePathToShaderWithOldMetaFormatPBS);
            FileUtil.DeleteFileOrDirectory(k_DestFilePathToShaderWithOldMetaFormatPBS + ".meta");

            AssetDatabase.Refresh();
        }
    }
}
