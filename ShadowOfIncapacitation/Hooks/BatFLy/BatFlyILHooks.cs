using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.BatFlyHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Fly.BitByPlayer += ILFlyBitByPlayer;
        IL.Fly.Update += ILFlyUpdate;

        IL.FlyAI.Update += ILFlyAIUpdate;

        IL.FlyGraphics.Update += ILFlyGraphicsUpdate;
    }

    #region Fly
    static void ILFlyBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchSub(),
            x => x.MatchStfld<Fly>("bites")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ILHooksMisc.CreatureBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyBitByPlayer ActuallyKill!");
        }
    }
    static void ILFlyUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        /*
        #region Drown
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Fly>("drown"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyUpdate Drown target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate Drown!");
        }
        #endregion
        */

        #region DrownDie
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate DrownDie!");
        }
        #endregion

        #region PlayerDie
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdfld<RainWorldGame>("devToolsActive")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate PlayerDie Skip!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Incon");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate PlayerDie!");
        }
        #endregion

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_grasps"),
            x => x.MatchLdcI4(0),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyUpdate NoAct target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate NoAct!");
        }
        #endregion
    }
    #endregion

    #region FlyAI
    static void ILFlyAIUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<FlyAI>("fly"),
            x => x.MatchCallvirt<Creature>("get_grasps"),
            x => x.MatchLdcI4(0),
            x => x.MatchLdelemRef(),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<FlyAI>(OpCodes.Ldfld, "fly");
            val.EmitDelegate(FlyAIUpdate);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyUpdate NoAct!");
        }
        #endregion
    }

    public static bool FlyAIUpdate(Fly self)
    {
        return self.AI.behavior == RescueIncon;
    }
    #endregion

    #region FlyGraphics
    static void ILFlyGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        /*
        #region Wings
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<Creature>("get_dead"),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyGraphicsUpdate Wings skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<GraphicsModule>("get_owner"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyGraphicsUpdate Wings target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<FlyGraphics>("fly"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<FlyGraphics>(OpCodes.Ldfld, "fly");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyGraphicsUpdate Wings!");
        }
        #endregion

        #region all
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<FlyGraphics>("horizontalSteeringCompensation"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyGraphicsUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<FlyGraphics>("fly"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<FlyGraphics>(OpCodes.Ldfld, "fly");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyGraphicsUpdate!");
        }
        #endregion
        */

        #region NoAll
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x =>x.MatchCall(typeof(RWCustom.Custom).GetMethod(nameof(RWCustom.Custom.AimFromOneVectorToAnother)))
        })) {}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyGraphicsUpdate skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchRet()
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILFlyGraphicsUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<FlyGraphics>("fly"),
            x => x.MatchLdfld<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<FlyGraphics>(OpCodes.Ldfld, "fly");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILFlyGraphicsUpdate!");
        }
        #endregion
    }
    #endregion
}