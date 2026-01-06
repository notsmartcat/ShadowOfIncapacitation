using RWCustom;
using System;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.DropBugHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.DropBug.Update += DropBugUpdate;

        On.DropBug.Collide += DropBugCollide;

        On.DropBugGraphics.Update += DropBugGraphicsUpdate;
        On.DropBugGraphics.DrawSprites += DropBugGraphicsDrawSprites;
    }

    static void DropBugGraphicsDrawSprites(On.DropBugGraphics.orig_DrawSprites orig, DropBugGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[self.HeadSprite].scaleX = 0.7f * MiscHooks.ApplyBreath(data, timeStacker);
        sLeaser.sprites[self.HeadSprite].scaleY = 0.8f * MiscHooks.ApplyBreath(data, timeStacker);
    }

    static void DropBugGraphicsUpdate(On.DropBugGraphics.orig_Update orig, DropBugGraphics self)
    {
        orig(self);

        if (BreathCheck(self.bug))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    private static void DropBugCollide(On.DropBug.orig_Collide orig, DropBug self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsIncon(self))
        {
            return;
        }

        if (otherObject is Creature)
        {
            self.AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
            bool flag = myChunk == 0 && self.grasps[0] == null && self.attemptBite > 0f && (self.jumping || self.fromCeilingJump || self.attemptBite > 0.75f) && ((self.fromCeilingJump && self.bodyChunks[2].pos.y > otherObject.bodyChunks[otherChunk].pos.y) || Vector2.Dot(Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos), Custom.DirVec(self.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos)) > Mathf.Lerp(0.7f, -0.2f, self.attemptBite)) && self.AI.DynamicRelationship((otherObject as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats;

            if (ShadowOfOptions.drop_attack.Value && flag)
            {
                if (self.jumping)
                {
                    self.mainBodyChunk.vel *= 0.5f;
                }
                for (int i = 0; i < 4; i++)
                {
                    self.room.AddObject(new WaterDrip(Vector2.Lerp(self.mainBodyChunk.pos, otherObject.bodyChunks[otherChunk].pos, UnityEngine.Random.value), Custom.RNV() * (UnityEngine.Random.value * 14f), false));
                }

                Slash(self, otherObject as Creature, otherObject.bodyChunks[otherChunk]);

                self.attemptBite = 0f;
                self.charging = 0f;
                self.jumping = false;
                self.fromCeilingJump = false;
            }
        }
    }

    static void DropBugUpdate(On.DropBug.orig_Update orig, DropBug self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        if (data.stunTimer > 0)
        {
            data.stunTimer -= 1;
            return;
        }
        if (data.stunCountdown > 0)
        {
            data.stunCountdown -= 1;
        }

        if (self.Stunned)
        {
            if (UnityEngine.Random.value < 0.05f)
            {
                self.bodyChunks[1].vel += Custom.RNV() * (UnityEngine.Random.value * 3f);
            }
            if (ShadowOfOptions.drop_attack.Value && self.stun < 35 && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is not Vulture && self.grabbedBy[0].grabber is not Leech)
            {
                self.grabbedCounter++;
                if (self.grabbedCounter == 50 && UnityEngine.Random.value < 0.85f * self.State.health)
                {
                    Slash(self, self.grabbedBy[0].grabber, null);
                    if (self.grabbedBy.Count == 0)
                    {
                        self.stun = 0;
                    }
                }
                else if (self.grabbedCounter < 50)
                {
                    for (int i = 0; i < self.bodyChunks.Length; i++)
                    {
                        self.bodyChunks[i].pos += Custom.RNV() * (UnityEngine.Random.value * 6f);
                        self.bodyChunks[i].pos += Custom.RNV() * (UnityEngine.Random.value * 6f);
                    }
                }
            }
            else
            {
                self.grabbedCounter = 0;
            }
            return;
        }
        else
        {
            self.grabbedCounter = 0;
        }
        if (self.grabOnNextAttack > 0)
        {
            self.grabOnNextAttack--;
        }
        if (self.releaseGrabbedCounter > 0)
        {
            self.releaseGrabbedCounter--;
            if (self.grasps[0] != null && self.grasps[0].grabbed is Creature && !(self.grasps[0].grabbed as Creature).dead && (self.grasps[0].grabbed as Creature).TotalMass > self.TotalMass * 0.3f)
            {
                Vector2 a = Custom.RNV();
                self.mainBodyChunk.pos += a * 4f;
                self.mainBodyChunk.vel += a * 4f;
                self.grasps[0].grabbedChunk.pos += Vector2.ClampMagnitude(a * 2f / self.grasps[0].grabbedChunk.mass, 5f);
                self.grasps[0].grabbedChunk.vel += Vector2.ClampMagnitude(a * 2f / self.grasps[0].grabbedChunk.mass, 7f);
                if (self.releaseGrabbedCounter == 1)
                {
                    Slash(self, self.grasps[0].grabbed as Creature, null);
                    self.LoseAllGrasps();
                }
            }
            else
            {
                self.releaseGrabbedCounter = 0;
            }
        }

        if (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[0].pos, self.Template) || self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
        {
            self.footingCounter++;
        }

        AIUpdate();
        if (self.Submersion > 0.3f)
        {
            Swim();
            self.swimming = true;
            return;
        }
        self.swimming = false;
        if (UnityEngine.Random.value < 0.005f)
        {
            self.walkBackwardsDist = UnityEngine.Random.value * 20f;
        }
        self.inCeilingMode = 0f;
        self.luredToDropCounter = 0;
        if (self.jumping)
        {
            bool flag = false;
            for (int j = 0; j < self.bodyChunks.Length; j++)
            {
                if ((self.bodyChunks[j].ContactPoint.x != 0 || self.bodyChunks[j].ContactPoint.y != 0) && self.room.aimap.TileAccessibleToCreature(self.bodyChunks[j].pos, self.Template))
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
            if (self.jumpAtChunk != null && self.room.VisualContact(self.mainBodyChunk.pos, self.jumpAtChunk.pos))
            {
                self.bodyChunks[0].vel += Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtChunk.pos) * 1.2f;
                self.bodyChunks[1].vel -= Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtChunk.pos) * 0.4f;
                self.bodyChunks[2].vel -= Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtChunk.pos) * 0.4f;
                if (self.fromCeilingJump && self.bodyChunks[0].pos.y > self.jumpAtChunk.pos.y + 250f && Custom.DistLess(self.bodyChunks[0].pos, self.jumpAtChunk.pos, 350f))
                {
                    self.bodyChunks[0].vel.x += Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtChunk.pos + self.jumpAtChunk.vel).x * 3f;
                }
            }
            else if (self.jumpAtPos != Vector2.zero)
            {
                self.bodyChunks[0].vel += Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtPos) * 1.2f;
                self.bodyChunks[1].vel -= Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtPos) * 0.4f;
                self.bodyChunks[2].vel -= Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtPos) * 0.4f;
                if (self.fromCeilingJump && self.bodyChunks[0].pos.y > self.jumpAtPos.y + 250f && Custom.DistLess(self.bodyChunks[0].pos, self.jumpAtPos, 350f))
                {
                    self.bodyChunks[0].vel.x += Custom.DirVec(self.bodyChunks[0].pos, self.jumpAtPos).x * 3f;
                }
            }
            if (self.Footing)
            {
                self.jumping = false;
                self.jumpAtChunk = null;
            }
            return;
        }

        if (!self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) && !self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
        {
            self.footingCounter = Custom.IntClamp(self.footingCounter - 3, 0, 35);
        }
        if (ShadowOfOptions.drop_attack.Value && self.Footing && self.charging > 0f)
        {
            self.sitting = true;
            self.GoThroughFloors = false;
            self.charging += 0.06666667f/5f;
            Vector2? vector = null;
            if (self.jumpAtPos != Vector2.zero)
            {
                vector = new Vector2?(Custom.DirVec(self.mainBodyChunk.pos, self.jumpAtPos));
            }
            if (self.jumpAtChunk != null)
            {
                vector = new Vector2?(Custom.DirVec(self.mainBodyChunk.pos, self.jumpAtChunk.pos));
            }
            if (vector != null)
            {
                self.bodyChunks[0].vel += vector.Value * Mathf.Pow(self.charging, 2f);
                self.bodyChunks[1].vel -= vector.Value * self.charging;
            }
            if (self.charging >= 1f)
            {
                InconAct(data);
                Attack();
            }
        }
        else if ((self.room.GetWorldCoordinate(self.mainBodyChunk.pos) == self.AI.pathFinder.GetDestination || self.room.GetWorldCoordinate(self.bodyChunks[1].pos) == self.AI.pathFinder.GetDestination) && self.AI.threatTracker.Utility() < 0.5f && !self.safariControlled)
        {
            self.sitting = true;
            self.GoThroughFloors = false;
        }
        else
        {
            self.GoThroughFloors = false;
        }

        float num = self.runCycle;
        if (!Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 2f))
        {
            self.runCycle += 0.125f;
        }
        if (num < Mathf.Floor(self.runCycle))
        {
            self.room.PlaySound(SoundID.Drop_Bug_Step, self.mainBodyChunk);
        }

        void AIUpdate()
        {
            DropBugAI ai = self.AI;

            MiscHooks.AIUpdate(ai);
            if (ai.bug.room == null)
            {
                return;
            }
            if (ai.bug.sitting)
            {
                ai.noiseTracker.hearingSkill = 1f;
            }
            else
            {
                ai.noiseTracker.hearingSkill = 0.3f;
            }
            ai.utilityComparer.GetUtilityTracker(ai.preyTracker).weight = 0.07f + ((ai.targetCreature != null) ? 0.83f : 0.57f) * Mathf.InverseLerp(0f, 100f, (float)ai.attackCounter);
            ai.utilityComparer.GetUtilityTracker(ai.ceilingModule).weight = Mathf.InverseLerp(200f, 0f, (float)ai.attackCounter) * 0.7f;
            if (ai.attackCounter > 0)
            {
                ai.attackCounter--;
            }
            else
            {
                ai.targetCreature = null;
            }
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = DropBugAI.Behavior.Flee;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = DropBugAI.Behavior.Hunt;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = DropBugAI.Behavior.EscapeRain;
                }
                else if (aimodule is StuckTracker)
                {
                    ai.behavior = DropBugAI.Behavior.GetUnstuck;
                }
                else if (aimodule is DropBugAI.CeilingSitModule)
                {
                    ai.behavior = DropBugAI.Behavior.SitInCeiling;
                }
                else if (aimodule is InjuryTracker)
                {
                    ai.behavior = DropBugAI.Behavior.Injured;
                }
            }
            if (ai.currentUtility < 0.05f)
            {
                ai.behavior = DropBugAI.Behavior.Idle;
            }
            if (ai.currentUtility < 0.6f && !ai.ceilingModule.AnyWhereToSitInRoom)
            {
                ai.currentUtility = 0.6f;
                ai.behavior = DropBugAI.Behavior.LeaveRoom;
            }
            if (ai.behavior != DropBugAI.Behavior.Flee && ai.bug.grasps[0] != null && ai.bug.grasps[0].grabbed is Creature && ai.DynamicRelationship((ai.bug.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                ai.behavior = DropBugAI.Behavior.ReturnPrey;
                ai.currentUtility = 1f;
            }
            if (ai.behavior != DropBugAI.Behavior.Idle)
            {
                ai.tempIdlePos = ai.creature.pos;
            }
            if (ai.behavior == DropBugAI.Behavior.Injured && ai.preyTracker.Utility() > 0.4f && ai.preyTracker.MostAttractivePrey != null && ai.creature.pos.room == ai.preyTracker.MostAttractivePrey.BestGuessForPosition().room && ai.creature.pos.Tile.FloatDist(ai.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile) < 6f)
            {
                ai.behavior = DropBugAI.Behavior.Hunt;
                ai.utilityComparer.GetUtilityTracker(ai.preyTracker).weight = 1f;
            }

            if (ai.behavior == DropBugAI.Behavior.Hunt)
            {
                if (ai.preyTracker.MostAttractivePrey != null)
                {
                    ai.focusCreature = ai.preyTracker.MostAttractivePrey;

                    if (ShadowOfOptions.drop_attack.Value && ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && ai.bug.grasps[0] == null && !ai.bug.jumping && ai.bug.attemptBite == 0f && ai.bug.charging == 0f && ai.bug.Footing && ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.room == ai.bug.room)
                    {
                        BodyChunk bodyChunk = ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length)];
                        if (Custom.DistLess(ai.bug.mainBodyChunk.pos, bodyChunk.pos, 120f) && (ai.bug.room.aimap.TileAccessibleToCreature(ai.bug.room.GetTilePosition(ai.bug.bodyChunks[1].pos - Custom.DirVec(ai.bug.bodyChunks[1].pos, bodyChunk.pos) * 30f), ai.bug.Template) || ai.bug.room.GetTile(ai.bug.bodyChunks[1].pos - Custom.DirVec(ai.bug.bodyChunks[1].pos, bodyChunk.pos) * 30f).Solid) && ai.bug.room.VisualContact(ai.bug.mainBodyChunk.pos, bodyChunk.pos))
                        {
                            if (Vector2.Dot((ai.bug.mainBodyChunk.pos - bodyChunk.pos).normalized, (ai.bug.bodyChunks[1].pos - ai.bug.mainBodyChunk.pos).normalized) > 0.2f)
                            {
                                ai.bug.InitiateJump(bodyChunk);
                            }
                            else
                            {
                                ai.bug.mainBodyChunk.vel += Custom.DirVec(ai.bug.mainBodyChunk.pos, bodyChunk.pos) * 2f;
                                ai.bug.bodyChunks[1].vel -= Custom.DirVec(ai.bug.mainBodyChunk.pos, bodyChunk.pos) * 2f;
                            }
                        }
                    }
                }
            }
            else if (ai.behavior == DropBugAI.Behavior.Flee)
            {
                ai.focusCreature = ai.threatTracker.mostThreateningCreature;
            }
            else if (ai.behavior == DropBugAI.Behavior.EscapeRain || ai.behavior == DropBugAI.Behavior.ReturnPrey || ai.behavior == DropBugAI.Behavior.Injured)
            {
                ai.focusCreature = null;
            }
            if (ai.noiseRectionDelay > 0)
            {
                ai.noiseRectionDelay--;
            }
        }

        void Swim()
        {
            self.bodyChunks[0].vel *= 1f - 0.1f * self.bodyChunks[0].submersion;
            self.bodyChunks[1].vel *= 1f - 0.2f * self.bodyChunks[1].submersion;
            self.runCycle += 0.125f;
            self.GoThroughFloors = true;
            self.bodyChunks[0].vel *= 0.8f;
            //self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
            if (!self.safariControlled || self.Submersion < 0.5f)
            {
                self.footingCounter = Math.Max(self.footingCounter, 25);
                self.outOfWaterFooting = 20;
                return;
            }
            self.mainBodyChunk.vel *= 0.75f;
            self.footingCounter = 0;
            self.outOfWaterFooting = 0;
        }

        void Attack()
        {
            if (self.grasps[0] != null || (self.jumpAtChunk == null && self.jumpAtPos == Vector2.zero) || (self.jumpAtChunk != null && (self.jumpAtChunk.owner.room != self.room || !self.room.VisualContact(self.mainBodyChunk.pos, self.jumpAtChunk.pos))))
            {
                self.charging = 0f;
                self.jumpAtChunk = null;
                return;
            }
            Vector2? vector = null;
            if (self.jumpAtPos != Vector2.zero)
            {
                vector = new Vector2?(self.jumpAtPos);
            }
            if (self.jumpAtChunk != null)
            {
                vector = new Vector2?(self.jumpAtChunk.pos);
            }
            if (vector == null)
            {
                return;
            }
            Vector2 p = new Vector2(vector.Value.x, vector.Value.y);
            if (!self.room.GetTile(vector.Value + new Vector2(0f, 20f)).Solid)
            {
                Vector2? vector2 = vector;
                Vector2 b = new Vector2(0f, Mathf.InverseLerp(40f, 200f, Vector2.Distance(self.mainBodyChunk.pos, vector.Value)) * 20f);
                vector = vector2 + b;
            }
            Vector2 vector3 = Custom.DirVec(self.mainBodyChunk.pos, vector.Value);
            if (!Custom.DistLess(self.mainBodyChunk.pos, p, Custom.LerpMap(Vector2.Dot(vector3, Custom.DirVec(self.bodyChunks[1].pos, self.bodyChunks[0].pos)), -0.1f, 0.8f, 0f, 300f, 0.4f)))
            {
                self.charging = 0f;
                self.jumpAtChunk = null;
                return;
            }
            if (!self.room.GetTile(self.mainBodyChunk.pos + new Vector2(0f, 20f)).Solid && !self.room.GetTile(self.bodyChunks[1].pos + new Vector2(0f, 20f)).Solid)
            {
                vector3 = Vector3.Slerp(vector3, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Distance(self.mainBodyChunk.pos, vector.Value), 40f, 200f, 0.05f, 0.2f));
            }
            self.room.PlaySound(SoundID.Drop_Bug_Jump, self.mainBodyChunk);
            if (self.voiceSound == null || self.voiceSound.slatedForDeletetion)
            {
                self.voiceSound = self.room.PlaySound(SoundID.Drop_Bug_Voice, self.mainBodyChunk);
            }
            Jump(vector3);

            void Jump(Vector2 jumpDir)
            {
                float num = Custom.LerpMap(jumpDir.y, -1f, 1f, 0.7f, 1.2f, 1.1f);
                self.footingCounter = 0;
                self.mainBodyChunk.vel *= 0.5f;
                self.bodyChunks[1].vel *= 0.5f;
                self.mainBodyChunk.vel += jumpDir * (15.5f * num);
                self.bodyChunks[1].vel += jumpDir * (8f * num);
                self.attemptBite = 1f;
                self.charging = 0f;
                self.jumping = true;
            }
        }
    }

    public static void Slash(DropBug self, Creature creature, BodyChunk chunk)
    {
        if (chunk == null)
        {
            chunk = creature.mainBodyChunk;
            float dst = float.MaxValue;
            for (int i = 0; i < creature.bodyChunks.Length; i++)
            {
                if (Custom.DistLess(self.mainBodyChunk.pos, creature.bodyChunks[i].pos, dst))
                {
                    dst = Vector2.Distance(self.mainBodyChunk.pos, creature.bodyChunks[i].pos);
                    chunk = creature.bodyChunks[i];
                }
            }
        }
        bool flag = UnityEngine.Random.value < 0.33333334f;
        bool flag2 = UnityEngine.Random.value < 0.2f;
        self.mainBodyChunk.vel = Custom.DirVec(chunk.pos, self.mainBodyChunk.pos) * 8f;
        self.grabOnNextAttack = 180;
        for (int j = 0; j < 5; j++)
        {
            self.room.AddObject(new WaterDrip(Vector2.Lerp(self.mainBodyChunk.pos, chunk.pos, UnityEngine.Random.value), Custom.RNV() * (UnityEngine.Random.value * (flag2 ? 24f : 14f)), false));
        }
        if (self.AI.DynamicRelationship(creature.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
        {
            if (flag || flag2 || creature.dead)
            {
                self.AI.attackCounter = Math.Max(70, self.AI.attackCounter);
            }
            self.AI.targetCreature = creature.abstractCreature;
        }
        self.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, self.mainBodyChunk);
    }
}