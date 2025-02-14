using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.Tiles.BlueshroomGroves
{
    public class CharbleWorkBenchTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            this.DefaultToWorkbench();
            DustType = DustID.Marble;
        }
    }
}
