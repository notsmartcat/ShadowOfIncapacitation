using RWCustom;
using System;
using UnityEngine;
using Watcher;

using static Incapacitation.Incapacitation;

namespace Incapacitation.BigMothHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Watcher.BigMoth.Update += BigMothUpdate;
    }

    static void BigMothUpdate(On.Watcher.BigMoth.orig_Update orig, BigMoth self, bool eu)
    {
        orig(self, eu);

        if (self.room == null || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
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
                return;
            }

            AIUpdate();

            self.drinkingChunk = false;
            
            float num = 1f;
            Vector2 pos = self.bodyChunks[2].pos;
            if (self.narrowUpcoming && self.Small)
            {
                pos = self.bodyChunks[1].pos;
            }

            MovementConnection movementConnection = (self.AI.pathFinder as BigMothPather).FollowPath(self.room.GetWorldCoordinate(pos), true);
            if (self.room == null)
            {
                return;
            }
            if (movementConnection == default)
            {
                for (int i = 0; i < 4; i++)
                {
                    movementConnection = (self.AI.pathFinder as BigMothPather).FollowPath(self.room.GetWorldCoordinate(pos + Custom.fourDirections[i].ToVector2() * 20f), true);
                    if (self.room == null)
                    {
                        return;
                    }
                    if (movementConnection != default)
                    {
                        break;
                    }
                }
            }
            if (movementConnection != default && (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation))
            {
                self.enteringShortCut = new IntVector2?(movementConnection.StartTile);
                if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                {
                    self.NPCTransportationDestination = movementConnection.destinationCoord;
                }
                return;
            }
            if (movementConnection != default)
            {
                Vector2 vector = self.room.MiddleOfTile(movementConnection.destinationCoord);
                self.narrowUpcoming = false;
                int num2 = 1;
                MovementConnection movementConnection2 = movementConnection;
                for (int j = 0; j < 3; j++)
                {
                    if (!self.narrowUpcoming && ((self.room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, 1)).Solid && self.room.GetTile(movementConnection2.destinationCoord + new IntVector2(0, -1)).Solid) || (self.room.GetTile(movementConnection2.destinationCoord + new IntVector2(1, 0)).Solid && self.room.GetTile(movementConnection2.destinationCoord + new IntVector2(-1, 0)).Solid)))
                    {
                        self.narrowUpcoming = true;
                    }
                    movementConnection2 = (self.AI.pathFinder as BigMothPather).FollowPath(movementConnection2.destinationCoord, false);
                    if (movementConnection2 == default || !self.room.IsPositionInsideBoundries(movementConnection2.DestTile))
                    {
                        break;
                    }
                    vector += self.room.MiddleOfTile(movementConnection2.destinationCoord);
                    num2++;
                }
                vector /= (float)num2;
                if (!self.Small)
                {
                    int num3 = 0;
                    int num4 = 0;
                    while (num4 < 5 && !self.room.GetTile(vector + Vector2.up * (20f * (float)(1 + num4))).Solid)
                    {
                        num3++;
                        num4++;
                    }
                    num = (float)(num3 - 1) / 4f;
                }
                else if (self.narrowUpcoming)
                {
                    num = 0f;
                }

                Vector2 vector2 = vector - pos;

                if (self.wantToFly && self.room.VisualContact(pos, self.room.MiddleOfTile(self.AI.pathFinder.GetDestination)))
                {
                    vector2 = vector2 * 0.3f + Vector2.ClampMagnitude(self.room.MiddleOfTile(self.AI.pathFinder.GetDestination) - pos, 20f) * 0.7f;
                }

                vector2 = Vector2.ClampMagnitude(vector2, 10f) / 10f;
                self.moveDirection = Vector2.MoveTowards(self.moveDirection, vector2, 0.1f);

                if (!self.room.IsPositionInsideBoundries(self.room.GetTilePosition(vector)))
                {
                    self.moveDirection = vector2;
                }

                if (self.flyUpness < vector2.y)
                {
                    self.flyUpness = Mathf.Lerp(self.flyUpness, vector2.y, 0.5f);
                }
                else
                {
                    self.flyUpness = Mathf.Lerp(self.flyUpness, 0.15f, 0.015f);
                }

                self.stuckCounter = 0;
            }

            if (self.randomLookCounter == 0)
            {
                if ((double)UnityEngine.Random.value < 0.02)
                {
                    self.randomLookAtCreature = ((double)UnityEngine.Random.value < 0.7);
                    self.randomLookCounter = UnityEngine.Random.Range(20, 400);
                    self.randomLookPos = new Vector2((float)(self.room.TileWidth * 20) * UnityEngine.Random.value, (float)(self.room.TileHeight * 20) * UnityEngine.Random.value);
                }
            }
            else
            {
                self.randomLookCounter--;
            }

            bool flag = self.AI.behavior != BigMothAI.Behavior.Idle;
            Vector2 vector3 = self.bodyChunks[1].pos + self.moveDirection * 300f;
            if (self.randomLookCounter > 0)
            {
                vector3 = self.randomLookPos;
                if (self.randomLookAtCreature)
                {
                    flag = true;
                }
            }

            if (flag && self.AI.creatureLooker.lookCreature != null)
            {
                if (self.AI.creatureLooker.lookCreature.VisualContact)
                {
                    vector3 = self.AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
                }
                else
                {
                    vector3 = self.room.MiddleOfTile(self.AI.creatureLooker.lookCreature.BestGuessForPosition());
                }
            }

            vector3 = Vector2.Lerp(vector3, self.bodyChunks[1].pos, ((self.bodyChunks[1].pos.x - vector3.x) * self.flip > 0f) ? 0.5f : 0f);
            self.lookPos = Vector2.MoveTowards(self.lookPos, vector3, Mathf.Max(Mathf.Min(Vector2.Distance(self.lookPos, vector3) * 0.1f, 200f), 5f));
            num *= 1f - Mathf.Max(self.runSpeed - 0.5f, 0f) * 0.8f;

            if (num > self.stance)
            {
                self.stance = Mathf.Lerp(self.stance, num, 0.02f);
            }
            else
            {
                self.stance = Mathf.Lerp(self.stance, num, 0.08f);
            }

            self.stance = Mathf.Clamp01(self.stance);
            self.flipness = Mathf.Lerp(self.flipness, self.moveDirection.x, 0.02f * Mathf.Abs(self.moveDirection.x));
            self.flipness = Mathf.Clamp(self.flipness, -1f, 1f);
            if ((double)self.flipness > 0.01)
            {
                self.flipSide = 1;
            }
            else if ((double)self.flipness < -0.01)
            {
                self.flipSide = -1;
            }

            self.flip = Mathf.Clamp(self.flipness, (self.flipSide == 1) ? 0.01f : -1f, (self.flipSide == -1) ? -0.01f : 1f);
            Vector2 vector4 = Vector2.up;
            if (self.legsOnGround > 0)
            {
                float num5 = self.bodyChunks[2].pos.x + self.flip * 57f * (0.5f + 0.5f * self.scale);
                vector4 = Custom.DirVec(self.feetAvgPos, new Vector2(num5, self.bodyChunks[2].pos.y));
                float num6 = Mathf.Clamp01(Mathf.Abs(self.feetAvgPos.x - num5) / (70f * (0.5f + 0.5f * self.scale)));
                vector4 = (Vector2.up * (1f - num6) + vector4 * num6).normalized;
            }

            float num7 = 0.6f * self.stance;
            Vector2 normalized = new Vector2((1f - num7) * self.flip, num7 + (1f - Mathf.Abs(self.flip)) * 0.1f).normalized;
            self.groundAngle = Mathf.LerpAngle(self.groundAngle, Custom.VecToDeg(vector4), 0.1f);
            Vector2 vector5 = Custom.DegToVec(self.groundAngle + Custom.VecToDeg(normalized));
            Vector2 normalized2 = (vector5 + Vector2.ClampMagnitude((self.lookPos - self.bodyChunks[1].pos) / 700f, 1f) * (0.7f + Mathf.Clamp01(Vector2.Dot(vector5, Custom.DirVec(self.bodyChunks[1].pos, self.lookPos))) * 2f)).normalized;
            self.WeightedPush(0, 1, normalized2, 3f);
            self.WeightedPush(1, 2, vector5, 3f);
            self.WeightedPush(2, 3, vector5, 2f);
            self.WeightedPush(3, 4, vector5, 2f);
            self.shake = Custom.LerpAndTick(self.shake, 0f, 0.008f, 0.005f);

            if ((double)self.shake > 0.01)
            {
                for (int l = 0; l < self.bodyChunks.Length; l++)
                {
                    self.bodyChunks[l].vel += UnityEngine.Random.insideUnitCircle * (self.shake * 3f * self.scale);
                }
                if (self.graphicsModule != null)
                {
                    BigMothGraphics bigMothGraphics = self.graphicsModule as BigMothGraphics;
                    for (int m = 0; m < bigMothGraphics.antennas.Length; m++)
                    {
                        Vector2 a = UnityEngine.Random.insideUnitCircle * (self.shake * 5f * self.scale);
                        for (int n = 0; n < bigMothGraphics.antennas[m].segments.GetLength(0); n++)
                        {
                            float num8 = Mathf.Pow(Mathf.InverseLerp(0f, (float)(bigMothGraphics.antennas[m].segments.GetLength(0) - 1), (float)n), 2f);
                            bigMothGraphics.antennas[m].segments[n, 2] += a * (num8 + 0.1f);
                        }
                    }
                }
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AIUpdate()
        {
            BigMothAI ai = self.AI;

            if (ai.focusCreature != null)
            {
                ai.lastFocusCreature = ai.focusCreature;
            }

            MiscHooks.AIUpdate(ai);

            ai.creatureLooker.Update();

            if (ai.bug.room == null)
            {
                return;
            }

            ai.grabCoolDown = Mathf.Max(ai.grabCoolDown - 1, 0);
            ai.utilityComparer.GetUtilityTracker(ai.threatTracker).weight = Custom.LerpMap(ai.threatTracker.ThreatOfTile(ai.creature.pos, true), 0.1f, 2f, 0.1f, 1f, 0.5f);
            ai.utilityComparer.GetUtilityTracker(ai.stuckTracker).weight = ((ai.pathFinder.GetDestination != default(WorldCoordinate) && ai.pathFinder.GetDestination.room == ai.bug.room.abstractRoom.index && ai.behavior != BigMothAI.Behavior.GetUnstuck && Vector2.Distance(ai.bug.room.MiddleOfTile(ai.pathFinder.GetDestination), ai.bug.bodyChunks[2].pos) < 95f) ? 0f : 1f);

            if (ai.bug.Small)
            {
                ai.utilityComparer.GetUtilityTracker(ai.threatTracker).weight = (((double)ai.preyTracker.Utility() > 0.2) ? 0.3f : 1f);
                for (int i = 0; i < ai.creature.state.socialMemory.relationShips.Count; i++)
                {
                    ai.creature.state.socialMemory.relationShips[i].tempLike = Custom.LerpAndTick(ai.creature.state.socialMemory.relationShips[i].tempLike, Mathf.Max(ai.creature.state.socialMemory.relationShips[i].tempLike, 0f), 0.001f, 0.001f);
                }
            }

            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    if (ai.threatTracker.mostThreateningCreature != null && ai.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type != CreatureTemplate.Relationship.Type.Afraid)
                    {
                        if (!ai.bug.Small && ((ai.childTracker.GetChildToCarry() != null && (ai.threatTracker.mostThreateningCreature.representedCreature == null || ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature == null || !(ai.threatTracker.mostThreateningCreature.representedCreature.realizedCreature is Player) || (ai.bug.abstractCreature.state as BigMoth.BigMothState).hatchedCycle < 0)) || ai.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks))
                        {
                            ai.behavior = BigMothAI.Behavior.Attack;
                        }
                        else if (ai.bug.Small && ai.threatTracker.mostThreateningCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
                        {
                            ai.behavior = BigMothAI.Behavior.Stalk;
                        }
                        else
                        {
                            ai.behavior = BigMothAI.Behavior.Idle;
                        }
                    }
                    else
                    {
                        ai.behavior = BigMothAI.Behavior.Flee;
                    }
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = BigMothAI.Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = BigMothAI.Behavior.Eat;
                }
            }
            if (ai.currentUtility < 0.02f && !(ai.behavior == BigMothAI.Behavior.Attack) && !(ai.behavior == BigMothAI.Behavior.Eat))
            {
                ai.behavior = BigMothAI.Behavior.Idle;
            }

            if (!ai.bug.drinkingChunk)
            {
                ai.bug.drinkChunk = null;
            }

            bool flag = false;
            if (!ai.bug.Small && ai.behavior == BigMothAI.Behavior.EscapeRain)
            {
                if (ai.childTracker.GetChildToCarry() != null && (ai.bug.grasps[0] == null || ai.bug.grasps[0].grabbed is not MothGrub))
                {
                    ai.behavior = BigMothAI.Behavior.Idle;
                    flag = true;

                    //InconAct();
                }
            }

            bool flag2 = false;
            if (ai.behavior == BigMothAI.Behavior.Idle && ai.lastFocusCreature != null && !ai.lastFocusCreature.deleteMeNextFrame && ai.lastFocusCreature.representedCreature != null && !ai.lastFocusCreature.representedCreature.slatedForDeletion && ai.lastFocusCreature.representedCreature.realizedCreature != null && ai.lastFocusCreature.representedCreature.realizedCreature.room == ai.bug.room && ai.DynamicRelationship(ai.lastFocusCreature.representedCreature).type != CreatureTemplate.Relationship.Type.Ignores)
            {
                Vector2 pos = ai.lastFocusCreature.representedCreature.realizedCreature.mainBodyChunk.pos;
                if (Vector2.Distance(ai.bug.bodyChunks[0].pos, pos) < 200f)
                {
                    ai.behavior = (ai.DynamicRelationship(ai.lastFocusCreature.representedCreature).type == CreatureTemplate.Relationship.Type.Eats) ? BigMothAI.Behavior.Eat : BigMothAI.Behavior.Attack;
                    ai.focusCreature = ai.lastFocusCreature;
                    flag2 = true;
                }
            }

            if (ai.behavior == BigMothAI.Behavior.Idle)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.5f, 0.01f, 0.1f);
                WorldCoordinate coord = new WorldCoordinate(ai.bug.room.abstractRoom.index, UnityEngine.Random.Range(0, ai.bug.room.TileWidth), UnityEngine.Random.Range(0, ai.bug.room.TileHeight), -1);
                if (ai.IdleScore(coord) < ai.IdleScore(ai.tempIdlePos))
                {
                    ai.tempIdlePos = coord;
                }
                bool flag3 = false;
                bool flag4 = false;
                if (ai.bug.grasps[0] != null)
                {
                    MothGrub mothGrub = ai.bug.grasps[0].grabbed as MothGrub;
                    if (mothGrub != null)
                    {
                        ai.TryGrab(ai.bug.grasps[0].grabbed, ai.bug.room.GetTile(mothGrub.bodyChunks[2].pos + new Vector2(0f, -20f)).Solid || ai.bug.room.GetTile(mothGrub.bodyChunks[2].pos + new Vector2(0f, -40f)).Solid, false, false);
                        flag4 = true;
                    }
                    else
                    {
                        ai.ThrowGrabbed(0f);
                    }
                }

                if (flag)
                {
                    ai.bug.runSpeed = 1f;
                }

                if (ai.timeInRoom > ai.timeToTire || flag)
                {
                    if (ai.bug.grasps[0] == null && !ai.bug.Small)
                    {
                        BigMothAI.ChildTrackerModule.TrackedChild childToCarry = ai.childTracker.GetChildToCarry();
                        if (childToCarry != null)
                        {
                            flag3 = true;
                            ai.TryGrab(childToCarry.critRep.representedCreature.realizedObject, false, true, false);
                        }
                    }
                    if (ai.pathFinder.CoordinateReachable(new WorldCoordinate(ai.bug.room.world.offScreenDen.index, -1, -1, 0)))
                    {
                        if (!flag3)
                        {
                            if (ai.idlePosCounter <= 0)
                            {
                                ai.timeInRoom = 0;
                                ai.tryingToLeaveRoom = false;
                            }
                            else if (!ai.tryingToLeaveRoom)
                            {
                                ai.idlePosCounter = UnityEngine.Random.Range(500, 1000);
                                ai.timeToTire = UnityEngine.Random.Range(400, 1000);
                                ai.timeInRoom = Mathf.Max(ai.timeToTire + 1, ai.timeInRoom);
                                ai.tryingToLeaveRoom = true;
                            }
                        }
                    }
                    else
                    {
                        ai.timeInRoom = 0;
                        ai.tryingToLeaveRoom = false;
                    }
                }
                else if (ai.IdleScore(ai.tempIdlePos) < ai.IdleScore(ai.pathFinder.GetDestination) + Custom.LerpMap((float)ai.idlePosCounter, 0f, 300f, 100f, -300f))
                {
                    ai.idlePosCounter = UnityEngine.Random.Range(200, 1000);
                    ai.tempIdlePos = new WorldCoordinate(ai.bug.room.abstractRoom.index, UnityEngine.Random.Range(0, ai.bug.room.TileWidth), UnityEngine.Random.Range(0, ai.bug.room.TileHeight), -1);
                }
                if (!flag3)
                {
                    ai.idlePosCounter -= ((flag4 && UnityEngine.Random.value < 0.4f) ? 0 : 1);
                }
            }
            else if (ai.behavior == BigMothAI.Behavior.Flee)
            {
                ai.timeToTire--;
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                if (ai.threatTracker.mostThreateningCreature != null)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                }

                //InconAct();
            }
            else if (ai.behavior == BigMothAI.Behavior.Eat)
            {
                if (!flag2)
                {
                    ai.focusCreature = ai.preyTracker.MostAttractivePrey;
                }
                ai.timeInRoom = 0;

                //InconAct();
            }
            else if (ai.behavior == BigMothAI.Behavior.Attack)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                if (!flag2)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                }

                //InconAct();
            }
            else if (ai.behavior == BigMothAI.Behavior.Stalk)
            {
                if (!flag2)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                }
                WorldCoordinate worldCoordinate = ai.focusCreature.BestGuessForPosition();
                Creature creature = null;
                if (ai.focusCreature.representedCreature.realizedCreature != null)
                {
                    Creature realizedCreature2 = ai.focusCreature.representedCreature.realizedCreature;
                    for (int k = 0; k < realizedCreature2.grasps.Length; k++)
                    {
                        if (realizedCreature2.grasps[k] != null && ai.SmallWantsToEat(realizedCreature2.grasps[k].grabbed))
                        {
                            creature = (realizedCreature2.grasps[k].grabbed as Creature);
                            break;
                        }
                    }
                }
                if (ai.bug.abstractCreature.Room == ai.focusCreature.representedCreature.Room && Custom.ManhattanDistance(ai.creature.pos, worldCoordinate) < 10 && creature == null)
                {
                    ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 1f, 0.01f, 0.1f);
                }
                else
                {
                    ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.7f, 0.01f, 0.1f);
                }

                //InconAct();
            }
            else if (ai.behavior == BigMothAI.Behavior.EscapeRain)
            {
                ai.bug.runSpeed = Custom.LerpAndTick(ai.bug.runSpeed, 0.7f, 0.01f, 0.1f);

                if (ai.bug.grasps[0] != null && ai.bug.grasps[0].grabbed is not MothGrub)
                {
                    ai.ThrowGrabbed(0f);
                }

                //InconAct();
            }

            ai.fear = Custom.LerpAndTick(ai.fear, Mathf.Max(ai.utilityComparer.GetUtilityTracker(ai.threatTracker).SmoothedUtility(), Mathf.Pow(ai.threatTracker.Panic, 0.7f)), 0.07f, 0.033333335f);
            if (ai.noiseRectionDelay > 0)
            {
                ai.noiseRectionDelay--;
            }
        }
    }
}