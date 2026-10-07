using GuidaSharedCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuidaSharedCode {
    // Twist-layer particles draw displacement data into the shared screen map.
    // Early particles affect the scene behind NPCs; late particles affect the full scene.
    public interface ITwistParticle {
        bool DrawBehindNPCs { get; }
        float TwistOpacity { get; }
        void DrawTwist(SpriteBatch spriteBatch);
    }

    public enum TwistCircleStyle {
        Circle,
        Shockwave
    }

    public class TwistCircleParticle : Particle<TwistCircleParticle>, ITwistParticle {
        public TwistCircleStyle style = TwistCircleStyle.Circle;
        public float size = 1.4f;
        public float strength = 0.1f;
        public int time = 30;
        public int timer = 0;
        public float image_alpha = 0;
        public float image_scale = 1;
        public bool inverse = false;
        public bool DrawBehindNPCs => false;
        public float TwistOpacity => image_alpha;
        public override Texture2D Texture => style == TwistCircleStyle.Shockwave
            ? ModAsset.TexTwistShockwave.Value
            : ModAsset.TexTwistCircle.Value;

        public void DrawTwist(SpriteBatch spriteBatch) {
            Texture2D texture = Texture;
            spriteBatch.Draw(texture, position - Terraria.Main.screenPosition, null,
                Color.White * (image_alpha * 0.5f), 0f,
                new Vector2(texture.Width, texture.Height) * 0.5f, image_scale,
                SpriteEffects.None, 0f);
        }

        public new static void Spawn(Vector2 position, float size = 1.4f, int time = 30,
            float strength = 0.1f, bool inverse = false,
            TwistCircleStyle style = TwistCircleStyle.Circle) {
            TwistCircleParticle particle = Particle.Spawn<TwistCircleParticle>(position);
            particle.size = size;
            particle.time = time;
            particle.strength = strength;
            particle.inverse = inverse;
            particle.style = style;
        }

        public override void SetDefaults() {
            drawLayer = ParticleLayer.Twist;
            base.SetDefaults();
        }
        public override void AI() {
            timer += 1;
            float duration = time * 1.12f;
            float rate = MathHelper.Clamp(timer / duration, 0f, 1f);
            if (inverse) rate = 1f - rate;
            image_alpha = (1f - rate) * strength * 6.6f;
            image_scale = rate * size * 1.2f;

            if (timer >= duration) Kill();
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            return false;
        }
    }
}
