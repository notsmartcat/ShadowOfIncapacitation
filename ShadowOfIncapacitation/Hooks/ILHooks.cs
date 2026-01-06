using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static Incapacitation.Incapacitation;

namespace Incapacitation;

internal class ILHooksMisc
{
    public static void Apply()
    {
        IL.Creature.Update += ILCreatureUpdate;

        IL.Creature.HeardNoise += ILCreatureHeardNoise;

        IL.Creature.HypothermiaUpdate += ILCreatureHypothermiaUpdate;

        IL.Creature.BrineWaterInteraction += ILCreatureBrineWaterInteraction;

        IL.DaddyCorruption.EatenCreature.Update += ILDaddyCorruptionUpdate;

        IL.DaddyLongLegs.Eat += ILDaddyLongLegsEat;

        IL.Hazer.Update += ILHazerUpdate;

        IL.InsectoidCreature.Update += ILInsectoidCreatureUpdate;

        IL.Leech.Attached += ILLeechAttached;

        IL.LocustSystem.Swarm.Update += ILSwarmUpdate;

        IL.MoreSlugcats.BigJellyFish.Collide += ILBigJellyFishCollide;

        IL.MoreSlugcats.StowawayBug.Eat += ILStowawayBugEat;

        IL.RoomRain.ThrowAroundObjects += ILRoomRainThrowAroundObjects;

        IL.Spider.Attached += ILSpiderAttached;

        IL.SporeCloud.Update += ILSporeCloudUpdate;

        IL.Watcher.Angler.JawsSlamShut += ILAnglerJawsSlamShut;

        IL.Watcher.ARZapper.ZapperContact += ILARZapperZapperContact;

        IL.Watcher.Barnacle.BitByPlayer += ILBarnacleBitByPlayer;

        IL.Watcher.BigMoth.Update += ILBigMothUpdate;

        IL.Watcher.BigSkyWhale.Update += ILBigSkyWhaleUpdate;

        IL.Watcher.Rat.BitByPlayer += ILRatBitByPlayer;

        IL.Watcher.SandGrub.Bury += ILSandGrubBury;

        IL.Watcher.SandGrub.PullFromBurrow += ILSandGrubPullFromBurrow;

        IL.Watcher.Sandstorm.AffectObjects += ILSandstormAffectObjects;

        IL.Watcher.Tardigrade.BitByPlayer += ILTardigradeBitByPlayer;

        IL.Weapon.HitSomethingWithoutStopping += ILWeaponHitSomethingWithoutStopping;

        IL.WormGrass.WormGrassPatch.InteractWithCreature += ILWormGrassPatchInteractWithCreature;

        IL.Tracker.CreatureNoticed += ILTrackerCreatureNoticed;
    }

    static void ILTrackerCreatureNoticed(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<AbstractCreature>("state"),
            x => x.MatchCallvirt<CreatureState>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.Emit<AbstractCreature>(OpCodes.Callvirt, "get_realizedCreature");
            val.EmitDelegate(IsInconBase);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTrackerCreatureNoticed!");
        }
    }

    static void ILWormGrassPatchInteractWithCreature(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.AfterLabel, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdcI4(1),
            x => x.MatchStloc(0)
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.Emit<WormGrass.WormGrassPatch.CreatureAndPull>(OpCodes.Ldfld, "creature");
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILWormGrassPatchInteractWithCreature!");
        }
    }

    static void ILWeaponHitSomethingWithoutStopping(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die"),
            x => x.MatchBr(out _)

        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Stab");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILWeaponHitSomethingWithoutStopping Spear!");
        }

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[1]
        {
            x => x.MatchIsinst(typeof(Rock))
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILWeaponHitSomethingWithoutStopping!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die"),
            x => x.MatchBr(out _)

        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Blunt");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILWeaponHitSomethingWithoutStopping Rock!");
        }
    }

    static void ILTardigradeBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(TardigradeBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTardigradeBitByPlayer!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(0),
            x => x.MatchIsinst(typeof(Player))
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILTardigradeBitByPlayer ActuallyKill!");
        }
    }

    public static void TardigradeBitByPlayer(Watcher.Tardigrade self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        int chance = UnityEngine.Random.Range(0, 101);

        if (IsIncon(self))
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
            else if (chance < 50)
            {
                data.isUncon = true;
            }
        }
        else
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
        }
    }

    static void ILSandstormAffectObjects(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(5),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc, 5);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSandGrubBury!");
        }
    }

    static void ILSandGrubPullFromBurrow(ILContext il)
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
                TryAddKillFeedEntry(creature, "Incon");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSandGrubPullFromBurrow!");
        }
    }

    static void ILSandGrubBury(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSandGrubBury!");
        }
    }

    static void ILRatBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchSub(),
            x => x.MatchStfld<Watcher.Rat>("bites")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(RatBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRatBitByPlayer ActuallyKill!");
        }
    }

    public static void RatBitByPlayer(Watcher.Rat self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        int chance = UnityEngine.Random.Range(0, 101);

        if (IsIncon(self))
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
            else if (chance < 50)
            {
                data.isUncon = true;
            }
        }
        else
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
        }
    }

    static void ILBigSkyWhaleUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc, 14);
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigSkyWhaleUpdate!");
        }
    }

    static void ILBigMothUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(3),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc_3);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigMothUpdate!");
        }
    }

    static void ILARZapperZapperContact(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(1),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILARZapperZapperContact!");
        }
    }

    static void ILBarnacleBitByPlayer(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(CreatureBitByPlayer);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBarnacleBitByPlayer!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBarnacleBitByPlayer ActuallyKill!");
        }
    }

    public static void CreatureBitByPlayer(Creature self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        if (self is Centipede centi)
        {
            centi.shockCharge *= 0.5f;
        }

        int chance = UnityEngine.Random.Range(0, 101);

        if (IsInconBase(self))
        {
            if (chance <= 25)
            {
                if(ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " Success! " + chance + "/25 for Dying after being bit");

                ActuallyKill(self);
                return;
            }
            else if (chance <= 50)
            {
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " Success! " + chance + "/50 for Unconscious after being bit");

                data.isUncon = true;
                return;
            }

            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + self + " Failure! " + chance + "/50 for Death or Unconscious after being bit");
        }
        else
        {
            if (chance <= 50)
            {
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " Success! " + chance + "/50 for Dying after being bit");

                ActuallyKill(self);
                return;
            }

            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + self + " Failure! " + chance + "/50 for Death after being bit");
        }
    }

    static void ILAnglerJawsSlamShut(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(11),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc, 11);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILAnglerJawsSlamShut!");
        }
    }

    static void ILSporeCloudUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(4),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldloc, 4);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSporeCloudUpdate!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdloc(9),
            x => x.MatchDup(),
            x => x.MatchCallvirt<HealthState>("get_health")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldloc, 4);
            val.EmitDelegate(SporeCloudUpdate);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSporeCloudUpdate!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(4),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldloc, 4);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSporeCloudUpdate!");
        }
    }

    public static void SporeCloudUpdate(Creature self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        data.lastDamageType = "Poison";
    }

    static void ILSpiderAttached(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(0),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc_0);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Incon");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSpiderAttached!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdloc(0),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchLdcI4(1),
            x => x.MatchStfld<Creature>("leechedOut")
        }))
        {
            val.Emit(OpCodes.Ldloc_0);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSpiderAttached!");
        }
    }

    static void ILRoomRainThrowAroundObjects(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(3),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldloc_3);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILRoomRainThrowAroundObjects!");
        }
    }

    static void ILStowawayBugEat(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.AfterLabel, new Func<Instruction, bool>[1]
        {
            x => x.MatchCallvirt<Tracker>("ForgetCreature")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldloc_1);
            val.EmitDelegate(StowawayBugEat);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILStowawayBugEat!");
        }
    }

    public static void StowawayBugEat(MoreSlugcats.StowawayBug bug, int i)
    {
        if (bug.eatObjects[i].chunk.owner is not Creature self || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        ActuallyKill(self);
    }

    static void ILBigJellyFishCollide(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(1),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_1);
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILBigJellyFishCollide Player!");
        }
    }

    static void ILSwarmUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LocustSystem.Swarm>("target"),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LocustSystem.Swarm>(OpCodes.Ldfld, "target");
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILSwarmUpdate Player!");
        }
    }

    static void ILLeechAttached(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(0),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Player)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc_0);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLeechAttached Player!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
{
            x => x.MatchLdloc(0),
            x => x.MatchCallvirt<BodyChunk>("get_owner"),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
}))
        {
            val.Emit(OpCodes.Ldloc_0);
            val.Emit<BodyChunk>(OpCodes.Callvirt, "get_owner");
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLeechAttached!");
        }
    }

    static void ILInsectoidCreatureUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(2)
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(InsectoidCreatureUpdate);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILInsectoidCreatureUpdate Actually Kill!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILInsectoidCreatureUpdate Actually Kill!");
        }
    }

    public static void InsectoidCreatureUpdate(Creature self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        data.lastDamageType = "Poison";
    }

    static void ILHazerUpdate(ILContext il)
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
                TryAddKillFeedEntry(creature, "Incon");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILHazerUpdate!");
        }
    }

    static void ILDaddyLongLegsEat(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdloc(2),
            x => x.MatchCallvirt<Player>("PermaDie")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldloc_2);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDaddyLongLegsEat Player!");
        }

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdfld<DaddyLongLegs.EatObject>("progression"),
            x => x.MatchLdcR4(0.5f),
            x => x.MatchBleUn(out _)
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDaddyLongLegsEat progression!");
        }

        if (val.TryGotoNext(MoveType.AfterLabel, new Func<Instruction, bool>[2]
        {
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldloc_1);
            val.EmitDelegate(DaddyLongLegsEat);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDaddyLongLegsEat!");
        }
    }

    public static void DaddyLongLegsEat(DaddyLongLegs daddyLongLegs, int i)
    {
        if (daddyLongLegs.eatObjects[i].chunk.owner is Creature self && inconstorage.TryGetValue(self.abstractCreature, out _))
        {
            ActuallyKill(self);
        }
    }

    static void ILDaddyCorruptionUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdsfld<ModManager>("CoopAvailable"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<DaddyCorruption.EatenCreature>(OpCodes.Ldfld, "creature");
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILDaddyCorruptionUpdate!");
        }
    }

    #region Creature_Update
    public static void ILCreatureUpdate(ILContext il)
    {
        ILCursor val = new(il);

        #region PyroDeath
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
                x => x.MatchLdarg(0),
                x => x.MatchIsinst(typeof(Player)),
                x => x.MatchCallvirt<Player>("PyroDeath")
        }))
        {
            val.MoveAfterLabels();
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureUpdate PyroDeath!");
        }
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
                x => x.MatchLdarg(0),
                x => x.MatchIsinst(typeof(Player)),
                x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureUpdate PyroDeath else!");
        }
        #endregion

        #region MeltedDeath
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureUpdate MeltedDeath!");
        }
        #endregion

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[10]
        {
                x => x.MatchCall<Creature>("get_dead"),
                x => x.Match(OpCodes.Brtrue_S),
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_State"),
                x => x.MatchIsinst<HealthState>(),
                x => x.Match(OpCodes.Brfalse_S),
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_State"),
                x => x.MatchIsinst<HealthState>(),
                x => x.MatchCallvirt<HealthState>("get_health")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureUpdate Bleed!");
        }

        #region PoisonDeath
        val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[2]
        {
                x => x.MatchLdarg(0),
                x => x.MatchCallvirt<Creature>("Die")
        });
        try
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        catch (Exception)
        {
            Incapacitation.Logger.LogError(all + "Failed to inject for Bleed Out!");
        }
        #endregion

        #region Bleed
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[10]
        {
                x => x.MatchCall<Creature>("get_dead"),
                x => x.Match(OpCodes.Brtrue_S),
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_State"),
                x => x.MatchIsinst<HealthState>(),
                x => x.Match(OpCodes.Brfalse_S),
                x => x.MatchLdarg(0),
                x => x.MatchCall<Creature>("get_State"),
                x => x.MatchIsinst<HealthState>(),
                x => x.MatchCallvirt<HealthState>("get_health")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureUpdate Bleed!");
        }
        val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
                x => x.MatchCallvirt<Creature>("Die")
        });
        try
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Bleed");
            });
        }
        catch (Exception)
        {
            Incapacitation.Logger.LogError(all + "Failed to inject for Bleed Out!");
        }

        #endregion

        #region FellOutOfRoom
        val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
                x => x.MatchLdsfld<ModManager>("CoopAvailable")
        });
        try
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        catch (Exception)
        {
            Incapacitation.Logger.LogError(all + "Failed to inject for FellOutOfRoom!");
        }
        #endregion
    }

    public static void TryAddKillFeedEntry(Creature receiver, string killType)
    {
        if (receiver != null && receiver.abstractCreature != null && inconstorage.TryGetValue(receiver.abstractCreature, out InconData data))
        {
            ViolenceCheck(receiver, data, killType);
        }
    }
    #endregion

    static void ILCreatureHeardNoise(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_abstractCreature"),
            x => x.MatchLdfld<AbstractCreature>("abstractAI"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCreatureHeardNoise target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureHeardNoise!");
        }
    }

    static void ILCreatureHypothermiaUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Gain
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdcR4(0.0f),
            x => x.MatchCall<RainWorldGame>("get_DefaultHeatSourceWarmth")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCreatureHypothermiaUpdate Gain target!");
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
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureHypothermiaUpdate Gain!");
        }
        #endregion

        #region Stun
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILCreatureHypothermiaUpdate Stun target!");
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
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureHypothermiaUpdate Stun!");
        }
        #endregion

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                TryAddKillFeedEntry(creature, "Bleed");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureHypothermiaUpdate Die!");
        }
    }

    static void ILCreatureBrineWaterInteraction(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchIsinst(typeof(Player)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureBrineWaterInteraction!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILCreatureBrineWaterInteraction!");
        }
    }
}

/*
class ILHooks
{
    public static bool ShadowOfLizardUpdate(Creature self)
    {
        return true;
    }
}
*/