using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using Humanizer;
using System.Collections.Generic;
using ITD.Content.NPCs.Friendly;

namespace ITD.Content.Items.Other
{
    public class StrawmanItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 40;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.sellPrice(0, 0, 10, 0);
            Item.rare = ItemRarityID.Expert;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.UseSound = SoundID.Item20;
        }
        int dummytype = 0;
        public override bool CanUseItem(Player player)
        {
            if (NPC.CountNPCS(ModContent.NPCType<StrawmanDummy>()) < 50)
                return true;
            else
                return false;
        }
        public override void RightClick(Player player)
        {
            dummytype++;

            if (dummytype > 6)
                dummytype = 0;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "Tooltip3")
                {
                    line.Text = line.Text + "[c/428af5:Current mode:]";
                    switch (dummytype)
                    {
                        case 0:
                            line.Text = line.Text + "[c/428af5: DEFAULT]";
                            break;
                        case 1:
                            line.Text = line.Text + "[c/42f548: 50 DEF]";
                            break;
                        case 2:
                            line.Text = line.Text + "[c/f5d142: KNOCKBACK]";
                            break;
                        case 3:
                            line.Text = line.Text + "[c/bf4406: CONTACT DAMAGE]";
                            break;
                        case 4:
                            line.Text = line.Text + "[c/ff2e2e: KILLABLE]";
                            break;
                        case 5:
                            line.Text = line.Text + "[c/bf4406: RANGED DAMAGE]";
                            break;
                        case 6:
                            line.Text = line.Text + "[c/7d28df: NOT A BOSS]";
                            break;
                    }
                }
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    switch (dummytype)
                    {
                        case 0:
                            line.Text = "Perfect " + line.Text;
                            break;
                        case 1:
                            line.Text = "Heavy " + line.Text;
                            break;
                        case 2:
                            line.Text = "Windswept " + line.Text;
                            break;
                        case 3:
                            line.Text = "Thorny " + line.Text;
                            break;
                        case 4:
                            line.Text = "Terminal " + line.Text;
                            break;
                        case 5:
                            line.Text = "Armed " + line.Text;
                            break;
                        case 6:
                            line.Text = "Demoted " + line.Text;
                            break;
                    }
                }
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.NewNPC(source, (int)Math.Round(Main.MouseWorld.X), (int)Math.Round(Main.MouseWorld.Y), ModContent.NPCType<StrawmanDummy>(), 0, dummytype);

                for (int i = 0; i < 2; i++)
                {
                    int goreIndex = Gore.NewGore(null, new Vector2(Main.MouseWorld.X - 15, Main.MouseWorld.Y - 30), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(null, new Vector2(Main.MouseWorld.X - 15, Main.MouseWorld.Y - 30), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y + 1f;
                    goreIndex = Gore.NewGore(null, new Vector2(Main.MouseWorld.X - 15, Main.MouseWorld.Y - 30), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X + 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                    goreIndex = Gore.NewGore(null, new Vector2(Main.MouseWorld.X - 15, Main.MouseWorld.Y - 30), default(Vector2), Main.rand.Next(61, 64), 1f);
                    Main.gore[goreIndex].velocity.X = Main.gore[goreIndex].velocity.X - 1f;
                    Main.gore[goreIndex].velocity.Y = Main.gore[goreIndex].velocity.Y - 1f;
                }
            }
            return false;
        }
        public override bool ConsumeItem(Player player) => false;

        public override bool CanRightClick()
        {
            return true;
        }
    }
}