using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.ScavengerHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.ScavengerGraphics.Update += ScavengerGraphicsUpdate;

        On.Scavenger.Update += ScavengerUpdate;

        On.Scavenger.Collide += ScavengerCollide;

        On.ScavengerGraphics.DrawSprites += ScavengerGraphicsDrawSprites;

        On.Creature.LoseAllGrasps += CreatureLoseAllGrasps;

        new Hook(
            typeof(Scavenger).GetProperty(nameof(Scavenger.HeadLookPoint)).GetGetMethod(), ShadowOfScavengerHeadLookPoint);
    }

    static void CreatureLoseAllGrasps(On.Creature.orig_LoseAllGrasps orig, Creature self)
    {
        if (!ShadowOfOptions.scav_back_spear.Value || self.abstractCreature.creatureTemplate.TopAncestor().type != CreatureTemplate.Type.Scavenger || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self))
        {
            orig(self);

            return;
        }

        self.ReleaseGrasp(0);
    }

    public static Vector2 ShadowOfScavengerHeadLookPoint(Func<Scavenger, Vector2> orig, Scavenger self)
    {
        try
        {
            if (IsIncon(self))
            {
                if (self.room == null)
                {
                    return new Vector2(0f, 0f);
                }

                if (self.critLooker == null || self.critLooker.lookCreature == null)
                {
                    return self.lookPoint;
                }
                if (self.critLooker.lookCreature.VisualContact && self.critLooker.lookCreature.representedCreature.realizedCreature != null)
                {
                    return self.critLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
                }
                return self.room.MiddleOfTile(self.critLooker.lookCreature.BestGuessForPosition());
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
        return orig(self);
    }

    static void ScavengerCollide(On.Scavenger.orig_Collide orig, Scavenger self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig(self, otherObject, myChunk, otherChunk);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self) || otherObject is not Creature || otherObject is Fly)
        {
            return;
        }

        self.AI.tracker.SeeCreature((otherObject as Creature).abstractCreature);
    }

    static void ScavengerUpdate(On.Scavenger.orig_Update orig, Scavenger self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        bool standup = false;

        if (data.stunTimer > 0)
        {
            standup = true;
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

        if (self.animation != null)
        {
            if (!self.animation.Continue)
            {
                self.animation = null;
            }
            else
            {
                self.animation.Update();
            }
        }

        if (self.animation == null)
        {
            self.visionFactor = Mathf.Lerp(self.visionFactor, self.moving ? 0.6f : 0.75f, 1f);
        }
        else if (self.Rummaging)
        {
            self.visionFactor = Mathf.Lerp(self.visionFactor, 0.3f, 0.8f);
        }
        else if (self.animation is Scavenger.AttentiveAnimation)
        {
            self.visionFactor = Mathf.Lerp(self.visionFactor, 1f, 0.8f);
        }
        else
        {
            self.visionFactor = Mathf.Lerp(self.visionFactor, self.moving ? 0.6f : 0.75f, 0.8f);
        }
        if (self.narrowVision < 1f)
        {
            self.narrowVision = Mathf.Min(1f, self.narrowVision + 1f / Mathf.Lerp(40f, 10f, self.abstractCreature.personality.energy));
        }

        if (standup)
        {
            self.WeightedPush(0, 1, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Dot((self.bodyChunks[0].pos - self.bodyChunks[1].pos).normalized, new Vector2(0f, 1f)), -1f, 1f, 5.5f, 0.3f) * (1f - self.LittleStuck) * Mathf.Lerp(1f, 0.1f, Mathf.Pow(0, 2f)));
        }
        self.WeightedPush(0, 1, self.HeadLookDir, 0.05f * (1f - self.LittleStuck));
        Vector2 a = self.mainBodyChunk.pos + self.HeadLookDir * self.bodyChunkConnections[1].distance * (self.Pointing ? 0.4f : 1f);

        float num = (self.animation != null && self.animation is Scavenger.AttentiveAnimation) ? 12f : 5f;
        self.WeightedPush(2, 0, Vector2.ClampMagnitude(a - self.bodyChunks[2].pos, num), 0.8f / num);

        MovementConnection movementConnection = default;
        self.occupyTile = new IntVector2(-1, -1);

        self.swingPos = null;
        self.swingClimbCounter = 0;

        int num2 = -1;
        int num4 = 0;
        while (num4 < 2 && num2 < 0)
        {
            int num5 = 0;
            while (num5 < 5 && num2 < 0)
            {
                if (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[num4].pos + Custom.fourDirectionsAndZero[num5].ToVector2() * self.bodyChunks[num4].rad, self.Template))
                {
                    self.occupyTile = self.room.GetTilePosition(self.bodyChunks[num4].pos + Custom.fourDirectionsAndZero[num5].ToVector2() * self.bodyChunks[num4].rad);
                    movementConnection = self.FollowPath(self.room.GetWorldCoordinate(self.occupyTile), true);
                    num2 = num4;
                }
                num5++;
            }
            num4++;
        }

        if (self.Submersion > 0f && (self.occupyTile.y < -100 || self.SwimTile(self.occupyTile)))
        {
            self.movMode = Scavenger.MovementMode.Swim;
            self.moveModeChangeCounter = 0;
        }
        self.connections.Clear();

        self.moving = false;

        if (movementConnection != default(MovementConnection))
        {
            Scavenger.MovementMode a2 = Scavenger.MovementMode.StandStill;

            if (self.movMode == Scavenger.MovementMode.Swim)
            {
                if (!self.SwimTile(movementConnection.DestTile))
                {
                    self.moveModeChangeCounter = 10;
                }
                else
                {
                    for (int m = 0; m < Math.Min(5, self.connections.Count); m++)
                    {
                        if (Custom.DistLess(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.connections[m].destinationCoord), 50f) && !self.SwimTile(self.connections[m].DestTile))
                        {
                            self.moveModeChangeCounter = 10;
                            break;
                        }
                    }
                }
            }

            if (a2 != self.movMode)
            {
                self.moveModeChangeCounter++;
                if (self.moveModeChangeCounter > 10)
                {
                    self.movMode = a2;
                }
            }
            else
            {
                self.moveModeChangeCounter = 0;
            }

            if (self.movMode != Scavenger.MovementMode.Swim)
            {
                for (int n = 0; n < 3; n++)
                {
                    BodyChunk bodyChunk = self.bodyChunks[n];
                    bodyChunk.vel.y += self.gravity;
                }
                if (!ModManager.DLCShared || self.animation == null || self.animation.id != DLCSharedEnums.ScavengerAnimationID.Jumping)
                {
                    self.bodyChunks[0].vel *= 0.9f;
                    self.bodyChunks[1].vel *= 0.8f;
                    self.bodyChunks[2].vel *= 0.8f;
                }
            }
            else
            {
                self.bodyChunks[2].vel *= 0.8f;
                BodyChunk bodyChunk2 = self.bodyChunks[2];
                bodyChunk2.vel.y += self.gravity;
            }
        }

        if (self.movMode == Scavenger.MovementMode.StandStill)
        {
            self.flip = Mathf.Lerp(self.flip, Mathf.Clamp(self.HeadLookDir.x * 10f, -1f, 1f), 0.1f);
        }
        else if (self.movMode == Scavenger.MovementMode.Swim)
        {
            self.flip = Mathf.Lerp(self.flip, Mathf.Clamp(self.HeadLookDir.x * 1.5f, -1f, 1f), 0.1f);

            float num14 = Mathf.Lerp(self.room.FloatWaterLevel(self.mainBodyChunk.pos), self.room.waterObject.MultiSurfaceFWaterLevel(self.mainBodyChunk.pos), 0.5f);
            BodyChunk bodyChunk8 = self.bodyChunks[1];
            bodyChunk8.vel.y -= self.gravity * (1f - self.bodyChunks[2].submersion);
            self.bodyChunks[2].vel *= Mathf.Lerp(1f, 0.9f, self.mainBodyChunk.submersion);
            BodyChunk mainBodyChunk4 = self.mainBodyChunk;
            mainBodyChunk4.vel.y *= Mathf.Lerp(1f, 0.95f, self.mainBodyChunk.submersion);
            BodyChunk bodyChunk9 = self.bodyChunks[1];
            bodyChunk9.vel.y *= Mathf.Lerp(1f, 0.95f, self.bodyChunks[1].submersion);
            BodyChunk mainBodyChunk5 = self.mainBodyChunk;
            mainBodyChunk5.vel.y += Mathf.Clamp((num14 - self.mainBodyChunk.pos.y) / 14f, -0.5f, 0.5f);
            BodyChunk bodyChunk10 = self.bodyChunks[1];
            bodyChunk10.vel.y += Mathf.Clamp((num14 - self.bodyChunkConnections[0].distance - self.bodyChunks[1].pos.y) / 14f, -0.5f, 0.5f);
        }

        void AIUpdate()
        {
            ScavengerAI ai = self.AI;

            if (!RainWorldGame.RequestHeavyAi(ai.scavenger))
            {
                return;
            }

            MiscHooks.AIUpdate(ai);

            if (ai.noiseReactDelay > 0)
            {
                ai.noiseReactDelay--;
            }
            if (ModManager.MSC && ai.scavenger.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.scavenger.LickedByPlayer.abstractCreature);
                if (ai.scared < 1f)
                {
                    ai.scared += 0.0125f;
                }
                if (ai.agitation < 1f)
                {
                    ai.agitation += 0.0213f;
                }
            }

            ai.backedByPack = 0f;
            ai.tradeSpot = null;
            ai.wantToTradeWith = null;
            ai.giftForMe = null;

            ai.age++;

            ai.noiseTracker.hearingSkill = (ai.scavenger.moving ? 0.8f : Mathf.Lerp(1.4f, 0.9f, ai.agitation));
            for (int i = 0; i < ai.creature.state.socialMemory.relationShips.Count; i++)
            {
                ai.creature.state.socialMemory.relationShips[i].EvenOutTemps(0.0005f);
                if (ai.creature.state.socialMemory.relationShips[i].subjectID.number == 0)
                {
                    ai.likeB = ai.creature.state.socialMemory.relationShips[i].like;
                    ai.tempLikeB = ai.creature.state.socialMemory.relationShips[i].tempLike;
                }
            }
            ai.arrangeInventoryCounter--;
            if (false && ai.arrangeInventoryCounter < 0)
            {
                bool flag = ai.scavenger.ArrangeInventory();
                ai.arrangeInventoryCounter = UnityEngine.Random.Range(10, (!ai.scavenger.moving || flag) ? 20 : 400);
            }
            if (ai.scavenger.room == null)
            {
                return;
            }
            
            ai.DecideBehavior();
            ai.UpdateLookPoint();

            ai.scavageItemCheck--;

            if (false && ai.scavageItemCheck < 1)
            {
                ai.CheckForScavangeItems(true);
                ai.scavageItemCheck = UnityEngine.Random.Range(40, 200);
            }
            if (false && ai.scavengeCandidate != null)
            {
                if (ai.scavengeCandidate.BestGuessForPosition().Tile.FloatDist(ai.creature.pos.Tile) < 4f)
                {
                    ai.scavenger.LookForItemsToPickUp();
                }
                if (ai.CollectScore(ai.scavengeCandidate, true) < 1 || ai.scavengeCandidate.deleteMeNextFrame || ai.scavengeCandidate.BestGuessForPosition().room != ai.creature.pos.room)
                {
                    ai.scavengeCandidate = null;
                    ai.CheckForScavangeItems(false);
                }
            }
            float num = 0f;
            //ai.UpdateCurrentViolenceType();
            if (ai.behavior == ScavengerAI.Behavior.Idle)
            {
                num = ai.discomfortWithOtherCreatures * 0.5f;
                ai.runSpeedGoal = ai.creature.personality.energy * 0.3f + ai.creature.personality.nervous * 0.7f;
                ai.runSpeedGoal = Mathf.Lerp(ai.runSpeedGoal, 1f, ai.discomfortWithOtherCreatures * 0.5f);

                ai.discomfortWithOtherCreatures = ai.discomfortTracker.DiscomfortOfTile(ai.scavenger.room.GetWorldCoordinate(ai.scavenger.occupyTile));
                ai.idleCounter--;
                if (!ai.scavenger.moving && !ai.scavenger.Rummaging)
                {
                    ai.idleCounter -= 3;
                }
                if (ai.scavenger.ReallyStuck > 1f)
                {
                    ai.idleCounter = Mathf.Min(ai.idleCounter, 10);
                }
                if (ai.idleCounter < 1)
                {
                    ai.idleCounter = UnityEngine.Random.Range(100, 200 + (int)(1900f * (1f - ai.creature.personality.nervous))) * 4;
                }
                if (!ai.scavenger.moving && ai.idleCounter > (ai.scavenger.Elite ? 300 : 100) && ai.scavenger.animation == null && Scavenger.RummageAnimation.RummagePossible(ai.scavenger))
                {
                    ai.scavenger.animation = new Scavenger.RummageAnimation(ai.scavenger);
                }
            }
            else if (ai.behavior == ScavengerAI.Behavior.Flee || ai.behavior == ScavengerAI.Behavior.Attack)
            {
                ai.runSpeedGoal = 1f;
                ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                InconAct();
            }
            else if (ai.behavior == ScavengerAI.Behavior.EscapeRain || ai.behavior == ScavengerAI.Behavior.Injured)
            {
                ai.runSpeedGoal = 0.7f;
                num = 0.5f;
                InconAct();
            }
            else if (ai.behavior == ScavengerAI.Behavior.Investigate)
            {
                ai.runSpeedGoal = 0.6f;
                InconAct();
            }
            else if (ai.behavior == ScavengerAI.Behavior.CommunicateWithPlayer && ai.communicationModule.target != null)
            {
                ai.focusCreature = ai.communicationModule.target;
            }
            ai.runSpeedGoal = Mathf.Lerp(ai.runSpeedGoal, Mathf.Max(ai.runSpeedGoal, ai.scared), 0.5f);
            num = Mathf.Max(num, ai.scared);
            num = Mathf.Lerp(num, 1f, ai.noiseTracker.Utility() * 0.3f);
            float num2 = Mathf.Lerp(Mathf.Pow(ai.threatTracker.Utility(), Mathf.Lerp(0.2f, 1.8f, ai.creature.personality.bravery)), 1f, ai.noiseTracker.Utility() * Mathf.Pow(1f - ai.creature.personality.bravery, 3f) * 0.6f);
            if (ai.scavenger.room.locusts != null && ai.scavenger.room.locusts.HasLocustThreat(ai.scavenger.room.GetTilePosition(ai.scavenger.mainBodyChunk.pos)))
            {
                num2 = 1f;
            }
            ai.scared = Mathf.Lerp(ai.scared, num2, 0.01f);
            if (ai.scared < num2)
            {
                ai.scared = Mathf.Min(num2, ai.scared + 0.033333335f);
            }
            else
            {
                ai.scared = Mathf.Max(num2, ai.scared - 1f / Mathf.Lerp(900f, 30f, Mathf.Pow(ai.creature.personality.bravery, 0.7f)));
            }
            if (ai.agitation < num)
            {
                ai.agitation = Mathf.Min(num, ai.agitation + 1f / (180f * (1f + ai.agitation * 2f)));
            }
            else
            {
                ai.agitation = Mathf.Max(num, ai.agitation - 1f / (Mathf.Lerp(280f, 600f, ai.creature.personality.nervous) * (1f + ai.agitation * 2f)));
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

    static void ScavengerGraphicsUpdate(On.ScavengerGraphics.orig_Update orig, ScavengerGraphics self)
    {
        orig(self);

        if (!IsComa(self.scavenger))
        {
            return;
        }

        if (IsIncon(self.scavenger))
        {
            self.markAlpha = Mathf.Lerp(self.markAlpha, 1f, 0.2f);
        }
        else
        {
            self.markAlpha = Mathf.Lerp(self.markAlpha, UnityEngine.Random.Range(0f, 0.5f), 0.25f);
        }

        if (IsUncon(self.scavenger))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    static void ScavengerGraphicsDrawSprites(On.ScavengerGraphics.orig_DrawSprites orig, ScavengerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPosV2)
    {
        orig(self, sLeaser, rCam, timeStacker, camPosV2);

        if (!breathstorage.TryGetValue(self, out BreathData data) || !IsUncon(self.scavenger))
        {
            return;
        }

        float num4 = (Mathf.Sin(Mathf.Lerp(data.lastBreath, data.breath, timeStacker) * 3.1415927f * 2f) + 1f) * 0.5f;

        float num6 = self.scavenger.bodyChunks[0].rad;

        num6 *= 1f + num4 * (float)3 * 0.1f * 0.5f;

        //sLeaser.sprites[self.ChestSprite].scale = num6 / 10f;

        sLeaser.sprites[self.ChestSprite].scaleX = num6 * Mathf.Lerp(0.7f, 1.3f, self.iVars.fatness) / 10f;
        sLeaser.sprites[self.ChestSprite].scaleY = (num6 + Mathf.Lerp(2f, 1.5f, self.iVars.narrowWaist)) / 10f;
    }
}