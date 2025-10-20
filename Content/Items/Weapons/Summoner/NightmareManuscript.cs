using ITD.Content.Projectiles.Friendly.Summoner;

namespace ITD.Content.Items.Weapons.Summoner;

public class NightmareManuscript : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
    }
    public override void SetDefaults()
    {
        Item.damage = 34;
        Item.DamageType = DamageClass.Summon;
        Item.mana = 8;
        Item.width = 30;
        Item.height = 34;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.knockBack = 5;
        Item.value = 10000;
        Item.master = true;
        Item.UseSound = SoundID.Item44;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.shoot = ModContent.ProjectileType<NightmareManuscriptProj>();
    }
    public override bool CanUseItem(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] < 1;
    }

}
public class WaxwellPlayer : ModPlayer
{
    public float codexClickCD;//Click cooldown is now player dependant, i cant do this shit no more
    public bool isholdingCodex;
    public float codexMode;

    public override void PreUpdate()
    {
        if (Player.HeldItem.ModItem is NightmareManuscript)
        {
            isholdingCodex = true;
        }
        else
        {
            isholdingCodex = false;
        }
        if (codexClickCD > 0)
        {
            /*                Main.NewText(codexClickCD.ToString(), Color.Violet);
            */
            codexClickCD--;
        }
    }
}