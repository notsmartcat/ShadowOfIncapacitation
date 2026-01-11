using static CreatureTemplate;
using static RelationshipTracker;
using static Incapacitation.Incapacitation;

namespace Incapacitation;
internal class CustomRelationsHooks
{
    public static void Apply()
    {
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += CentipedeAIUpdateDynamicRelationship;

        On.CicadaAI.IUseARelationshipTracker_UpdateDynamicRelationship += CicadaAIUpdateDynamicRelationship;

        On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += DropBugAIUpdateDynamicRelationship;

        On.EggBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += EggBugAIUpdateDynamicRelationship;

        On.JetFishAI.IUseARelationshipTracker_UpdateDynamicRelationship += JetFishAIUpdateDynamicRelationship;

        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAIUpdateDynamicRelationship;

        On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += ScavengerAIUpdateDynamicRelationship;

        On.MoreSlugcats.SlugNPCAI.IUseARelationshipTracker_UpdateDynamicRelationship += SlugNPCAIUpdateDynamicRelationship;
    }

    #region Centipede
    static Relationship CentipedeAIUpdateDynamicRelationship(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region Cicada
    static Relationship CicadaAIUpdateDynamicRelationship(On.CicadaAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CicadaAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region DropBug
    static Relationship DropBugAIUpdateDynamicRelationship(On.DropBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DropBugAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region EggBug
    static Relationship EggBugAIUpdateDynamicRelationship(On.EggBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, EggBugAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region JetFish
    static Relationship JetFishAIUpdateDynamicRelationship(On.JetFishAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, JetFishAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region Lizard
    static Relationship LizardAIUpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, DynamicRelationship dRelation)
    {
        if (!inconstorage.TryGetValue(self.lizard.abstractCreature, out InconData data) || dRelation.trackerRep.representedCreature.realizedCreature == null || (self.friendTracker.giftOfferedToMe != null && self.friendTracker.giftOfferedToMe.active && self.friendTracker.giftOfferedToMe.item == dRelation.trackerRep.representedCreature.realizedCreature) || dRelation.trackerRep.representedCreature.realizedCreature == null)
        {
            return orig(self, dRelation);
        }

        Creature crit = dRelation.trackerRep.representedCreature.realizedCreature;

        if (self.friendTracker.friend != null && inconstorage.TryGetValue(crit.abstractCreature, out InconData data2) && data2.returnToDen)
        {
            return new Relationship(Relationship.Type.Ignores, 0f);
        }

        return ReturnToDen(crit, data) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region Scavenger
    static Relationship ScavengerAIUpdateDynamicRelationship(On.ScavengerAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, ScavengerAI self, DynamicRelationship dRelation)
    {
        return ReturnToDenFull(self, dRelation) ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    #region SlugNPC
    static Relationship SlugNPCAIUpdateDynamicRelationship(On.MoreSlugcats.SlugNPCAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, MoreSlugcats.SlugNPCAI self, DynamicRelationship dRelation)
    {
        if (!inconstorage.TryGetValue(self.creature, out _) || dRelation.trackerRep.representedCreature.realizedCreature == null)
        {
            return orig(self, dRelation);
        }

        Creature crit = dRelation.trackerRep.representedCreature.realizedCreature;

        return inconstorage.TryGetValue(crit.abstractCreature, out InconData data) || data.returnToDen ? new Relationship(Relationship.Type.Ignores, 0f) : orig(self, dRelation);
    }
    #endregion

    static bool ReturnToDen(Creature crit, InconData data)
    {
        return data.returnToDen && (crit is Lizard liz && liz.AI != null && liz.AI.friendTracker.friend != null || crit is Player);
    }
    static bool ReturnToDenFull(ArtificialIntelligence self, DynamicRelationship dRelation)
    {
        return inconstorage.TryGetValue(self.creature, out InconData data) && data.returnToDen && dRelation.trackerRep.representedCreature.realizedCreature != null && (dRelation.trackerRep.representedCreature.realizedCreature is Lizard liz && liz.AI != null && liz.AI.friendTracker.friend != null || dRelation.trackerRep.representedCreature.realizedCreature is Player);
    }
}