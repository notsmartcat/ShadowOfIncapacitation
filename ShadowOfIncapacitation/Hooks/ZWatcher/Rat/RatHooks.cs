using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Watcher;

using static Incapacitation.Incapacitation;

namespace Incapacitation.RatHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Watcher.Rat.Update += RatUpdate;
    }

    #region Rat
    static void RatUpdate(On.Watcher.Rat.orig_Update orig, Rat self, bool eu)
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
                self.footingCounter = 0;
                return;
            }

            if (self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template))
            {
                self.footingCounter++;
                Act();
            }
            else
            {
                self.footingCounter = 0;
            }
            if (UnityEngine.Random.value < 0.005f && self.grabbedBy.Count == 0)
            {
                self.room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Chatter, self.mainBodyChunk);
            }

        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void Act()
        {
            if (self.Submersion > 0.3f && self.creatureParams.separateSwimBehavior)
            {
                self.Swim();
                AIUpdate();
                return;
            }
            AIUpdate();
            if (self.specialMoveCounter > 0)
            {
                self.specialMoveCounter--;
                if (Custom.DistLess(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.specialMoveDestination), 5f))
                {
                    self.specialMoveCounter = 0;
                }
                return;
            }
            if (!self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) && !self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
            {
                self.footingCounter = 0;
            }
            MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.mainBodyChunk.pos), true);
            if (movementConnection == default)
            {
                movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[1].pos), true);
            }
            if (movementConnection != default)
            {
                if (movementConnection.type == MovementConnection.MovementType.ReachUp)
                {
                    (self.AI.pathFinder as StandardPather).pastConnections.Clear();
                }
                if (movementConnection.type == MovementConnection.MovementType.OpenDiagonal || movementConnection.type == MovementConnection.MovementType.ReachOverGap || movementConnection.type == MovementConnection.MovementType.ReachUp || movementConnection.type == MovementConnection.MovementType.ReachDown || movementConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
                {
                    self.specialMoveCounter = 30;
                    self.specialMoveDestination = movementConnection.DestTile;
                }
                else
                {
                    Vector2 vector = self.room.MiddleOfTile(movementConnection.DestTile);
                    if (self.lastFollowedConnection != default && self.lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
                    {
                        self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, vector) * 4f;
                    }
                    if (self.Footing)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (movementConnection.startCoord.x == movementConnection.destinationCoord.x)
                            {
                                self.bodyChunks[j].vel.x += Mathf.Min((vector.x - self.bodyChunks[j].pos.x) / 8f, 1.2f);
                            }
                            else if (movementConnection.startCoord.y == movementConnection.destinationCoord.y)
                            {
                                self.bodyChunks[j].vel.y += Mathf.Min((vector.y - self.bodyChunks[j].pos.y) / 8f, 1.2f);
                            }
                        }
                    }
                    if (self.lastFollowedConnection != default && (self.Footing || self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template)) && ((movementConnection.startCoord.x != movementConnection.destinationCoord.x && self.lastFollowedConnection.startCoord.x == self.lastFollowedConnection.destinationCoord.x) || (movementConnection.startCoord.y != movementConnection.destinationCoord.y && self.lastFollowedConnection.startCoord.y == self.lastFollowedConnection.destinationCoord.y)))
                    {
                        self.mainBodyChunk.vel *= 0.7f;
                        self.bodyChunks[1].vel *= 0.5f;
                    }
                    if (movementConnection.type == MovementConnection.MovementType.DropToFloor)
                    {
                        self.footingCounter = 0;
                    }
                }
                self.lastFollowedConnection = movementConnection;
                return;
            }
            self.GoThroughFloors = false;
        }

        void AIUpdate()
        {
            RatAI ai = self.AI;

            MiscHooks.AIUpdate(ai);

            if (ai.realizedCreature.room == null)
            {
                return;
            }

            if (ModManager.MSC && ai.realizedCreature.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.realizedCreature.LickedByPlayer.abstractCreature);
            }

            if (ai.panic > 0)
            {
                ai.panic--;
            }

            ai.pathFinder.walkPastPointOfNoReturn = (ai.stranded || ai.denFinder.GetDenPosition() == null || !ai.pathFinder.CoordinatePossibleToGetBackFrom(ai.denFinder.GetDenPosition().Value));
            ai.fear = ((ai.threatTracker != null) ? ai.utilityComparer.GetSmoothedNonWeightedUtility(ai.threatTracker) : 0f);
            ai.hunger = ((ai.preyTracker != null) ? ai.utilityComparer.GetSmoothedNonWeightedUtility(ai.preyTracker) : 0f);
            ai.rainFear = ai.utilityComparer.GetSmoothedNonWeightedUtility(ai.rainTracker);
            ai.excitement = Mathf.Lerp(ai.excitement, Mathf.Max(ai.hunger, ai.CombinedFear), 0.1f);
            if (ai.fear > 0.8f)
            {
                ai.lastDistressLength = Custom.IntClamp(ai.lastDistressLength + 1, 0, 500);
            }
            else if (ai.fear < 0.2f)
            {
                ai.lastDistressLength = 0;
            }

            if (ai.threatTracker != null)
            {
                ((ai.utilityComparer.GetUtilityTracker(ai.threatTracker).smoother as FloatTweener.FloatTweenUpAndDown).down as FloatTweener.FloatTweenBasic).speed = 1f / ((float)(ai.lastDistressLength + 20) * 3f);
            }

            ai.currentUtility = ai.utilityComparer.HighestUtility();
            ai.behavior = ai.DetermineBehavior();

            ai.targetDeadCreature = null;
            ai.setOnDeadCreature = false;

            if (ai.behavior == RatAI.Behavior.Flee)
            {
                if (ai.realizedCreature.packLeader != null)
                {
                    ai.realizedCreature.room.PlaySound(WatcherEnums.WatcherSoundID.Rat_Scramble, ai.realizedCreature.mainBodyChunk);
                }

                ai.realizedCreature.packLeader = null;
                WorldCoordinate destination = ai.threatTracker.FleeTo(ai.creature.pos, 5, 20, ai.fear > 0.33333334f);

                if (ai.threatTracker.ThreatOfTile(ai.creature.pos, true) > 1f)
                {
                    if (ai.panic < 40)
                    {
                        ai.panic = 40;
                    }
                    else
                    {
                        ai.panic += 2;
                    }
                }

                ai.runSpeed = Mathf.Lerp(ai.runSpeed, Mathf.Pow(ai.CombinedFear, 0.1f), 0.5f);
                ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                return;
            }
            if (!(ai.behavior == RatAI.Behavior.Idle))
            {
                if (ai.behavior == RatAI.Behavior.EscapeRain)
                {
                    ai.runSpeed = Mathf.Lerp(ai.runSpeed, 1f, 0.1f);
                }
                return;
            }
            bool flag = true;

            if (flag)
            {
                if (!ai.forbiddenIdleSpot.CompareDisregardingNode(ai.creature.abstractAI.destination))
                {
                    ai.runSpeed = Mathf.Lerp(ai.runSpeed, 0.25f, 0.5f);
                }
            }
            else
            {
                ai.runSpeed = Mathf.Lerp(ai.runSpeed, 0f, 0.5f);
            }
            ai.focusCreature = null;
        }
    }
    #endregion
}