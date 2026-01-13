using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.CentipedeHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Centipede.BitByPlayer += ILCentipedeBitByPlayer;
        IL.Centipede.Shock += ILCentipedeShock;
        IL.Centipede.Update += ILCentipedeUpdate;

        IL.CentipedeGraphics.Update += ILCentipedeGraphicsUpdate;
    }

    #region Centipede
    static void ILCentipedeBitByPlayer(ILContext il)
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeBitByPlayer ActuallyKill!");
        }
    }
    static void ILCentipedeShock(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Player)),
            x => x.MatchCallvirt<Player>("PyroDeath")
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeShock arti!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdsfld<ModManager>("MSC"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_1);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeShock!");
        }
    }
    static void ILCentipedeUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region GlowerHead
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeUpdate GlowerHead!");
        }
        #endregion

        #region Skip
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcR4(0.0f),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Centipede>("shockCharge")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeUpdate Skip target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
            x => x.MatchLdfld<Room>("game"),
            x => x.MatchLdfld<RainWorldGame>("devToolsActive"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeUpdate Skip!");
        }
        #endregion

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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeUpdate Die!");
        }
        #endregion
    }
    #endregion

    #region CentipedeGraphics
    static void ILCentipedeGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region SoundLoop
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("centipede"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede");
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeGraphicsUpdate SoundLoop!");
        }
        #endregion

        #region walkCycle
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("walkCycle"),
            x => x.MatchLdarg(0)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate walkCycle target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("centipede"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeGraphicsUpdate walkCycle!");
        }
        #endregion

        #region wingFlapCycle
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("wingFlapCycle"),
            x => x.MatchLdarg(0)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate wingFlapCycle target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("centipede"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeGraphicsUpdate wingFlapCycle!");
        }
        #endregion

        #region FindGrip
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdfld<Limb>("reachedSnapPosition")
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate FindGrip skip!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("legs")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate FindGrip target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("centipede"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeGraphicsUpdate FindGrip!");
        }
        #endregion

        #region Dangle
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<Limb>("FindGrip")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate Dangle skip!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("legs")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCentipedeGraphicsUpdate Dangle target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("centipede"),
            x => x.MatchCallvirt<Creature>("get_Consious")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeGraphicsUpdate Dangle!");
        }
        #endregion
    }
    #endregion
}