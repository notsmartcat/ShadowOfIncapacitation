using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Unity.Mathematics;
using static Incapacitation.Incapacitation;

namespace Incapacitation.CicadaHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Cicada.Update += ILCicadaUpdate;

        IL.CicadaGraphics.Update += ILCicadaGraphicsUpdate;
    }

    static void ILCicadaGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region SoundCharge
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdloc(0),
            x => x.MatchLdcR4(0.0f),
            x => x.MatchBleUn(out _),
        }))
        {
            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate SoundCharge target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit <CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate SoundCharge!");
        }
        #endregion

        #region SoundStop
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate SoundStop!");
        }
        #endregion

        #region Blink
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CicadaGraphics>("blinkCounter"),
            x => x.MatchLdcI4(1),
            x => x.MatchSub(),
            x => x.MatchStfld<CicadaGraphics>("blinkCounter")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate Blink target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate Blink!");
        }
        #endregion

        #region CreatureLooker
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CicadaGraphics>("creatureLooker"),
            x => x.MatchLdfld<CreatureLooker>("lookCreature"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate CreatureLooker target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate CreatureLooker!");
        }
        #endregion

        #region d
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate d!");
        }
        #endregion

        #region WingDeployement
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_iVars"),
            x => x.MatchLdfld<Cicada.IndividualVariations>("bustedWing"),
        }))
        {
            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate WingDeployement target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate WingDeployement!");
        }
        #endregion

        #region WingsDown
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate WingsDown!");
        }
        #endregion

        #region TentacleMode
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchLdfld<Cicada>("flying"),
            x => x.MatchBrtrue(out _),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate TentacleMode Continue!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<CicadaGraphics>("get_cicada"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CicadaGraphics>(OpCodes.Call, "get_cicada");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaGraphicsUpdate TentacleMode!");
        }
        #endregion
    }

    public static void CicadaGraphicsUpdate(Cicada self, int k, int l)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsIncon(self) || !self.flying || (self.graphicsModule as CicadaGraphics).wingDeployment[k, l] != 0.9f)
        {
            return;
        }

        (self.graphicsModule as CicadaGraphics).wingDeployment[k, l] = 1f;
    }

    static void ILCicadaUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_State"),
            x => x.MatchIsinst(typeof(HealthState)),
            x => x.MatchCallvirt<HealthState>("get_health"),
            x => x.MatchLdcR4(0.5f),
            x => x.MatchBgeUn(out target),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaUpdate!");
        }

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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaUpdate Bleed!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Cicada>("flying"),
            x => x.MatchBrtrue(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCicadaUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdnull(),
            x => x.MatchStfld<Cicada>("stickyCling"),
        }))
        {
            //val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCicadaUpdate!");
        }
    }
}
