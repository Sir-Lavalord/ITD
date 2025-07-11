using System;

namespace ITD.Content.Gores
{
    public class ResonanceBarGore : ModGore
    {
        public override void SetStaticDefaults()
        {
            
        }
        public override bool Update(Gore gore)
        {
            gore.velocity.Y += 0.4f;
            Vector2 nextPosition = gore.position + gore.velocity;
            if (Collision.SolidCollision(nextPosition, (int)gore.Width, (int)gore.Height))
            {
                if (Math.Abs(gore.velocity.Y) > 0.1f)
                {
                    gore.velocity.Y *= -0.8f;
                    gore.velocity.X *= 0.95f;
                }
                else
                {
                    gore.velocity = Vector2.Zero;
                }
            }

            gore.rotation += gore.velocity.X * 0.1f;
            return true;
        }
    }
}