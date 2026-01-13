using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Watcher;

using static Incapacitation.Incapacitation;

namespace Incapacitation.RatHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Watcher.Rat.BitByPlayer += ILRatBitByPlayer;
        IL.Watcher.Rat.Grabbed += ILRatGrabbed;
        IL.Watcher.Rat.Update += ILRatUpdate;
        IL.Watcher.Rat.Violence += ILRatViolence;

        IL.Watcher.RatAI.Act += ILRatAIAct;

        IL.Watcher.RatGraphics.Update += ILRatGraphicsUpdate;
    }

    #region Rat
    static void ILRatBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchSub(),
            x => x.MatchStfld<Rat>("bites")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ILHooksMisc.CreatureBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatBitByPlayer!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<Creature.Grasp>("grabber"),
            x => x.MatchIsinst(typeof(Player))
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatBitByPlayer ActuallyKill!");
        }
    }
    static void ILRatGrabbed(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGrabbed Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGrabbed!");
        }
        #endregion
    }
    static void ILRatUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Rat>("eaten"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatUpdate Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatUpdate!");
        }
        #endregion
    }
    static void ILRatViolence(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatViolence Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatViolence!");
        }
        #endregion
    }
    #endregion

    #region RatAI
    static void ILRatAIAct(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region rat 
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchCallvirt<AbstractCreature>("get_realizedCreature"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<RatAI>(OpCodes.Ldfld, "realizedCreature");
            val.Emit(OpCodes.Ldloc, 4);
            val.EmitDelegate(RatAIAct);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatAIAct rat!");
        }
        #endregion
    }

    public static bool RatAIAct(Creature self, int i)
    {
        return ShadowOfOptions.rat_corpse.Value && IsComa(self.abstractCreature.Room.creatures[i].realizedCreature);
    }
    #endregion

    #region RatGraphics
    static void ILRatGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region breath 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<RatGraphics>("breath"),
            x => x.MatchLdcR4(1f),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate breath Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<RatGraphics>("get_Rat"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<RatGraphics>(OpCodes.Call, "get_Rat");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate breath!");
        }
        #endregion

        #region blink 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<RatGraphics>("blink"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate blink Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<RatGraphics>("get_Rat"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<RatGraphics>(OpCodes.Call, "get_Rat");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate blink!");
        }
        #endregion

        #region FindGrip 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(9),
            x => x.MatchLdfld<Limb>("reachedSnapPosition"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate FindGrip Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<RatGraphics>("get_Rat"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<RatGraphics>(OpCodes.Call, "get_Rat");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate FindGrip!");
        }
        #endregion

        #region Dangle 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(9),
            x => x.MatchLdfld<BodyPart>("pos"),
            x => x.MatchLdloc(9),
            x => x.MatchLdfld<Limb>("absoluteHuntPos"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate Dangle Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<RatGraphics>("get_Rat"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<RatGraphics>(OpCodes.Call, "get_Rat");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatGraphicsUpdate Dangle!");
        }
        #endregion
    }
    #endregion
}