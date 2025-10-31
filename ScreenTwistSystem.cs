using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Capture;
using GuidaSharedCode;

namespace GuidaSharedCode {



    class ScreenTwistSystem : ModSystem{
        public static RenderTarget2D twistTarget;
        public static RenderTarget2D twistTarget2;

        public static float UBloomIntensity = 0f;
        public static float ULerpIntensity = 0f;
        public static Color ULerpColor = Color.White;
        public static float URadialBlurIntensity = 0f;
        public static Vector2 URadialBlurPosition = Vector2.One * 0.5f;
        private uint lastCheckFrame;

        public override void Load() {
            if (Main.dedServ) return;

            On_Main.InitTargets_int_int += On_Main_InitTargets_int_int;
            Main.OnResolutionChanged += RecreateRenderTargets;

            Type filterManagerType = new FilterManager().GetType();
            MethodInfo detourMethod = filterManagerType.GetMethod("EndCapture", BindingFlags.Public | BindingFlags.Instance);
            if (detourMethod != null) {
                MonoModHooks.Add(detourMethod, On_Main_EndCapture);
            }
        }

        private void On_Main_InitTargets_int_int(On_Main.orig_InitTargets_int_int orig, Main self, int width, int height) {
            orig.Invoke(self, width, height);
            RecreateRenderTargets(Vector2.Zero);
        }
        public delegate void orig_EndCapture(object self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor);
        private void On_Main_EndCapture(orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture,
                                     RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor) {
            if (!CaptureManager.Instance.IsCapturing) {
                DrawTwist();
            }
            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }
        public override void PreUpdateEntities() {
            if (!Main.gameMenu) {
                if (twistTarget == null || twistTarget2 == null) {
                    RecreateRenderTargets(Vector2.Zero);
                }
                if (Main.GameUpdateCount != lastCheckFrame) {
                    UBloomIntensity = 0f;
                    ULerpIntensity = 0f;
                    ULerpColor = Color.White;
                    URadialBlurIntensity = 0f;
                    URadialBlurPosition = Vector2.One * 0.5f;
                } else {
                }
                lastCheckFrame = Main.GameUpdateCount;
            }
        }
        private static void RecreateRenderTargets(Vector2 vector2) {
            if (Main.dedServ || Main.graphics?.GraphicsDevice == null) return;

            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;
            GraphicsDevice device = Main.graphics.GraphicsDevice;

            twistTarget?.Dispose();
            twistTarget = new RenderTarget2D(device, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            twistTarget2?.Dispose();
            twistTarget2 = new RenderTarget2D(device, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }
        public static void DrawTwist() {
            if (Main.gameMenu || twistTarget == null || twistTarget2 == null)
                return;

            if (Main.screenTarget.RenderTargetUsage != RenderTargetUsage.PreserveContents)
                GuidaUtils.NewScreenTarget();

            SpriteBatch spriteBatch = Main.spriteBatch;
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;

            device.SetRenderTarget(twistTarget);
            device.Clear(Color.Transparent);

            var blendState = new BlendState {
                AlphaBlendFunction = BlendFunction.Add,
                AlphaDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add,
                ColorDestinationBlend = Blend.One,
                ColorSourceBlend = Blend.One
            };

            spriteBatch.Begin(SpriteSortMode.Deferred, blendState, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, ModAssets.TwistImageShader,
                Main.GameViewMatrix.TransformationMatrix);

            ModAssets.TwistImageShader.CurrentTechnique.Passes["P0"].Apply();

            if (ParticleManager.Instance.particlesByLayer.TryGetValue(ParticleLayer.Twist, out List<Particle> particles)) {
                foreach (var particle in particles) {
                    var texture = ModAssets.TwistCircleTexture;
                    var twistCircle = particle as TwistCircleParticle;
                    float size = twistCircle.image_scale;
                    float opacity = twistCircle.image_alpha * 0.5f;
                    Vector2 origin = new Vector2(texture.Width, texture.Height) * 0.5f;

                    spriteBatch.Draw(texture, particle.position - Main.screenPosition, null,
                        Color.White * opacity, 0f, origin, size, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                ModAssets.PostScreenEffects, Matrix.Identity);

            Vector2 screenSize = new Vector2(
                device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight);

            ModAssets.PostScreenEffects.Parameters["uImageSize1"].SetValue(screenSize);
            ModAssets.PostScreenEffects.Parameters["uBloomIntensity"].SetValue(UBloomIntensity);
            ModAssets.PostScreenEffects.Parameters["uLerpIntensity"].SetValue(ULerpIntensity);
            ModAssets.PostScreenEffects.Parameters["uLerpColor"].SetValue(ULerpColor.ToVector3());
            ModAssets.PostScreenEffects.Parameters["uRadialBlurIntensity"].SetValue(URadialBlurIntensity);
            ModAssets.PostScreenEffects.Parameters["uRadialBlurPosition"].SetValue(URadialBlurPosition);
            ModAssets.PostScreenEffects.CurrentTechnique.Passes["P0"].Apply();

            device.SetRenderTargets(twistTarget2);
            spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            device.SetRenderTargets(Main.screenTarget);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                ModAssets.TwistDoneShader, Matrix.Identity);

            ModAssets.TwistDoneShader.Parameters["uScreenResolution"].SetValue(screenSize);
            ModAssets.TwistDoneShader.Parameters["uImage1"].SetValue(twistTarget2);
            ModAssets.TwistDoneShader.CurrentTechnique.Passes["P0"].Apply();

            spriteBatch.Draw(twistTarget, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }
    }
}
