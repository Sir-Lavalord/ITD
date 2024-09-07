using ITD.Content.Buffs.Debuffs;
using ITD.Players;
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
    public class FishbackerProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }


        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.width = 28;
            Projectile.WhipSettings.RangeMultiplier = 1f;
            Projectile.WhipSettings.Segments = 12;
            Projectile.light = 0.5f;
        }
        public override void OnSpawn(IEntitySource source)
        {
        }
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        Color dustCol;
        bool CanParry = true;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }
            if ((Timer >= swingTime * 0.25f && Timer <= swingTime * 0.75f) && CanParry)
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];

                    if (i != Projectile.whoAmI && other.hostile &&
                        !other.friendly && other.active &&
                        //TODO: Change this
                        other.aiStyle != -100
                        && Math.Abs(points[points.Count-1].X - other.position.X)
                        + Math.Abs(points[points.Count-1].Y - other.position.Y) < 100)
                    {
                        if (!Main.dedServ)
                        {
                            CanParry = false;
                            SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraParry"), points[points.Count - 1]);
                            CombatText.NewText(Projectile.Hitbox, Color.LimeGreen, "PARRY", true);
                            Main.player[Projectile.owner].GetModPlayer<ITDPlayer>().Screenshake = 20;
                            other.GetGlobalProjectile<FishbackerReflectedProj>().IsReflected = true;
                            other.owner = Main.myPlayer;
                            other.velocity.X *= -2f;
                            other.velocity.Y *= -1f;

                            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                            {
                                PositionInWorld = other.Center,
                            }, other.whoAmI);

                            other.friendly = true;
                            other.hostile = false;
                            other.damage *= 2;
                            other.netUpdate = true;
                        }
                    }
                }
            }
                owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
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
                dustCol = new Color(124, 167, 207);
                if (Main.rand.NextBool(3))
                    dustCol = new Color(192, 59, 166);
                Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, DustID.TintableDustLighted, 0f, 0f, 100, Color.White);
                dust.position = points[pointIndex];
                dust.fadeIn = 0.5f;
                Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
                dust.noGravity = true;
                dust.velocity *= 0.5f;
                dust.velocity += spinningPoint.RotatedBy(owner.direction * ((float)Math.PI / 2f));
                dust.velocity *= 0.5f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                //NOTE: CHANGE ORGIN FOR SEGMENTS
                Rectangle frame = new Rectangle(0, 0, 18, 28);
                Vector2 origin = new Vector2(5, 11);
                float scale = 1;
                if (i == list.Count - 2)
                {
                    frame.Y = 74;
                    frame.Height = 18;
                    frame.Width = 28;
                    origin.Y = -2;
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 10)
                {
                    frame.Y = 58;
                    frame.Height = 16;
                    frame.Width = 28;
                    origin.Y = -2;
                }
                else if (i > 5)
                {
                    frame.Y = 42;
                    frame.Width = 28;
                    frame.Height = 16;
                    origin.Y = -2;

                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Width = 28;
                    frame.Height = 16;
                    origin.Y = -2;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, Color.White, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    public class FishbackerReflectedProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool IsReflected;
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsReflected)
            {
                target.AddBuff(ModContent.BuffType<FishbackerTagDebuff>(), 300);
            }
        }
    }
}