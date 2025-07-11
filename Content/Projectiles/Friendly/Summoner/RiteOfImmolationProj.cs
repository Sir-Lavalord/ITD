using System;
using Terraria.GameContent;

using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class RiteOfImmolationProj : ModProjectile
    {
		public override string Texture => ITD.BlankTexture;
        private static int duration = 30;
		
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 72;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = duration;
            Projectile.penetrate = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
			Projectile.manualDirectionChange = true;
        }
		
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(ModContent.BuffType<RiteOfImmolationTagDebuff>(), 300);
			Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
        }
		
		public override void AI()
        {
			if (Projectile.timeLeft > duration / 2)
			{
				Vector2 velocity = new Vector2(8f, 0f).RotatedBy(Projectile.rotation + MathHelper.PiOver2) * Main.rand.NextFloat(-1, 1) * Projectile.scale;
				Dust dust = Dust.NewDustPerfect(Projectile.Center + velocity, 6, velocity, 100, default, 2.5f);
				dust.noGravity = true;
			}
		}
		
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            float progressZeroToOne = 1f-(Projectile.timeLeft / (float)duration);

            float scaleX = 1f-progressZeroToOne;
            float scaleY = (float)Math.Sin(progressZeroToOne * Math.PI)*4f;
			scaleX *= Projectile.scale;
			scaleY *= Projectile.scale;
			
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Texture2D tex = TextureAssets.Extra[98].Value;

            Main.EntitySpriteDraw(tex, drawPosition, null, Color.Lerp(new Color(255, 50, 0, 127), new Color(150, 0, 0, 127), progressZeroToOne), Projectile.rotation, tex.Size()/2f, new Vector2(scaleX, scaleY)*1.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, drawPosition, null, Color.Lerp(new Color(255, 200, 0, 127), new Color(255, 50, 0, 127), progressZeroToOne), Projectile.rotation, tex.Size()/2f, new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
            return false;
        }
    }
}
