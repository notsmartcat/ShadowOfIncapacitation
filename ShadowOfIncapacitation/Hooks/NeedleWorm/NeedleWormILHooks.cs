using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

using static Incapacitation.Incapacitation;

namespace Incapacitation.NeedleWormHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.BigNeedleWorm.Update += ILBigNeedleWormUpdate;

        IL.NeedleWorm.AfterUpdate += ILNeedleWormAfterUpdate;
        IL.NeedleWorm.Update += ILNeedleWormUpdate;

        IL.NeedleWormGraphics.Update += ILNeedleWormGraphicsUpdate;

        IL.SmallNeedleWorm.BitByPlayer += ILSmallNeedleWormBitByPlayer;
        IL.SmallNeedleWorm.HangOnMom += SmallNeedleWormHangOnMom;
        IL.SmallNeedleWorm.Update += ILSmallNeedleWormUpdate;
    }

    #region BigNeedleWorm
    static void ILBigNeedleWormUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigNeedleWorm>("lameCounter"),
            x => x.MatchLdcI4(1),
            x => x.MatchBge(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target); ;
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigNeedleWorm NoAct!");
        }
        #endregion
    }
    #endregion

    #region NeedleWorm
    static void ILNeedleWormAfterUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<NeedleWorm>("atDestThisFrame"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormAfterUpdate Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormAfterUpdate!");
        }
        #endregion
    }
    static void ILNeedleWormUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region tail 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdloc(10),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormUpdate tail Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormUpdate tail!");
        }
        #endregion
    }
    #endregion

    #region NeedleWormGraphics
    static void ILNeedleWormGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region legs 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.11111111f),
            x => x.MatchBgeUn(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormGraphicsUpdate legs Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<NeedleWormGraphics>("worm"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<NeedleWormGraphics>(OpCodes.Ldfld, "worm");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormGraphicsUpdate legs!");
        }
        #endregion

        #region wings
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchCall(typeof(RWCustom.Custom).GetMethod(nameof(RWCustom.Custom.PerpendicularVector), BindingFlags.Static | BindingFlags.Public, null, [typeof(UnityEngine.Vector2)], null)),

        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormGraphicsUpdate wings skip!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<NeedleWormGraphics>("wings"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormGraphicsUpdate wings Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<NeedleWormGraphics>("worm"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<NeedleWormGraphics>(OpCodes.Ldfld, "worm");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILNeedleWormGraphicsUpdate wings!");
        }
        #endregion
    }
    #endregion

    #region SmallNeedleWorm
    static void ILSmallNeedleWormBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ILHooksMisc.CreatureBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSmallNeedleWormBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSmallNeedleWormBitByPlayer ActuallyKill!");
        }
    }
    static void SmallNeedleWormHangOnMom(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PhysicalObject>("grabbedBy"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match SmallNeedleWorm Target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match SmallNeedleWorm!");
        }
        #endregion
    }
    static void ILSmallNeedleWormUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PhysicalObject>("grabbedBy"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSmallNeedleWormUpdate Target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSmallNeedleWormUpdate!");
        }
        #endregion
    }
    #endregion
}