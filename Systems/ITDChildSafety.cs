using ITD.DetoursIL;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ITD.Systems
{
    /// <summary>
    /// your question: wtf is this
    /// my answer: this is where all gores and dusts that need to be child safetied are marked as such.
    /// this is especially needed for gores, since we don't actually create a class inheriting ModGore for most of these, and they're instead autoloaded by tml
    /// don't use this for explicitly created gores
    /// gores need to be explicitly marked as safe or else they get turned into a puff of smoke which we don't want in the case someone's playing with child safe mode on
    /// for whatever reason.
    /// </summary>
    public class ITDChildSafety : DetourGroup
    {
        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            Mod itd = ITD.Instance;

            int[] icyBoulderGores = [
                itd.Find<ModGore>("IcyBoulderGore0").Type,
                itd.Find<ModGore>("IcyBoulderGore1").Type,
                itd.Find<ModGore>("IcyBoulderGore2").Type
                ];

            int[] razedWineGores = [
                itd.Find<ModGore>("RazedWineGore0").Type,
                itd.Find<ModGore>("RazedWineGore1").Type,
                ];

            int[] tombstoneGores = [
                itd.Find<ModGore>("TombstoneGore0").Type,
                itd.Find<ModGore>("TombstoneGore1").Type,
                itd.Find<ModGore>("TombstoneGore2").Type,
                ];

            int[] hunterrGores = [
                itd.Find<ModGore>("HunterrGreatbowGore0").Type,
                itd.Find<ModGore>("HunterrGreatbowGore1").Type,
                itd.Find<ModGore>("HunterrGreatbowGore2").Type,
                ];

            int[] pyroclasticSlimeGores = [
                itd.Find<ModGore>("PyroclasticSlimeGore0").Type
                ];

            int[] safeGores = [.. icyBoulderGores, .. razedWineGores, .. tombstoneGores, .. hunterrGores, .. pyroclasticSlimeGores];

            for (int i = 0; i < safeGores.Length; i++)
            {
                ChildSafety.SafeGore[safeGores[i]] = true;
            }
        }
    }
}
