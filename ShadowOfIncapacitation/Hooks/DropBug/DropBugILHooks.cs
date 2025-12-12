using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.DropBugHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.DropBug.Update += ILDropBugUpdate;
    }

    static void ILDropBugUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Bleed");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugUpdate!");
        }
    }
}
