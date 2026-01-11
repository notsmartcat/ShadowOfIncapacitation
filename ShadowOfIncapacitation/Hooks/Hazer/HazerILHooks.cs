using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.HazerHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Hazer.BitByPlayer += ILHazerBitByPlayer;
        IL.Hazer.Die += ILHazerDie;
        IL.Hazer.Update += ILHazerUpdate;

        IL.HazerGraphics.DrawSprites += ILHazerGraphicsDrawSprites;
        IL.HazerGraphics.Update += ILHazerGraphicsUpdate;

        //IL.Player.CanBeSwallowed += ILPlayerCanBeSwallowed;
        //IL.Player.BiteEdibleObject += ILPlayerBiteEdibleObject;
    }

    #region Hazer
    static void ILHazerBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchSub(),
            x => x.MatchStfld<Hazer>("bites")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ILHooksMisc.CreatureBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerBitByPlayer ActuallyKill!");
        }
    }
    static void ILHazerDie(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Hazer>("hasSprayed"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(HazerDie);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerUpdate NoAct!");
        }
    }
    public static bool HazerDie(Hazer self)
    {
        return ShadowOfOptions.hazy_spray.Value && IsComa(self);
    }

    static void ILHazerUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Die
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Incon");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerUpdate Die!");
        }
        #endregion

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Hazer>("tossed"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerUpdate NoAct target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerUpdate NoAct!");
        }
        #endregion

        #region Spray
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Hazer>("inkLeft"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerUpdate Spray target!");
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
            val.EmitDelegate(HazerDie);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerUpdate Spray!");
        }
        #endregion
    }
    #endregion

    #region HazerGraphics
    static void ILHazerGraphicsDrawSprites(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Eye
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(2),
            x => x.MatchCallvirt<RoomCamera>("get_room"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerGraphicsDrawSprites target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<HazerGraphics>(OpCodes.Call, "get_bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerGraphicsDrawSprites!");
        }
        #endregion
    }
    static void ILHazerGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region deadColor
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<HazerGraphics>(OpCodes.Call, "get_bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerGraphicsUpdate deadColor!");
        }
        #endregion

        #region camoGetTo
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchLdfld<Hazer>("spraying"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerGraphicsUpdate camoGetTo target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<HazerGraphics>(OpCodes.Call, "get_bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerGraphicsUpdate camoGetTo!");
        }
        #endregion

        #region eyeOpen
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdnull(),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerGraphicsUpdate eyeOpen skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<HazerGraphics>(OpCodes.Call, "get_bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerGraphicsUpdate eyeOpen!");
        }
        #endregion

        #region tentacles
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.025f),
            x => x.MatchBgeUn(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILHazerGraphicsUpdate tentacles target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<HazerGraphics>("get_bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<HazerGraphics>(OpCodes.Call, "get_bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerGraphicsUpdate tentacles!");
        }
        #endregion
    }
    #endregion

    /*
    #region HazerOther
    static void ILPlayerCanBeSwallowed(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Hazer
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Hazer)),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILPlayerCanBeSwallowed Hazer skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdcI4(1)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILPlayerCanBeSwallowed Hazer target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Hazer)),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_1);
            val.EmitDelegate(HazerSwallow);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerCanBeSwallowed Hazer!");
        }
        #endregion
    }
    static void ILPlayerBiteEdibleObject(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Hazer
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[8]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_grasps"),
            x => x.MatchLdloc(0),
            x => x.MatchLdelemRef(),
            x => x.MatchLdfld<Creature.Grasp>("grabbed"),
            x => x.MatchIsinst(typeof(IPlayerEdible)),
            x => x.MatchCallvirt<IPlayerEdible>("get_Edible"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Creature>(OpCodes.Call, "get_grasps");
            val.Emit(OpCodes.Ldloc_0);
            val.Emit(OpCodes.Ldelem_Ref);
            val.Emit<Creature.Grasp>(OpCodes.Ldfld, "grabbed");
            val.EmitDelegate(HazerSwallow2);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerCanBeSwallowed Hazer!");
        }
        #endregion
    }

    public static bool HazerSwallow(PhysicalObject self)
    {
        bool flag = self is Hazer hazer && ShadowOfOptions.hazy_swallow.Value && IsComa(hazer);

        Debug.Log(flag);

        return flag;
    }

    public static bool HazerSwallow2(PhysicalObject self)
    {
        bool flag = self is Hazer hazer && ShadowOfOptions.hazy_swallow.Value && IsComa(hazer);

        return flag;
    }
    #endregion
    */
}