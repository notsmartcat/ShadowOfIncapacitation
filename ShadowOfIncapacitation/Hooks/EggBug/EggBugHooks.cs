using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.EggBugHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.EggBug.ctor += NewEggBug;

        On.EggBug.Update += EggBugUpdate;

        On.EggBugAI.CreatureSpotted += EggBugAICreatureSpotted;

        On.EggBugGraphics.Update += EggBugGraphicsUpdate;
        On.EggBugGraphics.DrawSprites += EggBugGraphicsDrawSprites;

        new Hook(
            typeof(EggBugGraphics).GetProperty(nameof(EggBugGraphics.ShowEggs)).GetGetMethod(), ShadowOfEggBugGraphicsShowEggs);
    }

    static void EggBugGraphicsDrawSprites(On.EggBugGraphics.orig_DrawSprites orig, EggBugGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[self.HeadSprite].scaleX = (self.bug.FireBug ? 0.85f : 0.45f) * MiscHooks.ApplyBreath(data, timeStacker);
        sLeaser.sprites[self.HeadSprite].scaleY = (self.bug.FireBug ? 0.95f : 0.55f) * MiscHooks.ApplyBreath(data, timeStacker);
    }

    static void EggBugGraphicsUpdate(On.EggBugGraphics.orig_Update orig, EggBugGraphics self)
    {
        orig(self);

        if (BreathCheck(self.bug))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    static void EggBugAICreatureSpotted(On.EggBugAI.orig_CreatureSpotted orig, EggBugAI self, bool firstSpot, Tracker.CreatureRepresentation otherCreature)
    {
        if (!inconstorage.TryGetValue(self.bug.abstractCreature, out InconData data) || !IsComa(self.bug))
        {
            orig(self, firstSpot, otherCreature);
            return;
        }

        if (!firstSpot && UnityEngine.Random.value > self.fear)
        {
            return;
        }
        CreatureTemplate.Relationship relationship = self.DynamicRelationship(otherCreature);
        if (relationship.type == CreatureTemplate.Relationship.Type.Ignores)
        {
            return;
        }
        if (!self.bug.safariControlled && firstSpot && relationship.type == CreatureTemplate.Relationship.Type.Afraid && relationship.intensity > 0.06f && Custom.DistLess(self.bug.DangerPos, self.bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()), Custom.LerpMap(relationship.intensity, 0.06f, 0.5f, 50f, 300f)))
        {
            TryJump(self.bug, self.bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()));
        }
        if (relationship.intensity > (firstSpot ? 0.02f : 0.1f))
        {
            Suprise(self.bug.room.MiddleOfTile(otherCreature.BestGuessForPosition()));
        }

        void Suprise(Vector2 surprisePos)
        {
            if (!IsIncon(self.bug))
            {
                return;
            }

            if (Custom.DistLess(surprisePos, self.bug.mainBodyChunk.pos, 300f))
            {
                for (int i = 0; i < self.bug.bodyChunks.Length; i++)
                {
                    if (self.bug.room.aimap.TileAccessibleToCreature(self.bug.bodyChunks[i].pos, self.bug.Template))
                    {
                        self.bug.bodyChunks[i].vel += (Custom.RNV() * 4f + Custom.DirVec(surprisePos, self.bug.bodyChunks[i].pos) * 2f) * (0.5f + 0.5f * self.fear);
                    }
                }
            }

            self.bug.shake = Math.Max(self.bug.shake, UnityEngine.Random.Range(5, 15));
            self.fear = Custom.LerpAndTick(self.fear, 1f, 0.3f, 0.142857149f);
            self.bug.Squirt(self.fear);
        }
    }

    static void EggBugUpdate(On.EggBug.orig_Update orig, EggBug self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self))
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

        if (self.grabbedBy.Count > 0)
        {
            if (IsIncon(self))
            {
                for (int m = 0; m < self.bodyChunks.Length; m++)
                {
                    self.bodyChunks[m].vel += Custom.RNV() * 2f;
                }
                AIUpdate();
            }
            self.footingCounter = 0;
            self.travelDir *= 0f;
        }

        if (self.Stunned || !IsIncon(self) || data.stunTimer > 0)
        {
            self.footingCounter = 0;
            return;
        }

        self.footingCounter++;

        if (self.Submersion > 0.3f)
        {
            Swim();
            AIUpdate();
            return;
        }
        if (self.specialMoveCounter > 0)
        {
            self.specialMoveCounter--;
            //self.MoveTowards(self.room.MiddleOfTile(self.specialMoveDestination));
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
            if (!self.safariControlled && ((self.FireBug && self.grasps[0] != null) || ((!self.FireBug || self.eggsLeft > 0) && (self.room.GetWorldCoordinate(self.mainBodyChunk.pos) == self.AI.pathFinder.GetDestination || self.room.GetWorldCoordinate(self.bodyChunks[1].pos) == self.AI.pathFinder.GetDestination) && self.AI.threatTracker.Utility() < 0.5f)))
            {
                self.sitting = true;
                self.GoThroughFloors = false;
            }
            else
            {
                MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.mainBodyChunk.pos), true);
                if (movementConnection == default(MovementConnection))
                {
                    movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[1].pos), true);
                }
                if (movementConnection != default(MovementConnection))
                {
                    self.travelDir = Vector2.Lerp(self.travelDir, Custom.DirVec(self.mainBodyChunk.pos, self.room.MiddleOfTile(movementConnection.destinationCoord)), 0.4f);
                }
                else
                {
                    self.GoThroughFloors = false;
                }
            }
        }
        if (self.FireBug && self.eggsLeft <= 0 && self.grasps[0] == null)
        {
            self.sitting = false;
        }
        AIUpdate();
        float num = self.runCycle;
        if (!Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 5f))
        {
            self.runCycle += self.runSpeed / 10f;
        }
        if (num < Mathf.Floor(self.runCycle))
        {
            self.room.PlaySound(SoundID.Egg_Bug_Scurry, self.mainBodyChunk);
        }
        if (self.sitting)
        {
            Vector2 a = new Vector2(0f, 0f);
            for (int i = 0; i < 8; i++)
            {
                if (self.room.GetTile(self.abstractCreature.pos.Tile + Custom.eightDirections[i]).Solid)
                {
                    a -= Custom.eightDirections[i].ToVector2();
                }
            }
            self.awayFromTerrainDir = Vector2.Lerp(self.awayFromTerrainDir, a.normalized, 0.1f);
            return;
        }
        self.awayFromTerrainDir *= 0.7f;

        void Swim()
        {
            self.bodyChunks[0].vel *= 1f - 0.05f * self.bodyChunks[0].submersion;
            self.bodyChunks[1].vel *= 1f - 0.1f * self.bodyChunks[1].submersion;
            self.GoThroughFloors = true;
            self.bodyChunks[0].vel *= 0.9f;
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

        void AIUpdate()
        {
            EggBugAI ai = self.AI;

            MiscHooks.AIUpdate(ai);
            if (ai.bug.room == null)
            {
                return;
            }
            if (ModManager.MSC && ai.bug.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.bug.LickedByPlayer.abstractCreature);
            }
            ai.pathFinder.walkPastPointOfNoReturn = (ai.stranded || ai.denFinder.GetDenPosition() == null || !ai.pathFinder.CoordinatePossibleToGetBackFrom(ai.denFinder.GetDenPosition().Value) || ai.threatTracker.Utility() > 0.95f);
            if (ai.bug.sitting)
            {
                ai.noiseTracker.hearingSkill = 2f;
            }
            else
            {
                ai.noiseTracker.hearingSkill = 0.2f;
            }
            ai.utilityComparer.GetUtilityTracker(ai.threatTracker).weight = Custom.LerpMap(ai.threatTracker.ThreatOfTile(ai.creature.pos, true), 0.1f, 2f, 0.1f, 1f, 0.5f);
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            bool flag = ai.bug.FireBug && ai.bug.eggsLeft <= 0;
            if (flag && ai.currentUtility < 0.02f)
            {
                ai.behavior = EggBugAI.Behavior.Hunt;
            }
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = EggBugAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = EggBugAI.Behavior.EscapeRain;
                }
                else if (ai.bug.FireBug && aimodule is PreyTracker && ai.bug.eggsLeft <= 0 && ai.preyTracker.MostAttractivePrey != null && ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && !ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.dead && ai.bug.grasps[0] == null)
                {
                    ai.behavior = MoreSlugcats.MoreSlugcatsEnums.EggBugBehavior.Kill;
                }
            }
            if (!flag && ai.currentUtility < 0.02f)
            {
                ai.behavior = EggBugAI.Behavior.Idle;
            }
            if (ai.behavior == EggBugAI.Behavior.Idle || ai.behavior == EggBugAI.Behavior.Hunt)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.5f + 0.5f * Mathf.Max(ai.threatTracker.Utility(), ai.fear), 0.01f, 0.016666668f);

                bool flag2 = ai.pathFinder.GetDestination.room != ai.bug.room.abstractRoom.index;
                if (!flag2 && ai.idlePosCounter <= 0)
                {
                    int abstractNode = ai.bug.room.abstractRoom.RandomNodeInRoom().abstractNode;
                    if (ai.bug.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
                    {
                        int num = ai.bug.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, ai.bug.Template);
                        if (num > -1)
                        {
                            int num2 = ai.bug.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(ai.bug.abstractCreature.pos.Tile, num, ai.bug.Template);
                            if (num2 > -1 && num2 < 400)
                            {
                                AbstractRoom abstractRoom = ai.bug.room.game.world.GetAbstractRoom(ai.bug.room.abstractRoom.connections[abstractNode]);
                                if (abstractRoom != null)
                                {
                                    WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
                                    if (ai.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                                    {
                                        ai.idlePosCounter = UnityEngine.Random.Range(200, 500);
                                        flag2 = true;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!flag2)
                {
                    WorldCoordinate coord = new WorldCoordinate(ai.bug.room.abstractRoom.index, UnityEngine.Random.Range(0, ai.bug.room.TileWidth), UnityEngine.Random.Range(0, ai.bug.room.TileHeight), -1);
                    if (ai.IdleScore(coord) < ai.IdleScore(ai.tempIdlePos))
                    {
                        ai.tempIdlePos = coord;
                    }
                    if (ai.IdleScore(ai.tempIdlePos) < ai.IdleScore(ai.pathFinder.GetDestination) + Custom.LerpMap((float)ai.idlePosCounter, 0f, 300f, 100f, -300f))
                    {
                        //ai.SetDestination(ai.tempIdlePos);
                        ai.idlePosCounter = UnityEngine.Random.Range(200, 800);
                        ai.tempIdlePos = new WorldCoordinate(ai.bug.room.abstractRoom.index, UnityEngine.Random.Range(0, ai.bug.room.TileWidth), UnityEngine.Random.Range(0, ai.bug.room.TileHeight), -1);
                    }
                }
                ai.idlePosCounter--;

            }
            else if (ai.behavior == EggBugAI.Behavior.Flee)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                if (UnityEngine.Random.value < ai.threatTracker.Panic && ai.threatTracker.mostThreateningCreature != null && ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature != null && ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.room == ai.bug.room)
                {
                    BodyChunk bodyChunk = ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.bodyChunks[UnityEngine.Random.Range(0, ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature.bodyChunks.Length)];
                    if (!ai.bug.safariControlled && Custom.DistLess(ai.bug.mainBodyChunk.pos, bodyChunk.pos, ai.bug.mainBodyChunk.rad + bodyChunk.rad + 40f * ai.fear))
                    {
                        TryJump(self, bodyChunk.pos);
                    }
                }
            }
            else if (ai.behavior == EggBugAI.Behavior.EscapeRain)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
            }
            else if (ai.bug.FireBug && ai.behavior == MoreSlugcats.MoreSlugcatsEnums.EggBugBehavior.Kill)
            {
                Tracker.CreatureRepresentation mostAttractivePrey = ai.preyTracker.MostAttractivePrey;

                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.025f, 0.1f);
            }
            ai.fear = Custom.LerpAndTick(ai.fear, Mathf.Max(ai.utilityComparer.GetUtilityTracker(ai.threatTracker).SmoothedUtility(), Mathf.Pow(ai.threatTracker.Panic, 0.7f)), 0.07f, 0.033333335f);
            if (ai.noiseRectionDelay > 0)
            {
                ai.noiseRectionDelay--;
            }
        }
    }

    public static void TryJump(EggBug self, Vector2 awayFromPoint)
    {
        self.Squirt(0.5f + 0.5f * self.AI.fear);

        if (self.noJumps <= 0 && (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[0].pos, self.Template) || self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template)) && !self.room.aimap.getAItile(self.bodyChunks[1].pos).narrowSpace)
        {
            self.room.PlaySound(SoundID.Egg_Bug_Scurry, self.mainBodyChunk);
            Vector2 vector = Custom.DirVec(awayFromPoint, (self.bodyChunks[0].pos + self.bodyChunks[1].pos) / 2f);
            vector += Custom.RNV() * 0.3f;
            vector.Normalize();
            vector = Vector3.Slerp(vector, new Vector2(0f, 1f), Custom.LerpMap(vector.y, -0.5f, 0.5f, 0.7f, 0.3f));
            self.bodyChunks[0].vel *= 0.5f;
            self.bodyChunks[1].vel *= 0.5f;
            self.bodyChunks[0].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
            self.bodyChunks[1].vel += vector * 17f + Custom.RNV() * 5f * UnityEngine.Random.value;
            self.footingCounter = 0;
            Vector2 vector2 = Custom.PerpendicularVector(vector) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
            self.bodyChunks[0].vel += vector2 * 11f;
            self.bodyChunks[1].vel -= vector2 * 11f;
            if (!self.safariControlled)
            {
                self.noJumps = 90;
            }
        }
    }

    static void NewEggBug(On.EggBug.orig_ctor orig, EggBug self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive || !data.returnToDen && !IsComa(self))
        {
            return;
        }

        if (data.returnToDen)
        {
            if (self.FireBug)
            {
                self.dropEggs = ShadowOfOptions.fire_den.Value;

                self.dropSpears = ShadowOfOptions.fire_den.Value || ShadowOfOptions.fire_spear.Value;
            }
            else
            {
                self.dropEggs = ShadowOfOptions.egg_egg.Value || ShadowOfOptions.egg_den.Value;
            }
        }
        else
        {
            if (self.FireBug)
            {
                self.dropEggs = false;

                self.dropSpears = ShadowOfOptions.fire_spear.Value;
            }
            else
            {
                self.dropEggs = ShadowOfOptions.egg_egg.Value;
            }
        }
    }

    public static bool ShadowOfEggBugGraphicsShowEggs(Func<EggBugGraphics, bool> orig, EggBugGraphics self)
    {
        try
        {
            if (ILHooks.EggBugEggs(self.bug))
            {
                return true;
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
        return orig(self);
    }
}