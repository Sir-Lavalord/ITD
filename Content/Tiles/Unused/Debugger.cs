using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ITD.Content.Tiles.Unused
{
    public class Debugger : ModTile
    {
        public override void SetStaticDefaults()
        {
            var name = CreateMapEntryName();
            AddMapEntry(Color.Red, name);
        }
    }
}
