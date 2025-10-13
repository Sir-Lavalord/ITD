using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using ITD.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Items.Favors.Prehardmode;

public class IncrediblySharpKnife : Favor
{
    public override int FavorFatigueTime => 0;
    public override bool IsCursedFavor => true;
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(7, 9));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }
    public override void SetFavorDefaults()
    {
        Item.width = Item.height = 32;
    }
    public override bool UseFavor(Player player)
    {
        player.statLife -= (int)(player.statLifeMax2 / 5f);
        CombatText.NewText(new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height), CombatText.LifeRegen, (int)(player.statLifeMax2 / 5f), true, false);

        int type = ModContent.ProjectileType<ShadowKnife>();
        for (int i = 0; i < 6; i++)
        {
            Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, type, 50, 0f, player.whoAmI);
        }
        for (int i = 0; i < 60; i++)
        {
            Vector2 pos = Main.rand.NextVector2Unit() * 64f;
            Dust d = Dust.NewDustDirect(player.Center + pos, 1, 1, DustID.CrystalPulse2, 0, 0f, 40, default, 1.5f);
            d.noGravity = true;
        }

        SoundEngine.PlaySound(SoundID.NPCDeath12, player.Center);
        SoundEngine.PlaySound(SoundID.NPCHit54, player.Center);

        if (player.statLife <= 0 && player.whoAmI == Main.myPlayer)
        {
            player.KillMeCustom("IncrediblySharpKnife");
        }

        return true;
    }
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var line = tooltips.First(x => x.Name == "Tooltip2");
        string hotkeyText = string.Format(line.Text, FavorPlayer.FavorKeybindString);
        line.Text = hotkeyText;
    }
}
