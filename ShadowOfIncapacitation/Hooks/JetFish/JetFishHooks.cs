using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.JetFishHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.JetFish.CarryObject += JetFishCarryObject;
        On.JetFish.Collide += JetFishCollide;
        On.JetFish.Update += JetFishUpdate;

        On.JetFishAI.Update += JetFishAIUpdate;
        On.JetFishAI.WantToStayInDenUntilEndOfCycle += JetFishAIWantToStayInDenUntilEndOfCycle;

        On.JetFishGraphics.DrawSprites += JetFishGraphicsDrawSprites;
        On.JetFishGraphics.Update += JetFishGraphicsUpdate;
    }

    #region JetFish
    static void JetFishCarryObject(On.JetFish.orig_CarryObject orig, JetFish self, bool eu)
    {
        if (!IsIncon(self))
        {
            orig(self, eu);
            return;
        }

        if (self.Submersion < 0.1f && UnityEngine.Random.value < 0.025f || !Custom.DistLess(self.mainBodyChunk.pos, self.grasps[0].grabbedChunk.pos, 100f))
        {
            self.LoseAllGrasps();
            return;
        }

        self.grasps[0].grabbedChunk.MoveFromOutsideMyUpdate(eu, self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * 10f);
        self.grasps[0].grabbedChunk.vel = self.mainBodyChunk.vel;
    }
    static void JetFishCollide(On.JetFish.orig_Collide orig, JetFish self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);

        if (!IsIncon(self))
        {
            return;
        }

        float num = Vector2.Distance(self.bodyChunks[myChunk].vel, otherObject.bodyChunks[otherChunk].vel);

        if (otherObject is Player && num < 8f)
        {
            self.grabable = Math.Max(self.grabable, 7);
        }

        if (num > 12f && otherObject is Creature)
        {
            self.room.PlaySound(SoundID.Jet_Fish_Ram_Creature, self.mainBodyChunk);
            Vector2 pos = self.bodyChunks[myChunk].pos + Custom.DirVec(self.bodyChunks[myChunk].pos, otherObject.bodyChunks[otherChunk].pos) * self.bodyChunks[myChunk].rad;
            for (int i = 0; i < 5; i++)
            {
                self.room.AddObject(new Bubble(pos, Custom.RNV() * (18f * UnityEngine.Random.value), false, false, false));
            }
            return;
        }

        if (myChunk == 0 && self.grasps[0] == null && self.AI.WantToEatObject(otherObject))
        {
            if (ShadowOfOptions.jet_eat.Value)
            {
                self.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanNotShare, 1f, true, false);
                self.room.PlaySound(SoundID.Jet_Fish_Grab_NPC, self.mainBodyChunk);
            }
        }
        else if (otherObject is not JetFish && otherObject.bodyChunks[otherChunk].pos.y < self.bodyChunks[myChunk].pos.y && self.AI.attackCounter > 0)
        {
            otherObject.bodyChunks[otherChunk].vel.y -= num / otherObject.bodyChunks[otherChunk].mass;
            self.bodyChunks[myChunk].vel.y += num / 2f;
            int num2 = 30;
            if (otherObject is Creature)
            {
                SocialMemory.Relationship relationship = self.abstractCreature.state.socialMemory.GetRelationship((otherObject as Creature).abstractCreature.ID);
                if (relationship != null)
                {
                    if (relationship.like > -0.5f)
                    {
                        relationship.like = Mathf.Lerp(relationship.like, 0f, 0.001f);
                    }
                    if (relationship.like >= 0f)
                    {
                        num2 = 10 + (int)(20f * Mathf.InverseLerp(1f, 0f, relationship.like));
                    }
                    else
                    {
                        num2 = 30 + (int)(220f * Mathf.InverseLerp(0f, -1f, relationship.like));
                    }
                }
            }

            if (self.AI.attackCounter > num2)
            {
                self.AI.attackCounter = num2;
            }
        }

        if (ShadowOfOptions.jet_eat.Value && myChunk == 0 && self.grasps[0] == null && otherObject is Creature && ((otherObject as Creature).dead || otherObject.TotalMass < self.TotalMass * 0.7f) && self.Template.CreatureRelationship((otherObject as Creature).Template).type == CreatureTemplate.Relationship.Type.Eats)
        {
            self.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanNotShare, 1f, true, false);
            self.room.PlaySound((otherObject is Player) ? SoundID.Jet_Fish_Grab_Player : SoundID.Jet_Fish_Grab_NPC, self.mainBodyChunk);
        }
    }
    static void JetFishUpdate(On.JetFish.orig_Update orig, JetFish self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self) || self.room == null)
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
                self.waterFriction = 0.95f;
                self.waterRetardationImmunity = 0f;
                return;
            }

            self.waterFriction = 0.95f;
            self.waterRetardationImmunity = 0f;

            bool flag = false;
            for (int j = 0; j < self.grabbedBy.Count; j++)
            {
                if (self.grabbedBy[j].grabber is Player)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                self.turnSpeed = 0f;
                self.diveSpeed *= 0.8f;
                self.surfSpeed *= 0.9f;
                Act();
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void Act()
        {
            AIUpdate();

            MovementConnection movementConnection = (self.AI.pathFinder as FishPather).FollowPath(self.room.GetWorldCoordinate(self.mainBodyChunk.pos), true);

            if (self.AI.floatGoalPos != null && self.AI.pathFinder.GetDestination.TileDefined && self.AI.pathFinder.GetDestination.room == self.room.abstractRoom.index && self.room.VisualContact(self.mainBodyChunk.pos, self.AI.floatGoalPos.Value) && !self.safariControlled)
            {
                self.swimDir = Custom.DirVec(self.mainBodyChunk.pos, self.AI.floatGoalPos.Value);
            }
            else if (movementConnection != default)
            {
                self.swimDir = Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(movementConnection.DestTile));
                WorldCoordinate destinationCoord = movementConnection.destinationCoord;
                for (int k = 0; k < 4; k++)
                {
                    MovementConnection movementConnection2 = (self.AI.pathFinder as FishPather).FollowPath(destinationCoord, false);
                    if (movementConnection2 != default && movementConnection2.destinationCoord.TileDefined && movementConnection2.destinationCoord.room == self.room.abstractRoom.index && self.room.VisualContact(movementConnection.destinationCoord.Tile, movementConnection2.DestTile))
                    {
                        self.swimDir += Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(movementConnection2.DestTile));
                        destinationCoord = movementConnection2.destinationCoord;
                        if (self.room.aimap.getAItile(movementConnection2.DestTile).narrowSpace)
                        {
                            self.slowDownForPrecision += 0.3f;
                            break;
                        }
                    }
                }
                self.swimDir = self.swimDir.normalized;
            }

            self.slowDownForPrecision = Mathf.Clamp(self.slowDownForPrecision - 0.1f, 0f, 1f);

            if (self.Submersion < 0.4f && (self.bodyChunks[0].ContactPoint.y < 0 || self.bodyChunks[1].ContactPoint.y < 0) && UnityEngine.Random.value < 0.05f)
            {
                self.mainBodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-5f, 5f, UnityEngine.Random.value)) * Mathf.Lerp(2f, 6f, UnityEngine.Random.value);
                self.room.PlaySound(SoundID.Jet_Fish_On_Land_Jump, self.mainBodyChunk);
            }
        }

        void AIUpdate()
        {
            JetFishAI ai = self.AI;

            ai.focusCreature = null;

            MiscHooks.AIUpdate(ai);

            if (ai.getAwayCounter > 0)
            {
                ai.getAwayCounter--;
            }

            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = JetFishAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = JetFishAI.Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = JetFishAI.Behavior.Hunt;
                }
                else if (aimodule is StuckTracker)
                {
                    ai.behavior = JetFishAI.Behavior.GetUnstuck;
                }
            }

            if (ai.goToFood != null)
            {
                if (!ai.WantToEatObject(ai.goToFood))
                {
                    ai.goToFood = null;
                }
                else if (ai.currentUtility < 0.75f && ai.fish.grasps[0] == null)
                {
                    ai.currentUtility = 0.75f;
                    ai.behavior = JetFishAI.Behavior.GoToFood;
                }
            }

            if (ai.currentUtility < 0.1f)
            {
                ai.behavior = JetFishAI.Behavior.Idle;
            }
            if (ai.behavior != JetFishAI.Behavior.Flee && ai.fish.grasps[0] != null)
            {
                ai.behavior = JetFishAI.Behavior.ReturnPrey;
            }

            if (ai.behavior == JetFishAI.Behavior.Idle)
            {
                if (ai.exploreCoordinate != null)
                {
                    if (Custom.ManhattanDistance(ai.creature.pos, ai.exploreCoordinate.Value) < 5 || (UnityEngine.Random.value < 0.0125f && ai.pathFinder.DoneMappingAccessibility && ai.fish.room.aimap.TileAccessibleToCreature(ai.creature.pos.x, ai.creature.pos.y, ai.creature.creatureTemplate) && !ai.pathFinder.CoordinateReachableAndGetbackable(ai.exploreCoordinate.Value)))
                    {
                        ai.exploreCoordinate = null;
                    }
                }
                else if (Custom.ManhattanDistance(ai.creature.pos, ai.pathFinder.GetDestination) < 5 || !ai.pathFinder.CoordinateReachableAndGetbackable(ai.pathFinder.GetDestination))
                {
                    WorldCoordinate worldCoordinate = ai.fish.room.GetWorldCoordinate(Custom.RestrictInRect(ai.fish.mainBodyChunk.pos, new FloatRect(0f, 0f, ai.fish.room.PixelWidth, ai.fish.room.PixelHeight)) + Custom.RNV() * 200f);
                }

                if (UnityEngine.Random.value < 1f / ((ai.exploreCoordinate != null) ? 1600f : 80f))
                {
                    WorldCoordinate worldCoordinate2 = new WorldCoordinate(ai.fish.room.abstractRoom.index, UnityEngine.Random.Range(0, ai.fish.room.TileWidth), UnityEngine.Random.Range(0, ai.fish.room.TileHeight), -1);
                    if (ai.fish.room.aimap.TileAccessibleToCreature(worldCoordinate2.Tile, ai.creature.creatureTemplate) && ai.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate2))
                    {
                        ai.exploreCoordinate = new WorldCoordinate?(worldCoordinate2);
                    }
                }

                ai.floatGoalPos = Custom.DistLess(ai.creature.pos, ai.pathFinder.GetDestination, 20f) && ai.fish.room.VisualContact(ai.creature.pos, ai.pathFinder.GetDestination) ? new Vector2?(ai.fish.room.MiddleOfTile(ai.pathFinder.GetDestination)) : null;
            }
            else
            {
                if (ai.behavior == JetFishAI.Behavior.Flee)
                {
                    WorldCoordinate destination = ai.threatTracker.FleeTo(ai.creature.pos, 3, 30, ai.currentUtility > 0.3f);
                    if (ai.threatTracker.mostThreateningCreature != null)
                    {
                        ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                    }

                    ai.floatGoalPos = null;
                }
                else if (ai.behavior == JetFishAI.Behavior.EscapeRain)
                {
                    ai.floatGoalPos = null;
                }
                else if (ai.behavior == JetFishAI.Behavior.Hunt)
                {
                    ai.attackCounter--;
                    ai.focusCreature = ai.preyTracker.MostAttractivePrey;
                    if (ai.attackCounter > 0)
                    {
                        if (ai.focusCreature.VisualContact)
                        {
                            ai.floatGoalPos = new Vector2?(ai.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos);
                        }
                    }
                    else if (ai.focusCreature.VisualContact)
                    {
                        ai.floatGoalPos = new Vector2?(ai.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos + Custom.DirVec(ai.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, ai.fish.mainBodyChunk.pos) * 200f);
                    }
                    if (ai.attackCounter < -50)
                    {
                        ai.attackCounter = UnityEngine.Random.Range(200, 400);
                    }
                    if (ai.focusCreature.VisualContact && (ai.focusCreature.representedCreature.rippleLayer == ai.fish.abstractPhysicalObject.rippleLayer || ai.focusCreature.representedCreature.rippleBothSides || ai.fish.abstractPhysicalObject.rippleBothSides) && ai.focusCreature.representedCreature.realizedCreature.collisionLayer != ai.fish.collisionLayer && Custom.DistLess(ai.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, ai.fish.mainBodyChunk.pos, ai.fish.mainBodyChunk.rad + ai.focusCreature.representedCreature.realizedCreature.mainBodyChunk.rad))
                    {
                        ai.fish.Collide(ai.focusCreature.representedCreature.realizedCreature, 0, 0);
                        return;
                    }
                }
            }
        }
    }
    #endregion

    #region JetFishAI
    static void JetFishAIUpdate(On.JetFishAI.orig_Update orig, JetFishAI self)
    {
        orig(self);

        MiscHooks.ReturnToDenUpdate(self);
    }
    static bool JetFishAIWantToStayInDenUntilEndOfCycle(On.JetFishAI.orig_WantToStayInDenUntilEndOfCycle orig, JetFishAI self)
    {
        return MiscHooks.ReturnToDenWantToStayInDenUntilEndOfCycle(self) || orig(self);
    }
    #endregion

    #region JetFishGraphics
    static void JetFishGraphicsDrawSprites(On.JetFishGraphics.orig_DrawSprites orig, JetFishGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        Vector2 vector = Vector3.Slerp(self.lastZRotation, self.zRotation, timeStacker);
        float num2 = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector);
        sLeaser.sprites[self.BodySprite].scaleX = ((num2 > 0f) ? -1f : 1f) * MiscHooks.ApplyBreath(data, timeStacker);

        sLeaser.sprites[self.BodySprite].scaleY = Mathf.Lerp(0.8f, 1f, self.fish.iVars.fatness) * MiscHooks.ApplyBreath(data, timeStacker);
    }
    static void JetFishGraphicsUpdate(On.JetFishGraphics.orig_Update orig, JetFishGraphics self)
    {
        orig(self);

        if (BreathCheck(self.fish))
        {
            MiscHooks.UpdateBreath(self);
        }
    }
    #endregion
}