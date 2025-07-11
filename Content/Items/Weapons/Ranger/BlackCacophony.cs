using Terraria.Audio;
using Terraria.DataStructures;
using ITD.Systems;
using ITD.Players;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using ITD.Content.Projectiles.Other;

namespace ITD.Content.Items.Weapons.Ranger
{
    public class BlackCacophony : ModItem
    {
        public int windup = 1;
        public int mode = 1;
        public float colorProgress = .02f;
        public bool isRightClickHeld = false;
        public bool chargedOnce = false;

        public override void SetStaticDefaults()
        {
            FrontGunLayer.RegisterData(Item.type);
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 20;
            Item.height = 20;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = -1;
            Item.knockBack = 5;
            Item.value = 500000;
            Item.rare = ItemRarityID.Cyan;
            Item.UseSound = SoundID.Item11;

            Item.width = 116;
            Item.height = 36;

            Item.shoot = ProjectileID.Bullet;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
        }

        public void Hold(Player player)
        {
            ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;

            if (mouse.X < player.Center.X)
                player.direction = -1;
            else
                player.direction = 1;

            float rotation = (Vector2.Normalize(mouse - player.MountedCenter)*player.direction).ToRotation();
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilFront * player.direction - MathHelper.PiOver2 * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rotation * player.gravDir - modPlayer.recoilBack * player.direction - MathHelper.PiOver2 * player.direction);
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame)
        {
            Hold(player);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, -20f);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        int privateMode = 0;
        public override bool CanUseItem(Player player)
        {
            Item.shoot = ProjectileID.None;
            Item.UseSound = SoundID.Item11;

            if (player.altFunctionUse == 2)
            {
                if (mode == 1)
                {
                    mode = 2; //Fire state
                    CombatText.NewText(player.getRect(), new Color(184, 115, 51), this.GetLocalization("Messages.FiringMode").Value, false, false);
                }
                else
                {
                    mode = 1; //Windup state
                    CombatText.NewText(player.getRect(), new Color(145, 145, 145), this.GetLocalization("Messages.WindupMode").Value, false, false);
                }


                SoundStyle blackcacophanymodeswap = new SoundStyle("ITD/Content/Sounds/BlackCacophonyModeSwitch") with
                {
                    Volume = 0.75f,
                    Pitch = 1f,
                    PitchVariance = 0.1f,
                    MaxInstances = 3,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                };
                SoundEngine.PlaySound(blackcacophanymodeswap, player.Center);
            }
            else
            {
                if (mode == 1)
                {
                    if (windup >= 60)
                    {
                        Item.shoot = ProjectileID.None;
                        Item.useAmmo = AmmoID.None;
                        Item.useTime = Item.useAnimation = 60;
                    } 
                    else
                    {
                        Item.shoot = ProjectileID.None;
                        Item.useAmmo = AmmoID.None;
                        Item.useTime = Item.useAnimation = 6;

                        SoundStyle blackcacophany = new SoundStyle("ITD/Content/Sounds/BarrelSpinShort") with
                        {
                            Volume = 0.75f,
                            Pitch = 1f * windup / 10,
                            PitchVariance = 0.1f,
                            MaxInstances = 3,
                            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                        };
                        SoundEngine.PlaySound(blackcacophany, player.Center);
                    }

                    windup += 2;
                    if (windup >= 60)
                    {
                        windup = 60;
                        if (chargedOnce == false)
                        {
                            SoundStyle blackcacophanycharged = new SoundStyle("ITD/Content/Sounds/BlackCacophonyCharged") with
                            {
                                Volume = 0.75f,
                                Pitch = 1f,
                                PitchVariance = 0.1f,
                                MaxInstances = 3,
                                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                            };
                            SoundEngine.PlaySound(blackcacophanycharged, player.Center);
                        }
                        else
                        {
                            SoundStyle blackcacophanychargedwarn = new SoundStyle("ITD/Content/Sounds/BlackCacophonyChargedWarn") with
                            {
                                Volume = 0.75f,
                                Pitch = 1f,
                                PitchVariance = 0.1f,
                                MaxInstances = 3,
                                SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                            };
                            SoundEngine.PlaySound(blackcacophanychargedwarn, player.Center);
                        }

                        chargedOnce = true;
                    }
                }
                else
                {
                    Item.shoot = ProjectileID.Bullet;
                    Item.useAmmo = AmmoID.Bullet;
                    Item.useTime = Item.useAnimation = 8 - windup / 10;
                    if (windup > 2)
                    {
                        SoundStyle cacophanyfire = new SoundStyle("ITD/Content/Sounds/BlackCacophonyFire") with
                        {
                            Volume = 0.5f,
                            Pitch = 0.5f,
                            PitchVariance = 0.1f,
                            MaxInstances = 3,
                            SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                        };
                        Item.UseSound = cacophanyfire;
                        return true;
                    }
                    else
                    {
                        Item.UseSound = SoundID.Item11;
                        return false;
                    }
                }
            }
            return base.CanUseItem(player);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            chargedOnce = false;
            if (mode == 1)
                return true;
            if (player.whoAmI == Main.myPlayer)
            {
                float shellShift = MathHelper.ToRadians(-50);
                float SVar = shellShift + MathHelper.ToRadians(Main.rand.Next(-100, 301) / 10);
                float Sspeed = .09f * Main.rand.Next(15, 41);
                Projectile.NewProjectile(source, player.position, new Vector2(MathF.Cos(SVar) * Sspeed * -player.direction, MathF.Sin(SVar) * Sspeed), ModContent.ProjectileType<CacophanyBulletCasing>(), 0, 0, player.whoAmI);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 trueSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(15));
                    float scale = Main.rand.NextFloat(.8f, 1.6f);
                    trueSpeed *= scale;

                    Projectile.NewProjectile(source, position, trueSpeed, type, damage, knockback, player.whoAmI);
                }
            }
            colorProgress = .02f;
            windup -= 1;
            if (windup < 0)
            {
                windup = 0;
            }
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            Vector2 muzzleOffset = velocity.SafeNormalize(Vector2.Zero) * 116f;

            if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
            {
                position += muzzleOffset;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            privateMode = mode;
            string textColor;
            if (privateMode == 1)
            {
                textColor = "[c/919191:";
            } 
            else
            {
                textColor = "[c/B87333:";
            }
            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "Tooltip0")
                {
                    string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(BlackCacophony)}.CacophonyType.{privateMode}")).Value;
                    line.Text = textColor + line.Text + "]";
                    line.Text += textColor + " " + Text + "]";
                }
                if (line.Mod == "Terraria" && line.Name == "Tooltip4")
                {
                    string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(BlackCacophony)}.Mode.{privateMode}")).Value;
                    line.Text = Text + " " + line.Text;//Fine for now
                }
            }
        }
    }
}