using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.Graphics.Capture;
using GuidaSharedCode;
using Terraria.ID;
using TombwardJourney.Content.FlyingTombstone;
using TombwardJourney;

namespace GuidaSharedCode {
    /// <summary>
    /// 简化的屏幕特效管理系统
    /// </summary>
    public class ScreenEffectSystem : ModSystem {
        // 渲染目标
        public static RenderTarget2D glowTarget;

        // 发光效果
        private static List<GlowInfo> glows = new List<GlowInfo>();
        public static Color GlowColor = Color.White;
        public static float GlowIntensity = 1f;


        // 注册的特效列表
        private static List<IScreenEffect> effects = new List<IScreenEffect>();
        private bool EffectsRegistered = false;

        public struct GlowInfo {
            public Vector2 Position;
            public Color Color;
            public float Scale;
            public float Intensity;

            public GlowInfo(Vector2 position, Color color, float scale = 1f, float intensity = 1f) {
                Position = position;
                Color = color;
                Scale = scale;
                Intensity = intensity;
            }
        }

        /// <summary>
        /// 简化的屏幕特效接口
        /// </summary>
        public interface IScreenEffect {
            string Name { get; }
            float Intensity { get; set; }
            bool IsActive { get; }
            float FadeSpeed { get; set; }

            float TargetIntensity();
            void OnActivate();
            void OnUpdate();
            void OnDeactivate();
            void Initialize();
            void Unload();
        }

        public override void PreUpdateEntities() {
            if (!Main.gameMenu) {
                if (glowTarget == null) {
                    RecreateRenderTargets(Vector2.Zero);
                }
                if (Main.GameUpdateCount != lastCheckFrame) {
                    GlowColor = Color.White;
                    glows.Clear();
                } else {
                }
                lastCheckFrame = Main.GameUpdateCount;
                UpdateAllEffects();
            }
        }

        // 添加状态追踪
        private static Dictionary<string, bool> previousActiveStates = new Dictionary<string, bool>();
        private uint lastCheckFrame = 0;

        private void UpdateAllEffects() {
            foreach (var effect in effects) {
                effect.OnUpdate();

                float targetIntensity = effect.TargetIntensity();

                bool wasActive = previousActiveStates.ContainsKey(effect.Name) ?
                               previousActiveStates[effect.Name] : false;

                float fadeSpeed = 0.05f;

                if (Math.Abs(effect.Intensity - targetIntensity) > 0.01f) {
                    if (effect.Intensity < targetIntensity) {
                        effect.Intensity = Math.Min(effect.Intensity + effect.FadeSpeed, targetIntensity);
                    } else {
                        effect.Intensity = Math.Max(effect.Intensity - fadeSpeed, targetIntensity);
                    }
                }
                bool isActive = effect.IsActive;

                if (isActive && !wasActive) {
                    effect.OnActivate();
                } else if (!isActive && wasActive) {
                    effect.OnDeactivate();
                }
                previousActiveStates[effect.Name] = isActive;
            }
        }

        /// <summary>
        /// 注册特效
        /// </summary>
        public static void RegisterEffect(IScreenEffect effect) {
            if (!effects.Contains(effect)) {
                effects.Add(effect);
                effect.Initialize();
            }
        }

        /// <summary>
        /// 获取特效实例
        /// </summary>
        public static T GetEffect<T>() where T : class, IScreenEffect {
            foreach (var effect in effects) {
                if (effect is T) return effect as T;
            }
            return null;
        }

        /// <summary>
        /// 检测特效是否开启
        /// </summary>
        public static bool IsEffectActive<T>() where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            return effect?.IsActive ?? false;
        }

        /// <summary>
        /// 获取特效强度
        /// </summary>
        public static float GetEffectIntensity<T>() where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            return effect?.Intensity ?? 0f;
        }

        /// <summary>
        /// 强制设置特效强度（用于特殊情况）
        /// </summary>
        public static void SetEffectIntensity<T>(float intensity) where T : class, IScreenEffect {
            var effect = GetEffect<T>();
            if (effect != null) {
                effect.Intensity = MathHelper.Clamp(intensity, 0f, 1f);
            }
        }

        /// <summary>
        /// 检测特定名称的特效是否开启
        /// </summary>
        public static bool IsEffectActive(string effectName) {
            foreach (var effect in effects) {
                if (effect.Name == effectName) {
                    return effect.IsActive;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取特定名称特效的强度
        /// </summary>
        public static float GetEffectIntensity(string effectName) {
            foreach (var effect in effects) {
                if (effect.Name == effectName) {
                    return effect.Intensity;
                }
            }
            return 0f;
        }

        public override void Load() {
            if (Main.dedServ) return;

            On_Main.InitTargets_int_int += On_Main_InitTargets_int_int;
            Main.OnResolutionChanged += RecreateRenderTargets;

            // Hook渲染
            Type filterManagerType = new FilterManager().GetType();
            MethodInfo detourMethod = filterManagerType.GetMethod("EndCapture", BindingFlags.Public | BindingFlags.Instance);
            if (detourMethod != null) {
                MonoModHooks.Add(detourMethod, On_Main_DrawDust);
            }
        }

        public override void Unload() {
            if (Main.dedServ) return;

            foreach (var effect in effects) {
                effect.Unload();
            }
            effects.Clear();
        }

        // 渲染相关方法保持不变
        private static void RecreateRenderTargets(Vector2 vector2) {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return;

            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;
            GraphicsDevice device = Main.graphics.GraphicsDevice;

            glowTarget?.Dispose();
            glowTarget = new RenderTarget2D(device, width / 2, height / 2, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private void On_Main_InitTargets_int_int(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height) {
            orig.Invoke(self, width, height);
            RecreateRenderTargets(Vector2.Zero);
            GuidaUtils.NewScreenTarget();
        }
        public delegate void orig_EndCapture(object self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);
        private void On_Main_DrawDust(orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture,
                                     RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor) {
            if (!CaptureManager.Instance.IsCapturing) {
                if (Main.screenTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents)
                    GuidaUtils.NewScreenTarget();

                if (glowTarget == null)
                    RecreateRenderTargets(Vector2.Zero);

                if (!GlowColor.Equals(Color.White))
                    DrawGlow(GlowColor, 2);
            }
            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }

        public static void DrawGlow(Color color, float sizeRate) {
            SpriteBatch spriteBatch = Main.spriteBatch;
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;

            device.SetRenderTarget(glowTarget);
            device.Clear(color);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var glow in glows) {
                Vector2 screenPos = (glow.Position - Main.screenPosition) / 2;
                Vector2 rate = new Vector2(width / Main.screenWidth, height / Main.screenHeight);
                float size = 128f * glow.Scale * sizeRate;
                float opacity = glow.Intensity * GlowIntensity;
                Vector2 origin = new Vector2(ModAssets.glowTexture.Width, ModAssets.glowTexture.Height) * 0.5f;

                spriteBatch.Draw(ModAssets.glowTexture, screenPos * rate, null,
                    glow.Color * opacity * 0.5f, 0f, origin,
                    rate * size / ModAssets.glowTexture.Width, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

            device.SetRenderTarget(Main.screenTarget);

            var blendState = new BlendState {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.SourceColor,
                ColorSourceBlend = Blend.Zero
            };

            spriteBatch.Begin(SpriteSortMode.Immediate, blendState);
            spriteBatch.Draw(glowTarget, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }

        public static void AddGlow(Vector2 position, Color color, float scale = 1f, float intensity = 0.5f) {
            glows.Add(new GlowInfo(position, color, scale, intensity));
        }
    }
}