using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace ITD.Content.UI
{
    // wrapper class, mainly to avoid fucking up stuff from other mods with UILoader. partly based on potionslot's SmartUIState without all the extra code
    public abstract class ITDUIState : UIState
    {
        protected internal virtual UserInterface UserInterface { get; set; }
        public abstract int InsertionIndex(List<GameInterfaceLayer> layers);
        public virtual bool Visible { get; set; } = false;
        public virtual InterfaceScaleType Scale { get; set; } = InterfaceScaleType.UI;
        public virtual void Unload() { }
    }
}
