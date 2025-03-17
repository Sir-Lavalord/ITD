using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.Audio;
using System;

using ITD.Players;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class ThrowableGuardian : ModProjectile
    {
		public override string Texture => "Terraria/Images/NPC_68";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 800;
        }
		
		public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(24, 24);
        }
		
        public override void OnKill(int timeLeft)
        {
			Main.player[Main.myPlayer].GetITDPlayer().BetterScreenshake(20, 4, 4, false);
			SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f)), 54);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f)), 55);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;
        }
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.SourceDamage.Base += target.lifeMax/20;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D projectileTexture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 trailPos = Projectile.oldPos[k] - Main.screenPosition + (Projectile.Size * 0.5f) + new Vector2(0f, Projectile.gfxOffY);
				Color color = lightColor * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.5f;
				Main.spriteBatch.Draw(projectileTexture, trailPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, SpriteEffects.None, 0f);
			}
			Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			Main.spriteBatch.Draw(projectileTexture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
    }
}
