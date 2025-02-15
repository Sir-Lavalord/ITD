using ITD.Systems.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Content.TileEntities.Chests
{
    public class Chest3x2 : ITDChestTE
    {
        public override Point8 Dimensions => new(3, 2);
    }
}
