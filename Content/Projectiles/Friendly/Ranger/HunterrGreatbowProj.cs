using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using ITD.Content.Items.Weapons.Ranger;
using Terraria.Audio;
using Terraria.ID;
using ITD.Utilities;
using System.Runtime.InteropServices;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class HunterrGreatbowProj : ModProjectile
    {

        private int syncTimer;
        private Vector2 mousePos;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 70;
            Projectile.height = 86;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;

        }

        public int timer;
        public float lerp = 0.12f;

        public override bool? CanDamage()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(mousePos.X);
            writer.Write(mousePos.Y);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Vector2 buffer;
            buffer.X = reader.ReadSingle();
            buffer.Y = reader.ReadSingle();
            if (Projectile.owner != Main.myPlayer)
            {
                mousePos = buffer;
            }
        }
        int ShieldHealth = 10;
        int Shattered;
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active)
                Projectile.Kill();

            if (Main.player[Projectile.owner].HeldItem.type == ModContent.ItemType<HunterrGreatbow>())
            {
                Projectile.damage = Main.player[Projectile.owner].GetWeaponDamage(Main.player[Projectile.owner].HeldItem);
                Projectile.damage = (int)(Projectile.damage * (1 + (ChargeTally * 0.75f)));
                Projectile.CritChance = player.GetWeaponCrit(player.HeldItem);
                Projectile.knockBack = Main.player[Projectile.owner].GetWeaponKnockback(Main.player[Projectile.owner].HeldItem, Main.player[Projectile.owner].HeldItem.knockBack);
            }
            Projectile.velocity = Vector2.Lerp(Vector2.Normalize(Projectile.velocity),
    Vector2.Normalize(mousePos - player.MountedCenter), lerp * 5) * 10;
            Projectile.velocity.Normalize();

            Vector2 center = player.MountedCenter;

            Projectile.Center = center;
            Projectile.rotation = Projectile.velocity.ToRotation();

            float extrarotate = ((Projectile.direction * player.gravDir) < 0) ? MathHelper.Pi : 0;
            float itemrotate = Projectile.direction < 0 ? MathHelper.Pi : 0;
            player.itemRotation = (Projectile.velocity.ToRotation() + itemrotate);
            player.itemRotation = (MathHelper.WrapAngle(player.itemRotation));
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 10;
            player.itemAnimation = 10;
            Vector2 HoldOffset = new Vector2(Projectile.width / 3, 0).RotatedBy(MathHelper.WrapAngle(Projectile.velocity.ToRotation()));

            Projectile.Center += HoldOffset;
            Projectile.spriteDirection = Projectile.direction * (int)player.gravDir;
            Projectile.rotation -= extrarotate;
            if (Projectile.owner == Main.myPlayer)
            {
                mousePos = Main.MouseWorld;

                if (++syncTimer > 20)
                {
                    syncTimer = 0;
                    Projectile.netUpdate = true;
                }
                if (Projectile.ai[2] >= (int)ShieldHealth/2)
                {
                    Shattered = 4;
                }
                if (Projectile.ai[2] >= ShieldHealth)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.Center += Projectile.velocity * 40;
                return;
            }
                Projectile.frame = (int)MathF.Round(ChargeTally) + Shattered;
            if (player.channel)
            {
                //Draw arrow
                if (Main.mouseRight)
                {
                    Shift = Math.Clamp(Shift - 0.05f, 0f, 1f);
                    if (Shift <= 0)
                    {
                        HaveArrow = true;
                        Shift = 0;
                        ChargeTally = Math.Clamp(ChargeTally + 0.05f, 0f, 3f);
                        Projectile.Center += Main.rand.NextVector2Circular((ChargeTally / 2) * 1.1f, (ChargeTally / 2) * 1.1f);
                    }
                }
                else
                {
                    Shift = Math.Clamp(Shift + 0.25f, 0f, 1f);
                    if (HaveArrow)
                    {
                        //RoundDown
                        Shift = 1;
                        SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, Projectile.Center);
                        HaveArrow = false;
                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 40 * (1 + ChargeTally * 0.5f),
        ModContent.ProjectileType<HunterrGreatarrow>(), Projectile.damage, Projectile.knockBack, Projectile.owner,MathF.Round(ChargeTally));
                        ChargeTally = 0;


                    }
                    else
                    {
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile other = Main.projectile[i];

                            if (i != Projectile.whoAmI && other.Reflectable()
                                && (Math.Abs(Projectile.Center.X - other.position.X)
                                 + Math.Abs(Projectile.Center.Y - other.position.Y) < 60))
                            {
                                if (!Main.dedServ)
                                {
                                    Projectile.ai[2]++;
                                    for (int d = 0; d < 4; d++)
                                    {
                                        Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.GoldCoin, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.2f));
                                        dust.noGravity = true;
                                        dust.velocity *= 4f;
                                        Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.t_Granite, 0, 0, 60, default, Main.rand.NextFloat(1f, 1.2f));
                                    }
                                    if (Projectile.ai[2] >= ShieldHealth)
                                    {
                                        if (Main.netMode != NetmodeID.Server)
                                        {
                                            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, other.velocity, Mod.Find<ModGore>("HunterrGreatbowGore0").Type);
                                            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, other.velocity, Mod.Find<ModGore>("HunterrGreatbowGore1").Type);
                                            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, other.velocity, Mod.Find<ModGore>("HunterrGreatbowGore2").Type);
                                        }
                                    }
                                    SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
                                    Projectile.velocity = other.velocity / 1.5f;
                                    CombatText.NewText(Projectile.Hitbox, Color.Orange, "BLOCKED", true);
                                    other.owner = Main.myPlayer;
                                    other.Kill();
                                    other.friendly = true;
                                    other.hostile = false;
                                    other.netUpdate = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (HaveArrow)
                {
                    Shift = 1;
                    SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, Projectile.Center);
                    HaveArrow = false;
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 40 * (1 +ChargeTally * 0.5f),
    ModContent.ProjectileType<HunterrGreatarrow>(), Projectile.damage, Projectile.knockBack, Projectile.owner, MathF.Floor(ChargeTally));
                    ChargeTally = 0;
                }
                Projectile.Kill();
            }

        }

        
        float Shift = 1;
        float ChargeTally;
        bool HaveArrow;

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            var Texture = ModContent.Request<Texture2D>("ITD/Content/Projectiles/Friendly/Ranger/HunterrGreatarrow").Value;
            Vector2 PointingTo = new((float)Math.Cos(Projectile.rotation), (float)Math.Sin(Projectile.rotation));
            Vector2 ShiftDown = PointingTo.RotatedBy(-MathHelper.PiOver2);
            float Dir = player.direction < 0 ? MathHelper.Pi : 0f;
            Vector2 drawPosition = player.Center + Main.rand.NextVector2Circular((ChargeTally / 2) * 1.5f, (ChargeTally / 2) * 1.5f) + PointingTo.RotatedBy(Dir) * (1f + (Shift * 40) - (ChargeTally * 6)) - ShiftDown.RotatedBy(Dir) * (Texture.Width / 2) - Main.screenPosition;

            Main.EntitySpriteDraw(Texture, drawPosition, null, Color.White * (1-Shift), Projectile.rotation + MathHelper.PiOver2 + Dir, Texture.Size(), 1f, 0, 0);
            return true;
        }
    }
}