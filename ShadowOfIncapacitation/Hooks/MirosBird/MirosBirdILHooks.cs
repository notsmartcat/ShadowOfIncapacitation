using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.MirosBirdHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.MirosBird.UpdateNeck += ILMirosBirdUpdateNeck;
    }

    #region MirosBird
    static void ILMirosBirdUpdateNeck(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region limp
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchStfld<Tentacle>("limp"),
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(MirosBirdUpdateNeck);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMirosBirdUpdateNeck limp!");
        }
        #endregion

        #region retractFac 
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdflda<Tentacle>("floatGrabDest"),
        })) {}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMirosBirdUpdateNeck retractFac Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target); ;
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMirosBirdUpdateNeck retractFac!");
        }
        #endregion
    }

    public static void MirosBirdUpdateNeck(MirosBird self)
    {
        self.neck.limp = !IsIncon(self) && !self.Consious;
    }
    #endregion
}