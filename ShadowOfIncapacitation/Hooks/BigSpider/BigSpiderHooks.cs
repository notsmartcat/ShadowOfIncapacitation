using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.BigSpiderHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.BigSpider.Revive += BigSpiderRevive;

        On.BigSpider.Update += BigSpiderUpdate;

        On.BigSpiderGraphics.Update += BigSpiderGraphicsUpdate;

        On.BigSpider.Collide += BigSpiderCollide;

        On.BigSpider.FlyingWeapon += BigSpiderFlyingWeapon;
    }

    static void BigSpiderFlyingWeapon(On.BigSpider.orig_FlyingWeapon orig, BigSpider self, Weapon weapon)
    {
        orig(self, weapon);

        if (!ShadowOfOptions.spid_dodge.Value || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsIncon(self) || self.spitter || self.jumpStamina < 0.3f || self.room.GetTile(self.room.GetTilePosition(self.mainBodyChunk.pos) + new IntVector2(0, 1)).Solid || Custom.DistLess(self.mainBodyChunk.pos, weapon.thrownPos, 60f) || (!self.room.GetTile(self.room.GetTilePosition(self.mainBodyChunk.pos) + new IntVector2(0, -1)).Solid && self.room.GetTile(self.room.GetTilePosition(self.mainBodyChunk.pos) + new IntVector2(0, -1)).Terrain != Room.Tile.TerrainType.Floor && !self.room.GetTile(self.room.GetTilePosition(self.mainBodyChunk.pos)).AnyBeam) || self.grasps[0] != null || Vector2.Dot((self.bodyChunks[1].pos - self.bodyChunks[0].pos).normalized, (self.bodyChunks[0].pos - weapon.firstChunk.pos).normalized) < -0.2f || !self.AI.VisualContact(weapon.firstChunk.pos, 0.3f))
        {
            return;
        }

        if (Custom.DistLess(weapon.firstChunk.pos + weapon.firstChunk.vel.normalized * 140f, self.mainBodyChunk.pos, 140f) && (Mathf.Abs(Custom.DistanceToLine(self.bodyChunks[0].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 7f || Mathf.Abs(Custom.DistanceToLine(self.bodyChunks[1].pos, weapon.firstChunk.pos, weapon.firstChunk.pos + weapon.firstChunk.vel)) < 7f))
        {
            BigSpiderJump(self, Custom.DirVec(self.mainBodyChunk.pos, weapon.thrownPos + new Vector2(0f, 400f)), 1f);

            self.bodyChunks[0].pos.y += 10f;
            self.bodyChunks[1].pos.y += 5f;
            self.jumpStamina = Mathf.Max(0f, self.jumpStamina - 0.30f);
        }
    }

    static void BigSpiderCollide(On.BigSpider.orig_Collide orig, BigSpider self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsIncon(self))
        {
            return;
        }

        if (otherObject is Creature)
        {
            self.AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);

            bool flag = false;
            bool consious = (otherObject as Creature).Consious || IsIncon(otherObject as Creature);
            if (ShadowOfOptions.spid_cling.Value && myChunk == 0 && self.grasps[0] == null)
            {
                bool flag2 = ((!self.spitter && self.mandiblesCharged > 0.8f && self.canBite > 0) || (self.spitter && UnityEngine.Random.value < 0.25f && !consious)) && Vector2.Dot(Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos), Custom.DirVec(self.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > 0f && self.AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;
                if (ModManager.MMF)
                {
                    flag2 = (flag2 && self.AI.preyTracker.TotalTrackedPrey > 0 && self.AI.preyTracker.Utility() > 0f && self.AI.preyTracker.MostAttractivePrey.representedCreature == (otherObject as Creature).abstractCreature);
                }
                if (flag2)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        self.room.AddObject(new WaterDrip(Vector2.Lerp(self.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos, UnityEngine.Random.value), Custom.RNV() * UnityEngine.Random.value * 14f, false));
                    }
                    if (self.safariControlled || UnityEngine.Random.value < Custom.LerpMap(otherObject.TotalMass, 0.84f, self.spitter ? 5.5f : 3f, 0.5f, 0.15f, 0.12f) || !consious)
                    {
                        if (self.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true))
                        {
                            flag = true;
                            self.room.PlaySound(SoundID.Big_Spider_Grab_Creature, self.mainBodyChunk);
                        }
                        else
                        {
                            self.room.PlaySound(SoundID.Big_Spider_Slash_Creature, self.mainBodyChunk);
                        }
                    }
                    else
                    {
                        self.room.PlaySound(SoundID.Big_Spider_Slash_Creature, self.mainBodyChunk);
                    }
                    self.canBite = 0;
                    self.ReleaseAllGrabChunks();
                    SpiderInconAct(data);
                }
                else if (self.AI.StaticRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && ((otherObject as Creature).Template.CreatureRelationship(self.Template).type != CreatureTemplate.Relationship.Type.Eats || UnityEngine.Random.value < 0.1f) && self.LegsGrabby)
                {
                    IntVector2 intVector2 = new IntVector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 4)));
                    self.grabChunks[intVector2.x, intVector2.y] = otherObject.bodyChunks[otherChunk];
                    flag = true;
                }
            }
            if (ShadowOfOptions.spid_collide.Value && !flag && !self.safariControlled && (self.spitter || self.mandiblesCharged <= 0.8f || self.canBite <= 0) && (self.spitter || self.jumpStamina > 0.15f) && self.grasps[0] == null && consious && (otherObject as Creature).Template.CreatureRelationship(self.Template).intensity > 0f && (otherObject as Creature).TotalMass > self.TotalMass * 0.2f)
            {
                for (int k = 0; k < self.grabChunks.GetLength(0); k++)
                {
                    for (int l = 0; l < self.grabChunks.GetLength(1); l++)
                    {
                        if (self.grabChunks[k, l] != null && self.grabChunks[k, l].owner == otherObject)
                        {
                            return;
                        }
                    }
                }
                if (self.spitter)
                {
                    self.mainBodyChunk.vel += Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, self.mainBodyChunk.pos) * 4f;
                    self.bodyChunks[1].vel += Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, self.mainBodyChunk.pos) * 4f;
                    otherObject.bodyChunks[otherChunk].vel -= Vector2.ClampMagnitude(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, self.mainBodyChunk.pos) * 8f * self.TotalMass / otherObject.bodyChunks[otherChunk].mass, 15f);
                    self.room.PlaySound(SoundID.Big_Spider_Jump, self.mainBodyChunk, false, 0.1f + 0.4f * self.bounceSoundVol, 1f);
                    self.bounceSoundVol = Mathf.Max(0f, self.bounceSoundVol - 0.2f);
                }
                else
                {
                    if(ShadowOfOptions.spid_jump.Value)
                        self.Jump(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, self.mainBodyChunk.pos + new Vector2(0f, 10f)), 0.1f + 0.4f * self.bounceSoundVol);
                    self.bounceSoundVol = Mathf.Max(0f, self.bounceSoundVol - 0.2f);
                    otherObject.bodyChunks[otherChunk].vel -= Vector2.ClampMagnitude(Custom.DirVec(otherObject.bodyChunks[otherChunk].pos, self.mainBodyChunk.pos + new Vector2(0f, 10f)) * 8f * self.TotalMass / otherObject.bodyChunks[otherChunk].mass, 15f);

                    self.bodyChunks[0].pos.y += 10f;
                    self.bodyChunks[1].pos.y += 5f;

                    self.jumpStamina = Mathf.Max(0f, self.jumpStamina - 0.30f);
                    SpiderInconAct(data);
                }
                self.footingCounter = 0;
                if ((otherObject as Creature).Consious && (otherObject as Creature).Template.CreatureRelationship(self.Template).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    self.AI.stayAway = true;
                }
            }
        }
    }

    static void BigSpiderGraphicsUpdate(On.BigSpiderGraphics.orig_Update orig, BigSpiderGraphics self)
    {
        orig(self);

        if (!inconstorage.TryGetValue(self.bug.abstractCreature, out InconData data) || !IsComa(self.bug))
        {
            return;
        }

        self.breathCounter += UnityEngine.Random.value * (IsInconBase(self.bug) ? 1.25f : 0.75f);
        if (self.bug.Stunned && self.bug.deathConvulsions > 0f)
        {
            self.soundLoop.Volume = Mathf.Max(self.soundLoop.Volume, 0.5f * self.bug.deathConvulsions);
        }
    }

    static void BigSpiderUpdate(On.BigSpider.orig_Update orig, BigSpider self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self))
        {
            return;
        }

        if (self.deathConvulsions > 0f)
        {
            self.footingCounter = 0;

            self.deathConvulsions = Mathf.Max(0f, self.deathConvulsions - UnityEngine.Random.value / 80f);
            
            if (self.mainBodyChunk.ContactPoint.x != 0 || self.mainBodyChunk.ContactPoint.y != 0)
            {
                self.mainBodyChunk.vel += Custom.RNV() * UnityEngine.Random.value * 8f * Mathf.Pow(self.deathConvulsions, 0.5f);
            }
            if (self.bodyChunks[1].ContactPoint.x != 0 || self.bodyChunks[1].ContactPoint.y != 0)
            {
                self.bodyChunks[1].vel += Custom.RNV() * UnityEngine.Random.value * 4f * Mathf.Pow(self.deathConvulsions, 0.5f);
            }
            if (UnityEngine.Random.value < 0.05f)
            {
                self.room.PlaySound(SoundID.Big_Spider_Death_Rustle, self.mainBodyChunk, false, 0.5f + UnityEngine.Random.value * 0.5f * self.deathConvulsions, 0.9f + 0.3f * self.deathConvulsions);
            }
            if (UnityEngine.Random.value < 0.025f)
            {
                self.room.PlaySound(SoundID.Big_Spider_Take_Damage, self.mainBodyChunk, false, UnityEngine.Random.value * 0.5f, 1f);
            }
        }

        if (!IsInconBase(self))
        {
            self.spitPos = null;
            self.footingCounter = 0;
            self.charging = 0f;
            self.jumping = false;
            self.mandiblesCharged *= 0.8f;
            self.revivingBuddy = null;

            self.LoseAllGrasps();
            return;
        }

        if (data.stunTimer > 0)
        {
            data.stunTimer -= 1;
        }
        if (data.stunCountdown > 0)
        {
            data.stunCountdown -= 1;
            if (data.stunCountdown <= 0)
            {
                data.stunTimer = Mathf.Max(data.stunTimer, UnityEngine.Random.Range(20, 41));
            }
        }

        if (self.Stunned || data.stunTimer > 0)
        {
            self.spitPos = null;
            self.footingCounter = 0;
            self.charging = 0f;
            self.jumping = false;
            self.mandiblesCharged *= 0.8f;
            self.revivingBuddy = null;

            self.LoseAllGrasps();
            return;
        }

        if (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[0].pos, self.Template) || self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
        {
            self.footingCounter++;
        }

        if (self.Submersion > 0.3f)
        {
            Swim();
            AIUpdate();
            self.spitPos = null;
            return;
        }
        if (self.jumping)
        {
            bool flag = false;
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                if ((self.bodyChunks[i].ContactPoint.x != 0 || self.bodyChunks[i].ContactPoint.y != 0) && self.room.aimap.TileAccessibleToCreature(self.bodyChunks[i].pos, self.Template))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                self.footingCounter++;
            }
            else
            {
                self.footingCounter = 0;
            }
            if (self.AI.preyTracker.MostAttractivePrey != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == self.room && self.AI.preyTracker.MostAttractivePrey.VisualContact)
            {
                Vector2 pos = self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
                self.bodyChunks[0].vel += Custom.DirVec(self.bodyChunks[0].pos, pos) * 1f;
                self.bodyChunks[1].vel -= Custom.DirVec(self.bodyChunks[0].pos, pos) * 0.5f;
                if (self.graphicsModule != null)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            (self.graphicsModule as BigSpiderGraphics).legs[j, k].mode = Limb.Mode.Dangle;
                            (self.graphicsModule as BigSpiderGraphics).legFlips[j, k, 0] = ((j == 0) ? -1f : 1f);
                            (self.graphicsModule as BigSpiderGraphics).legs[j, k].vel += (Vector2)Vector3.Slerp(Custom.DirVec(self.mainBodyChunk.pos, pos), Custom.PerpendicularVector(self.mainBodyChunk.pos, pos) * ((j == 0) ? -1f : 1f), (k == 0) ? 0.1f : 0.5f) * 3f;
                        }
                    }
                }
            }
            if (self.Footing)
            {
                self.jumping = false;
            }
            return;
        }

        self.stuckShake = 0;
        self.specialMoveCounter = 0;

        if (ModManager.DLCShared && self.selfDestruct > 0f)
        {
            for (int m = 0; m < self.bodyChunks.Length; m++)
            {
                self.bodyChunks[m].vel += Custom.RNV() * (UnityEngine.Random.value * 10f * self.selfDestruct + 3f);
            }
            self.mainBodyChunk.pos.x = self.selfDestructOrigin.x;
            if (self.mainBodyChunk.pos.y > self.selfDestructOrigin.y)
            {
                self.mainBodyChunk.pos.y = self.selfDestructOrigin.y;
            }
        }
        if (self.specialMoveCounter > 0)
        {
            self.specialMoveCounter--;
            self.footingCounter = Mathf.Max(self.footingCounter, 30);
            self.travelDir = Vector2.Lerp(self.travelDir, Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.specialMoveDestination)), 0.4f);
            if (Custom.DistLess(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.specialMoveDestination), 5f))
            {
                self.specialMoveCounter = 0;
            }
        }
        else
        {
            if (!self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) && !self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
            {
                self.footingCounter = Custom.IntClamp(self.footingCounter - 3, 0, 35);
            }
            if (ShadowOfOptions.spid_attack.Value && self.Footing && self.charging > 0f)
            {
                self.sitting = true;
                self.GoThroughFloors = false;
                self.charging += 0.05f;
                Vector2 a2 = Custom.DirVec(self.mainBodyChunk.pos, self.jumpAtPos);
                if (ShadowOfOptions.spid_jump.Value)
                {
                    self.bodyChunks[0].vel += a2 * Mathf.Pow(self.charging, 2f);
                    self.bodyChunks[1].vel -= a2 * Mathf.Lerp(0.7f, 2f, self.charging);
                }
                if (self.charging >= 1f)
                {
                    Attack();
                    SpiderInconAct(data);
                }
            }
            else if ((self.room.GetWorldCoordinate(self.mainBodyChunk.pos) == self.AI.pathFinder.GetDestination || self.room.GetWorldCoordinate(self.bodyChunks[1].pos) == self.AI.pathFinder.GetDestination) && self.AI.threatTracker.Utility() < 0.5f && !self.safariControlled)
            {
                self.sitting = true;
                self.GoThroughFloors = false;
            }
            else
            {
                MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[(!self.CarryBackwards) ? 0 : 1].pos), true);
                if (movementConnection == default(MovementConnection))
                {
                    movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[(!self.CarryBackwards) ? 1 : 0].pos), true);
                }
                if (movementConnection == default(MovementConnection))
                {
                    self.GoThroughFloors = false;
                }
            }
        }
        AIUpdate();
        if (!Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 2f))
        {
            self.runCycle += 0.0625f;
        }

        if (self.charging == 0f && self.canBite == 0)
        {
            self.mandiblesCharged = Mathf.Max(0f, self.mandiblesCharged - 0.1f);
            if (!self.jumping)
            {
                self.jumpStamina = Mathf.Min(1f, self.jumpStamina + 0.0033333334f);
                if (self.jumpStamina == 1f && !self.spitter)
                {
                    self.AI.stayAway = false;
                }
            }
        }

        else
        {
            self.mandiblesCharged = Custom.LerpAndTick(self.mandiblesCharged, (self.charging > 0f || self.canBite > 0) ? 1f : 0f, 0.01f, 0.022727273f);
        }
        if (self.AI.utilityComparer.GetUtilityTracker(self.AI.threatTracker).SmoothedUtility() < 0.9f)
        {
            self.canCling = 40;
        }
        else if (self.canCling > 0)
        {
            self.canCling--;
        }

        void AIUpdate()
        {
            BigSpiderAI ai = self.AI;

            if (ai.behavior == BigSpiderAI.Behavior.Flee && !RainWorldGame.RequestHeavyAi(ai.bug))
            {
                return;
            }
            MiscHooks.AIUpdate(ai);
            if (ai.bug.room == null)
            {
                return;
            }
            if (ModManager.MSC && ai.bug.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.bug.LickedByPlayer.abstractCreature);
                ai.stayAway = false;
            }
            ai.shyLightCycle += 0.0025f;
            if (ai.tracker.CreaturesCount > 0)
            {
                Tracker.CreatureRepresentation rep = ai.tracker.GetRep(UnityEngine.Random.Range(0, ai.tracker.CreaturesCount));
                if ((rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.BigSpider || rep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.SpitterSpider) && rep.representedCreature.realizedCreature != null && !ai.otherSpiders.Contains((rep.representedCreature.realizedCreature as BigSpider).AI))
                {
                    if (rep.representedCreature.state.alive)
                    {
                        if (rep.VisualContact)
                        {
                            ai.otherSpiders.Add((rep.representedCreature.realizedCreature as BigSpider).AI);
                            if (rep.representedCreature.personality.dominance > ai.creature.personality.dominance)
                            {
                                ai.shyLightCycle = (rep.representedCreature.realizedCreature as BigSpider).AI.shyLightCycle;
                            }
                        }
                    }
                }
            }
            for (int i = ai.otherSpiders.Count - 1; i >= 0; i--)
            {
                if (ai.otherSpiders[i].bug.dead || ai.otherSpiders[i].creature.realizedCreature == null || ai.otherSpiders[i].creature.pos.room != ai.creature.pos.room)
                {
                    ai.otherSpiders.RemoveAt(i);
                }
            }
            if (ai.bug.sitting)
            {
                ai.noiseTracker.hearingSkill = 1f;
            }
            else
            {
                ai.noiseTracker.hearingSkill = 0.3f;
            }
            if (ai.bug.spitter)
            {
                ai.utilityComparer.GetUtilityTracker(ai.threatTracker).weight = Mathf.InverseLerp(40f, 10f, (float)ai.spitModule.randomCritSpitDelay);
            }
            else if (ai.preyTracker.MostAttractivePrey != null)
            {
                ai.utilityComparer.GetUtilityTracker(ai.preyTracker).weight = Custom.LerpMap(ai.creature.pos.Tile.FloatDist(ai.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile), 26f, 36f, 1f, 0.1f);
            }
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = BigSpiderAI.Behavior.Flee;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = BigSpiderAI.Behavior.Hunt;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = BigSpiderAI.Behavior.EscapeRain;
                }
                else if (aimodule is StuckTracker)
                {
                    ai.behavior = BigSpiderAI.Behavior.GetUnstuck;
                }
            }
            if (ai.currentUtility < 0.05f)
            {
                ai.behavior = BigSpiderAI.Behavior.Idle;
            }
            ai.idlePos = ai.creature.pos;
            if (ai.bug.sitting)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0f, 0.01f, 0.016666668f);
            }
            ai.idlePosCounter--;

            if (ai.behavior == BigSpiderAI.Behavior.Idle)
            {
                if (!ai.bug.sitting)
                {
                    if (ai.bug.mother)
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.15f, 0.01f, 0.016666668f);
                    }
                    else
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.5f, 0.01f, 0.016666668f);
                    }
                }
            }
            else if (ai.behavior == BigSpiderAI.Behavior.Hunt)
            {
                if (!ai.bug.sitting)
                {
                    if (ai.bug.mother)
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.15f, 0.01f, 0.1f);
                    }
                    else
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                    }
                }
                if (ai.preyTracker.MostAttractivePrey != null && !ai.bug.safariControlled)
                {
                    if (ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && ai.bug.CanJump && !ai.bug.jumping && ai.bug.charging == 0f && ai.bug.Footing && ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == ai.bug.room)
                    {
                        Vector2 pos = ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
                        if (Custom.DistLess(ai.bug.mainBodyChunk.pos, pos, 120f) && (ai.bug.room.aimap.TileAccessibleToCreature(ai.bug.room.GetTilePosition(ai.bug.bodyChunks[1].pos - Custom.DirVec(ai.bug.bodyChunks[1].pos, pos) * 30f), ai.bug.Template) || ai.bug.room.GetTile(ai.bug.bodyChunks[1].pos - Custom.DirVec(ai.bug.bodyChunks[1].pos, pos) * 30f).Solid) && ai.bug.room.VisualContact(ai.bug.mainBodyChunk.pos, pos))
                        {
                            if (Vector2.Dot((ai.bug.mainBodyChunk.pos - pos).normalized, (ai.bug.bodyChunks[1].pos - ai.bug.mainBodyChunk.pos).normalized) > 0.2f)
                            {
                                ai.bug.InitiateJump(ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos);
                            }
                            else
                            {
                                ai.bug.mainBodyChunk.vel += Custom.DirVec(ai.bug.mainBodyChunk.pos, pos);
                                ai.bug.bodyChunks[1].vel -= Custom.DirVec(ai.bug.mainBodyChunk.pos, pos);
                            }
                        }
                    }
                }
            }
            else if (ai.behavior == BigSpiderAI.Behavior.Flee)
            {
                if (!ai.bug.sitting)
                {
                    if (ai.bug.mother)
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.2f, 0.01f, 0.1f);
                    }
                    else
                    {
                        ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                    }
                }
            }
            else if (ai.behavior == BigSpiderAI.Behavior.EscapeRain || ai.behavior == BigSpiderAI.Behavior.ReturnPrey)
            {
                if (!ai.bug.sitting)
                {
                    ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                }
            }
            if (ai.noiseRectionDelay > 0)
            {
                ai.noiseRectionDelay--;
            }
            for (int j = ai.lightThreats.Count - 1; j >= 0; j--)
            {
                if (ai.lightThreats[j].slatedForDeletion)
                {
                    ai.lightThreats.RemoveAt(j);
                }
                else
                {
                    ai.lightThreats[j].Update();
                }
            }
            if (ai.ShyFromLight > 0f && ai.creature.Room.realizedRoom != null && ai.creature.Room.realizedRoom.lightSources.Count > 0)
            {
                ai.TryAddLightThreat(ai.creature.Room.realizedRoom.lightSources[UnityEngine.Random.Range(0, ai.creature.Room.realizedRoom.lightSources.Count)]);
            }
        }

        void Swim()
        {
            if (ModManager.MMF)
            {
                self.charging = 0f;
            }
            self.selfDestruct = 0f;
            self.selfDestructOrigin = self.mainBodyChunk.pos;
            self.bodyChunks[0].vel *= 1f - 0.1f * self.bodyChunks[0].submersion;
            self.bodyChunks[1].vel *= 1f - 0.2f * self.bodyChunks[1].submersion;
            self.GoThroughFloors = true;
            MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[0].pos), true);
            if (movementConnection == default(MovementConnection))
            {
                movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[1].pos), true);
            }
            if (movementConnection == default(MovementConnection) && Math.Abs(self.abstractCreature.pos.y - self.room.defaultWaterLevel) < 4)
            {
                movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(new WorldCoordinate(self.abstractCreature.pos.room, self.abstractCreature.pos.x, self.room.defaultWaterLevel, self.abstractCreature.pos.abstractNode), true);
            }
            if (movementConnection == default)
            {
                BodyChunk mainBodyChunk = self.mainBodyChunk;
                mainBodyChunk.vel.y += 0.5f;
                return;
            }
            if (self.graphicsModule != null)
            {
                (self.graphicsModule as BigSpiderGraphics).flip = Mathf.Lerp((self.graphicsModule as BigSpiderGraphics).flip, Mathf.Sign(self.room.MiddleOfTile(movementConnection.StartTile).x - self.room.MiddleOfTile(movementConnection.DestTile).x), 0.25f);
            }

            self.bodyChunks[0].vel *= 0.8f;
            self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;

            self.mainBodyChunk.vel *= 0.65f;
            self.footingCounter = 0;
            self.outOfWaterFooting = 0;
        }

        void Attack()
        {
            if (!self.safariControlled && (self.AI.preyTracker.MostAttractivePrey == null || !self.CanJump || !self.AI.preyTracker.MostAttractivePrey.VisualContact || !self.room.VisualContact(self.mainBodyChunk.pos, self.jumpAtPos) || self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature == null || self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room != self.room))
            {
                self.charging = 0f;
                return;
            }
            Vector2 vector = Custom.DirVec(self.mainBodyChunk.pos, self.jumpAtPos);

            Vector2 vector2 = self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos;
            vector2 += new Vector2(0f, Mathf.InverseLerp(40f, 300f, Vector2.Distance(self.mainBodyChunk.pos, vector2)) * 40f);
            if (!Custom.DistLess(self.mainBodyChunk.pos, vector2, Custom.LerpMap(Vector2.Dot(vector, Custom.DirVec(self.mainBodyChunk.pos, vector2)), -1f, 1f, 0f, 500f)))
            {
                self.charging = 0f;
                return;
            }
            self.jumpStamina = Mathf.Max(0f, self.jumpStamina - 0.7f);
            if (self.jumpStamina < 0.2f && UnityEngine.Random.value < 0.5f && !self.spitter)
            {
                self.AI.stayAway = true;
            }
            if (!self.room.GetTile(self.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !self.room.GetTile(self.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
            {
                vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(self.mainBodyChunk.pos, self.jumpAtPos), 40f, 400f, 0.2f, 0.5f));
            }
            if(ShadowOfOptions.spid_jump.Value)
                BigSpiderJump(self, vector, 1f);
            self.canBite = 40;
        }
    }

    static void SpiderInconAct(InconData data)
    {
        if (data.stunCountdown > 0)
        {
            return;
        }

        data.stunCountdown = Mathf.Max(data.stunCountdown, UnityEngine.Random.Range(20, 51));
    }

    static void BigSpiderRevive(On.BigSpider.orig_Revive orig, BigSpider self)
    {
        orig(self);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data))
        {
            return;
        }

        data.returnToDen = false;
        data.actuallyDead= false;
        data.isAlive = true;
        data.isUncon = false;
        data.inconCycle = -2;
        data.wasDead = false;
    }

    static void BigSpiderJump(BigSpider self, Vector2 jumpDir, float soundVol)
    {
        float d = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
        self.footingCounter = 0;
        self.mainBodyChunk.vel *= 0.5f;
        self.bodyChunks[1].vel *= 0.5f;
        self.mainBodyChunk.vel += jumpDir * 8f * d;
        self.bodyChunks[1].vel += jumpDir * 5.5f * d;
        self.charging = 0f;
        self.jumping = true;
        if (self.graphicsModule != null)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    (self.graphicsModule as BigSpiderGraphics).legs[i, j].mode = Limb.Mode.Dangle;
                    (self.graphicsModule as BigSpiderGraphics).legs[i, j].vel += jumpDir * 30f * ((j < 2) ? 1f : -1f);
                }
            }
        }
        self.room.PlaySound(SoundID.Big_Spider_Jump, self.mainBodyChunk, false, soundVol, 1f);
    }
}