using ITD.Content.NPCs.Bosses;
using ITD.Content.Projectiles.Friendly.Misc;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.BossSummons;

public class OminousCandle : BossSummoner
{
    public override int NPCType => ModContent.NPCType<WispCandle>();

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
        ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = 20;
        Item.value = 100;
        Item.rare = ItemRarityID.Blue;
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<OminousCandleProj>();
        Item.shootSpeed = 10f;
    }

    public override bool CanUseItem(Player player)
    {
        bool noCandle = true;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<OminousCandleProj>())
            {
                noCandle = false;
                break;
            }
        }
        return !NPC.AnyNPCs(NPCType) && !NPC.AnyNPCs(ModContent.NPCType<MotherWisp>()) && !Main.dayTime && noCandle;
    }
    public override bool? UseItem(Player player)
    {
        return true;
    }
}