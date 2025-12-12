using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.TubeWormHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.TubeWorm.Tongue.Update += ILTubeWormTongueUpdate;

        IL.TubeWorm.Update += ILTubeWormUpdate;
    }

    static void ILTubeWormUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_mainBodyChunk"),
            x => x.MatchCallvirt<BodyChunk>("get_submersion")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTubeWormUpdate target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTubeWormUpdate!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTubeWormUpdate ActuallyKill!");
        }
    }

    static void ILTubeWormTongueUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<TubeWorm.Tongue>("worm"),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<TubeWorm.Tongue>(OpCodes.Ldfld, "worm");
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerTongueUpdate!");
        }
    }
}
