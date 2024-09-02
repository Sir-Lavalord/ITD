using Terraria;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class EmberSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 32; Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.ownerHitCheck = true;
        }
		
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 120, false);
        }
		
		public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate((int)(32*Projectile.ai[0]), (int)(32*Projectile.ai[0]));
        }
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return projHitbox.Intersects(targetHitbox) && Collision.CanHit(Projectile.Center, 0, 0, targetHitbox.Center.ToVector2(), 0, 0);
		}
		
        public override void AI()
        {
			Projectile.localAI[0] += 1f;
			float fromMax = 24;

			if (Projectile.localAI[0] >= fromMax)
				Projectile.Kill();
						
			Player player = Main.player[Projectile.owner];
			Projectile.Center = player.MountedCenter + Projectile.velocity;
			
			if (Projectile.ai[1] == 0 && Main.myPlayer == Projectile.owner)
			{
				if (Projectile.localAI[0] == 8)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 25f, Projectile.type, Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.ai[0]*1.4f, 1f);
			}
			
			double angle = Main.rand.NextDouble() * 2d * Math.PI;
			Vector2 offset = new Vector2((float)(Math.Sin(angle)), (float)(Math.Cos(angle))) * 40f * Projectile.ai[0] * (0.5f + Main.rand.NextFloat(0.5f));
			Vector2 velocity = offset.RotatedBy(90*Projectile.direction);
			velocity.Normalize();
			
			Dust dust = Dust.NewDustPerfect(Projectile.Center + Projectile.velocity + offset, 6, velocity*2f, 100, default, 2f);
			dust.noGravity = true;
		}
		
		public override bool PreDraw(ref Color lightColor) 
		{
			float num = 20f;
			float num2 = 4f;
			float fromMax = num + num2;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texture2 = ModContent.Request<Texture2D>(Texture + "_Flame").Value;
			Color value2 = Color.Transparent;
			Color color = new Color(255, 80, 20, 200);
			Color color2 = new Color(255, 255, 20, 70);
			Color color3 = Color.Lerp(new Color(255, 80, 20, 100), color2, 0.25f);
			Color color4 = new Color(80, 80, 80, 100);
			float num3 = 0.35f;
			float num4 = 0.7f;
			float num5 = 0.85f;
			float num6 = (Projectile.localAI[0] > num - 10f) ? 0.175f : 0.2f;
			float opacity = Utils.Remap(Projectile.localAI[0], num, fromMax, 1f, 0f, true);
			float num7 = Math.Min(Projectile.localAI[0], 20f);
			float num8 = Utils.Remap(Projectile.localAI[0], 0f, fromMax, 0f, 1f, true);
			float scale = Utils.Remap(num8, 0.2f, 0.5f, 0.5f*Projectile.ai[0], 1f*Projectile.ai[0], true);
			Rectangle rectangle = texture.Frame(1, 1);
			Rectangle rectangle2 = texture2.Frame(1, 1);
			if (num8 < 1f)
			{
				for (int i = 0; i < 2; i++)
				{
					for (float num10 = 1f; num10 >= 0f; num10 -= num6)
					{
						if (num8 < 0.1f)
						{
							value2 = Color.Lerp(Color.Transparent, color, Utils.GetLerpValue(0f, 0.1f, num8, true));
						}
						else
						{
							if (num8 < 0.2f)
							{
								value2 = Color.Lerp(color, color2, Utils.GetLerpValue(0.1f, 0.2f, num8, true));
							}
							else
							{
								if (num8 < num3)
								{
									value2 = color2;
								}
								else
								{
									if (num8 < num4)
									{
										value2 = Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, num8, true));
									}
									else
									{
										if (num8 < num5)
										{
											value2 = Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, num8, true));
										}
										else
										{
											if (num8 < 1f)
											{
												value2 = Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, num8, true));
											}
											else
											{
												value2 = Color.Transparent;
											}
										}
									}
								}
							}
						}
						float num11 = (1f - num10) * Utils.Remap(num8, 0f, 0.2f, 0f, 1f, true);
						Vector2 position = Projectile.Center - Main.screenPosition;
						Color color5 = value2 * num11;
						Color value3 = color5;
							value3.G /= 2;
							value3.B /= 2;
							value3.A = (byte)Math.Min((float)color5.A + 80f * num11, 255f);
							Utils.Remap(Projectile.localAI[0], 20f, fromMax, 0f, 1f, true);
						float num12 = 1f / num6 * (num10 + 1f);
						float rotation = Projectile.rotation + num10 * 1.57079637f + Main.GlobalTimeWrappedHourly * num12 * 2f * Projectile.direction;
						if (i == 0)
						{
							Main.EntitySpriteDraw(texture2, position, new Rectangle?(rectangle2), value3 * opacity, rotation * 0.5f, rectangle2.Size() / 2f, scale, SpriteEffects.None, 0f);
						}
						else if (i == 1)
						{
							Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), color5 * opacity, rotation + 1.57079637f, rectangle.Size() / 2f, scale * 0.45f, SpriteEffects.None, 0f);
						}
					}
				}
			}
			return false;
		}
    }
}
