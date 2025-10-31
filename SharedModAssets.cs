using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Reflection;
using Terraria.Audio;
using Terraria.ModLoader;

namespace GuidaSharedCode {
    // 自定义属性
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetAttribute : Attribute {
        public string Path { get; }
        public AssetAttribute(string path) {
            Path = path;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SoundAssetAttribute : AssetAttribute {
        public float Volume { get; set; } = 1f;
        public float PitchVariance { get; set; } = 0f;
        public int MaxInstances { get; set; } = 1;

        public SoundAssetAttribute(string path) : base(path) { }
    }

    public class SharedModAssets : ModSystem {
        public static string AssetDir = $".../GuidaSharedCode";
        public static string EmptyTextureDir = $".../GuidaSharedCode/Texture/Empty";
        // 着色器
        [Asset("Shader/AfterImage")]
        public static Effect AfterImageShader;

        [Asset("Shader/Bloom")]
        public static Effect BloomShader;

        [Asset("Shader/PostScreenEffects")]
        public static Effect PostScreenEffects;

        [Asset("Shader/Empty")]
        public static Effect EmptyShader;

        [Asset("Shader/MotionBlur")]
        public static Effect MotionBlurShader;

        [Asset("Shader/TwistDone")]
        public static Effect TwistDoneShader;

        [Asset("Shader/TwistImage")]
        public static Effect TwistImageShader;

        [Asset("Texture/TwistCircle")] public static Texture2D twistCircleTexture;

        public override void Load() {
            EmptyTextureDir = $"{Mod.Name}/GuidaSharedCode/Empty";
            AssetDir = $"{Mod.Name}/GuidaSharedCode";
            LoadAssetsByAttributes(Mod.Name);
        }

        private static void LoadAssetsByAttributes(string modName) {

            var type = typeof(SharedModAssets);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields) {
                var assetAttr = field.GetCustomAttribute<AssetAttribute>();
                if (assetAttr != null && !(assetAttr is SoundAssetAttribute)) {
                    var fullPath = $"{AssetDir}/{assetAttr.Path}";

                    if (field.FieldType == typeof(Effect)) {
                        field.SetValue(null, LoadAsset<Effect>(fullPath));
                    } else if (field.FieldType == typeof(Texture2D)) {
                        field.SetValue(null, LoadAsset<Texture2D>(fullPath));
                    }
                }

                var soundAttr = field.GetCustomAttribute<SoundAssetAttribute>();
                if (soundAttr != null) {
                    var fullPath = $"{AssetDir}/Sound/{soundAttr.Path}";
                    var soundStyle = new SoundStyle(fullPath) {
                        Volume = soundAttr.Volume,
                        PitchVariance = soundAttr.PitchVariance,
                        MaxInstances = soundAttr.MaxInstances
                    };
                    field.SetValue(null, soundStyle);
                }
            }
        }

        private static T LoadAsset<T>(string path) where T : class {
            return ModContent.Request<T>(path, AssetRequestMode.ImmediateLoad).Value;
        }
    }
}
