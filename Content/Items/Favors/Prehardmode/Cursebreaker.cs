using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Systems;
using ITD.Players;
using ITD.Utilities;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class Cursebreaker : Favor
    {
        public override int FavorFatigueTime => 60;
        public override bool IsCursedFavor => false;
        public override void SetFavorDefaults()
        {
            Item.width = Item.height = 32;
			Item.master = true;
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
			ITDPlayer modPlayer = player.GetITDPlayer();
            Vector2 mouse = modPlayer.MousePosition;
			
            Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center - new Vector2(20, 0), Vector2.Normalize(mouse - player.Center)*16f, ModContent.ProjectileType<ThrowableGuardian>(), 499, 0.1f, player.whoAmI);
			SoundEngine.PlaySound(SoundID.NPCDeath17, player.Center);
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DamageGiven)
            {
                return 0.01f;
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
}