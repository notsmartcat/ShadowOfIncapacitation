using static CreatureTemplate;
using static RelationshipTracker;
using static Incapacitation.Incapacitation;

namespace Incapacitation;
internal class CustomRelationsHooks
{
    public static void Apply()
    {
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAIUpdateDynamicRelationship;

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
}
