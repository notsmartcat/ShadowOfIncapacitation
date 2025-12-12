using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.CicadaHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Cicada.Update += CicadaUpdate;

        On.CicadaGraphics.Update += CicadaGraphicsUpdate;

        On.CicadaGraphics.DrawSprites += CicadaGraphicsDrawSprites;
    }

    static void CicadaGraphicsDrawSprites(On.CicadaGraphics.orig_DrawSprites orig, CicadaGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || !IsUncon(self.cicada))
        {
            return;
        }

        float num4 = (Mathf.Sin(Mathf.Lerp(data.lastBreath, data.breath, timeStacker) * 3.1415927f * 2f) + 1f) * 0.5f;

        float num6 = self.iVars.fatness;

        num6 *= 1f + num4 * (float)3 * 0.1f * 0.5f;

        float num7 = self.iVars.fatness;

        num7 *= 1f + num4 * (((float)3 * 0.1f * 0.5f)/2);

        sLeaser.sprites[self.BodySprite].scaleY = num7;

        Vector2 vector = Vector3.Slerp(self.lastZRotation, self.zRotation, timeStacker);
        float num2 = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector);
        sLeaser.sprites[self.BodySprite].scaleX = ((num2 > 0f) ? -1f : 1f) * (num6);
    }

    static void CicadaGraphicsUpdate(On.CicadaGraphics.orig_Update orig, CicadaGraphics self)
    {
        orig(self);

        if (IsUncon(self.cicada))
        {
            MiscHooks.UpdateBreath(self);
        }
    }

    static void CicadaUpdate(On.Cicada.orig_Update orig, Cicada self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
        {
            return;
        }

        if (data.stunTimer > 0)
        {
            self.flying = true;
            data.stunTimer -= 1;
        }
        else
        {
            self.flying = false;
        }

        if (data.stunCountdown > 0)
        {
            data.stunCountdown -= 1;
        }

        if (self.Stunned)
        {
            return;
        }

        for (int i = 0; i < self.grabbedBy.Count; i++)
        {
            if (self.grabbedBy[i].grabber is Player player)
            {
                self.AI.panicFleeCrit = player;
                return;
            }
        }
        if (self.Submersion > 0.5f)
        {
            self.Swim();
            return;
        }

        AIUpdate();

        if (self.flying)
        {
            self.flyingPower = Mathf.Lerp(self.flyingPower, 1f, 0.1f);
        }
        else
        {
            self.flyingPower = Mathf.Lerp(self.flyingPower, 0f, 0.05f);
        }

        if (self.AtSitDestination)
        {
            self.bodyChunks[1].vel += Vector2.ClampMagnitude(self.BodySitPosOffset(self.FindBodySitPos(self.AI.pathFinder.GetDestination.Tile)) - self.bodyChunks[1].pos, 10f) / 10f * 0.5f;
            self.mainBodyChunk.vel += Vector2.ClampMagnitude(self.BodySitPosOffset(self.AI.pathFinder.GetDestination.Tile) - self.mainBodyChunk.pos, 10f) / 10f * 0.5f;
        }

        if (ShadowOfOptions.cic_attack.Value && self.chargeCounter > 0)
        {
            self.chargeCounter++;
            if (self.chargeCounter < 21)
            {
                self.bodyChunks[0].vel *= 0.8f;
                self.bodyChunks[1].vel *= 0.8f;
                self.bodyChunks[1].vel -= self.chargeDir * 0.8f;
            }
            else if (self.chargeCounter == 21)
            {
                self.room.PlaySound(SoundID.Cicada_Wings_Start_Bump_Attack, self.mainBodyChunk);
            }
            else if (self.chargeCounter > 38)
            {
                self.chargeCounter = 0;
                self.bodyChunks[0].vel *= 0.5f;
                self.bodyChunks[1].vel *= 0.5f;
                self.room.PlaySound(SoundID.Cicada_Wings_Exit_Bump_Attack, self.mainBodyChunk);
            }
            else
            {
                self.bodyChunks[0].vel += self.chargeDir * 4f;
                if (self.mainBodyChunk.vel.magnitude > 15f)
                {
                    self.bodyChunks[1].vel *= 0.8f;
                }
                else
                {
                    self.bodyChunks[1].vel *= 0.98f;
                }
            }
            if (self.room.aimap.getAItile(self.mainBodyChunk.pos).narrowSpace)
            {
                self.chargeCounter = 0;
            }
            InconAct();
            self.flying = true;
            return;
        }

        void AIUpdate()
        {
            CicadaAI ai = self.AI;

            ai.focusCreature = null;

            MiscHooks.AIUpdate(ai);

            if (ModManager.MSC && ai.cicada.LickedByPlayer != null)
            {
                ai.tracker.SeeCreature(ai.cicada.LickedByPlayer.abstractCreature);
            }
            if (ai.panicFleeCrit != null)
            {
                if (!Custom.DistLess(ai.cicada.mainBodyChunk.pos, ai.panicFleeCrit.mainBodyChunk.pos, 300f) || ai.cicada.mainBodyChunk.ContactPoint.x != 0 || ai.cicada.mainBodyChunk.ContactPoint.y != 0)
                {
                    Debug.Log(self + " run away from " + ai.panicFleeCrit);
                    InconAct();
                    ai.panicFleeCrit = null;
                }
                return;
            }
            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null && ai.cicada.safariControlled)
            {
                ai.currentUtility = 0f;
            }
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = CicadaAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = CicadaAI.Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    if (ai.preyTracker.MostAttractivePrey == null)
                    {
                        ai.behavior = CicadaAI.Behavior.Idle;
                    }
                    else if (ai.DynamicRelationship(ai.preyTracker.MostAttractivePrey).type == CreatureTemplate.Relationship.Type.Antagonizes)
                    {
                        ai.behavior = CicadaAI.Behavior.Idle;
                    }
                    else
                    {
                        ai.behavior = CicadaAI.Behavior.Hunt;
                    }
                }
            }
            if (ai.currentUtility < 0.1f)
            {
                ai.behavior = CicadaAI.Behavior.Idle;
            }

            ai.swooshToPos = null;
            if (ai.cicada.grasps[0] != null && (ai.currentUtility < 0.7f || ai.behavior == CicadaAI.Behavior.Hunt))
            {
                if (ai.cicada.grasps[0].grabbed is Creature crit && ai.StaticRelationship(crit.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                {
                    if (ai.behavior == CicadaAI.Behavior.Hunt)
                    {
                        ai.behavior = CicadaAI.Behavior.Idle;
                    }

                    if (crit.dead && UnityEngine.Random.value < 0.025f)
                    {
                        ai.cicada.LoseAllGrasps();
                    }
                }
                else if (UnityEngine.Random.value < 0.025f)
                {
                    ai.cicada.LoseAllGrasps();
                }
            }
            ai.stuckTracker.satisfiedWithThisPosition = !ai.cicada.AtSitDestination;
            if (ai.behavior == CicadaAI.Behavior.Idle)
            {
                if (ai.circleGroup != null)
                {
                    ai.RemoveFromCircle();
                    return;
                }
                else
                {
                    if (!ai.idleSitSpot.TileDefined || ai.idleSitSpot.room != ai.cicada.room.abstractRoom.index || !ai.cicada.Climbable(ai.idleSitSpot.Tile))
                    {
                        ai.idleSitSpot = ai.cicada.room.GetWorldCoordinate(new IntVector2(UnityEngine.Random.Range(0, ai.cicada.room.TileWidth), UnityEngine.Random.Range(0, ai.cicada.room.TileHeight)));
                    }
                    ai.idleSitCounter--;
                    if (ai.idleSitCounter < 1 || ai.cicada.room.aimap.getAItile(ai.idleSitSpot).narrowSpace)
                    {
                        ai.idleSitCounter = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 650));
                        ai.forbiddenIdleSitSpot = ai.idleSitSpot;
                    }
                    if (ai.idleSitSpot == ai.forbiddenIdleSitSpot)
                    {
                        IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, ai.cicada.room.TileWidth), UnityEngine.Random.Range(0, ai.cicada.room.TileHeight));
                        if (ai.cicada.Climbable(intVector) && ai.pathFinder.CoordinateReachable(ai.cicada.room.GetWorldCoordinate(intVector)) && (UnityEngine.Random.value < 0.3f || ai.VisualContact(ai.cicada.room.MiddleOfTile(intVector), 0f)))
                        {
                            ai.idleSitSpot = ai.cicada.room.GetWorldCoordinate(intVector);
                        }
                    }
                }
            }
            else
            {
                if (ai.behavior == CicadaAI.Behavior.Flee)
                {
                    if (ai.threatTracker.mostThreateningCreature != null)
                    {
                        ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                    }
                    InconAct();
                    return;
                }
                if (ai.behavior == CicadaAI.Behavior.EscapeRain)
                {
                    if (ai.denFinder.GetDenPosition() != null)
                    {
                        InconAct();
                        return;
                    }
                }
                else if (ShadowOfOptions.cic_eat.Value && ai.behavior == CicadaAI.Behavior.Hunt)
                {
                    ai.focusCreature = ai.preyTracker.MostAttractivePrey;
                    if (ai.preyTracker.MostAttractivePrey.VisualContact && Custom.DistLess(ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, ai.cicada.mainBodyChunk.pos, 200f) && Custom.InsideRect(ai.preyTracker.MostAttractivePrey.BestGuessForPosition().Tile, new IntRect(-30, -30, ai.cicada.room.TileWidth + 30, ai.cicada.room.TileHeight + 30)))
                    {
                        if (ai.huntAttackCounter < 50)
                        {
                            ai.huntAttackCounter++;
                            if (Custom.DistLess(ai.cicada.mainBodyChunk.pos, ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.mainBodyChunk.pos, 50f))
                            {
                                InconAct();
                                ai.cicada.TryToGrabPrey(ai.preyTracker.MostAttractivePrey.representedCreature.realizedCreature);
                            }
                        }
                        else if (UnityEngine.Random.value < 0.1f)
                        {
                            ai.huntAttackCounter++;
                            if (ai.huntAttackCounter > 200)
                            {
                                ai.huntAttackCounter = 0;
                            }
                        }
                    }
                    ai.tiredOfHuntingCounter++;
                    if (ai.tiredOfHuntingCounter > 200)
                    {
                        ai.tiredOfHuntingCreature = ai.preyTracker.MostAttractivePrey.representedCreature;
                        ai.tiredOfHuntingCounter = 0;
                        ai.preyTracker.ForgetPrey(ai.tiredOfHuntingCreature);
                        ai.tracker.ForgetCreature(ai.tiredOfHuntingCreature);
                        return;
                    }
                }
            }
        }

        void InconAct()
        {
            if (data.stunCountdown > 0)
            {
                return;
            }

            data.stunTimer = Mathf.Max(data.stunTimer, UnityEngine.Random.Range(20, 51));

            if (!self.flying)
            {
                self.flying = true;
                self.room.PlaySound(SoundID.Cicada_Wings_TakeOff, self.mainBodyChunk, false, 1f, self.iVars.wingSoundPitch);
            }

            data.stunCountdown = data.stunTimer + UnityEngine.Random.Range(10, 21);
        }
    }
}