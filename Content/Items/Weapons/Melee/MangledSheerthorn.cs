using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Projectiles.Friendly.Melee;
using ITD.Systems;
using ITD.Utilities;
using System;
using Terraria.Audio;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Content.Projectiles.Hostile.CosJel;

namespace ITD.Content.Items.Weapons.Melee
{
    public class MangledSheerthorn : ModItem
    {
        public int attackCycle = 0;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5;
            Item.value = Item.sellPrice(copper: 60);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = 20;
        }
        int Timer;
        public override bool? UseItem(Player player)
        {
            //manual turning augh.
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.MouseWorld.X > player.Center.X)
                {
                    player.direction = 1;
                }
                else player.direction = -1;
            }
            return base.UseItem(player);
        }
        public override void HoldItem(Player player)
        {
            if (player.itemAnimation == 0)
            {
                Timer = 0;
                return;
            }

            if (player.itemAnimation == player.itemAnimationMax)
            {
                Timer = player.itemAnimationMax - 2;
            }
            if (player.itemAnimation > 0)
            {
                Timer--;
            }

            if (Timer == player.itemAnimationMax / 2)
            {
            }
            if (Timer > 2 * player.itemAnimationMax / 3)
            {
                player.itemAnimation = player.itemAnimationMax - 2;
            }
            else
            {
                float prog = (float)Timer / (3 * (player.itemAnimationMax - 1) / 2);
                player.itemAnimation = (int)((player.itemAnimationMax - 1) * Math.Pow(MomentumProgress(prog), 1));
            }
            if (player.itemAnimation >= player.itemAnimationMax)
            {
                player.itemAnimation += 1;
            }
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 spinPos;
            Vector2 spinningpoint;
            MiscHelpers.GetPointOnSwungItemPath(player, 60f, 60f, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(Item), out spinPos, out spinningpoint);
            Vector2 value = spinningpoint.RotatedBy((double)(MathHelper.PiOver2 * (float)player.direction * player.gravDir), default(Vector2));
            Dust.NewDustPerfect(spinPos, DustID.Copper, new Vector2?(value * 4f), 100, default(Color), 1.5f).noGravity = true;
            if (player.itemAnimation == 1f)
            {
                Vector2 position = player.Center + new Vector2(60f * player.direction, player.height * 0.5f);
                Point point = position.ToTileCoordinates();

                int j = 0;
                while (j < 5 && point.Y >= 5 && WorldGen.SolidTile(point.X, point.Y, false))
                {
                    point.Y--;
                    j++;
                }
                int k = 0;
                while (k < 5 && point.Y <= Main.maxTilesY - 5 && !WorldGen.ActiveAndWalkableTile(point.X, point.Y - 1))
                {
                    point.Y++;
                    k++;
                }

                if (WorldGen.ActiveAndWalkableTile(point.X, point.Y - 1) && !WorldGen.SolidTile(point.X, point.Y - 2, false))
                {
                    position = new Vector2((float)(point.X * 16 + 5), (float)(point.Y * 16 - 5));
                    float power = 2 * Utils.GetLerpValue(600f, 0f, point.ToWorldCoordinates().Distance(Main.LocalPlayer.Center), true);
                    player.GetITDPlayer().BetterScreenshake(4, power, power, false);
                    attackCycle = ++attackCycle % 3;
                    if (attackCycle == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                int rand = Main.rand.Next(0, 5);
                                Projectile.NewProjectileDirect(player.GetSource_FromThis(), position - new Vector2(0f, 34f), new Vector2((Main.rand.NextFloat(0,1.5f) * 2f + 1f) * player.direction, -4f + 1f * Main.rand.NextFloat()),
                            ModContent.ProjectileType<MangledSheerthornProj>(), 20, 5f, player.whoAmI,rand);
                                SoundEngine.PlaySound(SoundID.Item72, player.Center);
                            }
                        }
                        power = 6 * Utils.GetLerpValue(800f, 0f, point.ToWorldCoordinates().Distance(Main.LocalPlayer.Center), true);
                        player.GetITDPlayer().BetterScreenshake(8, power, power, false);
                    }
                        SoundEngine.PlaySound(SoundID.Dig, position);

                    for (int i = 0; i < 12; i++)
                    {
                        Dust.NewDustPerfect(position, DustID.Copper, new Vector2(Main.rand.NextFloat() * 6f * player.direction, -8f + 8f * Main.rand.NextFloat()), 0, default(Color), 1.5f).noGravity = true;
                    }
                }
            }
        }
        public static float MomentumProgress(float x)
        {
            return (x * x * 4) - (x * x * x);
        }

    }
}