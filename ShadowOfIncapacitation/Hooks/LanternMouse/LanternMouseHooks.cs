using RWCustom;
using System;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.LanternMouseHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.LanternMouse.Carried += LanternMouseCarried;
        On.LanternMouse.Squeak += LanternMouseSqueak;
        On.LanternMouse.Update += LanternMouseUpdate;

        On.MouseAI.Update += MouseAIUpdate;
        On.MouseAI.WantToStayInDenUntilEndOfCycle += MouseAIWantToStayInDenUntilEndOfCycle;
    }


    #region LanternMouse
    static void LanternMouseCarried(On.LanternMouse.orig_Carried orig, LanternMouse self)
    {
        orig(self);

        if (!ShadowOfOptions.mouse_struggle.Value || !IsIncon(self))
        {
            return;
        }

        bool flag = self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) || self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template);
        if (self.grabbedBy[0].grabber is Player && ((self.grabbedBy[0].grabber as Player).input[0].x != 0 || (self.grabbedBy[0].grabber as Player).input[0].y != 0))
        {
            flag = false;
        }
        if (flag)
        {
            self.struggleCountdownA--;
            if (self.struggleCountdownA < 0)
            {
                if (UnityEngine.Random.value < 0.008333334f)
                {
                    self.struggleCountdownA = UnityEngine.Random.Range(40, 600);
                }
                for (int i = 0; i < 2; i++)
                {
                    self.bodyChunks[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (3f * UnityEngine.Random.value);
                }
            }
        }
        self.struggleCountdownB--;
        if (self.struggleCountdownB < 0 && UnityEngine.Random.value < 0.008333334f)
        {
            self.struggleCountdownB = UnityEngine.Random.Range(10, 100);
        }
        if (!self.dead && self.graphicsModule != null && (self.struggleCountdownA < 0 || self.struggleCountdownB < 0))
        {
            if (UnityEngine.Random.value < 0.025f)
            {
                (self.graphicsModule as MouseGraphics).ResetUnconsiousProfile();
            }
            for (int j = 0; j < self.graphicsModule.bodyParts.Length; j++)
            {
                self.graphicsModule.bodyParts[j].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * (1.5f * UnityEngine.Random.value);
                self.graphicsModule.bodyParts[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * (3f * UnityEngine.Random.value);
            }
        }
    }
    static void LanternMouseSqueak(On.LanternMouse.orig_Squeak orig, LanternMouse self, float stress)
    {
        if (!ShadowOfOptions.mouse_squeak.Value || !IsIncon(self) || self.voiceCounter > 0)
        {
            orig(self, stress);
            return;
        }

        self.room.PlaySound(SoundID.Mouse_Squeak, self.mainBodyChunk, false, Mathf.Lerp(0.5f, 1f, stress), Mathf.Lerp(1f, 1.3f, stress));
        self.voiceCounter = UnityEngine.Random.Range(5, 12);
        if (self.graphicsModule != null)
        {
            (self.graphicsModule as MouseGraphics).head.pos += Custom.RNV() * (4f * UnityEngine.Random.value);
            if (UnityEngine.Random.value > Mathf.InverseLerp(0.5f, 1f, stress))
            {
                (self.graphicsModule as MouseGraphics).ouchEyes = Math.Max((self.graphicsModule as MouseGraphics).ouchEyes, self.voiceCounter);
            }
        }
    }
    static void LanternMouseUpdate(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self))
        {
            return;
        }

        if (IsInconBase(self) && self.State.battery < 3000 || IsUncon(self) && self.State.battery < 2000)
        {
            self.State.battery--;
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
            if (self.ropeAttatchedPos != null)
            {
                return;
            }

            self.footingCounter++;

            AIUpdate();

            if (self.specialMoveCounter > 0)
            {
                self.specialMoveCounter--;
                if (Custom.DistLess(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.specialMoveDestination), 5f))
                {
                    self.specialMoveCounter = 0;
                }
            }
            else
            {
                if (!self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) && !self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template))
                {
                    self.footingCounter = 0;
                }

                if (self.AI.behavior == MouseAI.Behavior.Idle && self.AI.threatTracker.Utility() < 0.5f && self.AI.dangle == null)
                {
                    self.Sit();
                    self.GoThroughFloors = false;
                }
                else
                {
                    MovementConnection movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.mainBodyChunk.pos), true);
                    if (movementConnection == default)
                    {
                        movementConnection = (self.AI.pathFinder as StandardPather).FollowPath(self.room.GetWorldCoordinate(self.bodyChunks[1].pos), true);
                    }

                    if (movementConnection == default)
                    {
                        self.GoThroughFloors = false;
                    }
                }
            }

            self.profileFac *= 0.97f;

            if (!Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 5f))
            {
                self.runCycle += self.runSpeed / 40f;
            }
            if (self.voiceCounter > 0)
            {
                self.voiceCounter--;
                return;
            }
            if (!self.safariControlled && !self.Sleeping && UnityEngine.Random.value < ((self.ropeAttatchedPos != null) ? 0.1f : 1f) / ((self.AI.behavior == MouseAI.Behavior.Flee) ? Mathf.Lerp(80f, 20f, self.AI.threatTracker.Utility()) : 100f))
            {
                self.Squeak(Mathf.InverseLerp(0.5f, 1f, self.AI.threatTracker.Utility()));
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AIUpdate()
        {
            MouseAI ai = self.AI;

            MiscHooks.AIUpdate(ai);

            if (self.room == null)
            {
                return;
            }

            if (ModManager.MSC && ai.mouse.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.mouse.LickedByPlayer.abstractCreature);
            }

            ai.pathFinder.walkPastPointOfNoReturn = ai.stranded || ai.denFinder.GetDenPosition() == null || !ai.pathFinder.CoordinatePossibleToGetBackFrom(ai.denFinder.GetDenPosition().Value) || ai.threatTracker.Utility() > 0.95f;

            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = MouseAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = MouseAI.Behavior.EscapeRain;
                }
            }
            if (ai.currentUtility < 0.2f)
            {
                ai.behavior = MouseAI.Behavior.Idle;
            }
            if (!ai.mouse.safariControlled && ai.mouse.ropeAttatchedPos != null)
            {
                if (ai.dangle != null)
                {
                    if (!(ai.dangle.Value.attachedPos.Tile != ai.mouse.room.GetTilePosition(ai.mouse.ropeAttatchedPos.Value)) && (!(ai.behavior != MouseAI.Behavior.Idle) || UnityEngine.Random.value >= ai.threatTracker.Panic * 0.5f))
                    {
                        goto IL_1A6;
                    }
                }
                ai.mouse.DetatchRope();
            }
        IL_1A6:
            if (ai.behavior == MouseAI.Behavior.Idle)
            {
                ai.mouse.runSpeed = Mathf.Lerp(ai.mouse.runSpeed, 0.5f, 0.05f);
                ai.ReconsiderDanglePos();

                if (ai.dangle != null && ai.dangle.Value.attachedPos.room == ai.mouse.room.abstractRoom.index)
                {
                    int i = 0;
                    while (i < Custom.eightDirectionsAndZero.Length)
                    {
                        PathFinder pathFinder = ai.pathFinder;
                        Room room = ai.mouse.room;
                        MouseAI.Dangle value = ai.dangle.Value;
                        if (pathFinder.CoordinateReachableAndGetbackable(room.GetWorldCoordinate(value.attachedPos.Tile + Custom.eightDirectionsAndZero[i])))
                        {
                            value = ai.dangle.Value;
                            IntVector2 intVector = value.attachedPos.Tile + Custom.eightDirectionsAndZero[i];
                            if (ai.mouse.room.GetTilePosition(ai.mouse.bodyChunks[0].pos) == intVector || ai.mouse.room.GetTilePosition(ai.mouse.bodyChunks[1].pos) == intVector)
                            {
                                LanternMouse lanternMouse = ai.mouse;
                                value = ai.dangle.Value;
                                lanternMouse.AttatchRope(value.attachedPos.Tile);
                                break;
                            }
                            break;
                        }
                        else
                        {
                            Room room2 = ai.mouse.room;
                            value = ai.dangle.Value;
                            if (!room2.GetTile(value.attachedPos.Tile + Custom.eightDirectionsAndZero[i]).Solid)
                            {
                                PathFinder pathFinder2 = ai.pathFinder;
                                Room room3 = ai.mouse.room;
                                value = ai.dangle.Value;
                                if (pathFinder2.CoordinateReachableAndGetbackable(room3.GetWorldCoordinate(value.attachedPos.Tile + Custom.eightDirectionsAndZero[i] + new IntVector2(0, 1))))
                                {
                                    value = ai.dangle.Value;
                                    IntVector2 intVector2 = value.attachedPos.Tile + Custom.eightDirectionsAndZero[i] + new IntVector2(0, 1);
                                    if (ai.mouse.room.GetTilePosition(ai.mouse.bodyChunks[0].pos) == intVector2 || ai.mouse.room.GetTilePosition(ai.mouse.bodyChunks[1].pos) == intVector2)
                                    {
                                        LanternMouse lanternMouse2 = ai.mouse;
                                        value = ai.dangle.Value;
                                        lanternMouse2.AttatchRope(value.attachedPos.Tile);
                                        break;
                                    }
                                    break;
                                }
                            }
                            i++;
                        }
                    }
                }
                else
                {
                    ai.dangle = null;

                    bool flag = ai.pathFinder.GetDestination.room != ai.mouse.room.abstractRoom.index;
                    if (!flag && ai.dangleChecksCounter > 200)
                    {
                        int abstractNode = ai.mouse.room.abstractRoom.RandomNodeInRoom().abstractNode;
                        if (ai.mouse.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
                        {
                            int num = ai.mouse.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, ai.mouse.Template);
                            if (num > -1 && ai.mouse.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(ai.mouse.abstractCreature.pos.Tile, num, ai.mouse.Template) > -1)
                            {
                                AbstractRoom abstractRoom = ai.mouse.room.game.world.GetAbstractRoom(ai.mouse.room.abstractRoom.connections[abstractNode]);
                                if (abstractRoom != null)
                                {
                                    WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
                                    if (ai.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                                    {
                                        ai.idlePosCounter = UnityEngine.Random.Range(200, 500);
                                        flag = true;
                                    }
                                }
                            }
                        }
                    }

                    if (!flag && (UnityEngine.Random.value < 0.0045454544f || ai.idlePosCounter <= 0))
                    {
                        IntVector2 pos = new IntVector2(UnityEngine.Random.Range(0, ai.mouse.room.TileWidth), UnityEngine.Random.Range(0, ai.mouse.room.TileHeight));
                        if (ai.pathFinder.CoordinateReachableAndGetbackable(ai.mouse.room.GetWorldCoordinate(pos)))
                        {
                            ai.idlePosCounter = UnityEngine.Random.Range(200, 1900);
                        }
                    }

                    if (!ai.mouse.sitting)
                    {
                        ai.idlePosCounter--;
                    }

                    ai.idlePosCounter--;
                }
            }
            else if (ai.behavior == MouseAI.Behavior.Flee)
            {
                ai.mouse.runSpeed = Mathf.Lerp(ai.mouse.runSpeed, 1f, 0.08f);
            }
            else if (ai.behavior == MouseAI.Behavior.EscapeRain)
            {
                ai.mouse.runSpeed = Mathf.Lerp(ai.mouse.runSpeed, 1f, 0.08f);
            }

            if (ai.behavior == MouseAI.Behavior.Flee)
            {
                ai.fear = Mathf.Lerp(ai.fear, Mathf.Pow(ai.threatTracker.Panic, 0.7f), 0.5f);
            }
            else
            {
                ai.fear = Mathf.Max(ai.fear - 0.0125f, 0f);
            }

            ai.wantToSleep = true;
            float num2 = 0f;
            for (int j = 0; j < ai.tracker.CreaturesCount; j++)
            {
                if (ai.StaticRelationship(ai.tracker.GetRep(j).representedCreature).type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    ai.wantToSleep = false;
                    if (!ai.tracker.GetRep(j).representedCreature.creatureTemplate.canFly && ai.tracker.GetRep(j).VisualContact && ai.tracker.GetRep(j).BestGuessForPosition().y < ai.creature.pos.y + 1)
                    {
                        num2 = Mathf.Max(num2, Mathf.Pow(Mathf.InverseLerp(400f, 30f, Vector2.Distance(ai.mouse.mainBodyChunk.pos, ai.tracker.GetRep(j).representedCreature.realizedCreature.DangerPos)), 2.7f));
                    }
                }
                else if (ai.StaticRelationship(ai.tracker.GetRep(j).representedCreature).type == CreatureTemplate.Relationship.Type.Eats && Custom.ManhattanDistance(ai.tracker.GetRep(j).BestGuessForPosition(), ai.creature.pos) < 10)
                {
                    ai.wantToSleep = false;
                }
            }
            ai.pullUp = Mathf.Lerp(ai.pullUp, num2, 0.05f);
        }
    }
    #endregion

    #region LanternMouseAI
    static void MouseAIUpdate(On.MouseAI.orig_Update orig, MouseAI self)
    {
        orig(self);

        MiscHooks.ReturnToDenUpdate(self);
    }
    static bool MouseAIWantToStayInDenUntilEndOfCycle(On.MouseAI.orig_WantToStayInDenUntilEndOfCycle orig, MouseAI self)
    {
        return MiscHooks.ReturnToDenWantToStayInDenUntilEndOfCycle(self) || orig(self);
    }
    #endregion
}