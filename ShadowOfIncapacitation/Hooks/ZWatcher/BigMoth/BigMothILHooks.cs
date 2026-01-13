using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Watcher;

using static Incapacitation.Incapacitation;

namespace Incapacitation.BigMothHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Watcher.BigMoth.Update += ILBigMothUpdate;

        IL.Watcher.BigMothGraphics.Update += ILBigMothGraphicsUpdate;
    }

    #region BigMoth
    static void ILBigMothUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(3),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc_3);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothUpdate!");
        }

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCall<PhysicalObject>("WeightedPush"),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothUpdate skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchRet(),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothUpdate Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothUpdate!");
        }
        #endregion
    }
    #endregion

    #region BigMothGraphics
    static void ILBigMothGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigMothGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigMothGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothGraphicsUpdate!");
        }
        #endregion
    }
    #endregion
}