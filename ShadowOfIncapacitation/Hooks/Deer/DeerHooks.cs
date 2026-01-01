using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.DeerHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Deer.Update += DeerUpdate;
    }

    static void DeerUpdate(On.Deer.orig_Update orig, Deer self, bool eu)
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

        #region support
        float support = 0f;

        for (int k = 0; k < 4; k++)
        {
            support += self.legs[k].Support();
        }
        support = Mathf.Pow(Mathf.Min(support / 3f, 1f), 0.8f) * Mathf.Pow(1f - self.resting, 0.3f);
        for (int l = 0; l < 5; l++)
        {
            if (self.bodyChunks[l].ContactPoint.x != 0 || self.bodyChunks[l].ContactPoint.y < 0)
            {
                support = Mathf.Lerp(support, 1f, 0.6f);
            }
        }
        support = Mathf.Lerp(support, 1f, self.CloseToEdge);
        support = Mathf.Pow(support, 0.1f);
        #endregion

        #region forwardPower
        float forwardPower = Mathf.Lerp(Mathf.Pow((float)self.legsGrabbing / 4f, 0.8f), support, 0.5f);

        bool forwardPowerflag = false;
        int forwardPowernum4 = 0;
        while (forwardPowernum4 < 4 && !forwardPowerflag)
        {
            if (self.legs[forwardPowernum4].attachedAtTip && self.legs[forwardPowernum4].Tip.pos.x > self.bodyChunks[2].pos.x == self.moveDirection.x > 0f)
            {
                forwardPowerflag = true;
            }
            forwardPowernum4++;
        }
        if (!forwardPowerflag)
        {
            if (!self.stayStill)
            {
                forwardPower = Mathf.Lerp(Custom.LerpMap((float)self.hesistCounter, 20f, 150f, -0.5f, 1f), forwardPower, self.CloseToEdge);
            }
        }
        forwardPower = Mathf.Lerp(forwardPower, 1f, self.GetUnstuckForce);
        #endregion

        if (ModManager.MMF && MoreSlugcats.MMF.cfgDeerBehavior.Value && self.Submersion > 0.8f)
        {
            self.WeightedPush(0, 1, new Vector2(0f, 1f), 0.26f);
        }
        AIUpdate();
        self.AI.stuckTracker.satisfiedWithThisPosition = (self.violenceReaction < 1 && (self.resting > 0.5f || self.Kneeling));
        if (self.violenceReaction > 0)
        {
            self.AI.stuckTracker.stuckCounter = self.AI.stuckTracker.maxStuckCounter;
        }
        if (self.eatCounter > 0)
        {
            self.eatCounter--;
            if (self.eatCounter < 1 || self.eatObject.room != self.room || self.eatObject.grabbedBy.Count > 0 || !Custom.DistLess(self.mainBodyChunk.pos, self.eatObject.firstChunk.pos, 100f))
            {
                self.eatObject = null;
            }
            if (self.eatObject != null)
            {
                self.WeightedPush(0, 1, Custom.DirVec(self.mainBodyChunk.pos, self.eatObject.firstChunk.pos), Custom.LerpMap((float)self.eatCounter, 80f, 10f, 0.1f, 1.2f));
                self.WeightedPush(0, 2, Custom.DirVec(self.mainBodyChunk.pos, self.eatObject.firstChunk.pos), Custom.LerpMap((float)self.eatCounter, 80f, 10f, 0.1f, 1.2f));
                self.mainBodyChunk.vel += Custom.DirVec(self.mainBodyChunk.pos, self.eatObject.firstChunk.pos) * Custom.LerpMap((float)self.eatCounter, 80f, 10f, 0.1f, 2.75f);
                self.AI.cantEatFromCoordinate = self.room.GetWorldCoordinate(self.eatObject.firstChunk.pos);
                if (Custom.DistLess(self.mainBodyChunk.pos, self.eatObject.firstChunk.pos, self.mainBodyChunk.rad + self.eatObject.firstChunk.rad + 20f))
                {
                    self.AI.closeEyesCounter = Math.Max(5, self.AI.closeEyesCounter);
                    self.eatObject.firstChunk.vel = Vector2.Lerp(self.eatObject.firstChunk.vel, Vector2.ClampMagnitude(self.mainBodyChunk.pos + new Vector2(0f, -14f) - self.eatObject.firstChunk.pos, 30f) / 10f, 0.8f);
                    self.mainBodyChunk.vel += Custom.RNV() * 2.6f;
                    if (self.eatCounter == 50)
                    {
                        self.room.PlaySound(SoundID.Puffball_Eaten_By_Deer, self.mainBodyChunk);
                        if (self.eatObject is PuffBall)
                        {
                            (self.eatObject as PuffBall).beingEaten = Mathf.Max((self.eatObject as PuffBall).beingEaten, 0.01f);
                        }
                        else
                        {
                            self.eatObject.Destroy();
                        }
                    }
                }
            }
            else
            {
                self.eatCounter = 0;
            }
        }

        if (Mathf.Abs(self.abstractCreature.pos.x) < 10)
        {
            if (self.room.VisualContact(self.mainBodyChunk.pos, self.mainBodyChunk.pos + new Vector2(400f, 0f)))
            {
                if (ModManager.MMF && MoreSlugcats.MMF.cfgDeerBehavior.Value)
                {
                    self.flipDir = Mathf.Lerp(self.flipDir, 1f, 0.5f);
                    self.AI.restingCounter = 0;
                    self.resting = 0f;
                    self.moveDirection.x += 0.01f;
                    self.AI.kneelCounter = 0;
                    self.moveDirection.x += Mathf.Pow(self.enterRoomForcePush, 2f) * 2f;
                }
                else
                {
                    self.bodyChunks[1].vel.x += Mathf.Pow(self.enterRoomForcePush, 2f) * 2f;
                }
                self.enterRoomForcePush = Mathf.Min(4.5f, self.enterRoomForcePush + 0.0055555557f);
            }
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                if (self.bodyChunks[i].ContactPoint.x > 0)
                {
                    self.bodyChunks[i].vel += Custom.DegToVec(-90f * UnityEngine.Random.value) * 15f;
                }
            }
        }
        else if (Mathf.Abs(self.abstractCreature.pos.x - (self.room.TileWidth - 1)) < 10)
        {
            if (self.room.VisualContact(self.mainBodyChunk.pos, self.mainBodyChunk.pos + new Vector2(-400f, 0f)))
            {
                if (ModManager.MMF && MoreSlugcats.MMF.cfgDeerBehavior.Value)
                {
                    self.flipDir = Mathf.Lerp(self.flipDir, -1f, 0.5f);
                    self.AI.restingCounter = 0;
                    self.resting = 0f;
                    self.moveDirection.x -= 0.01f;
                    self.AI.kneelCounter = 0;
                    self.moveDirection.x -= Mathf.Pow(self.enterRoomForcePush, 2f) * 2f;
                }
                else
                {
                    self.bodyChunks[1].vel.x -= Mathf.Pow(self.enterRoomForcePush, 2f) * 2f;
                }
                self.enterRoomForcePush = Mathf.Min(4.5f, self.enterRoomForcePush + 0.0055555557f);
            }
            for (int j = 0; j < self.bodyChunks.Length; j++)
            {
                if (self.bodyChunks[j].ContactPoint.x < 0)
                {
                    self.bodyChunks[j].vel += Custom.DegToVec(90f * UnityEngine.Random.value) * 15f;
                }
            }
        }
        self.stayStill = true;

        if (data.stunTimer > 0 || self.wormGrassBelow)
        {
            self.resting = Mathf.Min(1f, self.resting + 0.016666668f);
        }
        else
        {
            self.resting = Mathf.Max(0f, self.resting - 0.00625f);
        }
        Debug.Log(self.resting);
        if (!self.safariControlled && self.resting >= 0.1f && !self.Kneeling)
        {
            self.flipDir = Mathf.Sign(self.bodyChunks[0].pos.x - self.bodyChunks[4].pos.x);
        }
        self.preferredHeight = 5f + 10f * Mathf.Pow(1f - self.resting, 5f);
        IntVector2 tilePosition = self.room.GetTilePosition(self.bodyChunks[1].pos);
        self.wormGrassBelow = false;
        for (int k = tilePosition.y; k >= 0; k--)
        {
            if (self.room.GetTile(new IntVector2(tilePosition.x, k)).wormGrass)
            {
                self.preferredHeight = 15f;
                self.wormGrassBelow = true;
                break;
            }
            if (self.room.GetTile(new IntVector2(tilePosition.x, k)).Solid)
            {
                break;
            }
        }
        if (self.stayStill)
        {
            self.moveDirection = Vector2.ClampMagnitude(self.room.MiddleOfTile(self.AI.pathFinder.GetEffectualDestination) - self.mainBodyChunk.pos, 20f) / Mathf.Lerp(30f, 5f, self.resting);
            self.nextFloorHeight = Mathf.Lerp(self.nextFloorHeight, (float)Custom.IntClamp(self.room.aimap.getAItile(self.mainBodyChunk.pos).floorAltitude + 2, 0, 17) * 20f, 0.2f);
        }
        for (int m = 0; m < 5; m++)
        {
            //float num7 = (float)m / 5f;
            self.bodyChunks[m].vel *= Mathf.Lerp(1f, self.stayStill ? 0.7f : 0.92f, support);
            self.bodyChunks[m].vel *= 1f - self.resting * 0.5f;
            if (m < 4)
            {
                //self.bodyChunks[m].vel.y += self.gravity * Mathf.Lerp(1.3f, 2.5f, Mathf.Sin(Mathf.Pow(num7, 1.7f) * 3.1415927f)) * Mathf.Lerp(support * Custom.LerpMap((float)self.room.aimap.getClampedAItile(self.bodyChunks[m].pos).smoothedFloorAltitude, 14f, 18f, 1f, 0.5f) * Custom.LerpMap(self.moveDirection.y, -1f, 1f, 0.5f, 1f), 0.65f, self.CloseToEdge);
            }
            //self.bodyChunks[m].vel += self.moveDirection * (Mathf.Lerp(0.35f, 0f, num7) * forwardPower * Mathf.Lerp(1f, 3f, self.GetUnstuckForce));
        }
        BodyChunk mainBodyChunk = self.mainBodyChunk;
        mainBodyChunk.vel.y += self.gravity * 1.2f * self.resting;
        if (self.GetUnstuckForce > 0f && (!self.safariControlled || (self.safariControlled && self.inputWithoutDiagonals != null && self.inputWithoutDiagonals.Value.AnyDirectionalInput)))
        {
            self.bodyChunks[1].vel += Custom.RNV() * (UnityEngine.Random.value * 4f * self.GetUnstuckForce);
        }

        void AIUpdate()
        {
            DeerAI ai = self.AI;

            ai.focusCreature = null;

            MiscHooks.AIUpdate(ai);
            ai.utilityComparer.GetUtilityTracker(ai.sporeTracker).weight = 0.9f * Mathf.InverseLerp(100f, 30f, (float)ai.deerPileCounter);
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is RainTracker)
                {
                    ai.behavior = DeerAI.Behavior.EscapeRain;
                }
                else if (aimodule is DeerAI.SporeTracker)
                {
                    ai.behavior = DeerAI.Behavior.TrackSpores;
                }
            }
            if ((ai.creature.abstractAI as DeerAbstractAI).damageGoHome)
            {
                ai.currentUtility = Mathf.Max(ai.currentUtility, 0.8f);
                ai.behavior = DeerAI.Behavior.EscapeRain;
            }
            if (ai.currentUtility < 0.1f)
            {
                ai.behavior = DeerAI.Behavior.Idle;
            }
            if (ai.goToPuffBall != null && (ai.goToPuffBall.deleteMeNextFrame || !ai.PuffBallLegal(ai.goToPuffBall)))
            {
                ai.goToPuffBall = null;
            }
            if (!ModManager.MMF || !MoreSlugcats.MMF.cfgDeerBehavior.Value)
            {
                ai.FindGotoPuffBall();
            }
            if (ai.deer.playersInAntlers.Count > 0)
            {
                if (ModManager.MMF && MoreSlugcats.MMF.cfgDeerBehavior.Value)
                {
                    (ai.deer.abstractCreature.abstractAI as DeerAbstractAI).timeInRoom = 1;
                }
                ai.layDownAndRestCounter = 0;
                ai.restPos = null;
                if (!ai.lastPlayerInAntlers && ai.deer.room.TileWidth > 180 && Mathf.Abs(ai.inRoomDestination.x - ai.deer.room.TileWidth / 2) < ai.deer.room.TileWidth / 3)
                {
                    ai.inRoomDestination = ai.creature.pos;
                }
            }
            else if (ModManager.MMF && MoreSlugcats.MMF.cfgDeerBehavior.Value)
            {
                ai.FindGotoPuffBall();
            }
            ai.lastPlayerInAntlers = (ai.deer.playersInAntlers.Count > 0);
            if (ai.deerPileCounter < 50 && (ai.stuckTracker.Utility() < 0.5f & ai.behavior == DeerAI.Behavior.Idle))
            {
                for (int i = 0; i < ai.tracker.CreaturesCount; i++)
                {
                    if (ai.tracker.GetRep(i).representedCreature.creatureTemplate.type == CreatureTemplate.Type.Deer && ai.tracker.GetRep(i).representedCreature.personality.dominance > ai.deer.abstractCreature.personality.dominance && ai.tracker.GetRep(i).VisualContact && ai.tracker.GetRep(i).representedCreature.realizedCreature != null && (ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.behavior == DeerAI.Behavior.Idle)
                    {
                        Vector2 pos = ai.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos;
                        if (Mathf.Abs(ai.deer.mainBodyChunk.pos.x + 150f * ai.deer.flipDir - pos.x) < 300f && (ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).resting < 0.7f && Custom.DistLess(ai.deer.mainBodyChunk.pos + new Vector2(ai.deer.flipDir * 700f, 0f), pos, 900f) && (ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).flipDir != ai.deer.flipDir)
                        {
                            ai.kneelCounter = Math.Max(ai.kneelCounter, UnityEngine.Random.Range(40, 120));
                            if ((ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.tiredOfClosingEyesCounter < 300)
                            {
                                (ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.closeEyesCounter = 5;
                            }
                            (ai.tracker.GetRep(i).representedCreature.realizedCreature as Deer).AI.tiredOfClosingEyesCounter += 2;
                        }
                    }
                }
            }
            ai.kneelCounter--;
            ai.closeEyesCounter--;
            ai.layDownAndRestCounter--;
            ai.tiredOfClosingEyesCounter = Custom.IntClamp(ai.tiredOfClosingEyesCounter - 1, 0, 600);
            bool flag = false;

            if (ai.behavior == DeerAI.Behavior.EscapeRain)
            {
                InconAct(data);
            }
            else if (ai.behavior == DeerAI.Behavior.TrackSpores && ShadowOfOptions.deer_eat.Value)
            {
                if (ai.sporePos != null)
                {
                    ai.inRoomDestination = ai.sporePos.Value;
                    if (ai.sporePos.Value.room == ai.deer.room.abstractRoom.index && Custom.DistLess(ai.sporePos.Value.Tile, ai.creature.pos.Tile, 17f) && ai.VisualContact(ai.deer.room.MiddleOfTile(ai.sporePos.Value), 1f))
                    {
                        ai.sporePos = null;
                    }
                    flag = true;
                }
                if (!flag && ai.goToPuffBall != null)
                {
                    if (ai.goToPuffBall.representedItem.realizedObject != null && ai.goToPuffBall.VisualContact && ai.goToPuffBall.representedItem.realizedObject.grabbedBy.Count > 0 && Custom.DistLess(ai.deer.mainBodyChunk.pos, ai.goToPuffBall.representedItem.realizedObject.firstChunk.pos, 150f))
                    {
                        ai.heldPuffballNotGiven++;
                        if (ai.heldPuffballNotGiven > 200)
                        {
                            if (!ai.deniedPuffballs.Contains(ai.goToPuffBall.representedItem.ID))
                            {
                                ai.deniedPuffballs.Add(ai.goToPuffBall.representedItem.ID);
                            }
                            ai.heldPuffballNotGiven = 0;
                        }
                    }
                    if (ai.goToPuffBall.VisualContact && ai.goToPuffBall.representedItem.realizedObject != null && Custom.DistLess(ai.deer.mainBodyChunk.pos, ai.goToPuffBall.representedItem.realizedObject.firstChunk.pos, 100f) && !ai.deer.room.GetWorldCoordinate(ai.goToPuffBall.representedItem.realizedObject.firstChunk.pos).CompareDisregardingNode(ai.cantEatFromCoordinate))
                    {
                        ai.deer.EatObject(ai.goToPuffBall.representedItem.realizedObject);
                    }
                }
            }
            else if (ai.behavior == DeerAI.Behavior.Kneeling)
            {
                WorldCoordinate pos2 = ai.creature.pos;
                int num2 = ai.creature.pos.y;
                while (num2 >= 0 && ai.deer.room.aimap.TileAccessibleToCreature(pos2.x, num2, ai.creature.creatureTemplate) && !ai.deer.room.GetTile(pos2.x, num2 - 4).wormGrass && ai.deer.room.aimap.getAItile(pos2.x, num2).smoothedFloorAltitude > 5)
                {
                    pos2.y = num2;
                    num2--;
                }

            }
        }
        void InconAct(InconData data)
        {
            if (data.stunCountdown > 0)
            {
                return;
            }

            data.stunTimer = UnityEngine.Random.Range(20, 41);

            data.stunCountdown = data.stunTimer + UnityEngine.Random.Range(50, 101);
        }
    }
}