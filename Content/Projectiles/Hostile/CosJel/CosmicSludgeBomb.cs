using System.Collections.Generic;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicSludgeBomb : ModProjectile
    {
        private readonly Asset<Texture2D> effect = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Hostile/CosJel/CosmicSludgeBomb_Effect");
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5; // The length of old position to be recorded
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0; // The recording mode
            Main.projFrames[Projectile.type] = 7;
        }

        int defaultWidthHeight = 8;
        public override void SetDefaults()
        {
            Projectile.width = defaultWidthHeight; Projectile.height = defaultWidthHeight;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            DrawOffsetX = -16;
            DrawOriginOffsetY = -16;
            Projectile.hide = true;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = effect.Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY + DrawOriginOffsetX) + new Vector2(DrawOffsetX, DrawOriginOffsetY) + new Vector2(4, 4);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
        bool expertMode = Main.expertMode;
        bool masterMode = Main.masterMode;
        public override void AI()
        {
            if (Projectile.ai[1]++ >= 30)
            {
                Projectile.velocity.Y += 0.25f;
            }
            if (++Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
            }
        }
    }
}
