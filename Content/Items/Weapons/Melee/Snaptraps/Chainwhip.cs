using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Content.Projectiles.Friendly.Summoner;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps;

public class Chainwhip : ITDSnaptrapItem
{
    public override void SetDefaults()
    {
        Item.DefaultToSnaptrap(30, 10, ModContent.ProjectileType<ChainwhipSnaptrapProjectile>(), 12f, 22, 50);
        //Item.DamageType = DamageClass.SummonMeleeSpeed;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(0, 0, 25);
    }
    public override bool CanUseItem(Player player)
    {
        ITDSnaptrap snaptrap = player.Snaptrap().ActiveSnaptrap;
        return snaptrap == null || snaptrap.IsStickingToTarget;
    }
    public override bool AltFunctionUse(Player player) => true;
    //uh oh custom code
    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        ITDSnaptrap snaptrap = player.Snaptrap().ActiveSnaptrap;
        if (snaptrap != null && snaptrap.IsStickingToTarget && player.altFunctionUse == 2)
        {
            type = ModContent.ProjectileType<ChainwhipWhip>();
            return;
        }
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (type == ModContent.ProjectileType<ChainwhipWhip>())
            return player.Snaptrap().ActiveSnaptrap != null;
        return player.Snaptrap().ShootSnaptrap();
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        float pulseAmount = Main.mouseTextColor / 255f;
        Color textColor = Color.LightPink * pulseAmount;
        var line = tooltips.First(x => x.Name == "Tooltip1");
        string coloredText = string.Format(line.Text, textColor.Hex3());
        line.Text = coloredText;
    }
}