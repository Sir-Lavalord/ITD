using Daybreak.Common.Features.Hooks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Reflection;
using Terraria.WorldBuilding;

namespace ITD.DetoursIL;

public static class VanillaSnowBiomeChanges
{
    private static MethodReference methodReference;
    private static MethodBase snowBiomeGenMethod;
    [OnLoad]
    public static void Load()
    {
        IL_WorldGen.AddGenPasses += SnowBiomeExtension;
        MonoModHooks.Modify(snowBiomeGenMethod, ModifySnowBiomeHeight);
    }
    private static void SnowBiomeExtension(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);

            // find the instruction that loads this string onto the stack
            if (!c.TryGotoNext(i => i.MatchLdstr("Generate Ice Biome")))
            {
                ITD.Log("Generate Ice Biome string not found");
                return;
            }
            // get the actual delegate pointer of the anonymous GenIceBiome method
            // this has to be stored in a global variable cuz you can't access an out directly from that far of a scope
            if (!c.TryGotoNext(i => i.MatchLdftn(out methodReference)))
            {
                ITD.Log("Delegate pointer not found");
            }
            // store methodbase in the global variable
            snowBiomeGenMethod = methodReference.ResolveReflection();
        }
        catch
        {
            ITD.Dump(il);
        }
    }
    private static int GetYOffset()
    {
        return WorldGen.GetWorldSize() switch
        {
            // medium world
            1 => 500,
            // large world
            2 => 700,
            // small world and default
            _ => 300,
        };
    }
    private static void ModifySnowBiomeHeight(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            // get to the maxValue load of 200 for Main.genRand (next is modifying num949)
            if (!c.TryGotoNext(i => i.MatchLdcI4(200)))
            {
                ITD.Log("snowThickness declaration not found");
            }
            // from here we need to modify the number that is used as the lower Y limit in the loop (this is equal to num949 in the source code)
            if (!c.TryGotoNext(i => i.MatchLdsfld(typeof(GenVars), "lavaLine")))
            {
                ITD.Log("Couldn't find lavaLine storage");
            }
            c.Index++; // advance cursor position

            c.EmitDelegate(GetYOffset); // get the int we need to add to 949

            c.Emit(OpCodes.Add); // add the int to num949
        }
        catch
        {
            ITD.Dump(il);
        }
    }
}
