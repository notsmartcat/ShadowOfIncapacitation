using RWCustom;
using UnityEngine;
using System;
using System.Collections.Generic;

using static Incapacitation.Incapacitation;
using static ShadowOfLizards.ShadowOfLizards;

namespace Incapacitation;

internal class ForceDieHooks
{
    public static void Apply()
    {
        On.MoreSlugcats.SingularityBomb.Explode += SingularityBombExplode;

        On.BigEel.JawsSnap += BigEelJawsSnap;

        On.Creature.Update += CreatureUpdate;

        On.AirBreatherCreature.Update += AirBreatherCreatureUpdate;

        On.Creature.HypothermiaUpdate += CreatureHypothermiaUpdate;

        On.BigSpider.BabyPuff += BigSpiderBabyPuff;

        On.Fly.Update += FlyUpdate;

        On.InsectoidCreature.Update += InsectoidCreatureUpdate;

        On.Watcher.Loach.Eat += LoachEat;

        On.ZapCoil.Update += ZapCoilUpdate;
    }

    static void ZapCoilUpdate(On.ZapCoil.orig_Update orig, ZapCoil self, bool eu)
    {
        if (self.turnedOn > 0.5f)
        {
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                {
                    if (!ModManager.Watcher || !(self.room.physicalObjects[i][j] is Creature) || (!((self.room.physicalObjects[i][j] as Creature).abstractCreature.creatureTemplate.type == Watcher.WatcherEnums.CreatureTemplateType.BoxWorm) && !((self.room.physicalObjects[i][j] as Creature).abstractCreature.creatureTemplate.type == Watcher.WatcherEnums.CreatureTemplateType.FireSprite)))
                    {
                        for (int k = 0; k < self.room.physicalObjects[i][j].bodyChunks.Length; k++)
                        {
                            if ((self.horizontalAlignment && self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.y != 0) || (!self.horizontalAlignment && self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.x != 0))
                            {
                                Vector2 a = self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
                                Vector2 v = self.room.physicalObjects[i][j].bodyChunks[k].pos + a * (self.room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
                                if (self.GetFloatRect.Vector2Inside(v))
                                {
                                    if (self.room.physicalObjects[i][j] is Creature crit && inconstorage.TryGetValue(crit.abstractCreature, out InconData data) && data.isAlive)
                                    {
                                        ViolenceCheck(self.room.physicalObjects[i][j] as Creature, data, "Electric");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    static void LoachEat(On.Watcher.Loach.orig_Eat orig, Watcher.Loach self, bool eu)
    {
        for (int i = self.eatObjects.Count - 1; i >= 0; i--)
        {
            if (self.eatObjects[i].progression > 1f)
            {
                if (self.eatObjects[i].chunk.owner is Creature crit)
                {
                    ActuallyKill(crit);
                }
            }
            else
            {
                float progression = self.eatObjects[i].progression;
                float tempprogression = self.eatObjects[i].progression += 0.0125f;

                if (progression <= 0.5f && tempprogression > 0.5f)
                {
                    if (self.eatObjects[i].chunk.owner is Creature crit)
                    {
                        ActuallyKill(crit);
                    }
                }
            }
        }
    }

    static void InsectoidCreatureUpdate(On.InsectoidCreature.orig_Update orig, InsectoidCreature self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self) || !(self.poison > 0f && self.room != null && self.room.readyForAI))
        {
            return;
        }

        if (self.poison > 0.25f)
        {
            self.poison = Mathf.Clamp01(self.poison + 0.0025f);
        }
        else
        {
            self.poison = Mathf.Clamp01(self.poison - 0.0016666667f);
        }
        if (UnityEngine.Random.value < Mathf.Max(0.2f, self.poison * 0.5f) && UnityEngine.Random.value < self.poison)
        {
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                if (self.room.aimap.TileAccessibleToCreature(self.bodyChunks[i].pos, self.Template))
                {
                    self.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value * self.bodyChunks[i].rad * (0.5f + 0.5f * self.poison);
                }
            }
            if (UnityEngine.Random.value < 0.5f)
            {
                self.Stun(UnityEngine.Random.Range(2, (int)Mathf.Lerp(8f, 16f, self.poison)));
            }
            else if (self.State is HealthState)
            {
                data.lastDamageType = "Poison";
                (self.State as HealthState).health -= UnityEngine.Random.value * self.poison / (16f * Mathf.Max(1f, self.TotalMass));
            }
            if (self.poison >= 1f && self.State is not HealthState)
            {
                ActuallyKill(self);
            }
        }
    }

    static void FlyUpdate(On.Fly.orig_Update orig, Fly self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out _) || !IsComa(self))
        {
            return;
        }

        self.drown = Mathf.Clamp(self.drown + 0.0125f * ((self.mainBodyChunk.submersion == 1f) ? 1f : -1f), 0f, 1f);
        if (self.drown == 1f)
        {
            ActuallyKill(self);
        }
    }

    static void SingularityBombExplode(On.MoreSlugcats.SingularityBomb.orig_Explode orig, MoreSlugcats.SingularityBomb self)
    {
        for (int m = 0; m < self.room.physicalObjects.Length; m++)
        {
            for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
            {
                if (self.room.physicalObjects[m][n].abstractPhysicalObject.rippleLayer == self.abstractPhysicalObject.rippleLayer || self.room.physicalObjects[m][n].abstractPhysicalObject.rippleBothSides || self.abstractPhysicalObject.rippleBothSides)
                {
                    if (self.room.physicalObjects[m][n] is Creature crit && Custom.Dist(self.room.physicalObjects[m][n].firstChunk.pos, self.firstChunk.pos) < 350f && crit.abstractCreature != null && inconstorage.TryGetValue(crit.abstractCreature, out InconData data))
                    {
                        ActuallyKill(crit);
                    }
                }
            }
        }

        orig(self);
    }

    static void BigEelJawsSnap(On.BigEel.orig_JawsSnap orig, BigEel self)
    {
        for (int j = 0; j < self.room.physicalObjects.Length; j++)
        {
            for (int k = self.room.physicalObjects[j].Count - 1; k >= 0; k--)
            {
                if (self.room.physicalObjects[j][k] is Creature crit && inconstorage.TryGetValue(crit.abstractCreature, out InconData data) && (self.room.physicalObjects[j][k].abstractPhysicalObject.rippleLayer == self.abstractPhysicalObject.rippleLayer || self.room.physicalObjects[j][k].abstractPhysicalObject.rippleBothSides || self.abstractPhysicalObject.rippleBothSides))
                {
                    for (int l = 0; l < self.room.physicalObjects[j][k].bodyChunks.Length; l++)
                    {
                        if (self.InBiteArea(self.room.physicalObjects[j][k].bodyChunks[l].pos, self.room.physicalObjects[j][k].bodyChunks[l].rad / 2f))
                        {
                            ActuallyKill(crit);
                        }
                    }
                }
            }
        }

        orig(self);
    }

    static void CreatureUpdate(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out _) || !IsComa(self))
        {
            return;
        }

        if (self.Submersion > 0.1f && self.room.waterObject != null && self.room.waterObject.WaterIsLethal && !self.abstractCreature.lavaImmune)
        {
            if (self.Submersion > 0.2f)
            {
                ActuallyKill(self);
            }
        }

        if (self.injectedPoison > 0f)
        {
            if ((self.State as HealthState) == null)
            {
                float num4 = self.injectedPoison / self.Template.instantDeathDamageLimit;
                if (num4 >= 1f && !self.dead)
                {
                    ActuallyKill(self);
                }
            }
        }
    }

    static void AirBreatherCreatureUpdate(On.AirBreatherCreature.orig_Update orig, AirBreatherCreature self, bool eu)
    {
        orig(self, eu);

        if (self.abstractCreature == null || self.abstractCreature.realizedCreature == null || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsComa(self.abstractCreature.realizedCreature))
        {
            return;
        }

        try
        {
            if (self.lungs == 1f)
            {
                if (UnityEngine.Random.value < 0.016666668f && self.room != null && ((self.room.water && self.Submersion == 1f) || (double)self.firstChunk.sandSubmersion >= 0.9))
                {
                    self.lungs = Mathf.Max(0f, self.lungs - 1f / self.Template.lungCapacity);
                    return;
                }
            }
            else
            {
                if (self.room == null || ((!self.room.water || self.Submersion < 1f) && (double)self.firstChunk.sandSubmersion < 0.9))
                {
                    self.lungs = Mathf.Clamp01(self.lungs + 0.033333335f);
                    return;
                }
                self.lungs = Mathf.Max(-1f, self.lungs - 1f / self.Template.lungCapacity);
                if (self.lungs < 0.3f)
                {
                    if (UnityEngine.Random.value < Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, self.lungs) * 3.1415927f) * 0.5f)
                    {
                        self.room.AddObject(new Bubble(self.mainBodyChunk.pos, Custom.RNV() * UnityEngine.Random.value * 6f, false, false, false));
                    }
                    if (UnityEngine.Random.value < 0.025f)
                    {
                        self.LoseAllGrasps();
                    }
                    for (int i = 0; i < self.bodyChunks.Length; i++)
                    {
                        self.bodyChunks[i].vel += Custom.RNV() * self.bodyChunks[i].rad * 0.4f * UnityEngine.Random.value * Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, self.lungs) * 3.1415927f) + Custom.DegToVec(Mathf.Lerp(-30f, 30f, UnityEngine.Random.value)) * UnityEngine.Random.value * ((i == self.mainBodyChunkIndex) ? 0.4f : 0.2f) * Mathf.Pow(Mathf.Sin(Mathf.InverseLerp(0.3f, -0.3f, self.lungs) * 3.1415927f), 2f);
                    }
                    if (self.lungs <= 0f && UnityEngine.Random.value < 0.1f)
                    {
                        self.Stun(UnityEngine.Random.Range(0, 18));
                    }
                    if (self.lungs < -0.5f && UnityEngine.Random.value < 1f / Custom.LerpMap(self.lungs, -0.5f, -1f, 90f, 30f))
                    {
                        ActuallyKill(self);
                    }
                }
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
    }

    static void CreatureHypothermiaUpdate(On.Creature.orig_HypothermiaUpdate orig, Creature self)
    {
        orig(self);

        if (!inconstorage.TryGetValue(self.abstractCreature, out _) || !IsComa(self))
        {
            return;
        }

        if (self.Hypothermia >= 1f && (float)self.stun > 50f)
        {
            ActuallyKill(self);
        }
    }

    static void BigSpiderBabyPuff(On.BigSpider.orig_BabyPuff orig, BigSpider self)
    {
        if (inconstorage.TryGetValue(self.abstractCreature, out _) && self.mother)
        {
            ActuallyKill(self);
        }

        orig(self);
    }
}
