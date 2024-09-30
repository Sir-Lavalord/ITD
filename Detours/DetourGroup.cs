using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITD.Detours
{
    public abstract class DetourGroup // lighter class than using a modsystem for every single loader/detour. probably faster loading times?
    {
        public virtual void Load()
        {

        }
        public virtual void Unload()
        {

        }
    }
}
