using System;
using System.IO;
using Game.Core.Cards;
using Game.Data.Presentation;
using UnityEditor;
using UnityEngine;

namespace Game.Data.Editor
{
    /// <summary>
    /// Draws the placeholder icons (elements, rarity gem) and the card back as PNG files and puts them into the
    /// visual settings. Real art replaces the files or the references later; the code does not change.
    /// </summary>
    public static class PlaceholderArtMenu
    {
        private const string Folder = "Assets/Art/Icons";
        private const int IconSize = 256;
        private const int Samples = 4;

        [MenuItem("Tools/Game/Generate Placeholder Art")]
        public static void Generate()
        {
            Directory.CreateDirectory(Folder);
            var icons = new (Element element, string name, Func<float, float, float> shape)[]
            {
                (Element.Fire, "element-fire", PlaceholderShapes.Fire),
                (Element.Water, "element-water", PlaceholderShapes.Water),
                (Element.Earth, "element-earth", PlaceholderShapes.Earth),
                (Element.Wind, "element-wind", PlaceholderShapes.Wind),
                (Element.Light, "element-light", PlaceholderShapes.Light),
                (Element.Dark, "element-dark", PlaceholderShapes.Dark),
            };

            foreach (var icon in icons)
                Write(icon.name, IconSize, IconSize, icon.shape, transparent: true);
            Write("rarity-gem", IconSize, IconSize, PlaceholderShapes.Gem, transparent: true);
            Write("card-back", 358, 512, PlaceholderShapes.Back, transparent: false);
            AssetDatabase.Refresh();

            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(VisualSettings)))
            {
                var settings = AssetDatabase.LoadAssetAtPath<VisualSettings>(AssetDatabase.GUIDToAssetPath(guid));
                var data = new SerializedObject(settings);
                var elements = data.FindProperty("_elements");
                for (var i = 0; i < elements.arraySize; i++)
                {
                    var entry = elements.GetArrayElementAtIndex(i);
                    var element = (Element)entry.FindPropertyRelative("Element").enumValueIndex;
                    var name = Array.Find(icons, icon => icon.element == element).name;
                    entry.FindPropertyRelative("Icon").objectReferenceValue = Load(name);
                }

                var rarities = data.FindProperty("_rarities");
                for (var i = 0; i < rarities.arraySize; i++)
                    rarities.GetArrayElementAtIndex(i).FindPropertyRelative("Icon").objectReferenceValue = Load("rarity-gem");
                data.FindProperty("_cardBackArt").objectReferenceValue = Load("card-back");
                data.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Placeholder art written to {Folder} and set in the visual settings.");
        }

        private static void Write(string name, int width, int height, Func<float, float, float> shape, bool transparent)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            for (var py = 0; py < height; py++)
            {
                for (var px = 0; px < width; px++)
                {
                    var sum = 0f;
                    var covered = 0;
                    for (var sy = 0; sy < Samples; sy++)
                    {
                        for (var sx = 0; sx < Samples; sx++)
                        {
                            var value = shape((px + (sx + 0.5f) / Samples) / width, (py + (sy + 0.5f) / Samples) / height);
                            sum += value;
                            if (value > 0f)
                                covered++;
                        }
                    }

                    var coverage = (float)covered / (Samples * Samples);
                    var brightness = covered == 0 ? 1f : sum / covered;
                    var shade = (byte)Mathf.RoundToInt(brightness * 255f);
                    var alpha = transparent ? (byte)Mathf.RoundToInt(coverage * 255f) : (byte)255;
                    pixels[py * width + px] = new Color32(shade, shade, shade, alpha);
                }
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes($"{Folder}/{name}.png", texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);

            var path = $"{Folder}/{name}.png";
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = transparent;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 512;
            importer.SaveAndReimport();
        }

        private static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Folder}/{name}.png");
    }
}
