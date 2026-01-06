using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.VultureHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Vulture.Update += VultureUpdate;

        On.Vulture.VultureThruster.Utility += VultureThrusterUtility;

        On.VultureGraphics.Update += VultureGraphicsUpdate;

        On.VultureGraphics.DrawSprites += VultureGraphicsDrawSprites;
    }

    static void VultureGraphicsDrawSprites(On.VultureGraphics.orig_DrawSprites orig, VultureGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[self.BodySprite].scale = (self.IsKing ? 1.2f : 1f) * MiscHooks.ApplyBreath(data, timeStacker);
    }

    static void VultureGraphicsUpdate(On.VultureGraphics.orig_Update orig, VultureGraphics self)
    {
        orig(self);

        if (BreathCheck(self.vulture))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    static float VultureThrusterUtility(On.Vulture.VultureThruster.orig_Utility orig, Vulture.VultureThruster self)
    {
        return IsComa(self.vulture) ? 0 : orig(self);
    }

    static void VultureUpdate(On.Vulture.orig_Update orig, Vulture self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
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

        AIUpdate();

        for (int i = 0; i < self.tentacles.Length; i++)
        {
            self.tentacles[i].SwitchMode(VultureTentacle.Mode.Climb);
        }

        if (self.IsMiros)
        {
            self.lastJawOpen = self.jawOpen;
            if (self.grasps[0] != null)
            {
                self.jawOpen = 0.15f;
            }
            else if (self.jawSlamPause > 0)
            {
                self.jawSlamPause--;
            }
            else
            {
                if (self.isLaserActive())
                {
                    self.jawKeepOpenPause = 10;
                    self.keepJawOpenPos = 1f;
                }
                if (self.jawVel == 0f)
                {
                    self.jawVel = 0.15f;
                }
                if (self.abstractCreature.controlled && self.jawVel >= 0f && self.jawVel < 1f && !self.controlledJawSnap)
                {
                    self.jawVel = 0f;
                    self.jawOpen = 0f;
                }
                self.jawOpen += self.jawVel;
                if (self.jawKeepOpenPause > 0)
                {
                    self.jawKeepOpenPause--;
                    self.jawOpen = Mathf.Clamp(Mathf.Lerp(self.jawOpen, self.keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
                }
                else if (UnityEngine.Random.value < 1f / ((!self.Blinded) ? 40f : 15f) && !self.abstractCreature.controlled)
                {
                    self.jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
                    self.keepJawOpenPos = ((UnityEngine.Random.value >= 0.5f) ? 1f : 0f);
                    self.jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
                    self.jawOpen = Mathf.Clamp(self.jawOpen, 0f, 1f);
                }
                else if (self.jawOpen <= 0f)
                {
                    self.jawOpen = 0f;
                    if (self.jawVel < -0.4f)
                    {
                        if(ShadowOfOptions.vul_attack.Value)
                            JawSlamShut();
                        self.controlledJawSnap = false;
                    }
                    self.jawVel = 0.15f;
                    self.jawSlamPause = 5;
                }
                else if (self.jawOpen >= 1f)
                {
                    self.jawOpen = 1f;
                    self.jawVel = -0.5f;
                }
            }
        }

        float num2 = 0f;
        for (int j = 0; j < self.tentacles.Length; j++)
        {
            num2 += self.tentacles[j].Support() * (self.IsMiros ? 0.75f : 0.5f);
        }
        num2 = Mathf.Pow(num2, 0.5f);
        num2 = Mathf.Max(num2, 0.1f);
        self.hoverStill = false;
        IntVector2 intVector = self.room.GetTilePosition(self.mainBodyChunk.pos);
        for (int k = 0; k < 5; k++)
        {
            if (self.room.aimap.TileAccessibleToCreature(intVector + Custom.fourDirectionsAndZero[k], self.Template))
            {
                intVector += Custom.fourDirectionsAndZero[k];
            }
        }

        if (self.room == null)
        {
            return;
        }
        MovementConnection movementConnection = (self.AI.pathFinder as VulturePather).FollowPath(self.room.GetWorldCoordinate(intVector), true);
        VultureTentacle.Mode a = VultureTentacle.Mode.Climb;

        self.neck.retractFac = Mathf.Clamp(self.neck.retractFac + 0.033333335f, 0f, 0.6f);
        self.bodyChunks[4].vel *= 0.9f;
        for (int num3 = 0; num3 < 4; num3++)
        {
            self.bodyChunks[num3].vel *= Mathf.Lerp(0.98f, 0.9f, num2);
            if (num2 > 0.1f)
            {
                self.bodyChunks[num3].vel.y += Mathf.Lerp(1.2f, 0.5f, num2);
            }
        }
        self.bodyChunks[1].vel.y += 1.9f * num2 * Mathf.InverseLerp(1f, 7f, self.mainBodyChunk.vel.magnitude);
        self.bodyChunks[0].vel.y -= 1.9f * num2 * Mathf.InverseLerp(1f, 7f, self.mainBodyChunk.vel.magnitude);

        if (self.room == null)
        {
            return;
        }
        for (int num5 = 0; num5 < 5; num5++)
        {
            if (self.room.GetTile(self.abstractCreature.pos.Tile + Custom.fourDirectionsAndZero[num5]).wormGrass)
            {
                self.mainBodyChunk.vel -= Custom.fourDirectionsAndZero[num5].ToVector2() * 2f + Custom.RNV() * 6f + new Vector2(0f, 6f);
            }
        }
        if (!self.hoverStill)
        {
            bool flag3 = true;
            for (int num6 = 0; num6 < self.tentacles.Length; num6++)
            {
                flag3 = (flag3 && (self.tentacles[num6].hasAnyGrip || self.tentacles[num6].mode != VultureTentacle.Mode.Climb));
            }

            if (ShadowOfOptions.vul_grab.Value && self.hangingInTentacle && flag3)
            {
                self.releaseGrippingTentacle++;
                if (self.releaseGrippingTentacle > 5 && self.CheckTentacleModeAnd(VultureTentacle.Mode.Climb))
                {
                    self.tentacles[self.TentacleMaxReleaseInd()].ReleaseGrip();
                }
                else if (self.releaseGrippingTentacle > 50)
                {
                    self.tentacles[self.TentacleMaxReleaseInd()].ReleaseGrip();
                }
            }
            else
            {
                self.releaseGrippingTentacle = 0;
            }
            bool flag4 = true;
            for (int num7 = 0; num7 < self.tentacles.Length; num7++)
            {
                flag4 = (flag4 && self.tentacles[num7].WingSpace());
            }
            if (!self.safariControlled && self.IsMiros && self.isLaserActive() && self.CheckTentacleModeOr(VultureTentacle.Mode.Climb))
            {
                self.dontSwitchModesCounter = 200;
            }
            self.timeSinceLastTakeoff++;
            if (self.dontSwitchModesCounter > 0)
            {
                self.dontSwitchModesCounter--;
            }
            else if (self.IsMiros)
            {
                if (!self.hoverStill && self.room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && self.CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && self.mainBodyChunk.vel.y > 4f && self.moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(self.room, self.room.GetTilePosition(self.mainBodyChunk.pos), self.room.GetTilePosition(self.mainBodyChunk.pos + self.moveDirection * 400f)) && flag4)
                {
                    self.dontSwitchModesCounter = 200;
                }
                else if (!self.hoverStill && self.room.aimap.getTerrainProximity(movementConnection.DestTile) > 4 && self.CheckTentacleModeOr(VultureTentacle.Mode.Climb) && (!self.safariControlled || a == VultureTentacle.Mode.Fly))
                {
                    self.dontSwitchModesCounter = 200;
                }
                else if (self.room.aimap.getTerrainProximity(movementConnection.DestTile) <= (self.IsMiros ? 4 : 8) && self.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && self.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && self.CheckTentacleModeAnd(VultureTentacle.Mode.Fly) && (!self.safariControlled || a == VultureTentacle.Mode.Climb))
                {
                    InconAct(data);
                    self.AirBrake(30);
                    self.dontSwitchModesCounter = 200;
                }
            }
            else if (self.room.aimap.getTerrainProximity(movementConnection.DestTile) <= (self.IsMiros ? 4 : 8) && self.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && self.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && self.CheckTentacleModeAnd(VultureTentacle.Mode.Fly) && (!self.safariControlled || a == VultureTentacle.Mode.Climb))
            {
                InconAct(data);
                self.AirBrake(30);
                self.dontSwitchModesCounter = 200;
            }
            else if (!self.hoverStill && self.room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && self.CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && self.mainBodyChunk.vel.y > 4f && self.moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(self.room, self.room.GetTilePosition(self.mainBodyChunk.pos), self.room.GetTilePosition(self.mainBodyChunk.pos + self.moveDirection * 400f)) && flag4 && (!self.safariControlled || a == VultureTentacle.Mode.Fly))
            {
                self.dontSwitchModesCounter = 200;
            }
        }
        bool flag5 = true;
        for (int num12 = 0; num12 < self.tentacles.Length; num12++)
        {
            flag5 = (flag5 && !self.tentacles[num12].hasAnyGrip);
        }
        if (self.mainBodyChunk.vel.y < -10f && self.CheckTentacleModeAnd(VultureTentacle.Mode.Climb) && flag5 && self.landingBrake < 1 && (!self.safariControlled || a == VultureTentacle.Mode.Fly))
        {
            if (self.graphicsModule != null)
            {
                (self.graphicsModule as VultureGraphics).MakeColorWave(UnityEngine.Random.Range(10, 20));
            }
        }
        if (self.snapFrames == 0)
        {
            if (self.AI.preyInTuskChargeRange)
            {
                self.tuskCharge = Mathf.Clamp(self.tuskCharge + 0.005f, 0f, 1f);
            }
            else
            {
                self.tuskCharge = Mathf.Clamp(self.tuskCharge - 0.000111111f, 0f, 1f);
            }
        }
        else
        {
            Vector2 pos = self.snapAtPos;
            if (self.snapAt != null)
            {
                pos = self.snapAt.pos;
            }
            if (self.Snapping)
            {
                if(ShadowOfOptions.vul_attack_move.Value)
                    self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, pos) * 1f;
            }
            else if (self.ChargingSnap)
            {
                if (ShadowOfOptions.vul_attack_move.Value)
                    self.bodyChunks[1].vel -= Custom.DirVec(self.bodyChunks[1].pos, pos) * 0.5f;

                for (int num24 = 0; num24 < 4; num24++)
                {
                    if (ShadowOfOptions.vul_attack_move.Value)
                        self.bodyChunks[num24].vel *= Mathf.Lerp(1f, 0.2f, num2);
                }
            }
            self.snapFrames--;
        }
        self.lastHoverStill = self.hoverStill;
        if (movementConnection != default(MovementConnection))
        {
            self.lastConnection = movementConnection;
        }

        void AIUpdate()
        {
            VultureAI ai = self.AI;

            if (ai.behavior == VultureAI.Behavior.Hunt && !RainWorldGame.RequestHeavyAi(ai.vulture))
            {
                return;
            }
            if (ModManager.MSC && ai.vulture.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.vulture.LickedByPlayer.abstractCreature);
            }
            ai.creatureLooker?.Update();

            ai.disencouraged = Mathf.Max(0f, ai.disencouraged - 1f / Mathf.Lerp(600f, 4800f, ai.disencouraged));
            ai.preyInTuskChargeRange = false;
            ai.behavior = VultureAI.Behavior.Idle;
            ai.utilityComparer.GetUtilityTracker(ai.preyTracker).weight = 0.05f + 0.95f * Mathf.InverseLerp(ai.IsMiros ? 4000f : 9600f, ai.IsMiros ? 7600f : 6000f, (float)ai.timeInRoom);
            if (ai.IsMiros)
            {
                ai.utilityComparer.GetUtilityTracker(ai.disencouragedTracker).weight += Mathf.InverseLerp(2000f, 13600f, (float)ai.timeInRoom);
            }
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            if (ai.utilityComparer.HighestUtility() > 0.01f && aimodule != null)
            {
                if (aimodule is PreyTracker)
                {
                    ai.behavior = VultureAI.Behavior.Hunt;
                }
                if (aimodule is StuckTracker)
                {
                    ai.behavior = VultureAI.Behavior.GetUnstuck;
                }
                if (aimodule is VultureAI.DisencouragedTracker)
                {
                    ai.behavior = VultureAI.Behavior.Disencouraged;
                }
            }
            if (ai.vulture.grasps[0] != null && ai.vulture.grasps[0].grabbed is Creature && ai.vulture.Template.CreatureRelationship(ai.vulture.grasps[0].grabbed as Creature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                ai.behavior = VultureAI.Behavior.Idle;
            }
            if (!ai.IsMiros && (ai.creature.abstractAI as VultureAbstractAI).lostMask != null && ai.utilityComparer.HighestUtility() < 0.4f && (ai.creature.abstractAI as VultureAbstractAI).lostMask.Room.realizedRoom == ai.vulture.room && (ai.creature.abstractAI as VultureAbstractAI).lostMask.realizedObject != null)
            {
                ai.behavior = VultureAI.Behavior.GoToMask;
                WorldCoordinate worldCoordinate = ai.vulture.room.GetWorldCoordinate((ai.creature.abstractAI as VultureAbstractAI).lostMask.realizedObject.firstChunk.pos);
                if (ai.creature.world.GetAbstractRoom(worldCoordinate.room).AttractionForCreature(ai.creature) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                {
                    ai.SetDestination(worldCoordinate);
                }
            }
            if (!(ai.behavior == VultureAI.Behavior.GoToMask))
            {
                if (ai.behavior == VultureAI.Behavior.ReturnPrey || ai.behavior == VultureAI.Behavior.EscapeRain || ai.behavior == VultureAI.Behavior.Disencouraged)
                {
                    ai.focusCreature = null;
                }
                else if (ai.behavior == VultureAI.Behavior.Hunt)
                {
                    ai.focusCreature = ai.preyTracker.MostAttractivePrey;

                    if (ShadowOfOptions.vul_attack.Value && ai.focusCreature.VisualContact)
                    {
                        Creature realizedCreature = ai.focusCreature.representedCreature.realizedCreature;
                        if (realizedCreature.bodyChunks.Length != 0)
                        {
                            BodyChunk bodyChunk = realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)];
                            ai.preyInTuskChargeRange = Custom.DistLess(ai.vulture.mainBodyChunk.pos, bodyChunk.pos, 230f);

                            if ((!self.AirBorne || UnityEngine.Random.value < 0.016666668f) && self.tuskCharge == 1f && self.snapFrames == 0 && !self.isLaserActive() && !self.safariControlled && Custom.DistLess(self.mainBodyChunk.pos, bodyChunk.pos, 130f) && self.room.VisualContact(self.bodyChunks[4].pos, bodyChunk.pos))
                            {
                                //InconAct(data);
                                self.Snap(bodyChunk);
                            }
                        }
                    }
                }
            }

            MiscHooks.AIUpdate(ai);
        }

        void JawSlamShut()
        {
            Vector2 vector = Custom.DirVec(self.neck.Tip.pos, self.Head().pos);
            self.neck.Tip.vel -= vector * 10f;
            self.neck.Tip.pos += vector * 20f;
            self.Head().pos += vector * 20f;

            self.room.PlaySound(SoundID.Miros_Beak_Snap_Miss, self.Head());
        }
    }
}