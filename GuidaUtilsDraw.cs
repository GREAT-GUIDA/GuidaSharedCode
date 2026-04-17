using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GuidaSharedCode;
public static class SpriteBatchUtils {
    //public static BlendState originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
    //public static SamplerState originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
    //public static DepthStencilState originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
    //public static RasterizerState originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
    /*public static void SaveGraphicsDeviceParameters(this SpriteBatch spriteBatch) {
        originalBlendState = Main.spriteBatch.GraphicsDevice.BlendState;
        originalSamplerState = Main.spriteBatch.GraphicsDevice.SamplerStates[0];
        originalDepthStencilState = Main.spriteBatch.GraphicsDevice.DepthStencilState;
        originalRasterizerState = Main.spriteBatch.GraphicsDevice.RasterizerState;
    }*/
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

public static class ShaderUtils {
    public static void Apply(this Effect shader, Action<Effect> setupParams = null) {
        setupParams?.Invoke(shader);
        shader.CurrentTechnique.Passes[0].Apply();
    }
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


public static class ColorUtils {
    /// <param name="hOffset">色相偏移 (0-1)</param>
    /// <param name="sOffset">饱和度偏移 (-1 到 1)</param>
    /// <param name="lOffset">亮度偏移 (-1 到 1)</param>
    public static Color OffsetHSL(this Color color, float hOffset, float sOffset, float lOffset) {
        // 1. 转为 Vector3 (RGB 0-1)
        Vector3 rgb = color.ToVector3();

        // 2. RGB 转 HSL
        Vector3 hsl = RgbToHsl(rgb);

        // 3. 应用偏移
        hsl.X = (hsl.X + hOffset) % 1f; // 色相是环形的
        if (hsl.X < 0) hsl.X += 1f;

        hsl.Y = MathHelper.Clamp(hsl.Y + sOffset, 0f, 1f);
        hsl.Z = MathHelper.Clamp(hsl.Z + lOffset, 0f, 1f);

        // 4. 转回 RGB 并保持原有的 Alpha
        Vector3 finalRgb = HslToRgb(hsl);
        return new Color(finalRgb.X, finalRgb.Y, finalRgb.Z) * (color.A / 255f);
    }

    // 辅助计算：RGB -> HSL
    private static Vector3 RgbToHsl(Vector3 rgb) {
        float max = Math.Max(rgb.X, Math.Max(rgb.Y, rgb.Z));
        float min = Math.Min(rgb.X, Math.Min(rgb.Y, rgb.Z));
        float h, s, l = (max + min) / 2f;

        if (max == min) {
            h = s = 0f; // 灰色
        } else {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);
            if (max == rgb.X) h = (rgb.Y - rgb.Z) / d + (rgb.Y < rgb.Z ? 6f : 0f);
            else if (max == rgb.Y) h = (rgb.Z - rgb.X) / d + 2f;
            else h = (rgb.X - rgb.Y) / d + 4f;
            h /= 6f;
        }
        return new Vector3(h, s, l);
    }

    // 辅助计算：HSL -> RGB
    private static Vector3 HslToRgb(Vector3 hsl) {
        float h = hsl.X, s = hsl.Y, l = hsl.Z;
        if (s == 0) return new Vector3(l, l, l);

        float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
        float p = 2f * l - q;

        return new Vector3(
            HueToRgb(p, q, h + 1f / 3f),
            HueToRgb(p, q, h),
            HueToRgb(p, q, h - 1f / 3f)
        );
    }

    private static float HueToRgb(float p, float q, float t) {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}