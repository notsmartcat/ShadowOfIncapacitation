using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.CentipedeHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Centipede.Update += CentipedeUpdate;

        On.CentipedeGraphics.DrawSprites += CentipedeGraphicsDrawSprites;

        On.CentipedeGraphics.Update += CentipedeGraphicsUpdate;

        On.Centipede.Collide += CentipedeCollide;

        On.Centipede.UpdateGrasp += CentipedeUpdateGrasp;
    }

    static void CentipedeUpdateGrasp(On.Centipede.orig_UpdateGrasp orig, Centipede self, int g)
    {
        if (!IsComa(self))
        {
            orig(self, g);
            return;
        }

        if (!IsIncon(self) || UnityEngine.Random.value < 0.025f)
        {
            self.ReleaseGrasp(g);
            return;
        }

        BodyChunk bodyChunk = self.bodyChunks[(g == 0) ? 0 : (self.bodyChunks.Length - 1)];
        float num = Vector2.Distance(bodyChunk.pos, self.grasps[g].grabbedChunk.pos);
        if (num > 50f + self.grasps[g].grabbedChunk.rad)
        {
            self.ReleaseGrasp(g);
            return;
        }
        Vector2 a = Custom.DirVec(bodyChunk.pos, self.grasps[g].grabbedChunk.pos);
        float rad = self.grasps[g].grabbedChunk.rad;
        float num2 = 0.95f;
        float num3 = self.grasps[g].grabbedChunk.mass / (self.grasps[g].grabbedChunk.mass + bodyChunk.mass);
        Vector2 b = a * ((rad - num) * num3 * num2);
        bodyChunk.pos -= b;
        bodyChunk.vel -= b;
        Vector2 b2 = a * ((rad - num) * (1f - num3) * num2);
        self.grasps[g].grabbedChunk.pos += b2;
        self.grasps[g].grabbedChunk.vel += b2;
        if (self.grasps[1 - g] == null)
        {
            BodyChunk bodyChunk2 = self.bodyChunks[(g != 0) ? 0 : (self.bodyChunks.Length - 1)];
            int i = 0;
            while (i < self.grasps[g].grabbed.bodyChunks.Length)
            {
                if (Custom.DistLess(bodyChunk2.pos, self.grasps[g].grabbed.bodyChunks[i].pos, bodyChunk2.rad + self.grasps[g].grabbed.bodyChunks[i].rad + 10f))
                {
                    BodyChunk bodyChunk3 = self.grasps[g].grabbed.bodyChunks[i];
                    Vector2 a2 = Custom.DirVec(bodyChunk2.pos, bodyChunk3.pos);
                    rad = bodyChunk3.rad;
                    num2 = 0.95f;
                    num3 = bodyChunk3.mass / (self.grasps[g].grabbedChunk.mass + bodyChunk2.mass);
                    Vector2 b3 = a2 * ((rad - num) * num3 * num2);
                    bodyChunk2.pos -= b3;
                    bodyChunk2.vel -= b3;
                    Vector2 b4 = a2 * ((rad - num) * (1f - num3) * num2);
                    bodyChunk3.pos += b4;
                    bodyChunk3.vel += b4;
                    self.shockCharge += 1f / Mathf.Lerp(100f, 5f, self.size);
                    if (!self.safariControlled && self.shockCharge >= 1f)
                    {
                        self.Shock(self.grasps[g].grabbed);
                        self.shockCharge = 0f;
                        return;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }
        }
    }

    static void CentipedeCollide(On.Centipede.orig_Collide orig, Centipede self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsIncon(self))
        {
            return;
        }

        self.AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);

        if (self.AI.DoIWantToShockCreature((otherObject as Creature).abstractCreature) && ShadowOfOptions.centi_grab.Value)
        {
            for (int i = 0; i < 2; i++)
            {
                if (myChunk == ((i == 0) ? 0 : (self.bodyChunks.Length - 1)) && self.grasps[i] == null)
                {
                    bool flag = true;
                    int num = 0;
                    while (num < self.grabbedBy.Count && flag)
                    {
                        if (self.grabbedBy[num].grabber == otherObject)
                        {
                            flag = false;
                        }
                        num++;
                    }
                    if (self.Centiwing && (self.room.aimap.getAItile(self.bodyChunks[myChunk].pos).fallRiskTile.y < 0 || UnityEngine.Random.value < 0.5f))
                    {
                        flag = false;
                    }
                    if (self.shockGiveUpCounter > 0)
                    {
                        flag = false;
                    }
                    if (flag)
                    {
                        self.room.PlaySound(SoundID.Centipede_Attach, self.bodyChunks[myChunk]);
                        self.Grab(otherObject, i, otherChunk, Creature.Grasp.Shareability.NonExclusive, 1f, false, false);
                    }
                }
            }
        }
    }

    static void CentipedeGraphicsUpdate(On.CentipedeGraphics.orig_Update orig, CentipedeGraphics self)
    {
        orig(self);

        if (IsUncon(self.centipede))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    static void CentipedeGraphicsDrawSprites(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || !IsUncon(self.centipede))
        {
            return;
        }

        float num4 = (Mathf.Sin(Mathf.Lerp(data.lastBreath, data.breath, timeStacker) * 3.1415927f * 2f) + 1f) * 0.5f;

        //sLeaser.sprites[self.BodySprite].scale = num6;

        for (int i = 0; i < self.owner.bodyChunks.Length; i++)
        {
            float num = (float)i / (float)(self.owner.bodyChunks.Length - 1);
            Vector2 normalized = self.RotatAtChunk(i, timeStacker).normalized;

            float num2 = Mathf.Clamp(Mathf.Sin(num * 3.1415927f), 0f, 1f);
            num2 *= Mathf.Lerp(1f, 0.5f, self.centipede.size);
            if (self.centipede.Centiwing)
            {
                num2 = Mathf.Lerp(0.6f, 0.3f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(num * 3.1415927f), 0f, 1f), 2f));
            }
            else if (self.centipede.AquaCenti)
            {
                num2 = Mathf.Lerp(0.8f, 0.2f, Mathf.Pow(Mathf.Clamp(Mathf.Sin(num * 3.1415927f), 0f, 1f), 2f));
            }
            sLeaser.sprites[self.SegmentSprite(i)].scaleX = self.owner.bodyChunks[i].rad * (Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num2) * 2f * 0.0625f) *1f + num4 * (float)3 * 0.1f * 0.5f;
            for (int k = 0; k < (self.centipede.AquaCenti ? 2 : 1); k++)
            {
                if (normalized.y > 0f)
                {
                    sLeaser.sprites[self.ShellSprite(i, k)].scaleX = (self.owner.bodyChunks[i].rad * Mathf.Lerp(1f, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(normalized.x)), num2) * 1.8f * normalized.y * 0.071428575f) * 1f + num4 * (float)3 * 0.1f * 0.5f;
                    //sLeaser.sprites[self.ShellSprite(i, k)].scaleY = (self.owner.bodyChunks[i].rad * (self.centipede.Red ? 1.7f : 1.5f) * 0.09090909f) *1f + num4 * (float)3 * 0.1f * 0.5f;
                }
                else
                {
                    sLeaser.sprites[self.ShellSprite(i, k)].scaleX = (self.owner.bodyChunks[i].rad * -1.8f * normalized.y * 0.071428575f) * 1f + num4 * (float)3 * 0.1f * 0.5f;
                    //sLeaser.sprites[self.ShellSprite(i, k)].scaleY = (self.owner.bodyChunks[i].rad * 1.3f * 0.09090909f) * 1f + num4 * (float)3 * 0.1f * 0.5f;
                }
            }
            if (i > 0)
            {
                sLeaser.sprites[self.SecondarySegmentSprite(i - 1)].scaleX = self.owner.bodyChunks[i].rad * Mathf.Lerp(0.9f, Mathf.Lerp(1.1f, 0.8f, Mathf.Abs(normalized.x)), num2) * 2f;
            }
        }
    }

    static void CentipedeUpdate(On.Centipede.orig_Update orig, Centipede self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        if (ShadowOfOptions.centi_shock.Value && self.grabbedBy.Count > 0 && self.Small)
        {
            self.shockCharge += IsInconBase(self) ? 0.016666668f : 0.016666668f * 0.75f;
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                self.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value;
            }
            if (self.shockCharge >= 1f)
            {
                Creature grabber = self.grabbedBy[0].grabber;
                self.Shock(grabber);
                if (ModManager.MSC && grabber is Player player && player.SlugCatClass == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    player.SaintStagger(480);
                }
                self.Stun(20);
                for (int j = 0; j < self.bodyChunks.Length; j++)
                {
                    self.bodyChunks[j].vel += Custom.RNV() * (UnityEngine.Random.value * 3.5f);
                }
            }
        }

        if (self.room.game.devToolsActive && Input.GetKey("b") && self.room.game.cameras[0].room == self.room)
        {
            self.Stun(12);
        }

        if (data.stunTimer > 0)
        {
            self.wantToFly = true;
            data.stunTimer -= 1;
        }
        else
        {
            self.wantToFly = false;
        }
        if (data.stunCountdown > 0)
        {
            data.stunCountdown -= 1;
        }

        if (self.Stunned)
        {
            return;
        }

        if (self.AquacentiSwim)
        {
            Act();
        }
        else if (self.Submersion > 0.5f && !self.flying)
        {
            Swim();
        }
        else
        {
            Act();
        }

        void Act()
        {
            self.moving = false;

            if (self.AquacentiSwim)
            {
                self.flyModeCounter = 100;
            }
            if (self.Centiwing)
            {
                if (self.wantToFly)
                {
                    if (self.flyModeCounter == 100)
                    {
                        self.flying = true;
                    }
                    self.flyModeCounter = Math.Min(100, self.flyModeCounter + 1);
                }
                else
                {
                    if (self.flyModeCounter < 90)
                    {
                        self.flying = false;
                    }
                    self.flyModeCounter = Math.Max(0, self.flyModeCounter - 1);
                }
                float num = Mathf.InverseLerp(80f, 100f, (float)self.flyModeCounter);
                if (self.wingsStartedUp < num)
                {
                    self.wingsStartedUp = Mathf.Min(1f, self.wingsStartedUp + 0.025f);
                }
                else
                {
                    self.wingsStartedUp = Mathf.Max(0f, self.wingsStartedUp - 0.025f);
                }
                self.wingsStartedUp = Mathf.Lerp(self.wingsStartedUp, num, 0.05f);
            }
            if (self.directionChangeBlock > 0)
            {
                self.directionChangeBlock -= (self.moving ? 1 : 0);
            }
            else if (!self.flying && !self.safariControlled)
            {
                if ((self.AI.pathFinder as CentipedePather).TileClosestToGoal(self.room.GetWorldCoordinate(self.bodyChunks[self.bodyDirection ? (self.bodyChunks.Length - 1) : 0].pos), self.room.GetWorldCoordinate(self.bodyChunks[self.bodyDirection ? 0 : (self.bodyChunks.Length - 1)].pos)))
                {
                    self.changeDirCounter++;
                    bool flag = self.room.aimap.getTerrainProximity(self.bodyChunks[self.bodyDirection ? 0 : (self.bodyChunks.Length - 1)].pos) > 1 || self.room.aimap.getTerrainProximity(self.bodyChunks[self.bodyDirection ? (self.bodyChunks.Length - 1) : 0].pos) > 1;
                    if (self.changeDirCounter > (self.Centiwing ? (flag ? 40 : 10) : (flag ? 10 : 2)))
                    {
                        self.bodyDirection = !self.bodyDirection;
                        self.directionChangeBlock = 40;
                        self.changeDirCounter = 0;
                    }
                }
                else
                {
                    self.changeDirCounter = 0;
                }
            }
            AIUpdate();
            //self.moving = (self.AI.run > 0f && Custom.ManhattanDistance(self.room.GetWorldCoordinate(self.HeadChunk.pos), self.AI.pathFinder.GetDestination) > 2);
            if (self.grasps[0] == null && self.grasps[1] == null)
            {
                if (self.flying || self.AquacentiSwim)
                {
                    if (self.AquaCenti)
                    {
                        self.buoyancy = 0.78f;
                    }
                    //self.Fly();
                }
                else
                {
                    //self.Crawl();
                }

                self.doubleGrabCharge = Mathf.Max(0f, self.doubleGrabCharge - 0.025f);
                self.shockGiveUpCounter = Math.Max(0, self.shockGiveUpCounter - 2);
            }
            else
            {
                if (self.AquaCenti)
                {
                    self.buoyancy = 0.15f;
                }
                if (self.grasps[0] == null || self.grasps[1] == null)
                {
                    for (int num7 = 0; num7 < self.grasps.Length; num7++)
                    {
                        if (self.grasps[num7] != null && self.grasps[num7].grabbed is Creature && self.AI.DoIWantToShockCreature((self.grasps[num7].grabbed as Creature).abstractCreature))
                        {
                            self.moveToPos = self.grasps[num7].grabbedChunk.pos;
                            if (!self.room.VisualContact(self.HeadChunk.pos, self.grasps[num7].grabbedChunk.pos))
                            {
                                if (self.HeadIndex == 0)
                                {
                                    for (int num8 = self.bodyChunks.Length - 1; num8 >= 0; num8--)
                                    {
                                        if (self.room.VisualContact(self.bodyChunks[num8].pos, self.HeadChunk.pos))
                                        {
                                            self.moveToPos = self.bodyChunks[num8].pos;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int num9 = 0; num9 < self.bodyChunks.Length; num9++)
                                    {
                                        if (self.room.VisualContact(self.bodyChunks[num9].pos, self.HeadChunk.pos))
                                        {
                                            self.moveToPos = self.bodyChunks[num9].pos;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //self.HeadChunk.vel += Custom.DirVec(self.HeadChunk.pos, self.moveToPos) * (Mathf.Pow(self.doubleGrabCharge, 2f) * 6f * Mathf.Lerp(0.7f, 1.3f, self.size));
                self.doubleGrabCharge = Mathf.Min(1f, self.doubleGrabCharge + 0.0125f);
                if (self.doubleGrabCharge > 0.9f)
                {
                    self.shockGiveUpCounter = Math.Min(110, self.shockGiveUpCounter + 1);
                    if (self.shockGiveUpCounter >= 110)
                    {
                        self.Stun(12);
                        self.shockGiveUpCounter = 30;
                        self.LoseAllGrasps();
                    }
                }
            }
            if (self.AI.preyTracker.MostAttractivePrey != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.collisionLayer != self.collisionLayer)
            {
                for (int num10 = 0; num10 < self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks.Length; num10++)
                {
                    for (int num11 = 0; num11 < self.bodyChunks.Length; num11++)
                    {
                        if (Custom.DistLess(self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num10].pos, self.bodyChunks[num11].pos, self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.bodyChunks[num10].rad + self.bodyChunks[num11].rad))
                        {
                            self.Collide(self.AI.preyTracker.MostAttractivePrey.representedCreature.realizedCreature, num11, num10);
                        }
                    }
                }
            }
        }

        void AIUpdate()
        {
            CentipedeAI ai = self.AI;

            MiscHooks.AIUpdate(ai);

            if (ModManager.MSC && ai.centipede.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.centipede.LickedByPlayer.abstractCreature);
            }
            if (ai.annoyingCollisions > 0)
            {
                ai.annoyingCollisions--;
            }
            if (ai.noiseTracker != null)
            {
                ai.noiseTracker.hearingSkill = (ai.centipede.moving ? 0f : 1.5f);
            }
            if (ai.preyTracker.MostAttractivePrey != null && !ai.centipede.Red)
            {
                ai.utilityComparer.GetUtilityTracker(ai.preyTracker).weight = Mathf.InverseLerp(50f, 10f, (float)ai.preyTracker.MostAttractivePrey.TicksSinceSeen);
            }
            if (ai.threatTracker.mostThreateningCreature != null)
            {
                ai.utilityComparer.GetUtilityTracker(ai.threatTracker).weight = Mathf.InverseLerp(500f, 100f, (float)ai.threatTracker.mostThreateningCreature.TicksSinceSeen);
            }
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = CentipedeAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = CentipedeAI.Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = CentipedeAI.Behavior.Hunt;
                }
                else if (aimodule is NoiseTracker)
                {
                    ai.behavior = CentipedeAI.Behavior.InvestigateSound;
                }
                else if (aimodule is InjuryTracker)
                {
                    ai.behavior = CentipedeAI.Behavior.Injured;
                }
            }
            if (ai.currentUtility < 0.1f)
            {
                ai.behavior = CentipedeAI.Behavior.Idle;
            }
            float b = 0f;
            if (ai.behavior == CentipedeAI.Behavior.Idle)
            {

            }
            else if (ai.behavior == CentipedeAI.Behavior.Flee)
            {
                b = 1f;
                InconAct();
            }
            else if (ai.behavior == CentipedeAI.Behavior.EscapeRain)
            {
                b = 0.5f;
                InconAct();
            }
            else if (ai.behavior == CentipedeAI.Behavior.Injured)
            {
                b = 1f;
            }
            else if (ai.behavior == CentipedeAI.Behavior.Hunt)
            {
                b = ai.DynamicRelationship(ai.preyTracker.MostAttractivePrey).intensity;
                InconAct();
            }
            else if (ai.behavior == CentipedeAI.Behavior.InvestigateSound)
            {
                b = 0.2f;
                InconAct();
            }
            ai.excitement = Mathf.Lerp(ai.excitement, b, 0.1f);
            if (ai.centipede.Centiwing || (ai.centipede.Red && ai.behavior == CentipedeAI.Behavior.Hunt))
            {
                ai.run = 500f;
                return;
            }
            ai.run -= 1f;
            if (ai.run < Mathf.Lerp(-50f, -5f, ai.excitement))
            {
                ai.run = Mathf.Lerp(30f, 50f, ai.excitement);
            }
            int num = 0;
            float num2 = 0f;
            for (int i = 0; i < ai.tracker.CreaturesCount; i++)
            {
                if (ai.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Centipede && ai.tracker.GetRep(i).representedCreature.realizedCreature != null && ai.tracker.GetRep(i).representedCreature.Room == ai.creature.Room && (ai.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).AI.run > 0f == ai.run > 0f)
                {
                    num2 += (ai.tracker.GetRep(i).representedCreature.realizedCreature as Centipede).AI.run;
                    num++;
                }
            }
            if (num > 0)
            {
                ai.run = Mathf.Lerp(ai.run, num2 / (float)num, 0.1f);
            }
        }

        void Swim()
        {
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                //self.bodyChunks[i].vel += Custom.DirVec(movementConnection.StartTile.ToVector2(), movementConnection.DestTile.ToVector2()) * 0.05f + Custom.RNV() * (UnityEngine.Random.value * 4f);
                if (ModManager.DLCShared)
                {
                    self.bodyChunks[i].vel.y += Mathf.Clamp(self.room.WaterLevelDisplacement(self.bodyChunks[i].pos), -5f, 5f) * 0.05f;
                }
                else
                {
                    self.bodyChunks[i].vel.y += Mathf.Clamp(self.room.FloatWaterLevel(self.bodyChunks[i].pos) - self.bodyChunks[i].pos.y, -5f, 5f) * 0.05f;
                }
            }
        }

        void InconAct()
        {
            if (data.stunCountdown > 0)
            {
                return;
            }

            data.stunTimer = Mathf.Max(data.stunTimer, UnityEngine.Random.Range(30, 71));

            data.stunCountdown = data.stunTimer + UnityEngine.Random.Range(30, 71);
        }
    }
}