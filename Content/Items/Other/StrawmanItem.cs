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
using ITD.Content.Items.Accessories.Combat.All;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;

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
            if (ItemSlot.ShiftInUse)
            {
                dummytype--;
            }
            else
            {
                dummytype++;
            }
            if (dummytype < 0)
                dummytype = 6;
            if (dummytype > 6)
                dummytype = 0;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "Tooltip0")
                {
                    string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(StrawmanItem)}.Mode.{dummytype}")).Value;
                    line.Text = "[c/0388fc:" + line.Text + "]";
                    line.Text += "[c/0388fc: " + Text + "]";
                }
                if (line.Mod == "Terraria" && line.Name == "ItemName")
                {
                    string Text = Language.GetOrRegister(Mod.GetLocalizationKey($"Items.{nameof(StrawmanItem)}.DummyType.{dummytype}")).Value;

                    line.Text = Text + " " + line.Text;//Fine for now
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