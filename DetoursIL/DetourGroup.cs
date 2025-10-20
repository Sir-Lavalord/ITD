using MonoMod.Cil;
using System.Reflection;

namespace ITD.DetoursIL;

public abstract class DetourGroup // lighter class than using a modsystem for every single loader/detour. probably faster loading times?
{
    public static void LogError(string message) => ITD.Instance.Logger.Error($"{MethodBase.GetCurrentMethod().Name}: {message}");
    public static void DumpIL(ILContext il) => MonoModHooks.DumpIL(ITD.Instance, il);
    public virtual void SetStaticDefaults()
    {

    }
    public virtual void Load()
    {

    }
    public virtual void Unload()
    {

    }
}
