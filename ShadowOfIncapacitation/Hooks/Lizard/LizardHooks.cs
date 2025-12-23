using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.LizardHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.LizardGraphics.Update += LizardGraphicsUpdate;

        On.Lizard.Update += LizardUpdate;

        On.Lizard.Violence += LizardViolence;

        On.LizardAI.Update += LizardAIUpdate;

        On.LizardAI.WantToStayInDenUntilEndOfCycle += LizardAIWantToStayInDenUntilEndOfCycle;

        On.LizardAI.LizardInjuryTracker.Utility += LizardInjuryTrackerUtility;

        On.Lizard.ctor += NewLizard;

        new Hook(
            typeof(LizardLimb).GetProperty(nameof(LizardLimb.health)).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(LizardLimbHealth)));

        new Hook(
            typeof(Lizard).GetProperty(nameof(Lizard.IsWallClimber)).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(LizardIsWallClimber)));
    }

    static void NewLizard(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);

        if (!inconstorage.TryGetValue(abstractCreature, out InconData data) || !data.forceTame)
        {
            return;
        }

        data.forceTame = false;

        if (ShadowOfOptions.debug_logs.Value)
            Debug.Log(all + self + " attempting to force friendship");

        for (int j = 0; j < abstractCreature.world.game.Players.Count; j++)
        {
            SocialMemory.Relationship relationship = abstractCreature.state.socialMemory.GetRelationship(abstractCreature.world.game.Players[j].ID);

            relationship.like = 1f;
            relationship.tempLike = 1f;

            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + self + " and " + abstractCreature.world.game.Players[j] + " are now friends!");
        }
    }

    static bool LizardAIWantToStayInDenUntilEndOfCycle(On.LizardAI.orig_WantToStayInDenUntilEndOfCycle orig, LizardAI self)
    {
        if (!inconstorage.TryGetValue(self.lizard.abstractCreature, out InconData data) || !data.returnToDen)
        {
            return orig(self);
        }

        return true;
    }

    static void LizardAIUpdate(On.LizardAI.orig_Update orig, LizardAI self)
    {
        orig(self);

        if (self == null || self.lizard == null || self.lizard.abstractCreature == null || self.creature == null || self.creature.abstractAI == null || self.creature.abstractAI.destination == null || !inconstorage.TryGetValue(self.lizard.abstractCreature, out InconData data) || !data.returnToDen || self.denFinder == null || self.denFinder.denPosition == null)
        {
            return;
        }

        self.creature.abstractAI.SetDestination(self.denFinder.denPosition.Value);
    }

    static float LizardInjuryTrackerUtility(On.LizardAI.LizardInjuryTracker.orig_Utility orig, LizardAI.LizardInjuryTracker self)
    {
        return IsIncon(self.AI.creature.realizedCreature) ? 0 : orig(self);
    }

    public static float LizardLimbHealth(Func<LizardLimb, float> orig, LizardLimb self)
    {
        try
        {
            if (IsIncon(((LizardGraphics)self.owner).lizard) && inconstorage.TryGetValue(((LizardGraphics)self.owner).lizard.abstractCreature, out InconData data) && data.stunTimer > 0 && (!shadowOfLizardsCheck || !ShadowOfLizardsLegDead()))
            {
                return 1f;
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
        return orig(self);

        bool ShadowOfLizardsLegDead()
        {
            return ShadowOfLizards.ShadowOfLizards.lizardstorage.TryGetValue(((LizardGraphics)self.owner).lizard.abstractCreature, out ShadowOfLizards.ShadowOfLizards.LizardData data) && data.armState[self.limbNumber] != "Normal";
        }
    }

    public static bool LizardIsWallClimber(Func<Lizard, bool> orig, Lizard self)
    {
        try
        {
            if (IsIncon(self) && inconstorage.TryGetValue(self.abstractCreature, out InconData data))
            {
                return ModManager.MMF && self.room != null && self.room.gravity <= global::Lizard.zeroGravityMovementThreshold;
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
        return orig(self);
    }

    static void LizardUpdate(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        LizardAI ai = self.AI;

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

            AI();

            bool flag = false;
            bool flag2 = false;
            int num = 0;
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                if (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[i].pos, self.abstractCreature.creatureTemplate))
                {
                    flag = (flag || self.bodyChunks[i].index == 0);
                    flag2 = (flag2 || self.bodyChunks[i].index > 0);
                    num++;
                }
            }
            if ((flag || flag2 || self.movementAnimation != null) && self.inAllowedTerrainCounter < 100)
            {
                self.inAllowedTerrainCounter++;
            }
            else
            {
                bool flag3 = false;
                int num2 = 0;
                while (!flag3 && num2 < 3)
                {
                    int num3 = 0;
                    while (!flag3 && num3 < 4)
                    {
                        if (self.room.aimap.TileAccessibleToCreature(self.room.GetTilePosition(self.bodyChunks[num2].pos) + Custom.fourDirections[num3], self.abstractCreature.creatureTemplate) && !self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[num2].pos) + Custom.fourDirections[num3]).AnyWater)
                        {
                            flag3 = true;
                        }
                        num3++;
                    }
                    num2++;
                }
                if (flag3)
                {
                    self.inAllowedTerrainCounter = Math.Max(0, self.inAllowedTerrainCounter - 10);
                }
                else
                {
                    self.inAllowedTerrainCounter = 0;
                }
            }
            self.applyGravity = (self.inAllowedTerrainCounter < self.lizardParams.regainFootingCounter || self.NoGripCounter > 10 || self.commitedToDropConnection != default(MovementConnection));
            //self.swim = Mathf.Clamp(self.swim - 0.05f, 0f, 1f);
            ai.stuckTracker.satisfiedWithThisPosition = (self.swim > 0.5f);
            float num4 = self.ActAnimation();
            if (ModManager.DLCShared && self.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard && self.Submersion == 0f)
            {
                bool flag4 = true;
                if (self.room.aimap.getAItile(self.mainBodyChunk.pos).narrowSpace || self.room.GetTile(self.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                {
                    flag4 = false;
                }
                if (self.Submersion > 0f)
                {
                    flag4 = false;
                }
                if (flag4)
                {
                    self.snakeTicker++;
                    num4 *= Mathf.Lerp(0.25f, 1f, self.SnakeCoil);
                }
            }

            self.desperationSmoother = Custom.LerpAndTick(self.desperationSmoother, Mathf.Max((float)self.timeSpentTryingThisMove + Mathf.Lerp(-300f, 0f, num4), ai.stuckTracker.Utility() * 100f), 0.05f, 0.5f);
            if (self.desperationSmoother > Mathf.Lerp(40f, 10f, num4))
            {
                self.bodyWiggleCounter += 2;
            }
            else
            {
                int n = self.bodyWiggleCounter;
                self.bodyWiggleCounter = n - 1;
            }

            self.timeInAnimation++;
            if (self.timeToRemainInAnimation > -1 && self.timeInAnimation > self.timeToRemainInAnimation)
            {
                self.EnterAnimation(global::Lizard.Animation.Standard, true);
            }
            if (self.bubble == 0 && UnityEngine.Random.value < 0.05f && UnityEngine.Random.value < ai.excitement && UnityEngine.Random.value < ai.excitement)
            {
                self.bubbleIntensity = ai.excitement * ai.excitement * UnityEngine.Random.value;
                self.bubble = 1;
            }

            if (false && ShadowOfOptions.liz_rot.Value && self.rotModule != null)
            {
                self.rotModule.Act();
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AI()
        {
            if (ai.behavior == LizardAI.Behavior.Flee && !RainWorldGame.RequestHeavyAi(self))
            {
                return;
            }
            if (ModManager.MSC && self.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(self.LickedByPlayer.abstractCreature);
            }
            if (ai.panic > 0)
            {
                ai.panic--;
            }
            ai.timeInRoom++;

            ai.creature.state.socialMemory.EvenOutAllTemps(0.0005f);
            ai.pathFinder.walkPastPointOfNoReturn = (ai.stranded || ai.denFinder.GetDenPosition() == null || !ai.pathFinder.CoordinatePossibleToGetBackFrom(ai.denFinder.GetDenPosition().Value));
            ai.fear = ai.utilityComparer.GetSmoothedNonWeightedUtility(ai.threatTracker);
            ai.hunger = ai.utilityComparer.GetSmoothedNonWeightedUtility(ai.preyTracker);
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
            ((ai.utilityComparer.GetUtilityTracker(ai.threatTracker).smoother as FloatTweener.FloatTweenUpAndDown).down as FloatTweener.FloatTweenBasic).speed = 1f / ((float)(ai.lastDistressLength + 20) * 3f);
            ai.utilityComparer.GetUtilityTracker(ai.agressionTracker).weight = (ai.creature.world.game.IsStorySession ? 0.25f : 0.125f) * self.LizardState.health;
            ai.utilityComparer.GetUtilityTracker(ai.rainTracker).weight = (ai.friendTracker.CareAboutRain() ? 0.9f : 0.1f);
            ai.behavior = ai.DetermineBehavior();

            if (UnityEngine.Random.value < 0.0125f)
            {
                if(ShadowOfOptions.liz_voice.Value)
                    self.voice.MakeSound(LizardVoice.Emotion.PainIdle, 1f - (self.State as LizardState).health);

                if (UnityEngine.Random.value < 0.2f)
                {
                    InconAct(data);
                }
            }

            self.JawOpen = self.JawOpen - 0.01f;
            if (ai.behavior == LizardAI.Behavior.Flee)
            {
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
                    if (UnityEngine.Random.value < 0.05f && ai.threatTracker.mostThreateningCreature != null && ai.threatTracker.mostThreateningCreature.BestGuessForPosition().Tile.FloatDist(ai.creature.pos.Tile) < 15f)
                    {
                        InconAct(data);
                        self.EnterAnimation(global::Lizard.Animation.ThreatSpotted, false);
                    }
                }
                ai.runSpeed = Mathf.Lerp(ai.runSpeed, Mathf.Pow(ai.CombinedFear, 0.1f), 0.5f);
                ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                if (self.graphicsModule != null)
                {
                    (self.graphicsModule as LizardGraphics).lookPos = self.room.MiddleOfTile(ai.pathFinder.GetDestination);
                }
            }
            else if (ai.behavior == LizardAI.Behavior.Hunt)
            {
                Tracker.CreatureRepresentation mostAttractivePrey = ai.preyTracker.MostAttractivePrey;
                if (mostAttractivePrey != null)
                {
                    ai.AggressiveBehavior(mostAttractivePrey, 0f);
                }
                ai.runSpeed = Mathf.Lerp(ai.runSpeed, Mathf.Max(ai.hunger, 0.3f), 0.1f);
                if (ShadowOfOptions.liz_voice.Value && UnityEngine.Random.value < 0.0125f && !ai.lizard.safariControlled && ai.creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
                {
                    ai.lizard.voice.MakeSound(LizardVoice.Emotion.BloodLust);
                }
            }
            else if (ai.behavior == LizardAI.Behavior.Idle)
            {
                int num = ai.idleCounter;
                ai.idleCounter--;
                if (ai.creature.pos.room == ai.pathFinder.GetDestination.room)
                {
                    if (!ai.pathFinder.GetDestination.TileDefined)
                    {
                        ai.idleCounter = 0;
                    }
                    else if (Custom.ManhattanDistance(ai.creature.pos.Tile, ai.pathFinder.GetDestination.Tile) < 5)
                    {
                        ai.idleCounter -= self.lizardParams.idleCounterSubtractWhenCloseToIdlePos;
                        ai.idleRestlessness--;
                        bool flag = (ai.idleCounter < 1 || ai.idleRestlessness > 0);
                        if (!flag && !ai.ComfortableIdlePosition())
                        {
                            ai.unableToFindComfortablePosition++;
                        }
                        else if (self.swim < 0.5f && (self.bodyChunks[0].vel.magnitude > 2f || self.bodyChunks[1].vel.magnitude > 2f || self.bodyChunks[2].vel.magnitude > 2f))
                        {
                            ai.unableToFindComfortablePosition++;
                        }
                        else
                        {
                            ai.unableToFindComfortablePosition = 0;
                        }
                        if (ai.unableToFindComfortablePosition > 40)
                        {
                            ai.idleCounter = 0;
                            ai.unableToFindComfortablePosition = 0;
                        }
                        else if (self.IsTileSolid(0, 0, -1) && self.IsTileSolid(1, 0, -1) && self.IsTileSolid(2, 0, -1))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                BodyChunk bodyChunk = self.bodyChunks[i];
                                bodyChunk.vel.y = bodyChunk.vel.y - 0.1f;
                            }
                        }
                        if (ai.idleRestlessness < 40 && UnityEngine.Random.value < 0.0016666667f)
                        {
                            ai.idleRestlessness = UnityEngine.Random.Range(1, 40);
                        }
                    }
                }
                if (ai.IdleSpotScore(ai.creature.abstractAI.destination) == 3.4028235E+38f)
                {
                    ai.idleCounter = 0;
                }
                if (ai.idleCounter < 1)
                {
                    if (num > 0)
                    {
                        ai.forbiddenIdleSpot = ai.pathFinder.GetDestination;
                    }
                    WorldCoordinate worldCoordinate = new WorldCoordinate(ai.creature.Room.index, UnityEngine.Random.Range(0, self.room.TileWidth), UnityEngine.Random.Range(0, self.room.TileHeight), -1);
                    if (ai.IdleSpotScore(worldCoordinate) < ai.IdleSpotScore(ai.creature.abstractAI.destination))
                    {
                        ai.idleSpotWinStreak = 0;
                    }
                    else
                    {
                        ai.idleSpotWinStreak++;
                        if (ai.idleSpotWinStreak > 50)
                        {
                            ai.idleCounter = UnityEngine.Random.Range(1000, 4000);
                            ai.idleSpotWinStreak = 0;
                        }
                    }
                }

                ai.runSpeed = Mathf.Lerp(ai.runSpeed, 0f, 0.5f);

                if (ShadowOfOptions.liz_voice.Value && UnityEngine.Random.value < 0.00125f && !self.safariControlled)
                    self.voice.MakeSound(LizardVoice.Emotion.Boredom);

                ai.focusCreature = null;
            }
            else if (ai.behavior == LizardAI.Behavior.EscapeRain)
            {
                if (ai.denFinder.GetDenPosition() != null)
                {
                    InconAct(data);
                }
                ai.runSpeed = Mathf.Lerp(ai.runSpeed, 1f, 0.1f);
            }
            else if (ai.behavior == LizardAI.Behavior.Frustrated)
            {
                ai.focusCreature = ai.preyTracker.MostAttractivePrey;
                ai.runSpeed = Mathf.Lerp(ai.runSpeed, 1f, 0.5f);
                if (UnityEngine.Random.value < 0.1f || (Custom.ManhattanDistance(ai.creature.pos.Tile, ai.creature.abstractAI.destination.Tile) < 10 && ai.creature.pos.room == ai.creature.abstractAI.destination.room))
                {
                    WorldCoordinate worldCoordinate2 = new WorldCoordinate(ai.creature.Room.index, UnityEngine.Random.Range(0, self.room.TileWidth), UnityEngine.Random.Range(0, self.room.TileHeight), -1);
                    if (ai.pathFinder.CoordinateReachable(worldCoordinate2) && ai.pathFinder.CoordinatePossibleToGetBackFrom(worldCoordinate2))
                    {
                        ai.idleRestlessness = UnityEngine.Random.Range(30, 100);
                    }
                }
                if (UnityEngine.Random.value < 0.0125f)
                {
                    InconAct(data);
                    self.EnterAnimation(global::Lizard.Animation.PreyReSpotted, false);
                }
                if (UnityEngine.Random.value < 0.0125f)
                {
                    InconAct(data);
                    self.bodyWiggleCounter = Math.Max(self.bodyWiggleCounter, (int)(UnityEngine.Random.value * 100f));
                }
                if (ShadowOfOptions.liz_voice.Value && UnityEngine.Random.value < 0.0125f && !self.safariControlled && ai.creature.creatureTemplate.type != CreatureTemplate.Type.BlackLizard)
                {
                    self.voice.MakeSound(LizardVoice.Emotion.Frustration);
                }
            }
            else if (ai.behavior == LizardAI.Behavior.InvestigateSound)
            {
                InconAct(data);

                ai.runSpeed = Mathf.Lerp(ai.runSpeed, 0.6f, 0.2f);
            }
            else if (ai.behavior == LizardAI.Behavior.FollowFriend)
            {
                InconAct(data);

                ai.runSpeed = Mathf.Lerp(ai.runSpeed, ai.friendTracker.RunSpeed(), 0.6f);
                if (self.grasps[0] != null && self.grasps[0].grabbed == ai.friendTracker.friend)
                {
                    self.ReleaseGrasp(0);
                }
                Tracker tracker = ai.tracker;
                Creature friend = ai.friendTracker.friend;
                ai.focusCreature = tracker.RepresentationForCreature((friend != null) ? friend.abstractCreature : null, false);
            }
            if (ai.behavior != LizardAI.Behavior.Idle && ai.creature.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
            {
                ai.runSpeed = Mathf.Pow(ai.runSpeed, 0.7f);
            }
            ai.runSpeed = Mathf.Max(ai.runSpeed, ai.stuckTracker.Utility());
            if (self.Template.type == CreatureTemplate.Type.BlackLizard || self.Template.type == Watcher.WatcherEnums.CreatureTemplateType.IndigoLizard)
            {
                ai.noiseTracker.hearingSkill = 2f;
            }
            else
            {
                ai.noiseTracker.hearingSkill = Custom.LerpMap(ai.runSpeed, 0f, 0.6f, 1.3f, 0.6f);
            }

            if (self.rotModule != null)
            {
                ai.noiseTracker.hearingSkill = ((self.rotModule.RotEyesClose < 1) ? 2f : 1f);
            }
            if (ShadowOfOptions.liz_attack.Value && ai.casualAggressionTarget != null && !self.safariControlled && UnityEngine.Random.value < 0.5f && ai.casualAggressionTarget.VisualContact && Custom.DistLess(ai.casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos, self.mainBodyChunk.pos, self.lizardParams.attemptBiteRadius))
            {
                InconAct(data);
                self.AttemptBite(ai.casualAggressionTarget.representedCreature.realizedCreature);
            }
            if (ShadowOfOptions.liz_spit.Value && (self.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.DLCShared && self.Template.type == DLCSharedEnums.CreatureTemplateType.SpitLizard)) && ai.redSpitAI.spitting)
            {
                InconAct(data);
                self.EnterAnimation(global::Lizard.Animation.Spit, false);
            }
            if (ShadowOfOptions.liz_blizzard.Value && self.blizzardModule != null)
            {
                if (ai.behavior == LizardAI.Behavior.Hunt && self.blizzardModule.beamTimer == 0 && ai.preyTracker.MostAttractivePrey != null && ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null && !ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.dead && ai.VisualContact(ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk))
                {
                    InconAct(data);
                    self.blizzardModule.targetInViewTime = Mathf.Min(self.blizzardModule.targetInViewTime + 1, 220);
                    float num2 = Vector2.Distance(ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, self.firstChunk.pos);
                    float num3 = Mathf.Min((float)self.blizzardModule.targetInViewTime / 220f, 1f);
                    if ((num2 < 700f) ? ((double)UnityEngine.Random.value < 0.03 * (double)num3) : ((double)UnityEngine.Random.value < 0.05 * (double)num3))
                    {
                        self.blizzardModule.beamTimer = 1;
                        self.room.AddObject(new ShockWave(self.firstChunk.pos, 530f, 0.045f, 9, false));
                        self.room.PlaySound(Watcher.WatcherEnums.WatcherSoundID.BlizLiz_Pop, self.firstChunk.pos, 0.5f, 1f);
                        self.room.PlaySound(Watcher.WatcherEnums.WatcherSoundID.BlizLiz_Charge, self.firstChunk.pos, 1f, 1f);
                    }
                }
                else if (self.blizzardModule.targetInViewTime > 0)
                {
                    self.blizzardModule.targetInViewTime--;
                }
            }

            MiscHooks.AIUpdate(ai);
        }
    }

    static void LizardGraphicsUpdate(On.LizardGraphics.orig_Update orig, LizardGraphics self)
    {
        orig(self);

        if (!IsComa(self.lizard))
        {
            return;
        }

        if (IsInconBase(self.lizard))
        {
            self.breath += 0.0200f;

            if (self.flicker <= 0)
            {
                self.blink += (Mathf.Lerp(0.0025f, 0.07f, Mathf.Max(self.lizard.AI.excitement, self.showDominance)) + UnityEngine.Random.value * 0.001f) / 4;
            }
        }
        else
        {
            self.breath += 0.0100f;

            self.lizard.AI.runSpeed = 0.3f;

            if (self.flicker <= 0)
            {
                self.blink += (Mathf.Lerp(0.0025f, 0.07f, Mathf.Max(self.lizard.AI.excitement, self.showDominance)) + UnityEngine.Random.value * 0.001f) / 10;
            }
        }
    }

    static void LizardViolence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || type == null || !data.isAlive)
        {
            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);

            return;
        }

        try
        {
            bool sourceOwnerFlag = source != null && source.owner != null;

            if (data.inconCycle != CycleNum(self.abstractCreature))
                PreViolenceCheck(self, data);

            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);

            #region lastDamageType
            data.lastDamageType = type.ToString();

            if (type == Creature.DamageType.Bite || type == Creature.DamageType.Stab)
            {
                data.lastDamageType = "Stab";
            }

            if (sourceOwnerFlag && source.owner is DartMaggot)
            {
                data.lastDamageType = "Uncon";
            }
            #endregion

            if (data.returnToDen && sourceOwnerFlag && source.owner is Player)
            {
                data.returnToDen = false;
            }

            if (self.LizardState.health <= ShadowOfOptions.insta_die_threshold.Value || self.LizardState.health <= data.dieHealthThreshold && type != Creature.DamageType.Blunt)
            {
                data.isAlive = false;

                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " reached the insta die threshold and died");

                return;
            }

            if (data.inconCycle != CycleNum(self.abstractCreature))
            {
                PostViolenceCheck(self, data, type.ToString(), sourceOwnerFlag && source.owner is Creature crit ? crit : null);
                return;
            }

            if (IsInconBase(self) && hitChunk != null && (sourceOwnerFlag && source.owner is DartMaggot && !LizHitHeadShield(directionAndMomentum.Value) || hitChunk.index == 0 && UnityEngine.Random.Range(0, 100) < UnconChance() || type == Creature.DamageType.Blunt && self.LizardState.health <= (data.healthThreshold + data.dieHealthThreshold) / 2))
            {
                data.isUncon = true;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " was knocked Uncontious");
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        int UnconChance()
        {
            int chance;

            switch (data.lastDamageType)
            {
                case "Blunt":
                    chance = ShadowOfOptions.uncon_chance_blunt.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Blunt chance to uncon");
                    break;
                case "Stab":
                    chance = ShadowOfOptions.uncon_chance_stab.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Stab chance to uncon");
                    break;
                case "Explosion":
                    chance = ShadowOfOptions.uncon_chance_explosion.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Explosion chance to uncon");
                    break;
                case "Electric":
                    chance = ShadowOfOptions.uncon_chance_electric.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Electric chance to uncon");
                    break;
                default:
                    chance = 0;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Null chance to uncon");
                    break;
            }

            return chance;
        }

        bool LizHitHeadShield(Vector2 direction)
        {
            float num19 = Vector2.Angle(direction, -self.bodyChunks[0].Rotation);
            if (LizHitInMouth(direction))
            {
                return false;
            }
            if (num19 < self.lizardParams.headShieldAngle + 20f * self.JawOpen)
            {
                return true;
            }
            return false;
        }
        bool LizHitInMouth(Vector2 direction)
        {
            if (direction.y > 0f)
            {
                return false;
            }
            direction = Vector3.Slerp(direction, new Vector2(0f, 1f), 0.1f);
            return Mathf.Abs(Vector2.Angle(direction, -self.bodyChunks[0].Rotation)) < Mathf.Lerp(-15f, 11f, self.JawOpen);
        }
    }
}