using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ITD.DetoursIL
{
    public abstract class DetourGroup // lighter class than using a modsystem for every single loader/detour. probably faster loading times?
    {
        public static void LogError(string message) => ITD.Instance.Logger.Error(message);
        public static void DumpIL(ILContext il) => MonoModHooks.DumpIL(ITD.Instance, il);
        public virtual void Load()
        {

        }
        public virtual void Unload()
        {

        }
    }
}
