using ITD.Utilities;
using ITD.Utilities.Placeholders;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.DetoursIL;

namespace ITD.Content.Items.Accessories.Misc
{
    public class PortableLab : ModItem
    {
        public override string Texture => Placeholder.PHAxe;
        public override void Load()
        {
            IL_Player.AdjTiles += AddAlchTableIfLabEquipped;
        }
        private static void ApplyChange(Player player)
        {
            if (player.GetITDPlayer().portableLab)
            {
                player.adjTile[TileID.Bottles] = true;
                player.adjTile[TileID.AlchemyTable] = true;
                player.alchemyTable = true;
            }
        }
        private static void AddAlchTableIfLabEquipped(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // find the alchemyTable = false statement
                if (!c.TryGotoNext(MoveType.After, i => i.MatchStfld(typeof(Player), "alchemyTable")))
                {
                    DetourGroup.LogError("PortableLab: Assign to field alchemyTable not found");
                }
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(ApplyChange);
            }
            catch
            {
                DetourGroup.DumpIL(il);
            }
        }
        public override void SetDefaults()
        {
            Item.DefaultToAccessory();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetITDPlayer().portableLab = true;
        }
    }
}
