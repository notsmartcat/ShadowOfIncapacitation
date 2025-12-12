using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using static Incapacitation.Incapacitation;

namespace Incapacitation.CentipedeHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Centipede.Update += ILCentipedeUpdate;

        IL.Centipede.BitByPlayer += ILCentipedeBitByPlayer;

        IL.Centipede.Shock += ILCentipedeShock;
    }

    static void ILCentipedeUpdate(ILContext il)
    {
        ILCursor val = new(il);

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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCentipedeUpdate!");
        }
    }

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
}
