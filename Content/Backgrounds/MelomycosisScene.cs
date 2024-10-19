using ITD.Content.Buffs.Debuffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ITD.Content.Backgrounds
{
    public class MelomycosisScene : ModSceneEffect
    {
        public override bool IsSceneEffectActive(Player player) => player.HasBuff<MelomycosisBuff>();
        public override void SpecialVisuals(Player player, bool isActive)
        {
            player.ManageSpecialBiomeVisuals("BlackMold", isActive);
        }
    }
}
