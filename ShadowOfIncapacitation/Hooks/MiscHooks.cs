using RWCustom;
using UnityEngine;
using System;
using System.Collections.Generic;

using static Incapacitation.Incapacitation;
using Mono.Cecil;

namespace Incapacitation;

internal class MiscHooks
{
    public static void Apply()
    {
        #region Apply Creature
        On.AbstractCreature.ctor += NewAbstractCreature;
        On.Creature.Die += CreatureDie;

        On.SaveState.AbstractCreatureToStringStoryWorld_AbstractCreature_WorldCoordinate += SaveStateSaveAbstractCreature;
        On.CreatureState.LoadFromString += CreatureState_LoadFromString;
        #endregion

        On.Tracker.CreatureNoticed += TrackerCreatureNoticed;

        #region Creature Hooks
        On.Creature.Violence += CreatureViolence;
        #endregion

        On.StuckTracker.Utility += StuckTrackerUtility;
    }

    static float StuckTrackerUtility(On.StuckTracker.orig_Utility orig, StuckTracker self)
    {
        return self.AI.creature.realizedCreature != null && IsComa(self.AI.creature.realizedCreature) ? 0 : orig(self);
    }

    #region Apply Creature
    static void NewAbstractCreature(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);

        if (!IsAbstractCreatureValid(self))
        {
            return;
        }

        if (!inconstorage.TryGetValue(self, out _))
        {
            inconstorage.Add(self, new InconData());
        }

        if (ShadowOfOptions.debug_logs.Value)
            Debug.Log(all + "Added InconData to " + self);
    }

    static void CreatureDie(On.Creature.orig_Die orig, Creature self)
    {
        if (!ShadowOfOptions.cheat_death.Value || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || IsAbstractCreatureCheatDeathValid(self.abstractCreature) || (shadowOfLizardsCheck && HasLizardData()))
        {
            orig(self);
            return;
        }

        try
        {
            if (!data.isAlive && ModManager.DLCShared && self is BigSpider spid && spid.mother && ShadowOfOptions.spid_mother.Value && ShadowOfOptions.spid_state.Value != "Disabled")
            {
                if (!spid.spewBabies)
                {
                    data.spiderMotherWasDead = true;
                    spid.BabyPuff();
                }
                else
                {
                    data.spiderMotherWasDead = false;
                }
            }

            if (data.actuallyDead)
            {
                orig(self);
                return;
            }

            if (!self.dead)
            {
                //ViolenceCheck(self, data, data.lastDamageType);

                data.cheatDeathChance = UnityEngine.Random.Range(0, 101);
            }

            if (!data.isAlive || self.State is HealthState healthstate && (healthstate.health <= ShadowOfOptions.insta_die_threshold.Value || healthstate.health <= data.dieHealthThreshold) || self.State is PlayerState playerState && playerState.permanentDamageTracking >= 0.8)
            {
                data.actuallyDead = true;

                //self.dead = false;

                if (self.abstractCreature.state != null)
                    self.abstractCreature.state.alive = true;

                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " is dead, it will not survive the cycle");

                orig(self);
            }
            else if (!data.isUncon)
            {
                if (ShadowOfOptions.incon_chance_cheat_death.Value < data.cheatDeathChance)
                {
                    //self.dead = false;

                    if (self.abstractCreature.state != null)
                        self.abstractCreature.state.alive = false;

                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " is Incapacitated, it will survive the cycle");

                    orig(self);
                }
                else
                {
                    data.actuallyDead = true;

                    //self.dead = false;

                    if (self.abstractCreature.state != null)
                        self.abstractCreature.state.alive = true;

                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " is Incapacitated, it will not survive the cycle");

                    orig(self);
                }
            }
            else if (data.isUncon)
            {
                if (ShadowOfOptions.uncon_chance_cheat_death.Value < data.cheatDeathChance)
                {
                    //self.dead = false;

                    if (self.abstractCreature.state != null)
                        self.abstractCreature.state.alive = false;

                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " is Unconsious, it will survive the cycle");

                    orig(self);
                }
                else
                {
                    data.actuallyDead = true;

                    //self.dead = false;

                    if (self.abstractCreature.state != null)
                        self.abstractCreature.state.alive = true;

                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " is Unconsious, it will not survive the cycle");

                    orig(self);
                }
            }
            else
            {
                data.actuallyDead = true;

                //self.dead = false;

                if (self.abstractCreature.state != null)
                    self.abstractCreature.state.alive = true;

                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + self + " is neither Incapacitated nor Unconsious, it will not survive the cycle");

                orig(self);
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        bool HasLizardData()
        {
            return ShadowOfLizards.ShadowOfLizards.lizardstorage.TryGetValue(self.abstractCreature, out _);
        }
    }

    static string SaveStateSaveAbstractCreature(On.SaveState.orig_AbstractCreatureToStringStoryWorld_AbstractCreature_WorldCoordinate orig, AbstractCreature self, WorldCoordinate cc)
    {
        if (self == null || self.state == null || self.state.unrecognizedSaveStrings == null || !inconstorage.TryGetValue(self, out InconData data))
        {
            return orig(self, cc);
        }

        try
        {
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + "Saving values for Abstract " + self);

            Dictionary<string, string> savedData = self.state.unrecognizedSaveStrings;

            if (data.actuallyDead && !data.isAlive)
            {
                savedData["IncapacitationOfactuallyDead"] = "Dead";

                if (ShadowOfOptions.debug_logs.Value)
                {
                    Debug.Log(all + self + " is actuallyDead");
                }

                return orig(self, cc);
            }

            savedData["IncapacitationOfactuallyDead"] = data.actuallyDead ? "True" : "False";

            savedData["IncapacitationOfisAlive"] = data.actuallyDead ? "Dead" : data.isAlive ? "True" : "False";

            savedData["IncapacitationOfisUncon"] = data.isUncon ? "True" : "False";

            savedData["IncapacitationOfinconCycle"] = data.inconCycle.ToString();

            savedData["IncapacitationOfdieHealthThreshold"] = data.dieHealthThreshold.ToString();

            savedData["IncapacitationOfcheatDeathChance"] = data.cheatDeathChance.ToString();

            if (ShadowOfOptions.debug_logs.Value)
            {
                Debug.Log(all + self + " actuallyDead = " + savedData["IncapacitationOfactuallyDead"]);

                Debug.Log(all + self + " isAlive = " + savedData["IncapacitationOfisAlive"]);

                Debug.Log(all + self + " isUncon = " + savedData["IncapacitationOfisUncon"]);

                Debug.Log(all + self + " inconCycle = " + savedData["IncapacitationOfinconCycle"]);

                Debug.Log(all + self + " dieHealthThreshold = " + savedData["IncapacitationOfdieHealthThreshold"]);

                Debug.Log(all + self + " cheatDeathChance = " + savedData["IncapacitationOfcheatDeathChance"]);
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        return orig(self, cc);
    }

    static void CreatureState_LoadFromString(On.CreatureState.orig_LoadFromString orig, CreatureState self, string[] s)
    {
        orig(self, s);

        if (self.creature == null || self.creature.creatureTemplate == null || self.unrecognizedSaveStrings == null || !IsAbstractCreatureValid(self.creature) && !self.unrecognizedSaveStrings.ContainsKey("IncapacitationOfisAlive"))
        {
            return;
        }

        Dictionary<string, string> savedData = self.unrecognizedSaveStrings;

        if (!IsAbstractCreatureValid(self.creature) && self.unrecognizedSaveStrings.ContainsKey("IncapacitationOfisAlive"))
        {
            if (savedData.ContainsKey("IncapacitationOfisAlive"))
            {
                savedData.Remove("IncapacitationOfisAlive");
            }

            if (savedData.ContainsKey("IncapacitationOfisUncon"))
            {
                savedData.Remove("IncapacitationOfisUncon");
            }

            if (savedData.ContainsKey("IncapacitationOfinconCycle"))
            {
                savedData.Remove("IncapacitationOfinconCycle");
            }

            if (savedData.ContainsKey("IncapacitationOfdieHealthThreshold"))
            {
                savedData.Remove("IncapacitationOfdieHealthThreshold");
            }

            if (savedData.ContainsKey("IncapacitationOfcheatDeathChance"))
            {
                savedData.Remove("IncapacitationOfcheatDeathChance");
            }

            if (savedData.ContainsKey("IncapacitationOfactuallyDead"))
            {
                savedData.Remove("IncapacitationOfactuallyDead");
            }
            return; 
        }

        if (!inconstorage.TryGetValue(self.creature, out InconData data))
        {
            inconstorage.Add(self.creature, new InconData());
            inconstorage.TryGetValue(self.creature, out data);
        }

        try
        {
            if (savedData.TryGetValue("IncapacitationOfisAlive", out string isAlive)) //Loads info from the Lizard
            {
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + "Not the first time creating Abstract " + self.creature + " Loading values for Abstract");

                data.isAlive = isAlive == "True";
                savedData.Remove("IncapacitationOfisAlive");

                if (savedData.ContainsKey("IncapacitationOfisUncon"))
                {
                    data.isUncon = savedData["IncapacitationOfisUncon"] == "True";
                    savedData.Remove("IncapacitationOfisUncon");
                }

                if (savedData.ContainsKey("IncapacitationOfinconCycle"))
                {
                    data.inconCycle = int.Parse(savedData["IncapacitationOfinconCycle"]);
                    savedData.Remove("IncapacitationOfinconCycle");
                }

                if (savedData.ContainsKey("IncapacitationOfdieHealthThreshold"))
                {
                    data.dieHealthThreshold = float.Parse(savedData["IncapacitationOfdieHealthThreshold"]);
                    savedData.Remove("IncapacitationOfdieHealthThreshold");
                }

                if (savedData.ContainsKey("IncapacitationOfcheatDeathChance"))
                {
                    data.cheatDeathChance = int.Parse(savedData["IncapacitationOfcheatDeathChance"]);
                    savedData.Remove("IncapacitationOfcheatDeathChance");
                }

                if (savedData.TryGetValue("IncapacitationOfactuallyDead", out string actuallyDead))
                {
                    data.actuallyDead = actuallyDead == "True";

                    savedData.Remove("IncapacitationOfactuallyDead");
                }

                if (ShadowOfOptions.debug_logs.Value)
                {
                    Debug.Log(all + self.creature + " actuallyDead = " + data.actuallyDead);

                    Debug.Log(all + self.creature + " isAlive = " + data.isAlive);

                    Debug.Log(all + self.creature + " isUncon = " + data.isUncon);

                    Debug.Log(all + self.creature + " inconCycle = " + data.inconCycle);

                    Debug.Log(all + self.creature + " dieHealthThreshold = " + data.dieHealthThreshold);

                    Debug.Log(all + self.creature + " cheatDeathChance = " + data.cheatDeathChance);
                }

                if (IsAbstractCreatureDenRespawnValid(self.creature) && data.inconCycle != CycleNum(self.creature) && data.inconCycle != -2 && data.isAlive)
                {
                    data.isUncon = false;
                    data.dieHealthThreshold = -2;

                    data.inconCycle = -2;

                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self.creature + " Cycle Number does not equal current");

                    if (data.actuallyDead)
                    {
                        data.actuallyDead = true;
                        data.isAlive = false;
                    }
                    if (self.creature.Room != null && self.creature.Room.shelter)
                    {
                        data.returnToDen = true;

                        self.alive = true;
                        data.actuallyDead = false;

                        if (ShadowOfOptions.debug_logs.Value)
                            Debug.Log(all + self.creature + " Is forced alive and told to return to den");

                        if (ShadowOfOptions.liz_friend.Value && self.creature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
                        {
                            data.forceTame = true;
                        }
                    }
                }

                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + "Finished creating Abstract " + self.creature);
            }
            else if (savedData.ContainsKey("IncapacitationOfactuallyDead"))
            {
                savedData.Remove("IncapacitationOfactuallyDead");

                data.actuallyDead = true;
                data.isAlive = false;

                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + "Finished creating Dead Abstract " + self.creature);
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
    }
    #endregion

    static Tracker.CreatureRepresentation TrackerCreatureNoticed(On.Tracker.orig_CreatureNoticed orig, Tracker self, AbstractCreature crit)
    {
        if (self.AI.creature.realizedCreature is not Lizard liz || IsIncon(liz))
        {
            return orig(self, crit);
        }

        if (self.AI.StaticRelationship(crit).type == CreatureTemplate.Relationship.Type.DoesntTrack)
        {
            return null;
        }

        try
        {
            bool flag = false;
            if (self.AI.creature.creatureTemplate.grasps > 0)
            {
                foreach (AbstractPhysicalObject.AbstractObjectStick abstractObjectStick in self.AI.creature.stuckObjects)
                {
                    if (abstractObjectStick.A == crit || abstractObjectStick.B == crit)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            Tracker.CreatureRepresentation creatureRepresentation = self.AI.CreateTrackerRepresentationForCreature(crit);
            if (creatureRepresentation == null)
            {
                return null;
            }
            self.AI.CreatureSpotted(!flag, creatureRepresentation);
            if (self.AI.relationshipTracker != null)
            {
                self.AI.relationshipTracker.EstablishDynamicRelationship(creatureRepresentation);
            }
            if (creatureRepresentation != null)
            {
                self.creatures.Add(creatureRepresentation);
                if (self.creatures.Count > self.maxTrackedCreatures)
                {
                    float num = float.MaxValue;
                    Tracker.CreatureRepresentation creatureRepresentation2 = null;
                    foreach (Tracker.CreatureRepresentation creatureRepresentation3 in self.creatures)
                    {
                        float num2 = ((creatureRepresentation3.dynamicRelationship != null) ? creatureRepresentation3.dynamicRelationship.currentRelationship.intensity : self.AI.creature.creatureTemplate.CreatureRelationship(creatureRepresentation3.representedCreature.creatureTemplate).intensity) * 100000f + (creatureRepresentation3.VisualContact ? 2f : 1f) / (1f + Vector2.Distance(IntVector2.ToVector2(creatureRepresentation3.BestGuessForPosition().Tile), IntVector2.ToVector2(self.AI.creature.pos.Tile)));
                        num2 /= Mathf.Lerp((float)creatureRepresentation3.forgetCounter, 100f, 0.7f);
                        if (num2 < num)
                        {
                            num = num2;
                            creatureRepresentation2 = creatureRepresentation3;
                        }
                    }
                    if (creatureRepresentation2 == creatureRepresentation)
                    {
                        creatureRepresentation = null;
                    }
                    creatureRepresentation2.Destroy();
                }
            }
            return creatureRepresentation;
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        return orig(self, crit);
    }

    #region Creature Hooks
    static void CreatureViolence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || type == null)
        {
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

            return;
        }

        bool sourceOwnerFlag = source != null && source.owner != null;

        if (data.inconCycle != CycleNum(self.abstractCreature))
            PreViolenceCheck(self, data);

        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

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

        
        if (IsComa(self) && type != Creature.DamageType.Blunt && (self.State is HealthState healthstate && (healthstate.health <= ShadowOfOptions.insta_die_threshold.Value || healthstate.health <= data.dieHealthThreshold) || self.State is not HealthState && UnityEngine.Random.Range(0, 100) < DieChance() || self.State is PlayerState playerState && playerState.permanentDamageTracking >= 0.8))
        {
            data.isAlive = false;

            self.Die();

            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + self + " reached the insta die threshold and died");

            return;
        }
        
        if (data.inconCycle != CycleNum(self.abstractCreature))
        {
            PostViolenceCheck(self, data, type.ToString(), sourceOwnerFlag && source.owner is Creature crit ? crit : null);
            return;
        }

        if (IsInconBase(self) && hitChunk != null && (sourceOwnerFlag && source.owner is DartMaggot || Head() && UnityEngine.Random.Range(0, 100) < UnconChance() || type == Creature.DamageType.Blunt && (self.State is HealthState healthstate2 && healthstate2.health <= (data.healthThreshold + data.dieHealthThreshold) / (Head() ? 2 : 1) || self.State is PlayerState playerState2 && playerState2.permanentDamageTracking >= (Head() ? 0.5 : 1.0))))
        {
            data.isUncon = true;

            self.Die();

            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + self + " was knocked Uncontious");
        }

        if (IsIncon(self))
        {
            InconAct(data);
        }
        
        int UnconChance()
        {
            int chance;

            switch (data.lastDamageType)
            {
                case "Blunt":
                    chance = ShadowOfOptions.uncon_chance_blunt.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Blunt " + chance + " chance to uncon");
                    break;
                case "Stab":
                    chance = ShadowOfOptions.uncon_chance_stab.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Stab " + chance + " chance to uncon");
                    break;
                case "Explosion":
                    chance = ShadowOfOptions.uncon_chance_explosion.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Explosion " + chance + " chance to uncon");
                    break;
                case "Electric":
                    chance = ShadowOfOptions.uncon_chance_electric.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Electric " + chance + " chance to uncon");
                    break;
                default:
                    chance = 0;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Null " + chance + " chance to uncon");
                    break;
            }

            return chance;
        }

        int DieChance()
        {
            int chance;

            switch (data.lastDamageType)
            {
                case "Blunt":
                    chance = 100 - ShadowOfOptions.incon_chance_blunt.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Blunt " + chance + " chance to die");
                    break;
                case "Stab":
                    chance = 100 - ShadowOfOptions.incon_chance_stab.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Stab " + chance + " chance to die");
                    break;
                case "Explosion":
                    chance = 100 - ShadowOfOptions.incon_chance_explosion.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Explosion " + chance + " chance to die");
                    break;
                case "Electric":
                    chance = 100 - ShadowOfOptions.incon_chance_electric.Value;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Electric " + chance + " chance to die");
                    break;
                default:
                    chance = 0;
                    if (ShadowOfOptions.debug_logs.Value)
                        Debug.Log(all + self + " Null " + chance + " chance to die");
                    break;
            }

            return chance;
        }

        bool Head()
        {
            if (self is Vulture)
            {
                return hitChunk.index == 4;
            }
            else if (self is Scavenger)
            {
                return hitChunk.index == 2;
            }
            else if (self is Centipede)
            {
                return hitChunk.index == 0 || hitChunk.index == self.bodyChunks.Length - 1;
            }

            return hitChunk.index == 0;
        }
    }
    #endregion

    public static void AIUpdate(ArtificialIntelligence self)
    {
        self.timeInRoom++;
        for (int i = 0; i < self.modules.Count; i++)
        {
            self.modules[i].Update();
        }
        if (ModManager.Expedition && self.creature.world.game.rainWorld.ExpeditionMode && Expedition.ExpeditionGame.activeUnlocks.Contains("bur-hunted") && self.creature.world.rainCycle.CycleProgression > 0.05f && self.tracker != null && self.creature.world.game.Players != null)
        {
            int j = 0;
            while (j < self.creature.world.game.Players.Count)
            {
                if (self.creature.world.game.Players[j].realizedCreature != null && !(self.creature.world.game.Players[j].realizedCreature as Player).dead)
                {
                    if (self.creature.Room != self.creature.world.game.Players[j].Room)
                    {
                        self.tracker.SeeCreature(self.creature.world.game.Players[j]);
                        break;
                    }
                    break;
                }
                else
                {
                    j++;
                }
            }
        }
        if (self.creature.rippleCreature)
        {
            int num = (self.creature.rippleLayer != self.creature.world.game.ActiveRippleLayer) ? 30 : 120;
            if (UnityEngine.Random.value <= 1f / (40f * (float)num) && self.creature.realizedCreature != null && self.creature.realizedCreature.room != null)
            {
                List<Watcher.CosmeticRipple> list = new List<Watcher.CosmeticRipple>();
                for (int k = 0; k < self.creature.realizedCreature.room.cosmeticRipples.Count; k++)
                {
                    if (self.creature.realizedCreature.room.cosmeticRipples[k].Data != null && self.creature.realizedCreature.room.cosmeticRipples[k].Data.cycleExpiry > 0)
                    {
                        list.Add(self.creature.realizedCreature.room.cosmeticRipples[k]);
                    }
                }
                if (list.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    self.ripplePathingTarget = list[index];
                    self.ripplePathingTime = 400;
                }
            }
        }
        if (self.ripplePathingTarget != null)
        {
            self.ripplePathingTime--;
            if (self.ripplePathingTarget.slatedForDeletetion || self.ripplePathingTime <= 0)
            {
                self.ripplePathingTarget = null;
                self.ripplePathingTime = 0;
                return;
            }
            if (self.creature.realizedCreature != null && self.creature.realizedCreature.room != null)
            {
                self.SetDestination(self.creature.realizedCreature.room.GetWorldCoordinate(self.ripplePathingTarget.pos));
            }
        }
    }

    public static void UpdateBreath(GraphicsModule self)
    {
        if (!breathstorage.TryGetValue(self, out BreathData breathData))
        {
            breathstorage.Add(self, new BreathData());
            breathstorage.TryGetValue(self, out breathData);

            breathData.lastBreath = breathData.breath;
            breathData.breath = UnityEngine.Random.value;
        }

        breathData.lastBreath = breathData.breath;

        breathData.breath += 0.0125f;
    }
}