using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.ScavengerHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.ScavengerGraphics.Update += ILScavengerGraphicsUpdate;

        IL.Scavenger.Update += ILScavengerUpdate;

        IL.ScavengerGraphics.ScavengerHand.Update += ILScavengerHandUpdate;

        IL.ScavengerGraphics.ScavengerLeg.Update += ILScavengerLegUpdate;

        IL.ScavengerGraphics.ShockReaction += ILScavengerGraphicsShockReaction;

        IL.ScavengerOutpost.GuardOutpostModule.Utility += ILGuardOutpostModuleUtility;
    }

    static void ILGuardOutpostModuleUtility(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdcR4(0.0f),
            x => x.MatchRet()
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILGuardOutpostModuleUtility target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerOutpost.GuardOutpostModule>("outpost"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerOutpost.GuardOutpostModule>(OpCodes.Call, "get_scavAI");
            val.Emit<ScavengerAI>(OpCodes.Ldfld, "scavenger");
            val.Emit<Creature>(OpCodes.Callvirt, "get_dead");
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILGuardOutpostModuleUtility!");
        }
        #endregion
    }

    static void ILScavengerGraphicsShockReaction(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_stun")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsShockReaction target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics>(OpCodes.Ldfld, "scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsShockReaction!");
        }
        #endregion
    }

    static void ILScavengerLegUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<ScavengerGraphics.ScavengerLeg>("get_scavenger"),
            x => x.MatchLdfld<Scavenger>("movMode")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerLegUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<ScavengerGraphics.ScavengerLeg>("get_scavenger"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics.ScavengerLeg>(OpCodes.Call, "get_scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerLegUpdate!");
        }
        #endregion
    }

    static void ILScavengerHandUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<ScavengerGraphics.ScavengerHand>("get_scavenger"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics.ScavengerHand>(OpCodes.Call, "get_scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerHandUpdate!");
        }
        #endregion
    }

    static void ILScavengerUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Stun
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdarg(0),
            x => x.MatchCall<Scavenger>("get_Injured"),
            x => x.MatchBgeUn(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerUpdate Stun!");
        }
        #endregion

        #region Hypothermia
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(2)
        }))
        {
            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerUpdate Hypothermia target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrtrue(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerUpdate Hypothermia!");
        }
        #endregion

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerUpdate NoAct target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerUpdate NoAct!");
        }
        #endregion
    }

    static void ILScavengerGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Blink
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("blink"),
            x => x.MatchLdcI4(1)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate Blink target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics>(OpCodes.Ldfld, "scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate Blink!");
        }
        #endregion

        #region deadEyesPop
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics>(OpCodes.Ldfld, "scavenger");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate deadEyesPop!");
        }
        #endregion

        #region Blink
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("lookPoint")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate Blink target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics>(OpCodes.Ldfld, "scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate Blink!");
        }
        #endregion

        #region nervous
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_abstractCreature")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate nervous target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ScavengerGraphics>("scavenger"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<ScavengerGraphics>(OpCodes.Ldfld, "scavenger");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILScavengerGraphicsUpdate nervous!");
        }
        #endregion
    }
}
