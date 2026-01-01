using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Incapacitation;

[BepInPlugin("notsmartcat.incapacitation", "Incapacitation", "1.0.0")]
public class Incapacitation : BaseUnityPlugin
{
    #region Classes
    public class InconData
    {
        //Used to check the last damage type that was dealt to the lizard for determining death couse in case a lizard bleeds out
        public string lastDamageType = "null";

        public bool wasDead = false;

        public bool actuallyDead = false;

        public bool isAlive = true;
        public bool isUncon = false;
        public int inconCycle = -2;

        public float healthThreshold = 1;
        public float dieHealthThreshold = -2;

        public float stunCountdown = 0;
        public float stunTimer = 0;

        public int cheatDeathChance = 0;

        public bool returnToDen = false;

        public bool spiderMotherWasDead = false;

        public bool forceTame = false;
    }

    public class BreathData
    {
        //Used to check the last damage type that was dealt to the lizard for determining death couse in case a lizard bleeds out
        public float breath = 0;
        public float lastBreath = 0;
    }
    #endregion

    #region ConditionalWeakTable
    public static readonly ConditionalWeakTable<AbstractCreature, InconData> inconstorage = new();

    public static readonly ConditionalWeakTable<GraphicsModule, BreathData> breathstorage = new();
    #endregion

    #region Misc Values
    public static string all = "IncapacitationOf: ";

    private bool init = false;

    public static bool shadowOfLizardsCheck = false;

    internal static new ManualLogSource Logger;

    public static ShadowOfOptions optionsMenuInstance;
    #endregion

    public void OnEnable()
    {
        try
        {
            Logger = base.Logger;

            #region Applying other hooks
            ILHooksMisc.Apply();

            MiscHooks.Apply();
            ForceDieHooks.Apply();

            BigSpiderHooks.Hooks.Apply();
            BigSpiderHooks.ILHooks.Apply();

            CentipedeHooks.Hooks.Apply();
            CentipedeHooks.ILHooks.Apply();

            CicadaHooks.Hooks.Apply();
            CicadaHooks.ILHooks.Apply();

            DeerHooks.Hooks.Apply();
            DeerHooks.ILHooks.Apply();

            DropBugHooks.Hooks.Apply();
            DropBugHooks.ILHooks.Apply();

            EggBugHooks.Hooks.Apply();
            EggBugHooks.ILHooks.Apply();

            LanternMouseHooks.Hooks.Apply();
            LanternMouseHooks.ILHooks.Apply();

            LizardHooks.Hooks.Apply();
            LizardHooks.ILHooks.Apply();

            NeedleWormHooks.Hooks.Apply();
            NeedleWormHooks.ILHooks.Apply();

            PlayerHooks.Hooks.Apply();
            PlayerHooks.ILHooks.Apply();

            ScavengerHooks.Hooks.Apply();
            ScavengerHooks.ILHooks.Apply();

            TubeWormHooks.Hooks.Apply();
            TubeWormHooks.ILHooks.Apply();

            VultureHooks.Hooks.Apply();
            VultureHooks.ILHooks.Apply();
            #endregion

            On.RainWorld.OnModsInit += ModInit;
            On.RainWorldGame.ctor += OptionalModsCheck;

            On.Player.Update += DebugKeys;
        }
        catch (Exception e) { Logger.LogError(e); }
    }

    void OptionalModsCheck(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig.Invoke(self, manager);
        try
        {
            if (ModManager.ActiveMods.Any(mod => mod.id == "notsmartcat.shadowoflizards"))
            {
                shadowOfLizardsCheck = true;
            }
        }
        catch (Exception e) { Logger.LogError(e); }
    }

    void ModInit(On.RainWorld.orig_OnModsInit orig, RainWorld rainWorld)
    {
        orig.Invoke(rainWorld);
        try
        {
            if (!init)
            {
                init = true;
                //Futile.atlasManager.LoadAtlas("atlases/ShadowOfAtlas");
            }
            optionsMenuInstance = new ShadowOfOptions(this);
            MachineConnector.SetRegisteredOI("notsmartcat.incapacitation", optionsMenuInstance);
        }
        catch (Exception e) { Logger.LogError(e); }
    }

    void DebugKeys(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig.Invoke(self, eu);

        if (!ShadowOfOptions.debug_keys.Value || self == null || self.room == null || self.room.game == null || !self.room.game.devToolsActive)
        {
            return;
        }

        try
        {
            if (Input.GetKey("n"))
            {
                List<AbstractCreature> list = new(self.abstractCreature.Room.creatures);
                foreach (AbstractCreature creature in list)
                {
                    if (creature.realizedCreature == null || creature.realizedCreature.dead || !inconstorage.TryGetValue(creature, out InconData data))
                    {
                        continue;
                    }

                    data.lastDamageType = "Bleed";
                    data.inconCycle = CycleNum(creature);

                    creature.realizedCreature.Die();
                }
            }

            if (Input.GetKey("m"))
            {
                List<AbstractCreature> list = new(self.abstractCreature.Room.creatures);
                foreach (AbstractCreature creature in list)
                {
                    if (creature.realizedCreature == null || creature.realizedCreature is Player || !inconstorage.TryGetValue(creature, out _))
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception e) { Logger.LogError(e); }
    }

    public static bool IsIncon(Creature self)
    {
        return IsComa(self) && inconstorage.TryGetValue(self.abstractCreature, out InconData data) && !data.isUncon && !self.Stunned;
    }
    public static bool IsInconBase(Creature self)
    {
        return IsComa(self) && inconstorage.TryGetValue(self.abstractCreature, out InconData data) && !data.isUncon;
    }
    public static bool IsComa(Creature self)
    {
        return self.dead && inconstorage.TryGetValue(self.abstractCreature, out InconData data) && (data.isAlive || CanIBeRevived());

        bool CanIBeRevived()
        {
            if (self.abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider)
            {
                BigSpider spid = self as BigSpider;

                return !spid.mother && spid.State.health > -8f && spid.State.meatLeft > 0 && spid.poison < 0.2f && spid.lungs > 0f;
            }

            return false;
        }
    }
    public static bool IsUncon(Creature self)
    {
        return IsComa(self) && inconstorage.TryGetValue(self.abstractCreature, out InconData data) && data.isUncon;
    }

    public static void PreViolenceCheck(Creature receiver, InconData data)
    {
        data.wasDead = receiver == null || receiver.dead || !data.isAlive;
    }

    public static void PostViolenceCheck(Creature receiver, InconData data, string killType, Creature sender = null)
    {
        if (receiver != null && data != null && receiver.abstractCreature != null && !data.wasDead && receiver.dead && data.isAlive)
        {
            data.lastDamageType = killType;

            ViolenceCheck(receiver, data, killType, sender);
        }
    }

    public static void ViolenceCheck(Creature receiver, InconData data, string killType, Creature sender = null)
    {
        if (receiver == null || receiver.abstractCreature == null || killType == null)
        {
            return;
        }

        data.wasDead = true;

        int chance = UnityEngine.Random.Range(0, 100);

        //Debug.Log(killType);

        int unconChance;
        int inconChance;

        if (killType == "Bleed" && data.lastDamageType != null && data.lastDamageType != "")
        {
            killType = data.lastDamageType;
        }

        switch (killType)
        {
            case "Bleed":
                unconChance = ShadowOfOptions.uncon_chance_bleed.Value;
                inconChance = ShadowOfOptions.incon_chance_bleed.Value;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Bleed");
                break;
            case "Blunt":
                unconChance = ShadowOfOptions.uncon_chance_blunt.Value;
                inconChance = ShadowOfOptions.incon_chance_blunt.Value;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Blunt");
                break;
            case "Stab":
                unconChance = ShadowOfOptions.uncon_chance_stab.Value;
                inconChance = ShadowOfOptions.incon_chance_stab.Value;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Stab");
                break;
            case "Explosion":
                unconChance = ShadowOfOptions.uncon_chance_explosion.Value;
                inconChance = ShadowOfOptions.incon_chance_explosion.Value;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Explosion");
                break;
            case "Electric":
                unconChance = ShadowOfOptions.uncon_chance_electric.Value;
                inconChance = ShadowOfOptions.incon_chance_electric.Value;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Electric");
                break;
            case "Uncon":
                unconChance = 100;
                inconChance = 100;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " forced to Uncon");
                break;
            case "Incon":
                unconChance = 0;
                inconChance = 100;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " forced to Incon");
                break;
            default:
                unconChance = 0;
                inconChance = 0;
                if (ShadowOfOptions.debug_logs.Value)
                    Debug.Log(all + receiver + " died by Null");
                break;
        }

        if (receiver.State is HealthState)
        {
            data.healthThreshold = (receiver.State as HealthState).health;
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " default healthThreshold is " + data.healthThreshold);

            data.dieHealthThreshold = data.healthThreshold - UnityEngine.Random.Range(ShadowOfOptions.die_threshold_min.Value, ShadowOfOptions.die_threshold_max.Value);
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " actual healthThreshold is " + data.dieHealthThreshold);
        }

        if (chance < unconChance)
        {
            data.isUncon = true;
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " is Uncontious, Chance = " + chance + " lower then " + unconChance);
        }
        else if (chance < inconChance)
        {
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " is Incapacitated, Chance = " + chance + " lower then " + inconChance);
        }
        else if (ShadowOfOptions.slugpup_never_die.Value && ModManager.MSC && receiver.abstractCreature.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " is forced Incapacitated due to Remix Settings");
        }
        else
        {
            ActuallyKill(receiver);
            if (ShadowOfOptions.debug_logs.Value)
                Debug.Log(all + receiver + " is Dead, Chance = " + chance + " higher then " + inconChance);
        }

        data.inconCycle = CycleNum(receiver.abstractCreature);
    }

    public static bool IsAbstractCreatureValid(AbstractCreature self)
    {
        if (self != null && (self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && ShadowOfOptions.liz_state.Value != "Disabled" || 
            (self.creatureTemplate.type == CreatureTemplate.Type.Slugcat || self.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) && ShadowOfOptions.slug_state.Value != "Disabled" || 
            (self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture || (ModManager.DLCShared && self.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture)) && ShadowOfOptions.vul_state.Value != "Disabled" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA && ShadowOfOptions.cic_state.Value != "Disabled" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && ShadowOfOptions.scav_state.Value != "Disabled" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider && ShadowOfOptions.spid_state.Value != "Disabled" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede && ShadowOfOptions.centi_state.Value != "Disabled" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Deer && ShadowOfOptions.deer_state.Value != "Disabled"))
        {
            return true;
        }

        return false;
    }

    public static bool IsAbstractCreatureDenRespawnValid(AbstractCreature self)
    {
        if (self != null && ShadowOfOptions.den_revive.Value && (self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && ShadowOfOptions.liz_state.Value == "Incapacitation, Cheating Death and Den Revive" ||
            self.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && ShadowOfOptions.slug_state.Value == "Incapacitation and Den Revive" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA && ShadowOfOptions.cic_state.Value == "Incapacitation, Cheating Death and Den Revive" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && (!ModManager.MSC || self.creatureTemplate.type != MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing) && ShadowOfOptions.scav_state.Value == "Incapacitation, Cheating Death and Den Revive" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider && ShadowOfOptions.spid_state.Value == "Incapacitation, Cheating Death and Den Revive" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede && ShadowOfOptions.centi_state.Value == "Incapacitation, Cheating Death and Den Revive"))
        {
            return true;
        }

        return false;
    }

    public static bool IsAbstractCreatureCheatDeathValid(AbstractCreature self)
    {
        if (self != null && ShadowOfOptions.cheat_death.Value && (self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && ShadowOfOptions.liz_state.Value != "Disabled" && ShadowOfOptions.liz_state.Value != "Incapacitation Only" ||
            (self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Vulture || (ModManager.DLCShared && self.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture)) && ShadowOfOptions.vul_state.Value != "Incapacitation and Cheating Death" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.CicadaA && ShadowOfOptions.cic_state.Value != "Disabled" && ShadowOfOptions.cic_state.Value != "Incapacitation Only" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Scavenger && (!ModManager.MSC || self.creatureTemplate.type != MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing) && ShadowOfOptions.scav_state.Value != "Disabled" && ShadowOfOptions.scav_state.Value != "Incapacitation Only" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.BigSpider && ShadowOfOptions.spid_state.Value != "Disabled" && ShadowOfOptions.spid_state.Value != "Incapacitation Only" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede && ShadowOfOptions.centi_state.Value != "Disabled" && ShadowOfOptions.centi_state.Value != "Incapacitation Only" ||
            self.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Deer && ShadowOfOptions.deer_state.Value == "Incapacitation and Cheating Death"))
        {
            return true;
        }

        return false;
    }

    public static void InconAct(InconData data)
    {
        if (data.stunCountdown > 0)
        {
            return;
        }

        data.stunTimer = UnityEngine.Random.Range(1, 31);

        data.stunCountdown = data.stunTimer + UnityEngine.Random.Range(1, 31);
    }

    public static int CycleNum(AbstractCreature self)
    {
        return self.world.game.IsStorySession ? self.world.game.GetStorySession.saveState.cycleNumber : 0;
    }

    public static void ActuallyKill(Creature self)
    {
        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive || ShadowOfOptions.slugpup_never_die.Value && ModManager.MSC && self.abstractCreature.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            return;
        }

        if (ShadowOfOptions.debug_logs.Value)
            Debug.Log(all + self + " is Actually Killed");

        data.actuallyDead = true;
        data.isAlive = false;

        if (self.dead)
        {
            //self.dead = false;

            if (self.abstractCreature.state != null)
                self.abstractCreature.state.alive = true;

            self.Die();
        }
    }
}