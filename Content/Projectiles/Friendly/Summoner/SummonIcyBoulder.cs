using Terraria.Audio;
using Terraria.GameContent;
using Terraria.DataStructures;
using System;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class SummonIcyBoulder : ModProjectile
    {
        float progress = 0f;
        NPC travelTarget = null;
        Vector2 start = Vector2.Zero;
        private static readonly int duration = 42;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.timeLeft = 42;
        }
		
		public override void OnSpawn(IEntitySource source)
        {
            start = Projectile.Center;
            travelTarget = Main.npc[(int)Projectile.ai[0]];
        }
		public override void AI()
        {
            Projectile.rotation += Math.Sign(travelTarget.position.X - Projectile.position.X) / 4f;
            progress = 1f - (Projectile.timeLeft / (float)duration);
            Projectile.Center = Vector2.Lerp(start, travelTarget.Center, progress) - new Vector2(0f, (float)Math.Sin(progress * Math.PI)*128f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item50, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item51, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow);
                d.scale *= 1.5f;
				d.noGravity = true;
            }
        }
		
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = texture.Size()/2f;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin;
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }
    }
}
