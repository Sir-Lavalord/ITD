using ITD.Content.Buffs.Debuffs;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class ChainwhipWhip : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true; // This prevents the projectile from hitting through solid tiles.
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 24;
            Projectile.WhipSettings.RangeMultiplier = 0.72f;
            Projectile.damage = 72;
        }

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float ChargeTime
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; 

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }

            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
                // Plays a whipcrack sound at the tip of the whip.
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
            }

            float swingProgress = Timer / swingTime;

            if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && !Main.rand.NextBool(3))
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                points.Clear();
                Projectile.FillWhipControlPoints(Projectile, points);
                int pointIndex = Main.rand.Next(points.Count - 10, points.Count);
                Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
                int dustType = DustID.Enchanted_Gold;
                if (Main.rand.NextBool(3))
                    dustType = DustID.TintableDustLighted;

                Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 100, Color.White);
                dust.position = points[pointIndex];
                dust.fadeIn = 0.3f;
                Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
                dust.noGravity = true;
                dust.velocity *= 0.5f;
             
                dust.velocity += spinningPoint.RotatedBy(owner.direction * ((float)Math.PI / 2f));
                dust.velocity *= 0.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
			//target.AddBuff(ModContent.BuffType<ChainwhipTagDebuff>(), 300);
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            Projectile.damage = (int)(Projectile.damage * 0.7f);
        }

        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            //DrawLine(list);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            Texture2D whipHeadTexture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Summoner/ChainwhipWhipHead").Value;

            for (int i = 0; i < list.Count - 1; i++)
            {
                Rectangle frame;
                Vector2 origin = new Vector2(5, 8);
                float scale = 1;

                if (i == list.Count - 2)
                {
                    frame = new Rectangle(0, 0, whipHeadTexture.Width, whipHeadTexture.Height);
					Vector2 WhipHeadOrigin = whipHeadTexture.Size() * 0.5f;
						
                    // Add your scaling logic if required
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));

                    Vector2 element = list[i];
                    Vector2 diff = list[i + 1] - element;

                    float rotation = diff.ToRotation() - MathHelper.PiOver2;
                    Color color = Lighting.GetColor(element.ToTileCoordinates());

                    Main.EntitySpriteDraw(whipHeadTexture, pos - Main.screenPosition, frame, color, rotation, WhipHeadOrigin, 1f, flip, 0);
                }
                else
                {
                    if (i > 10)
                    {
                        frame = new Rectangle(0, 58, 10, 16);
                    }
                    else if (i > 5)
                    {
                        frame = new Rectangle(0, 42, 10, 16);
                    }
                    else if (i > 0)
                    {
                        frame = new Rectangle(0, 26, 10, 16);
                    }
                    else
                    {
                        frame = new Rectangle(0, 0, 10, 26); 
                    }

                    Vector2 element = list[i];
                    Vector2 diff = list[i + 1] - element;

                    float rotation = diff.ToRotation() - MathHelper.PiOver2;
                    Color color = Lighting.GetColor(element.ToTileCoordinates());

                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                    pos += diff;
                }
            }
            return false;
        }
    }
}