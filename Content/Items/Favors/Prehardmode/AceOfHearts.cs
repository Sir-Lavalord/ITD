using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria;
using ITD.Systems;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ITD.Content.Items.Favors.Prehardmode
{
    public class AceOfHearts : Favor
    {

        public static LocalizedText HotkeyText { get; private set; }

        public override int FavorFatigueTime => 240;
        public override void SetDefaults()
        {
            Item.width = Item.height = 32;
            HotkeyText = Language.GetOrRegister(Mod.GetLocalizationKey($"Projectiles.{nameof(AceOfHearts)}.HotkeyText"));
            Item.rare = ModContent.RarityType<Content.Rarities.CursedFavorRarity>();
        }
        public override string GetBarStyle()
        {
            return "AceOfHeartsBarStyle";
        }
        public override bool UseFavor(Player player)
        {
            // does nothing rn, this is just for testing something
            return true;
        }
        public override float ChargeAmount(ChargeData chargeData)
        {
            if (chargeData.Type == ChargeType.DamageGiven)
            {
                return 0.05f;
            }
            return 0f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            // Retrieving the assigned keys for the favor key
            var assignedKeys = FavorPlayer.UseFavorKey.GetAssignedKeys();

            HotkeyText.WithFormatArgs(assignedKeys[0]);

            tooltips.Add(new TooltipLine(Mod, "Tooltip0", $"Grants Invulnerability."));
            tooltips.Add(new TooltipLine(Mod, "Tooltip1", $"All damage taken during invulnerability is inflicted after effect ends."));
            tooltips.Add(new TooltipLine(Mod, "Tooltip2", $"Can be used indefinitely, but effect duration is reduced per use until deactivation."));
            tooltips.Add(new TooltipLine(Mod, "Tooltip2", $"Press {assignedKeys[0]} to use when at full charge."));
            tooltips.Add(new TooltipLine(Mod, "Tooltip2", $"''Jackpot!''"));
        }
    }
}
