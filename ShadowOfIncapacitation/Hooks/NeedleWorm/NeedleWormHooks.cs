using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.NeedleWormHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.BigNeedleWorm.Update += BigNeedleWormUpdate;

        On.BigNeedleWormAI.BigRespondCry += BigNeedleWormAIBigRespondCry;
        On.BigNeedleWormAI.Update += BigNeedleWormAIUpdate;

        On.NeedleWormAI.Update += NeedleWormAIUpdate;
        On.NeedleWormAI.WantToStayInDenUntilEndOfCycle += NeedleWormAIWantToStayInDenUntilEndOfCycle;

        On.SmallNeedleWorm.Update += SmallNeedleWormUpdate;
    }

    #region BigNeedleWorm
    static void BigNeedleWormUpdate(On.BigNeedleWorm.orig_Update orig, BigNeedleWorm self, bool eu)
    {
        orig(self, eu);

        if (self.room == null || self.enteringShortCut != null || self.impaleChunk != null || self.stuckInWallPos != null || self.lameCounter >= 1 || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        try
        {
            if (data.stunTimer > 0)
            {
                data.stunTimer -= 1;
            }
            if (data.stunCountdown > 0)
            {
                data.stunCountdown -= 1;
            }

            if (self.Stunned)
            {
                if (self.swishCounter >= 0)
                {
                    self.attackReady = Mathf.Max(0f, self.attackReady - 0.016666668f);
                    self.chargingAttack = 0f;
                }
                return;
            }

            AIUpdate();
            NeedleWormAct(self);
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AIUpdate()
        {
            BigNeedleWormAI ai = self.BigAI;

            NeedleAIUpdate(ai);

            if (ModManager.MSC && ai.worm.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.worm.LickedByPlayer.abstractCreature);
                if (ShadowOfOptions.noodle_scream.Value && ai.creature.abstractAI.followCreature != ai.worm.LickedByPlayer.abstractCreature)
                {
                    ai.worm.BigCry();
                }
            }

            if (ai.worm.room == null)
            {
                return;
            }

            if (ai.respondScreamCounter < 0)
            {
                ai.respondScreamCounter++;
                if (ShadowOfOptions.noodle_scream.Value && ai.respondScreamCounter == 0)
                {
                    ai.worm.BigCry();
                }
            }
            else if (ai.respondScreamCounter > 0)
            {
                ai.respondScreamCounter--;
                if (ShadowOfOptions.noodle_scream.Value && ai.respondScreamCounter == 0)
                {
                    ai.worm.SmallCry();
                }
            }

            ai.creature.state.socialMemory.EvenOutAllTemps(0.0005f);
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            float num = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = NeedleWormAI.Behavior.Flee;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = NeedleWormAI.Behavior.Attack;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = NeedleWormAI.Behavior.EscapeRain;
                }
            }
            if (num < 0.1f)
            {
                ai.behavior = NeedleWormAI.Behavior.Idle;
            }

            if (ai.creature.abstractAI.followCreature != null && ai.creature.abstractAI.followCreature.pos.room == ai.creature.pos.room)
            {
                if (!Custom.DistLess(ai.creature.pos, ai.creature.abstractAI.followCreature.pos, 30f) && UnityEngine.Random.value < 0.033333335f)
                {
                    ai.tracker.SeeCreature(ai.creature.abstractAI.followCreature);
                }
                else if (num < 0.8f && ai.creature.abstractAI.followCreature.pos.room != ai.creature.pos.room && ai.creature.world.GetAbstractRoom(ai.creature.abstractAI.followCreature.pos.room) != null && ai.creature.world.GetAbstractRoom(ai.creature.abstractAI.followCreature.pos.room).AttractionForCreature(ai.creature) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                {
                    ai.behavior = NeedleWormAI.Behavior.Migrate;
                }
            }

            ai.attackCounter--;

            if (UnityEngine.Random.value < 0.008333334f)
            {
                ai.targetChunk = UnityEngine.Random.Range(0, 100);
            }

            Vector2 vector = ai.attackTargetPos;
            ai.targetVel *= 0.99f;

            if (ai.behavior == NeedleWormAI.Behavior.Flee)
            {
                if (ai.threatTracker.mostThreateningCreature != null)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                }
            }

            ai.attackTargetPos = Vector2.Lerp(ai.attackTargetPos, vector + ai.targetVel * Custom.LerpMap(Vector2.Distance(ai.attackFromPos, vector), 80f, 400f, 0f, 30f, 0.35f), Mathf.InverseLerp(0.9f, 0.5f, ai.worm.chargingAttack));
            if (ai.worm.room.abstractRoom.creatures.Count > 0)
            {
                AbstractCreature abstractCreature = ai.worm.room.abstractRoom.creatures[UnityEngine.Random.Range(0, ai.worm.room.abstractRoom.creatures.Count)];
                if (abstractCreature.realizedCreature != null && !abstractCreature.state.dead && (abstractCreature.rippleLayer == ai.worm.abstractCreature.rippleLayer || abstractCreature.rippleBothSides || ai.worm.abstractCreature.rippleBothSides) && abstractCreature.realizedCreature.room == ai.worm.room && ai.creature.state.socialMemory.GetTempLike(abstractCreature.ID) < -0.25f && ai.tracker.RepresentationForCreature(abstractCreature, false) != null && ai.tracker.RepresentationForCreature(abstractCreature, false).TicksSinceSeen > 80)
                {
                    ai.tracker.SeeCreature(abstractCreature);
                }
            }
            ai.attackCounter = Custom.IntClamp(ai.attackCounter, 0, 100);
        }
    }
    #endregion

    #region BigNeedleWormAI
    static void BigNeedleWormAIBigRespondCry(On.BigNeedleWormAI.orig_BigRespondCry orig, BigNeedleWormAI self)
    {
        orig(self);

        if (!inconstorage.TryGetValue(self.worm.abstractCreature, out InconData data))
        {
            return;
        }

        data.returnToDen = false;
    }
    static void BigNeedleWormAIUpdate(On.BigNeedleWormAI.orig_Update orig, BigNeedleWormAI self)
    {
        orig(self);

        if (!ShadowOfOptions.noodle_rescue.Value || !inconstorage.TryGetValue(self.worm.abstractCreature, out InconData data))
        {
            return;
        }

        try
        {
            if (UnityEngine.Random.value < 0.0125f && (self.behavior == null || self.behavior == NeedleWormAI.Behavior.Idle))
            {
                if (data.rescueCandidate == null)
                {
                    List<AbstractCreature> list = new(self.worm.abstractCreature.Room.creatures);
                    foreach (AbstractCreature creature in list)
                    {
                        if (creature.realizedCreature == null || creature.realizedCreature is not SmallNeedleWorm noodle || !ValidNeedlCheck(noodle))
                        {
                            continue;
                        }

                        data.rescueCandidate = creature.realizedCreature;

                        break;
                    }
                }

                if (data.rescueCandidate != null)
                {
                    self.behavior = NoodleRescueIncon;
                }
            }

            if (self.behavior == NoodleRescueIncon)
            {
                if (data.rescueCandidate == null || !ValidNeedlCheck(data.rescueCandidate as SmallNeedleWorm) && ((data.rescueCandidate as SmallNeedleWorm).grasps[0] == null || (data.rescueCandidate as SmallNeedleWorm).grasps[0].grabbedChunk.owner != self.worm))
                {
                    data.rescueCandidate = null;
                    self.behavior = NeedleWormAI.Behavior.Idle;
                    return;
                }

                SmallNeedleWorm noodle = data.rescueCandidate as SmallNeedleWorm;

                if (noodle.grasps[0] != null && noodle.grasps[0].grabbedChunk.owner != self.worm || noodle.grasps[0] == null)
                {
                    self.creature.abstractAI.SetDestination(noodle.coord);
                }
                else if (noodle.grasps[0] != null && noodle.grasps[0].grabbedChunk.owner == self.worm)
                {
                    if (self.denFinder.GetDenPosition() != null)
                    {
                        self.creature.abstractAI.SetDestination(self.denFinder.GetDenPosition().Value);
                        //noodle.abstractCreature.abstractAI.SetDestination(self.denFinder.GetDenPosition().Value);
                    }
                }
            }
            else if (data.rescueCandidate != null)
            {
                data.rescueCandidate = null;
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        #region Local
        bool ValidNeedlCheck(SmallNeedleWorm noodle)
        {
            if (!noodle.dead || noodle.Mother != self.worm || noodle.grabbedBy.Count != 0 || (noodle.grasps[0] != null && noodle.grasps[0].grabbedChunk.owner != self.worm))
            {
                return false;
            }

            List<PhysicalObject>[] list = self.worm.abstractCreature.Room.realizedRoom.physicalObjects;

            foreach (PhysicalObject obj in list[2])
            {
                if (obj is not Spear spear)
                {
                    continue;
                }

                foreach (AbstractPhysicalObject.AbstractObjectStick stick in spear.abstractPhysicalObject.stuckObjects)
                {
                    if (stick.B == noodle.abstractCreature)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion
    }
    #endregion

    #region NeedleWormAI
    static void NeedleWormAIUpdate(On.NeedleWormAI.orig_Update orig, NeedleWormAI self)
    {
        orig(self);

        MiscHooks.ReturnToDenUpdate(self);
    }
    static bool NeedleWormAIWantToStayInDenUntilEndOfCycle(On.NeedleWormAI.orig_WantToStayInDenUntilEndOfCycle orig, NeedleWormAI self)
    {
        return MiscHooks.ReturnToDenWantToStayInDenUntilEndOfCycle(self) || IsComa(self.worm) || orig(self);
    }

    public static void NeedleAIUpdate(NeedleWormAI self)
    {
        MiscHooks.AIUpdate(self);

        if (self.addOldIdlePosDelay > 0)
        {
            self.addOldIdlePosDelay--;
        }

        self.inRoomCounter++;

        if (self.focusCreature != null && self.focusCreature.TicksSinceSeen > 40)
        {
            self.focusCreature = null;
        }

        if (self.worm.room.aimap.getAItile(self.creature.pos).narrowSpace)
        {
            self.inFreeSpaceCounter -= 3;
        }
        else if (self.worm.room.aimap.getTerrainProximity(self.creature.pos) < 3)
        {
            self.inFreeSpaceCounter--;
        }
        else if (self.worm.room.aimap.getTerrainProximity(self.creature.pos) > 5)
        {
            self.inFreeSpaceCounter++;
        }

        self.inFreeSpaceCounter = Custom.IntClamp(self.inFreeSpaceCounter, 0, 100);
        if (self.behavior == NeedleWormAI.Behavior.Idle)
        {
            self.flySpeed = Custom.LerpAndTick(self.flySpeed, (self.creature.abstractAI.followCreature != null && self.creature.abstractAI.destination.room != self.creature.pos.room) ? 0.5f : 0f, 0.06f, 0.016666668f);
        }
        else
        {
            self.flySpeed = Custom.LerpAndTick(self.flySpeed, 1f, 0.06f, 0.016666668f);
        }

        if (self.behavior == NeedleWormAI.Behavior.Flee)
        {
            //self.flyHeightAdd = Mathf.Min(1f, self.flyHeightAdd + 0.016666668f);
            return;
        }

        //self.flyHeightAdd = Mathf.Max(0f, self.flyHeightAdd - 0.0045454544f);
    }
    #endregion

    #region SmallNeedleWormAI
    static void SmallNeedleWormUpdate(On.SmallNeedleWorm.orig_Update orig, SmallNeedleWorm self, bool eu)
    {
        orig(self, eu);

        if (self.room == null || self.enteringShortCut != null || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self))
        {
            return;
        }

        try
        {
            if (self.grasps != null && self.grasps[0] != null && self.grasps[0].grabbed != null && self.grasps[0].grabbed is BigNeedleWorm mom && mom.enteringShortCut.HasValue && mom.room != null && mom.room.shortcutData(mom.enteringShortCut.Value).shortCutType != null && mom.room.shortcutData(mom.enteringShortCut.Value).shortCutType == ShortcutData.Type.CreatureHole)
            {
                self.enteringShortCut = mom.enteringShortCut;
                self.LoseAllGrasps();
            }

            if (!IsInconBase(self))
            {
                return;
            }

            if (data.stunTimer > 0)
            {
                data.stunTimer -= 1;
            }
            if (data.stunCountdown > 0)
            {
                data.stunCountdown -= 1;
            }

            if (self.Stunned)
            {
                return;
            }
            if (ShadowOfOptions.noodle_scream.Value && ModManager.MSC && self.LickedByPlayer != null)
            {
                Scream();
            }

            AIUpdate();
            NeedleWormAct(self);

            bool flag = UnityEngine.Random.value < 0.05882353f;
            if (flag && self.grasps[0] == null && self.MotherAttachable && self.Mother.NormalFlyingState)
            {
                for (int i = self.Mother.tail.GetLength(0) - 2; i >= 0; i--)
                {
                    flag = UnityEngine.Random.value < Mathf.InverseLerp(0f, (float)(self.Mother.tail.GetLength(0) - 2), (float)i);

                    if (flag && Custom.DistLess(self.mainBodyChunk.pos, self.Mother.tail[i, 0], 25f))
                    {
                        bool flag2 = true;
                        for (int j = 0; j < self.Mother.grabbedBy.Count; j++)
                        {
                            if (self.Mother.grabbedBy[j].grabber is SmallNeedleWorm && (self.Mother.grabbedBy[j].grabber as SmallNeedleWorm).momTailSegment == i)
                            {
                                flag2 = false;
                                break;
                            }
                        }
                        if (flag2)
                        {
                            self.momTailSegment = i;
                            self.Grab(self.Mother, 0, self.Mother.bodyChunks.Length - 1, Creature.Grasp.Shareability.NonExclusive, 0f, false, false);
                            return;
                        }
                    }
                }
            }

        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AIUpdate()
        {
            SmallNeedleWormAI ai = self.SmallAI;

            NeedleAIUpdate(ai);

            if (ai.worm.room == null)
            {
                return;
            }

            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            float num = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = NeedleWormAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = NeedleWormAI.Behavior.EscapeRain;
                }
                else if (aimodule is StuckTracker && !ai.worm.OffscreenSuperSpeed)
                {
                    ai.behavior = NeedleWormAI.Behavior.GetUnstuck;
                }
            }
            if (num < 0.1f)
            {
                ai.behavior = NeedleWormAI.Behavior.Idle;
            }

            if (ai.behavior == NeedleWormAI.Behavior.Idle)
            {
                if (ai.Mother != null && (!ModManager.MSC || !ai.creature.controlled))
                {
                    if (ai.Mother.pos.room != ai.creature.pos.room || ai.Mother.pos.Tile.FloatDist(ai.creature.pos.Tile) > 5f)
                    {
                        ai.flySpeed = 1f;
                    }
                    if (ShadowOfOptions.noodle_scream.Value && UnityEngine.Random.value < 0.003125f && ai.worm.screaming == 0f && ai.tracker.RepresentationForCreature(ai.Mother, false) != null && ai.tracker.RepresentationForCreature(ai.Mother, false).TicksSinceSeen > UnityEngine.Random.Range(80, 300))
                    {
                        ai.worm.SmallScream(true);
                        return;
                    }
                }
            }
            else if (ai.behavior == NeedleWormAI.Behavior.Flee)
            {
                if (ai.threatTracker.mostThreateningCreature != null)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                    return;
                }
            }
        }

        void Scream()
        {
            if (self.hasScreamed)
            {
                return;
            }

            self.screaming = 1f;
            self.room.PlaySound(SoundID.Small_Needle_Worm_Intense_Trumpet_Scream, self.mainBodyChunk, false, 1f, 1f);
            self.hasScreamed = true;
        }
    }
    #endregion

    public static void NeedleWormAct(NeedleWorm self)
    {
        self.flyingThisFrame = false;
        self.extraMovementForce = Custom.LerpAndTick(self.extraMovementForce, 0.25f * self.brokenLineOfSight + 0.25f * self.segmentsStuckOnTerrain + 0.5f * self.stuckAtSamePos, 0.07f, 0.025f);
    }
}