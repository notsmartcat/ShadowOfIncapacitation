using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static Incapacitation.Incapacitation;

namespace Incapacitation.DeerHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Deer.Update += ILDeerUpdate;

        IL.DeerGraphics.Update += ILDeerGraphicsUpdate;
    }

    private static void ILDeerGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Blink
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<DeerGraphics>("get_deer"),
            x => x.MatchCallvirt<Deer>("get_Kneeling"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDeerGraphicsUpdate Blink target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<DeerGraphics>("get_deer"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DeerGraphics>(OpCodes.Call, "get_deer");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerGraphicsUpdate Blink!");
        }
        #endregion

        #region antlerRandomMovementVel
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DeerGraphics>("antlerRandomMovementVel"),
            x => x.MatchLdcR4(-1)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDeerGraphicsUpdate antlerRandomMovementVel target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<DeerGraphics>("get_deer"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DeerGraphics>(OpCodes.Call, "get_deer");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerGraphicsUpdate antlerRandomMovementVel!");
        }
        #endregion
    }

    static void ILDeerUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Push1
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push1!");
        }
        #endregion

        #region Push2
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdcI4(4)
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push2 Skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push2!");
        }
        #endregion

        #region Push3
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdcR4(0.35f)
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push3 Skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push3!");
        }
        #endregion

        #region BodyDir

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchCall<Deer>("Act")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate Push3 Skip!");
        }
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDeerUpdate BodyDir!");
        }
        #endregion
    }
}
