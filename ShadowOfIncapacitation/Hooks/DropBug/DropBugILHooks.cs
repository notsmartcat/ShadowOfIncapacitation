using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.DropBugHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.DropBug.Update += ILDropBugUpdate;

        IL.DropBugGraphics.Update += ILDropBugGraphicsUpdate;
    }

    static void ILDropBugGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region legsDangleCounter
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<DropBug>("get_Footing"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugGraphicsUpdate legsDangleCounter target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate legsDangleCounter!");
        }
        #endregion

        #region Breath
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brfalse_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate Breath!");
        }
        #endregion

        #region mandibleMovements
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<BodyPart>("ConnectToPoint"),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugGraphicsUpdate mandibleMovements Skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("mandibleMovements"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugGraphicsUpdate mandibleMovements target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate mandibleMovements!");
        }
        #endregion

        #region Flip
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchLdfld<UpdatableAndDeletable>("room")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugGraphicsUpdate Flip target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate Flip!");
        }
        #endregion

        #region drawPositions
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("drawPositions"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugGraphicsUpdate drawPositions target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate drawPositions!");
        }
        #endregion

        #region legsTravelDirs
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<DropBugGraphics>("bug"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DropBugGraphics>(OpCodes.Ldfld, "bug");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugGraphicsUpdate legsTravelDirs!");
        }
        #endregion
    }

    static void ILDropBugUpdate(ILContext il)
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
                ILHooksMisc.TryAddKillFeedEntry(creature, "Bleed");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugUpdate Die!");
        }
        #endregion

        #region Act
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<DropBug>("get_Footing"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILDropBugUpdate Act target!");
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
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDropBugUpdate Act!");
        }
        #endregion
    }
}
