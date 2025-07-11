using Terraria.DataStructures;
using System.Collections.Generic;
using ITD.Content.NPCs.Friendly;
using Terraria.Localization;
using Terraria.UI;

namespace ITD.Content.Items.Other
{
    public class StrawmanItem : ModItem
    {
        // make the dummy type titles and description localizable please
        // -qangel
        // ok
        // -me
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
            Item.shoot = ProjectileID.WoodenArrowFriendly;

            Item.useTurn = true;
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

                Vector2 pos = new((int)Main.MouseWorld.X - 9, (int)Main.MouseWorld.Y - 20);
                Projectile.NewProjectile(player.GetSource_ItemUse(Item), pos, Vector2.Zero, ModContent.ProjectileType<StrawmanSpawner>(), 0, 0, player.whoAmI, ModContent.NPCType<StrawmanDummy>(), dummytype, player.whoAmI);

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
            return false;
        }

        public override bool ConsumeItem(Player player) => false;
        public override bool CanRightClick() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TargetDummy)
                .AddIngredient(ItemID.Fedora)
                .AddIngredient(ItemID.Hay, 64)
                .Register();
        }
    }
    //Special thanks to fargo's mutant network spawning crap
public class StrawmanSpawner : ModProjectile
{
    public override string Texture => ITD.BlankTexture;

    public override void SetStaticDefaults()
    {
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.hide = true;
    }

    public override bool? CanDamage()
    {
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int n = NPC.NewNPC(NPC.GetBossSpawnSource(Main.myPlayer), (int)Projectile.Center.X, (int)Projectile.Center.Y, (int)Projectile.ai[0],0, (int)Projectile.ai[1], (int)Projectile.ai[2]);
        if (n != Main.maxNPCs && Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, number: n);
    }
}
}