using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Projectiles.Hostile.Gravekeeper
{
    public class NecroSkull : ModProjectile
    {
		public override void SetStaticDefaults()
        {
             Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.penetrate = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
        }
		
		public override Color? GetAlpha(Color drawColor)
        {
            return Color.White;
        }
		
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<NecrosisBuff>(), 300, false);
        }
		
		public override void AI()
        {
			int target = (int)Player.FindClosest(Projectile.Center, 1, 1);
			Projectile.ai[1] += 1f;
			if (Projectile.ai[1] < 110f)
			{
				float scaleFactor = Projectile.velocity.Length();
				Vector2 distance = Main.player[target].Center - Projectile.Center;
				distance.Normalize();
				distance *= scaleFactor;
				Projectile.velocity = (Projectile.velocity * 19f + distance) / 20f;
				Projectile.velocity.Normalize();
				Projectile.velocity *= scaleFactor;
			}
			if (Projectile.velocity.Length() < 18f)
			{
				Projectile.velocity *= 1.02f;
			}
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[0] = 1f;
				SoundEngine.PlaySound(SoundID.Item20, Projectile.position);
				for (int i = 0; i < 10; i++)
				{
					int spawnDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GiantCursedSkullBolt, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 2f);
					Main.dust[spawnDust].noGravity = true;
					Main.dust[spawnDust].velocity = Projectile.Center - Main.dust[spawnDust].position;
					Main.dust[spawnDust].velocity.Normalize();
					Main.dust[spawnDust].velocity *= -5f;
					Main.dust[spawnDust].velocity += Projectile.velocity / 2f;
				}
			}
			
			Projectile.spriteDirection = (Projectile.velocity.X > 0).ToDirectionInt();
			
			if (Projectile.spriteDirection == 1)
				Projectile.rotation = Projectile.velocity.ToRotation();
			else
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2*2;

			if (++Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
				
				int trailDust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GiantCursedSkullBolt, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 1.5f);
				Main.dust[trailDust].noGravity = true;
				Main.dust[trailDust].velocity *= 0f;
            }
        }
		
		public override void OnKill(int timeLeft)
        {
			for (int i = 1; i < 10; i += 1)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.GiantCursedSkullBolt, new Vector2?(Main.rand.NextVector2Circular(3f, 3f)), 0, default(Color), 2f);
				dust.velocity.Y *= 0.2f;
				dust.velocity += Projectile.velocity * 0.2f;
			}
        }
    }
}
