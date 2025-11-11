using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;

namespace ITD.Content.Items.Favors.Prehardmode;

public class JarOfEyes : Favor
{
    public override int FavorFatigueTime => 60;
    public override bool IsCursedFavor => false;
    public override void SetFavorDefaults()
    {
        Item.width = Item.height = 32;
    }
    public override string GetBarStyle()
    {
        return "DefaultBarStyle";
    }
    public override string GetChargeSound()
    {
        return "DefaultChargeSound";
    }
    public override bool UseFavor(Player player)
    {
        //ITDPlayer modPlayer = player.ITD();
        for (int i = 0; i < 10; i++)
        {
            Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.UnitX.RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi)) * Main.rand.NextFloat(8f, 12f), ModContent.ProjectileType<PickledEye>(), 40, 0.1f, player.whoAmI);
        }
        SoundEngine.PlaySound(SoundID.Item155, player.Center);
        return true;
    }
    public override float ChargeAmount(ChargeData chargeData)
    {
        if (chargeData.Type == ChargeType.DamageTaken)
        {
            return 0.02f * chargeData.AmountX;
        }
        return 0f;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var line = tooltips.First(x => x.Name == "Tooltip2");
        string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
        line.Text = hotkeyText;
    }
}