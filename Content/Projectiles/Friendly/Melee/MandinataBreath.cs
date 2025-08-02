using System;

namespace ITD.Content.Projectiles.Friendly.Melee
{
    public class MandinataBreath : ModProjectile
    {
		private static float Lifespan = 35f;
        public override void SetDefaults()
        {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0f;
            return false;
        }

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 120, false);
        }

		public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Inflate(32, 32);
        }

        public override void AI()
        {
            Projectile.localAI[0] += 1f;
            if (Projectile.localAI[0] >= Lifespan)
                Projectile.Kill();
			
			Projectile.velocity *= 0.95f;
        }

		public override bool PreDraw(ref Color lightColor)  // horrible evil vanilla code
		{
			float num = 28f;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Color value2 = Color.Transparent;
			Color color2 = new Color(255, 255, 20, 70);
			Color color3 = Color.Lerp(new Color(255, 80, 20, 100), color2, 0.25f);
			Color color4 = new Color(80, 80, 80, 100);
			float num3 = 0.35f;
			float num4 = 0.7f;
			float num5 = 0.85f;
			float num6 = (Projectile.localAI[0] > num - 5f) ? 0.175f : 0.2f;
			float opacity = Utils.Remap(Projectile.localAI[0], num, Lifespan, 1f, 0f, true);
			float num7 = Math.Min(Projectile.localAI[0], 20f);
			float progress = Utils.Remap(Projectile.localAI[0], 0f, Lifespan, 0f, 1f, true);
			float scale = Utils.Remap(progress, 0f, 1f, 0.5f, 1.5f, true);
			Rectangle rectangle = texture.Frame(1, 1);
			if (progress < 1f)
			{
				for (int i = 0; i < 2; i++)
				{
					for (float num10 = 1f; num10 >= 0f; num10 -= num6)
					{
						if (progress < 0.2f)
						{
							value2 = Color.Lerp(Color.Transparent, color2, Utils.GetLerpValue(0f, 0.2f, progress, true));
						}
						else
						{
							if (progress < num3)
							{
								value2 = color2;
							}
							else
							{
								if (progress < num4)
								{
									value2 = Color.Lerp(color2, color3, Utils.GetLerpValue(num3, num4, progress, true));
								}
								else
								{
									if (progress < num5)
									{
										value2 = Color.Lerp(color3, color4, Utils.GetLerpValue(num4, num5, progress, true));
									}
									else
									{
										if (progress < 1f)
										{
											value2 = Color.Lerp(color4, Color.Transparent, Utils.GetLerpValue(num5, 1f, progress, true));
										}
										else
										{
											value2 = Color.Transparent;
										}
									}
								}
							}
						}
						float num11 = (1f - num10) * Utils.Remap(progress, 0f, 0.2f, 0f, 1f, true);
						Vector2 position = Projectile.Center - Main.screenPosition;
						Color color5 = value2 * num11;
						Color value3 = color5;
							value3.G /= 2;
							value3.B /= 2;
							value3.A = (byte)Math.Min((float)color5.A + 80f * num11, 255f);
							Utils.Remap(Projectile.localAI[0], 20f, Lifespan, 0f, 1f, true);
						float num12 = 1f / num6 * (num10 + 1f);
						float rotation = Projectile.rotation + num10 * 1.57079637f + Main.GlobalTimeWrappedHourly * num12 * 2f * Projectile.direction;
						if (i == 0)
						{
							Main.EntitySpriteDraw(texture, position + Projectile.velocity * -num7 * num6, new Rectangle?(rectangle), value3 * opacity, rotation + 0.5f, rectangle.Size() / 2f, scale, SpriteEffects.None, 0f);
							Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), value3 * opacity, rotation * 0.5f, rectangle.Size() / 2f, scale, SpriteEffects.None, 0f);
						}
						else if (i == 1)
						{
							Main.EntitySpriteDraw(texture, position, new Rectangle?(rectangle), color5 * opacity, rotation + 1.57079637f, rectangle.Size() / 2f, scale * 0.75f, SpriteEffects.None, 0f);
						}
					}
				}
			}
			return false;
		}
	}
}
