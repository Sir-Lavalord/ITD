using System;
using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using ITD.Particles.Projectile;
using ITD.Particles;
using ITD.Players;
using ITD.Utilities;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class WeepWandWisp : ModProjectile
    {
		public override string Texture => ITD.BlankTexture;
		public ParticleEmitter emitter;
		
		public const float speed = 12f;
		
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 6;
			
			emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldOverProjectiles);
            emitter.tag = Projectile;
        }

		public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255);
        }

        public override void AI()
        {
			if (emitter != null)
                emitter.keptAlive = true;
            //This whole thing is for projectile control
            if (Main.myPlayer == Projectile.owner && Projectile.ai[0] == 0f)
            {
				Player player = Main.player[Projectile.owner];
                if (player.channel)
                {
					ITDPlayer modPlayer = player.GetITDPlayer();
					Vector2 mouse = modPlayer.MousePosition;

                    float xDistance = (float)mouse.X - Projectile.Center.X;
                    float yDistance = (float)mouse.Y - Projectile.Center.Y;
					float distance = Vector2.Distance(mouse, Projectile.Center);
                    if (distance > speed)
                    {
                        distance = speed / distance;
                        xDistance *= distance;
                        yDistance *= distance;
                        if ((int)xDistance != (int)Projectile.velocity.X || (int)yDistance != (int)Projectile.velocity.Y)
                        {
                            Projectile.netUpdate = true;
                        }
                        Projectile.velocity.X = xDistance;
                        Projectile.velocity.Y = yDistance;
                    }
                    else
                    {
                        if ((int)xDistance != (int)Projectile.velocity.X || (int)yDistance != (int)Projectile.velocity.Y)
                        {
                            Projectile.netUpdate = true;
                        }
                        Projectile.velocity.X = xDistance;
                        Projectile.velocity.Y = yDistance;
                    }
                }
                else
                {
                    
                    if (Projectile.ai[0] == 0f)
                    {
                        Projectile.ai[0] = 1f;
						Projectile.timeLeft = 200;
                        Projectile.netUpdate = true;
                        if (Projectile.velocity.Length() < 2f) {
							Vector2 fromPlayer = Vector2.Normalize(Projectile.Center - player.Center);
							Projectile.velocity = fromPlayer *= speed;
						}
						else {
							Projectile.velocity = Vector2.Normalize(Projectile.velocity) * speed;
						}
                    }
                }
            }
            //if (++Projectile.localAI[0] >= 2f)
            //{
            //    Projectile.localAI[0] = 0f;
                emitter?.Emit(Projectile.Center + Main.rand.NextVector2Square(-8f, 8f), Projectile.velocity*0.2f, 0f, 20);
            //}
        }
    }
}