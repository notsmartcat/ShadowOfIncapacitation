using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.VultureHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Vulture.UpdateNeck += ILVultureUpdateNeck;

        IL.Vulture.Update += ILVultureUpdate;

        IL.VultureTentacle.Update += ILVultureTentacleUpdate;

        IL.Vulture.Collide += ILVultureCollide;
    }

    static void ILVultureCollide(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Vulture>("get_IsMiros"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILVultureCollide!");
        }
    }

    static void ILVultureUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region NeckLimp
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[8]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.75f),
            x => x.MatchMul(),
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_State"),
            x => x.MatchIsinst(typeof(HealthState)),
            x => x.MatchCallvirt<HealthState>("get_health"),
            x => x.MatchBleUn(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);

        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILVultureUpdate!");
        }
        #endregion
    }

    static void ILVultureTentacleUpdate(ILContext il)
    {
        ILCursor val = new(il);

        #region Limp
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchStfld<Tentacle>("limp")
        }))
        {
            //val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(VultureTentacleUpdate);
            val.Emit<Tentacle>(OpCodes.Stfld, "limp");
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILVultureTentacleUpdate!");
        }
        #endregion
    }

    public static bool VultureTentacleUpdate(VultureTentacle self)
    {
        return (!IsIncon(self.vulture) && !self.vulture.Consious) || self.stun > 0;
    }

    static void ILVultureUpdateNeck(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region NeckLimp
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchCeq(),
            x => x.MatchStfld<Tentacle>("limp")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Vulture>(OpCodes.Ldfld, "neck");
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(VultureUpdateNeck);
            val.Emit(OpCodes.Ldc_I4_0);
            val.Emit(OpCodes.Ceq);
            val.Emit<Tentacle>(OpCodes.Stfld, "limp");
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILVultureUpdateNeck Neck!");
        }
        #endregion

        #region Continue
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchRet()
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILVultureUpdateNeck target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILVultureUpdateNeck!");
        }
        #endregion
    }

    public static bool VultureUpdateNeck(Vulture self)
    {
        return !IsIncon(self) && !self.Consious;
    }
}
