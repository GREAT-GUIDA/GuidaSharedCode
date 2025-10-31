
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace GuidaSharedCode {


    public static class GuidaUtils {
        public static void DrawBorderStringEightWay(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 baseDrawPosition, Color main, Color border, float scale = 1f) {
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    Vector2 drawPosition = baseDrawPosition + new Vector2(x, y);
                    if (x == 0 && y == 0)
                        continue;
                    DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, drawPosition, border, 0f, default, scale, SpriteEffects.None, 0f);
                }
            }
            DynamicSpriteFontExtensionMethods.DrawString(sb, font, text, baseDrawPosition, main, 0f, default, scale, SpriteEffects.None, 0f);
        }
        public static float Smoothstep(float t1, float t2, float x) {
            x = MathHelper.Clamp((x - t1) / (t2 - t1), 0, 1);
            return x * x * (3 - 2 * x);
        }
        public static float Cross(this Vector2 vec, Vector2 vec2) {
            return vec.X * vec2.Y - vec.Y * vec2.X;
        }

        public static float PackVec2(Vector2 value, float min = -1000, float max = 1000) {
            float range = max - min;
            if (range <= 0) {
                return 0;
            }

            Vector2 clampedValue = Vector2.Clamp(value, new Vector2(min, min), new Vector2(max, max));

            float normalizedX = (clampedValue.X - min) / range;
            float normalizedY = (clampedValue.Y - min) / range;

            uint shortX = (uint)(normalizedX * 65535.0f);
            uint shortY = (uint)(normalizedY * 65535.0f);

            uint packedUInt = (shortY << 16) | shortX;

            return BitConverter.ToSingle(BitConverter.GetBytes(packedUInt), 0);
        }

        public static Vector2 UnpackVec2(float packed, float min = -1000, float max = 1000) {
            uint packedUInt = BitConverter.ToUInt32(BitConverter.GetBytes(packed), 0);

            uint shortX = packedUInt & 0xFFFF;
            uint shortY = packedUInt >> 16;

            float normalizedX = shortX / 65535.0f;
            float normalizedY = shortY / 65535.0f;

            float range = max - min;
            float x = normalizedX * range + min;
            float y = normalizedY * range + min;

            return new Vector2(x, y);
        }

        public static void NewScreenTarget() {
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            int width = Main.screenTarget.Width;
            int height = Main.screenTarget.Height;

            Main.screenTarget.Dispose();
            Main.screenTarget = new RenderTarget2D(device, width, height, false,
                device.PresentationParameters.BackBufferFormat,
                DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }
    }



    public static class SpriteBatchUtils {
        public static BlendState originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
        public static SamplerState originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
        public static DepthStencilState originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
        public static RasterizerState originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
        public static void SaveGraphicsDeviceParameters(this SpriteBatch spriteBatch) {
            originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
            originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
            originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
            originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
        }
        public static void EndAndBeginShader(this SpriteBatch spriteBatch, Effect shader, BlendState bs = null) {
            spriteBatch.End();
            if (bs == null) {
                bs = BlendState.NonPremultiplied;
            }
            spriteBatch.Begin(default, bs, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBeginShaderAdd(this SpriteBatch spriteBatch, Effect shader) {
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBeginAlpha(this SpriteBatch spriteBatch) {
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.NonPremultiplied, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBeginAdd(this SpriteBatch spriteBatch) {
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBeginDefault(this SpriteBatch spriteBatch) {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBeginBs(this SpriteBatch spriteBatch, BlendState bs) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss, Effect shader, Matrix mr) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, shader, mr);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, SamplerState ss, Effect shader) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, ss, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs, Effect shader) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, SamplerState.PointClamp, default, Main.Rasterizer, shader, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, SpriteSortMode ssm, BlendState bs) {
            spriteBatch.End();
            spriteBatch.Begin(ssm, bs, SamplerState.PointClamp, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public static void EndAndBegin(this SpriteBatch spriteBatch, BlendState bs) {
            spriteBatch.End();
            spriteBatch.Begin(default, bs, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    public static class Easing {
        public static float Linear(float t) => t;

        public static float QuadIn(float t) => t * t;

        public static float QuadOut(float t) => 1f - (1f - t) * (1f - t);

        public static float QuadInOut(float t) =>
            t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);

        public static float CubicIn(float t) => t * t * t;

        public static float CubicOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t);

        public static float CubicInOut(float t) =>
            t < 0.5f ? 4f * t * t * t : 1f - 4f * (1f - t) * (1f - t) * (1f - t);

        public static float QuartIn(float t) => t * t * t * t;

        public static float QuartOut(float t) => 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t);

        public static float QuartInOut(float t) =>
            t < 0.5f ? 8f * t * t * t * t : 1f - 8f * (1f - t) * (1f - t) * (1f - t) * (1f - t);

        public static float SineIn(float t) => 1f - (float)Math.Cos(t * Math.PI * 0.5);

        public static float SineOut(float t) => (float)Math.Sin(t * Math.PI * 0.5);

        public static float SineInOut(float t) => 0.5f * (1f - (float)Math.Cos(t * Math.PI));

        public static float ExpoIn(float t) => t == 0f ? 0f : (float)Math.Pow(2, 10 * (t - 1));

        public static float ExpoOut(float t) => t == 1f ? 1f : 1f - (float)Math.Pow(2, -10 * t);

        public static float ExpoInOut(float t) {
            if (t == 0f) return 0f;
            if (t == 1f) return 1f;
            return t < 0.5f ?
                0.5f * (float)Math.Pow(2, 20 * t - 10) :
                0.5f * (2f - (float)Math.Pow(2, -20 * t + 10));
        }

        public static float CircIn(float t) => 1f - (float)Math.Sqrt(1 - t * t);

        public static float CircOut(float t) => (float)Math.Sqrt(1 - (t - 1) * (t - 1));

        public static float CircInOut(float t) =>
            t < 0.5f ?
                0.5f * (1f - (float)Math.Sqrt(1 - 4 * t * t)) :
                0.5f * ((float)Math.Sqrt(1 - 4 * (t - 1) * (t - 1)) + 1);

        public static float BackIn(float t, float s = 1.70158f) => t * t * ((s + 1) * t - s);

        public static float BackOut(float t, float s = 1.70158f) =>
            1f + (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s);

        public static float BackInOut(float t, float s = 1.70158f) {
            s *= 1.525f;
            return t < 0.5f ?
                2f * t * t * ((s + 1) * 2f * t - s) :
                1f + 2f * (t - 1) * (t - 1) * ((s + 1) * 2f * (t - 1) + s);
        }

        public static float ElasticIn(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return -(amplitude * (float)Math.Pow(2, 10 * (t - 1)) *
                     (float)Math.Sin((t - 1 - s) * (2 * Math.PI) / period));
        }

        public static float ElasticOut(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return amplitude * (float)Math.Pow(2, -10 * t) *
                   (float)Math.Sin((t - s) * (2 * Math.PI) / period) + 1f;
        }

        public static float ElasticInOut(float t, float amplitude = 1f, float period = 0.3f) {
            if (t == 0f || t == 1f) return t;
            float s = period / 4f;
            return t < 0.5f ?
                -0.5f * amplitude * (float)Math.Pow(2, 20 * t - 10) *
                (float)Math.Sin((2 * t - 1 - s) * Math.PI / period) :
                0.5f * amplitude * (float)Math.Pow(2, -20 * t + 10) *
                (float)Math.Sin((2 * t - 1 - s) * Math.PI / period) + 1f;
        }

        public static float BounceOut(float t) {
            if (t < 1f / 2.75f) {
                return 7.5625f * t * t;
            } else if (t < 2f / 2.75f) {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            } else if (t < 2.5f / 2.75f) {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            } else {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }

        public static float BounceIn(float t) => 1f - BounceOut(1f - t);

        public static float BounceInOut(float t) =>
            t < 0.5f ? 0.5f * BounceIn(t * 2f) : 0.5f * BounceOut(t * 2f - 1f) + 0.5f;

        public static float SmoothStep(float t) => t * t * (3f - 2f * t);

        public static float SmootherStep(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        public static float Pulse(float t, float center = 0.5f, float width = 0.5f) {
            float distance = Math.Abs(t - center);
            return distance < width ? 1f - distance / width : 0f;
        }

        public static float Spike(float t) => t <= 0.5f ? 2f * t : 2f * (1f - t);

        public static float Wave(float t, float frequency = 1f) =>
            0.5f + 0.5f * (float)Math.Sin(t * frequency * 2f * Math.PI);

        public static float Sawtooth(float t, float frequency = 1f) =>
            2f * (t * frequency - (float)Math.Floor(t * frequency + 0.5f));

        public static float Square(float t, float frequency = 1f, float dutyCycle = 0.5f) =>
            t * frequency % 1f < dutyCycle ? 1f : 0f;
    }


    public struct VertexPositionColorTexture : IVertexType {
        public Vector2 Position;
        public Vector3 TexCoord;
        public Color Color;
        public VertexPositionColorTexture(Vector2 position, Vector3 texCoord, Color color) {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }
    public struct VertexPositionColor : IVertexType {
        public Vector2 Position;
        public Color Color;
        public VertexPositionColor(Vector2 position, Color color) {
            Position = position;
            Color = color;
        }
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }

}