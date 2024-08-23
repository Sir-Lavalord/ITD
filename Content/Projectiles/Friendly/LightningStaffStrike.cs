using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Projectiles.Friendly
{
    //Revertable because i'm a nice guy?!
    public class LightningStaffStrike : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/BlankTexture";
        public override void SetDefaults()
        {
            Projectile.height = 64; Projectile.width = 64;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                for (int i = 0; i < 8; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Electric, (float)Math.Cos(MathHelper.PiOver4 * i) * 6f, (float)Math.Sin(MathHelper.PiOver4 * i) * 6f);
                    d.noGravity = true;
                }
            }
        }
    }
    public class LightningStaffStrike2 : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.SentryShot[Projectile.type] = true;

        }
        Vector2 vSpawnposdefault;
        Vector2 vSpawnveldefault;

        public override void OnSpawn(IEntitySource source)
        {
            vSpawnposdefault = Main.MouseWorld;
            vSpawnveldefault = Projectile.velocity;
        }
        
        public override void SetDefaults()
        {

            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = 88;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 4;
            Projectile.timeLeft = 300;
            Projectile.scale = 2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;

        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            if (Projectile.localAI[1] < 1f)
            {
                Projectile.localAI[1] += 2f;
                Projectile.position += Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
            }
            bStopfalling = true;
            if (bStopfalling)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                    for (int i = 0; i < 10; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 226, vSpawnveldefault.X / 3, vSpawnveldefault.Y / 3, 60, default, Main.rand.NextFloat(1f, 1.7f));
                        dust.noGravity = true;
                        dust.velocity *= 4f;
                        Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 226, vSpawnveldefault.X / 3, vSpawnveldefault.Y / 3, 60, default, Main.rand.NextFloat(1f, 1.7f));

                    }

                    int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlankExplosion>(), Projectile.damage, 0, Main.myPlayer);
                    Main.projectile[projID].scale = 2f;//200x200
                    Main.projectile[projID].CritChance = Projectile.CritChance;
                    Main.projectile[projID].knockBack = 3f;
                    for (int i = 0; i < 8; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Main.projectile[projID].position, Main.projectile[projID].width, Main.projectile[projID].height, DustID.SilverCoin, 0, 0, 120, default, Main.rand.NextFloat(1f, 1.7f));
                        dust.noGravity = true;
                        dust.velocity *= 4f;
                        Dust.NewDustDirect(Main.projectile[projID].position, Main.projectile[projID].width, Main.projectile[projID].height, 226, 0, 0, 120, default, Main.rand.NextFloat(1f, 1.2f));

                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(Main.rand.NextFloat(-3, 3), -1), Main.rand.Next(61, 64));
                    }

                }
            }
            Projectile.velocity *= 0;
            if (vSpawnposdefault.Y > Projectile.Center.Y)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public override bool? Colliding(Rectangle myRect, Rectangle targetRect)
        {
            for (int i = 0; i < Projectile.oldPos.Length && (Projectile.oldPos[i].X != 0f || Projectile.oldPos[i].Y != 0f); i++)
            {
                myRect.X = (int)Projectile.oldPos[i].X;
                myRect.Y = (int)Projectile.oldPos[i].Y;
                if (myRect.Intersects(targetRect))
                {
                    return true;
                }
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.SilverBulletSparkle,
    new ParticleOrchestraSettings { PositionInWorld = (target.Center) },
    Projectile.owner);
            target.AddBuff(BuffID.Electrified, 180);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Electrified, 180);
        }
        bool bStopfalling;
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = 15;
            height = 15;

            if (Projectile.ai[1] == 1) ;
            Tile tile = Framing.GetTileSafely(Projectile.Center);
            fallThrough = vSpawnposdefault.Y >= Projectile.Bottom.Y - 10 && !bStopfalling;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool PreDraw(ref Color lightColor)
        {

            Color color = Lighting.GetColor((int)((double)Projectile.position.X + (double)Projectile.width * 0.5) / 16, (int)(((double)Projectile.position.Y + (double)Projectile.height * 0.5) / 16.0));
            Vector2 end = Projectile.position + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Projectile.GetAlpha(color);
            Vector2 vector = new Vector2(Projectile.scale) / 2f;
            for (int i = 0; i < 2; i++)
            {
                float num = ((Projectile.localAI[1] == -1f || Projectile.localAI[1] == 1f) ? (-0.2f) : 0f);
                if (i == 0)
                {
                    vector = new Vector2(Projectile.scale) * (0.5f + num);
                    DelegateMethods.c_1 = new Color(68, 214, 255, 0) * 0.5f;
                }
                else
                {
                    vector = new Vector2(Projectile.scale) * (0.3f + num);
                    DelegateMethods.c_1 = new Color(154, 230, 255, 0) * 0.5f;
                }
                DelegateMethods.f_1 = 1f;
                for (int j = Projectile.oldPos.Length - 1; j > 0; j--)
                {
                    if (!(Projectile.oldPos[j] == Vector2.Zero))
                    {
                        Vector2 start = Projectile.oldPos[j] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                        Vector2 end2 = Projectile.oldPos[j - 1] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                        Terraria.Utils.DrawLaser(Main.spriteBatch, tex, start, end2, vector, DelegateMethods.LightningLaserDraw);
                    }
                }
                if (Projectile.oldPos[0] != Vector2.Zero)
                {
                    DelegateMethods.f_1 = 1f;
                    Vector2 start2 = Projectile.oldPos[0] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
                    Terraria.Utils.DrawLaser(Main.spriteBatch, tex, start2, end, vector, DelegateMethods.LightningLaserDraw);
                }
            }
            return false;
        }
        int iStartCol;
        public override void AI()
        {
            if (iStartCol++ >= 8)
            {
                Projectile.tileCollide = true;
            }
            if (Projectile.scale > 0.01f)
            {
                Projectile.scale -= 0.005f;
            }
            else
            {
                Projectile.Kill();
            }

            if (Projectile.localAI[1] == 0f && Projectile.ai[0] >= 900f)
            {
                Projectile.ai[0] -= 1000f;
                Projectile.localAI[1] = -1f;
            }
            int frameCounter = Projectile.frameCounter;
            Projectile.frameCounter = frameCounter + 1;
            if (!Main.dedServ)
            {
                Lighting.AddLight(Projectile.Center, 0.3f, 0.45f, 0.5f);
            }
            if (Projectile.velocity == Vector2.Zero)
            {
                if (Projectile.frameCounter >= Projectile.extraUpdates * 2)
                {
                    Projectile.frameCounter = 0;
                    bool flag = true;
                    for (int i = 1; i < Projectile.oldPos.Length; i++)
                    {
                        if (Projectile.oldPos[i] != Projectile.oldPos[0])
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        Projectile.Kill();
                        return;
                    }
                }
                if (Main.rand.Next(Projectile.extraUpdates) == 0)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        float num = Projectile.rotation + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 3f);
                        float num2 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                        Vector2 vector = new Vector2((float)Math.Cos(num) * num2, (float)Math.Sin(num) * num2);
                        int num3 = Dust.NewDust(Projectile.Center, 0, 0, 226, vector.X, vector.Y);
                        Main.dust[num3].noGravity = true;
                        Main.dust[num3].scale = 1.2f;
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        Vector2 vector2 = Projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * Projectile.width;
                        int num4 = Dust.NewDust(Projectile.Center + vector2 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                        Dust dust = Main.dust[num4];
                        dust.velocity *= 0.5f;
                        Main.dust[num4].velocity.Y = 0f - Math.Abs(Main.dust[num4].velocity.Y);
                    }
                }
            }
            else
            {
                if (Projectile.frameCounter < Projectile.extraUpdates * 2)
                {
                    return;
                }
                Projectile.frameCounter = 0;
                float num5 = Projectile.velocity.Length();
                UnifiedRandom unifiedRandom = new UnifiedRandom((int)Projectile.ai[1]);
                int num6 = 0;
                Vector2 spinningpoint = -Vector2.UnitY;
                while (true)
                {
                    int num7 = unifiedRandom.Next();
                    Projectile.ai[1] = num7;
                    num7 %= 100;
                    float f = (float)num7 / 100f * ((float)Math.PI * 2f);
                    Vector2 vector3 = f.ToRotationVector2();
                    if (vector3.Y > 0f)
                    {
                        vector3.Y *= -1f;
                    }
                    bool flag2 = false;
                    if (vector3.Y > -0.02f)
                    {
                        flag2 = true;
                    }
                    if (vector3.X * (float)(Projectile.extraUpdates + 2) * 2f * num5 + Projectile.localAI[0] > 40f)
                    {
                        flag2 = true;
                    }
                    if (vector3.X * (float)(Projectile.extraUpdates + 2) * 2f * num5 + Projectile.localAI[0] < -40f)
                    {
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        if (num6++ >= 100)
                        {
                            Projectile.velocity = Vector2.Zero;
                            /*if (Projectile.localAI[1] < 1f)
							{
								Projectile.localAI[1] += 2f;
							}*/
                            Projectile.localAI[1] = 1f;
                            break;
                        }
                        continue;
                    }
                    spinningpoint = vector3;
                    break;
                }
                if (Projectile.velocity != Vector2.Zero)
                {

                    Projectile.localAI[0] += spinningpoint.X * (float)(Projectile.extraUpdates + 2) * 2f * num5;
                    Projectile.velocity = spinningpoint.RotatedBy(Projectile.ai[0] + (float)Math.PI / 2f) * num5;
                    Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.rand.Next(50) == 0)
            {
                float num = Projectile.rotation + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                float num2 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                Vector2 vector = new Vector2((float)Math.Cos(num) * num2, (float)Math.Sin(num) * num2);
                int num3 = Dust.NewDust(Projectile.Center, 0, 0, 226, vector.X, vector.Y);
                Main.dust[num3].noGravity = true;
                Main.dust[num3].scale = 1.2f;
            }
        }
    }
}