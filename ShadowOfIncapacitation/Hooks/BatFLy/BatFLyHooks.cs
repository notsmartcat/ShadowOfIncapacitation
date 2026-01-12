using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.BatFlyHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Fly.Update += FlyUpdate;

        On.FlyAI.Update += FlyAIUpdate;

        On.FlyGraphics.DrawSprites += FlyGraphicsDrawSprites;
        On.FlyGraphics.Update += FlyGraphicsUpdate;

        On.FliesRoomAI.MoveFlyToHive += FliesRoomAIMoveFlyToHive;
    }

    #region Fly
    static void FlyUpdate(On.Fly.orig_Update orig, Fly self, bool eu)
    {
        orig(self, eu);

        if (IsComa(self))
        {
            self.drown = Mathf.Clamp(self.drown + 0.0125f * ((self.mainBodyChunk.submersion == 1f) ? 1f : -1f), 0f, 1f);
            if (self.drown == 1f)
            {
                ActuallyKill(self);
            }
        }

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
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

            if (self.inShortcut)
            {
                self.flapDepth = 1f;
                return;
            }

            if (self.mainBodyChunk.submersion == 0f)
            {
                Act();
            }
            else if (IsInconBase(self))
            {
                WaterBehavior();
            }

            if (self.room == null)
            {
                return;
            }

            if (self.shortcutDelay == 0 && self.room.GetTile(self.bodyChunks[0].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && self.room.shortcutData(self.room.GetTilePosition(self.bodyChunks[0].pos)).shortCutType != ShortcutData.Type.DeadEnd)
            {
                self.enteringShortCut = new IntVector2?(self.room.GetTilePosition(self.bodyChunks[0].pos));
                self.LoseAllGrasps();
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        #region Local
        void Act()
        {
            AIUpdate();

            if (self.movMode == Fly.MovementMode.Burrow)
            {
                if (self.burrowOrHangSpot != null)
                {
                    self.mainBodyChunk.pos = self.burrowOrHangSpot.Value + new Vector2(Mathf.Lerp(-2f, 2f, UnityEngine.Random.value), 0f);
                    self.burrowOrHangSpot = new Vector2?(self.burrowOrHangSpot.Value + new Vector2(0f, -0.4f));
                    if (self.room.GetTile(self.burrowOrHangSpot.Value + new Vector2(0f, 5f)).Terrain == Room.Tile.TerrainType.Solid)
                    {
                        self.Burrowed();
                    }
                    return;
                }
                self.burrowOrHangSpot = null;
                return;
            }

        }

        void AIUpdate()
        {
            FlyAI ai = self.AI;

            if (ModManager.MMF && FlyAI.RoomNotACycleHazard(ai.room))
            {
                ai.fleeFromRain = false;
            }
            if (IsInconBase(self) && (ai.fleeFromRain || ai.afraid >= 1f || self.grabbedBy.Count != 0))
            {
                InconAct(data);
            }

            if (ai.fly.grasps[0] != null && IsInconBase(self))
            {
                ai.behavior = FlyAI.Behavior.Chain;
            }

            if (ai.dropStatus == FlyAI.DropStatus.Dropping)
            {
                ai.ChangeBehavior(FlyAI.Behavior.Drop);
            }

            if (ai.behavior == FlyAI.Behavior.Drop)
            {
                ai.fly.movMode = Fly.MovementMode.Passive;
                ai.DropUpdate();
            }
            else if (ai.behavior == FlyAI.Behavior.Burrow)
            {
                ai.fly.movMode = Fly.MovementMode.Burrow;
                if (ai.fly.burrowOrHangSpot == null || !Custom.DistLess(ai.FlyPos, ai.fly.burrowOrHangSpot.Value, 100f))
                {
                    ai.fly.burrowOrHangSpot = null;
                }
            }
            else if (ai.behavior == FlyAI.Behavior.Chain)
            {
                if ((ai.fly.burrowOrHangSpot != null && Custom.DistLess(ai.FlyPos, ai.fly.burrowOrHangSpot.Value, 40f)) || ai.fly.grasps[0] != null)
                {
                    ai.fly.movMode = Fly.MovementMode.Hang;
                    ai.HangInChainUpdate();
                }
            }

            if (ai.room.GetTile(ai.FlyPos).hive)
            {
                ai.fly.mainBodyChunk.vel.y--;
            }

            bool flag = ai.fly.safariControlled && ai.fly.inputWithDiagonals != null && ai.fly.inputWithDiagonals.Value.pckp;
            bool flag2 = !ModManager.MSC || !ai.room.world.game.IsArenaSession || ai.room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta == null || !ai.room.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.fly_no_burrow;
            if (ai.fly.mainBodyChunk.ContactPoint.y == -1 && (ai.afraid >= 0.75f || flag) && ai.room.GetTile(ai.FlyPos).hive && flag2)
            {
                ai.ChangeBehavior(FlyAI.Behavior.Burrow);
                ai.fly.burrowOrHangSpot = new Vector2?(ai.FlyPos);
            }

            ai.UpdateThreats();

            ai.afraid = Mathf.Clamp(ai.afraid - 0.003125f, 0f, 1f);
        }

        void WaterBehavior()
        {
            data.stunTimer = 0;
            data.stunCountdown = 2;

            self.flap = Mathf.Clamp(self.flap + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f, 0f, 1f);

            if (self.graphicsModule != null)
            {
                FlyGraphics graphics = self.graphicsModule as FlyGraphics;

                graphics.lastHorizontalSteeringCompensation = 0f;
                graphics.horizontalSteeringCompensation = 0f;

                for (int i = 0; i < 2; i++)
                {
                    graphics.wings[i, 1] = graphics.wings[i, 0];
                    graphics.wings[i, 0] = graphics.fly.flap;
                }
            }

            if (self.mainBodyChunk.submersion == 1f)
            {
                self.flap = Mathf.Clamp(self.flap + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f, 0f, 1f);
                self.mainBodyChunk.vel += Custom.DegToVec(-45f + 90f * UnityEngine.Random.value) * 0.75f;
                return;
            }
        }
        #endregion
    }
    #endregion

    #region FlyAI
    static void FlyAIUpdate(On.FlyAI.orig_Update orig, FlyAI self)
    {
        orig(self);

        if (!ShadowOfOptions.bat_rescue.Value || !inconstorage.TryGetValue(self.fly.abstractCreature, out InconData data))
        {
            return;
        }

        try
        {
            if (data.rescueCandidate == null || (data.rescueCandidate as Fly).grasps[0] == null || (data.rescueCandidate as Fly).grasps[0].grabbedChunk.owner != self.fly)
            {
                if (data.stunTimer > 0)
                {
                    data.stunTimer -= 1;
                }
                if (data.stunCountdown > 0)
                {
                    data.stunCountdown -= 1;
                }
            }

            if (UnityEngine.Random.value < 0.005f && (self.behavior == null || self.behavior == FlyAI.Behavior.Idle || self.behavior == FlyAI.Behavior.Chain || self.behavior == FlyAI.Behavior.Swarm))
            {
                if (data.rescueCandidate == null)
                {
                    List<AbstractCreature> list = new(self.fly.abstractCreature.Room.creatures);
                    foreach (AbstractCreature creature in list)
                    {
                        if (creature.realizedCreature == null || creature.realizedCreature is not Fly fly || !ValidFLyCheck(fly))
                        {
                            continue;
                        }

                        data.rescueCandidate = creature.realizedCreature;

                        break;
                    }
                }

                if (data.rescueCandidate != null)
                {
                    InconAct();

                    self.ChangeBehavior(FlyRescueIncon);
                }
            }

            if (self.behavior == FlyRescueIncon)
            {
                if (data.rescueCandidate == null || !ValidFLyCheck(data.rescueCandidate as Fly) || data.stunTimer <= 0 && ((data.rescueCandidate as Fly).grasps[0] == null || (data.rescueCandidate as Fly).grasps[0].grabbedChunk.owner != self.fly))
                {
                    data.stunTimer = 0;
                    data.stunCountdown = 0;

                    data.rescueCandidate = null;
                    self.ChangeBehavior(FlyAI.Behavior.Idle);
                    return;
                }

                Fly fly = data.rescueCandidate as Fly;

                self.GenericFlightUpdate();

                if (fly.grasps[0] != null && fly.grasps[0].grabbedChunk.owner != self.fly || fly.grasps[0] == null)
                {
                    if (!Custom.DistLess(data.rescueCandidate.mainBodyChunk.pos, self.fly.mainBodyChunk.pos, 20f))
                    {
                        self.localGoal = data.rescueCandidate.mainBodyChunk.pos;
                        self.ProgressLocalGoalAlongDijkstraMap(self.localGoal, 1);
                        return;
                    }

                    fly.Grab(self.fly, 0, 0, Creature.Grasp.Shareability.NonExclusive, 10f, false, false);
                    self.room.PlaySound(SoundID.Bat_Attatch_To_Chain, fly.mainBodyChunk);

                    for (; ; )
                    {
                        if (self.fly.graphicsModule != null)
                        {
                            self.fly.graphicsModule.BringSpritesToFront();
                        }
                        if (self.fly.grasps[0] == null || self.fly.grasps[0].grabbed is not Fly)
                        {
                            break;
                        }
                    }
                }
                else if (fly.grasps[0] != null && fly.grasps[0].grabbedChunk.owner == self.fly)
                {
                    int num4 = 0;
                    IntVector2 tilePosition = self.room.GetTilePosition(self.FlyPos);
                    int num5 = tilePosition.y;
                    while (num5 < self.room.TileHeight && self.room.GetTile(tilePosition.x, num5).Terrain != Room.Tile.TerrainType.Solid)
                    {
                        num4++;
                        num5++;
                    }
                    if (num4 >= 3)
                    {
                        Vector2 vector = Custom.DirVec(self.FlyPos, self.localGoal);
                        vector = Vector3.Slerp(vector, new Vector2(0f, 1f), 0.2f);
                        self.localGoal = self.FlyPos + vector * self.localGoal.magnitude;
                    }

                    int num6 = self.fly.abstractCreature.pos.y;
                    while (num6 > self.fly.abstractCreature.pos.y - 10 && self.room.GetTile(self.fly.abstractCreature.pos.x, num6).Terrain != Room.Tile.TerrainType.Solid)
                    {
                        if (self.room.GetTile(self.fly.abstractCreature.pos.x, num6).hive)
                        {
                            (data.rescueCandidate as Fly).AI.localGoal = self.ProgressLocalGoalAlongDijkstraMap(self.localGoal, self.followingDijkstraMap);

                            (data.rescueCandidate as Fly).AI.ChangeBehavior(FlyAI.Behavior.Drop);

                            data.rescueCandidate.grasps[0] = null;

                            self.dropStatus = FlyAI.DropStatus.Dropping;
                        }
                        num6--;
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
        bool ValidFLyCheck(Fly fly)
        {
            if (!fly.dead || fly.grabbedBy.Count != 0 || (fly.grasps[0] != null && fly.grasps[0].grabbedChunk.owner != self.fly) || self.room.PointSubmerged(fly.firstChunk.pos, -40f) || fly.firstChunk.pos.y <= self.room.FloatWaterLevel(fly.firstChunk.pos) + 40f)
            {
                return false;
            }

            List<PhysicalObject>[] list = self.fly.abstractCreature.Room.realizedRoom.physicalObjects;

            foreach (PhysicalObject obj in list[2])
            {
                if (obj is not Spear spear)
                {
                    continue;
                }

                foreach (AbstractPhysicalObject.AbstractObjectStick stick in spear.abstractPhysicalObject.stuckObjects)
                {
                    if (stick.B == fly.abstractCreature)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void InconAct()
        {
            if (data.stunCountdown > 0)
            {
                return;
            }

            data.stunTimer = UnityEngine.Random.Range(180, 241);

            data.stunCountdown = data.stunTimer + UnityEngine.Random.Range(10, 41);
        }
        #endregion
    }
    #endregion

    #region FlyGraphics
    static void FlyGraphicsDrawSprites(On.FlyGraphics.orig_DrawSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[0].scaleX = 1f * MiscHooks.ApplyBreath(data, timeStacker);
    }
    static void FlyGraphicsUpdate(On.FlyGraphics.orig_Update orig, FlyGraphics self)
    {
        orig(self);

        if (BreathCheck(self.fly))
        {
            MiscHooks.UpdateBreath(self);
        }

        if (!inconstorage.TryGetValue(self.fly.abstractCreature, out InconData data) || !IsInconBase(self.fly) || data.stunTimer <= 0)
        {
            return;
        }

        self.lastHorizontalSteeringCompensation = 0f;
        self.horizontalSteeringCompensation = 0f;

        for (int i = 0; i < 2; i++)
        {
            self.wings[i, 1] = self.wings[i, 0];
            self.wings[i, 0] = UnityEngine.Random.value;
        }
    }
    #endregion

    #region FlyOther
    static void FliesRoomAIMoveFlyToHive(On.FliesRoomAI.orig_MoveFlyToHive orig, FliesRoomAI self, Fly fly)
    {
        if (!IsComa(fly))
        {
            orig(self, fly);
            return;
        }

        self.flies.Remove(fly);
        fly.RemoveFromRoom();
        fly.slatedForDeletetion = true;
    }
    #endregion
}