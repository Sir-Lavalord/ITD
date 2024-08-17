using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Other
{
    public class CicadianTreeEnd : ModProjectile
    {
        private static readonly int lifetime = 80;
        public override string Texture => "ITD/Content/NPCs/BlueshroomGroves/CicadianTree";
        public override string GlowTexture => null;
        public override void SetDefaults()
        {
            Projectile.height = 12;
            Projectile.width = 12;
            Projectile.tileCollide = false;
            Projectile.timeLeft = lifetime;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
            DrawOriginOffsetY = -81;
            DrawOffsetX = -34;
        }
        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X < 0f ? -0.1f : 0.1f;
            Projectile.velocity.Y += 0.1f;
            Projectile.alpha = (int)((1f - (float)Projectile.timeLeft / (float)lifetime)*255f);
        }
    }
}
