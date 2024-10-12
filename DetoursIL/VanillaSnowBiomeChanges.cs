using ITD.DetoursIL;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Terraria;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Utils;
using System;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ITD.DetoursIL
{
    public class VanillaSnowBiomeChanges : DetourGroup
    {
        private static MethodReference methodReference;
        private static MethodBase snowBiomeGenMethod;
        private static ILHook hook;
        public override void Load()
        {
            IL_WorldGen.AddGenPasses += SnowBiomeExtension;
            ILHook newHook = new(snowBiomeGenMethod, ModifySnowBiomeHeight); // create hook into anonymous method
            hook = newHook;
            hook.Apply(); // hook into it
        }
        private static void LogError(string message) => ITD.Instance.Logger.Error(message);
        private static void SnowBiomeExtension(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);

                // find the instruction that loads this string onto the stack
                if (!c.TryGotoNext(i => i.MatchLdstr("Generate Ice Biome")))
                {
                    LogError("SnowBiomeExtension: Generate Ice Biome string not found");
                    return;
                }
                // get the actual delegate pointer of the anonymous GenIceBiome method
                // this has to be stored in a global variable cuz you can't access an out directly from that far of a scope
                if (!c.TryGotoNext(i => i.MatchLdftn(out methodReference)))
                {
                    LogError("SnowBiomeExtension: Delegate pointer not found");
                }
                // store methodbase in the global variable
                snowBiomeGenMethod = methodReference.ResolveReflection();
            }
            catch
            {
                MonoModHooks.DumpIL(ITD.Instance, il);
            }
        }
        private static void ModifySnowBiomeHeight(ILContext il)
        {
            var c = new ILCursor(il);
            // get to the maxValue load of 200 for Main.genRand (next is modifying num949)
            if (!c.TryGotoNext(i => i.MatchLdcI4(200)))
            {
                LogError("SnowBiomeExtension: snowThickness declaration not found");
            }
            // from here we need to modify the number that is used as the lower Y limit in the loop (this is equal to num949 in the source code)
            if (!c.TryGotoNext(i => i.MatchLdsfld(typeof(GenVars), "lavaLine")))
            {
                LogError("SnowBiomeExtension: Couldn't find lavaLine storage");
            }
            c.Index++; // advance cursor position
            c.Emit(OpCodes.Ldc_I4, 500); // load this int onto the stack
            c.Emit(OpCodes.Add); // add the int to num949
        }
        public override void Unload()
        {
            hook?.Dispose();
            IL_WorldGen.AddGenPasses -= SnowBiomeExtension;
        }
    }
}
