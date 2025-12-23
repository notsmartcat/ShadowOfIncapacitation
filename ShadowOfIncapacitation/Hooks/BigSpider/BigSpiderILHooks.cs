using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static Incapacitation.Incapacitation;

namespace Incapacitation.BigSpiderHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.BigSpider.Update += ILBigSpiderUpdate;

        IL.BigSpiderGraphics.Update += ILBigSpiderGraphicsUpdate;

        IL.BigSpider.Violence += ILBigSpiderViolence;

        IL.BigSpider.BabyPuff += ILBigSpiderBabyPuff;
    }

    static void ILBigSpiderBabyPuff(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(1),
            x => x.MatchStfld<BigSpider>("spewBabies"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILBigSpiderBabyPuff Manual target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(BigSpiderBabyPuffManual);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderBabyPuff Manual!");
        }
        #endregion

        #region Normal
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(BigSpiderBabyPuff);
            val.Emit(OpCodes.Brfalse, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderBabyPuff!");
        }
        #endregion

        #region ActuallyKill
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(1),
            x => x.MatchStfld<BigSpider>("spewBabies"),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderBabyPuff ActuallyKill!");
        }
        #endregion
    }

    public static bool BigSpiderBabyPuff(BigSpider self)
    {
        if (ShadowOfOptions.spid_mother.Value && ShadowOfOptions.spid_state.Value != "Disabled" && inconstorage.TryGetValue(self.abstractCreature, out InconData data) && data.isAlive)
        {
            Debug.Log("Spider mother not spew");
            return false;
        }
        Debug.Log("Spider mother spew");
        return true;
    }

    public static bool BigSpiderBabyPuffManual(BigSpider self)
    {
        if (inconstorage.TryGetValue(self.abstractCreature, out InconData data) && data.spiderMotherWasDead)
        {
            return true;
        }
        return false;
    }

    static void ILBigSpiderViolence(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Normal
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(6),
            x => x.MatchLdcR4(0.1f),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILBigSpiderViolence target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderViolence!");
        }
        #endregion
    }

    static void ILBigSpiderGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region legsDangleCounter
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<BigSpider>("get_Footing"),
            x => x.MatchBrtrue(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate legsDangleCounter target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigSpiderGraphicsUpdate legsDangleCounter!");
        }
        #endregion

        #region TailEnd
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[7]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("tailEnd"),
            x => x.MatchDup(),
            x => x.MatchLdfld<BodyPart>("vel"),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("breathDir"),
            x => x.MatchLdcR4(0.7f)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate TailEnd target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_dead")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigSpiderGraphicsUpdate TailEnd!");
        }
        #endregion

        #region Mandibles
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[7]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("mandibles"),
            x => x.MatchLdloc(9),
            x => x.MatchLdelemRef(),
            x => x.MatchDup(),
            x => x.MatchLdfld<BodyPart>("vel"),
            x => x.MatchCall(typeof(RWCustom.Custom).GetMethod(nameof(RWCustom.Custom.RNV)))
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate Mandibles target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigSpiderGraphicsUpdate Mandibles!");
        }
        #endregion

        #region Flip
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate Flip target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigSpiderGraphicsUpdate Flip!");
        }
        #endregion

        #region legsTravelDirs
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate legsTravelDirs!");
        }
        #endregion

        #region LegVel
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdflda<BigSpiderGraphics>("deadLeg")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for BigSpiderGraphicsUpdate LegVel target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpiderGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<BigSpiderGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match BigSpiderGraphicsUpdate LegVel!");
        }
        #endregion
    }

    static void ILBigSpiderUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Bleed
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderUpdate Bleed!");
        }
        #endregion

        #region Legs
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
            x => x.MatchLdfld<Room>("aimap"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILBigSpiderUpdate Legs target!");
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
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderUpdate Legs!");
        }
        #endregion

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<BigSpider>("get_Footing"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILBigSpiderUpdate NoAct target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(0),
            x => x.MatchStfld<BigSpider>("footingCounter")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderUpdate NoAct!");
        }
        #endregion

        #region NoStun
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigSpider>("spitter"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSpiderUpdate NoStun!");
        }
        #endregion
    }
}
