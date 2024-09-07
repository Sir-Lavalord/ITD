using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class TheUnburied : ModProjectile
    {
        public int Variant;
		public NPC Target;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 15;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 150;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Variant);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Variant = reader.ReadInt32();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.localAI[0] == 0f)
            {
                Variant = Main.rand.Next(3);
                Projectile.netUpdate = true;
                Projectile.localAI[0] += 1f;
            }
			
			if (Math.Abs(Projectile.velocity.X) > 1f)
			{
				Projectile.frameCounter++;
				if (Projectile.frameCounter > 1)
				{
					Projectile.frame++;
					Projectile.frameCounter = 0;
				}
				if (Projectile.frame < 1)
				{
					Projectile.frame = 1;
				}
				if (Projectile.frame >= Main.projFrames[Projectile.type])
				{
					Projectile.frame = 1;
				}
			}
			else
			{
				Projectile.frame = 6;
			}

            Target = Projectile.FindClosestNPC(900f);
            if (Target == null)
            {
                Projectile.velocity.X *= 0.9f;
            }
            else
            {
				if (Projectile.velocity.Y == 0 && Projectile.Center.Y > Target.Bottom.Y)
				{
					Projectile.velocity.Y = -10f;
				}

                Projectile.velocity.X += (Target.Center.X - Projectile.Center.X > 0f).ToDirectionInt() * 0.4f;
                Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -6f, 6f);
				NPCHelpers.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height);
            }
			
			if (Projectile.velocity.Y != 0f)
			{
				Projectile.frame = 0;
			}
			if (Projectile.velocity.Y > -16f)
            {
                Projectile.velocity.Y += 0.3f;
            }
			
            if (Projectile.velocity.X > 0.25f)
                Projectile.spriteDirection = 1;
            else if (Projectile.velocity.X < -0.25f)
                Projectile.spriteDirection = -1;
        }
		
		public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 20; ++j)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 26, 0, 0, 0, default, 1f);
			}
        }
		
        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle frame = new Rectangle(Variant * Projectile.width, Projectile.frame * Projectile.height, Projectile.width, Projectile.height);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, Projectile.Size / 2f, 1f, spriteEffects, 0);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
			if (Target != null)
				fallThrough = Projectile.Bottom.Y < Target.Top.Y;
			else
				fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;
    }
}
