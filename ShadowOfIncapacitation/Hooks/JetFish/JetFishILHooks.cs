using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.JetFishHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.JetFish.Update += ILJetFishUpdate;

        IL.JetFishGraphics.Update += ILJetFishGraphicsUpdate;
    }

    #region JetFish
    static void ILJetFishUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_grasps"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishUpdate NoAct target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse (out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishUpdate NoAct!");
        }
        #endregion
    }
    #endregion

    #region JetFishGraphics
    static void ILJetFishGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region SwimSpeed
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("swim"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishGraphicsUpdate SwimSpeed target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<JetFishGraphics>(OpCodes.Ldfld, "fish");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishGraphicsUpdate SwimSpeed!");
        }
        #endregion

        #region zRotation
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("zRotation"),
            x => x.MatchLdloc(0),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishGraphicsUpdate zRotation target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<JetFishGraphics>(OpCodes.Ldfld, "fish");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishGraphicsUpdate zRotation!");
        }
        #endregion

        #region airEyes
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdcR4(1),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<PhysicalObject>("get_bodyChunks"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishGraphicsUpdate airEyes target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<JetFishGraphics>(OpCodes.Ldfld, "fish");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishGraphicsUpdate airEyes!");
        }
        #endregion

        #region jetActive
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(3),
            x => x.MatchLdcI4(0),
            x => x.MatchCeq(),
            x => x.MatchLdarg(0),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishGraphicsUpdate jetActive target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<JetFishGraphics>(OpCodes.Ldfld, "fish");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishGraphicsUpdate jetActive!");
        }
        #endregion

        #region flippersVel
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdfld<JetFishGraphics>("flippers")
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILJetFishGraphicsUpdate flippersVel skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<JetFishGraphics>("fish"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<JetFishGraphics>(OpCodes.Ldfld, "fish");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILJetFishGraphicsUpdate flippersVel!");
        }
        #endregion
    }
    #endregion
}