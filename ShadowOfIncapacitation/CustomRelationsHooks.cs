using IL.MoreSlugcats;
using IL.Watcher;
using UnityEngine;
using static CreatureTemplate;
using static RelationshipTracker;
using static Incapacitation.Incapacitation;
using static ShadowOfLizards.ShadowOfLizards;

namespace Incapacitation;
internal class CustomRelationsHooks
{
    public static void Apply()
    {
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAIUpdateDynamicRelationship;

        //On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += BigSpiderAIUpdateDynamicRelationship;

        On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += SlugNPCAIIUseARelationshipTracker_UpdateDynamicRelationship; ;
    }

    static Relationship SlugNPCAIIUseARelationshipTracker_UpdateDynamicRelationship(On.MoreSlugcats.SlugNPCAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, MoreSlugcats.SlugNPCAI self, DynamicRelationship dRelation)
    {
        Creature crit = dRelation.trackerRep.representedCreature.realizedCreature;

        if (crit is Lizard liz && inconstorage.TryGetValue(liz.abstractCreature, out InconData data) && data.returnToDen)
        {
            return new Relationship(Relationship.Type.Ignores, 0f);
        }

        return orig(self, dRelation);
    }

    static Relationship BigSpiderAIUpdateDynamicRelationship(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI self, DynamicRelationship dRelation)
    {
        return orig(self, dRelation);
    }

    static Relationship LizardAIUpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, DynamicRelationship dRelation)
    {
        if (!inconstorage.TryGetValue(self.lizard.abstractCreature, out InconData data) || (self.friendTracker.giftOfferedToMe != null && self.friendTracker.giftOfferedToMe.active && self.friendTracker.giftOfferedToMe.item == dRelation.trackerRep.representedCreature.realizedCreature) || dRelation.trackerRep.representedCreature.realizedCreature == null)
        {
            return orig(self, dRelation);
        }

        Creature crit = dRelation.trackerRep.representedCreature.realizedCreature;

        if (self.friendTracker.friend != null)
        {
            if (crit is Lizard liz && inconstorage.TryGetValue(liz.abstractCreature, out InconData data2) && data2.returnToDen)
            {
                return new Relationship(Relationship.Type.Ignores, 0f);
            }
        }
        else if (data.returnToDen)
        {
            if (crit is Lizard liz && liz.AI != null && liz.AI.friendTracker.friend == crit)
            {
                return new Relationship(Relationship.Type.Ignores, 0f);
            }
            else if (crit is Player)
            {
                return new Relationship(Relationship.Type.Ignores, 0f);
            }
        }

        return orig(self, dRelation);
    }

    private static bool IsThisBigCreatureForShelter(AbstractCreature creature)
    {
        Type type = creature.creatureTemplate.type;
        return type == Type.Deer || type == Type.BrotherLongLegs || type == Type.DaddyLongLegs || type == Type.RedCentipede || type == Type.MirosBird || type == Type.PoleMimic || type == Type.TentaclePlant || creature.creatureTemplate.IsVulture || (ModManager.DLCShared && MSCIsThisBigCreatureForShelter()) || (ModManager.Watcher && WatcherIsThisBigCreatureForShelter());

        bool MSCIsThisBigCreatureForShelter()
        {
            if (type == DLCSharedEnums.CreatureTemplateType.TerrorLongLegs)
            {
                return true;
            }
            if (type == DLCSharedEnums.CreatureTemplateType.MirosVulture)
            {
                return true;
            }

            return false;
        }
        bool WatcherIsThisBigCreatureForShelter()
        {
            return false;
        }
    }

    static bool SpiderTemplateCheck(Creature crit)
    {
        return crit != null && (crit is Spider || crit is BigSpider);
    }

    static bool CentipedeTemplateCheck(Creature crit)
    {
        return crit != null && crit is Centipede;
    }
}
