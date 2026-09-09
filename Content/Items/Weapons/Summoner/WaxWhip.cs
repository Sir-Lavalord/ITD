using ITD.Content.Items.Materials;
using ITD.Content.Projectiles.Friendly.Summoner;
using ITD.Systems;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Summoner;
public class WaxWhip : ModItem
{
    public override void SetStaticDefaults()
    {
    }
    public override void SetDefaults()
    {
        Item.DefaultToWhip(ModContent.ProjectileType<WaxWhipProj>(), 6, 1, 30, 35);
        Item.rare = ItemRarityID.Blue;
        Item.value = 1000;
    }

    public override bool MeleePrefix()
    {
        return true;
    }
}