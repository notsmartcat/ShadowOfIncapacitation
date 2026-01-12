using RWCustom;
using System;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.MirosBirdHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.MirosBird.Update += MirosBirdUpdate;

        On.MirosBirdGraphics.DrawSprites += MirosBirdGraphicsDrawSprites;
        On.MirosBirdGraphics.Update += MirosBirdGraphicsUpdate;
    }

    #region MirosBird
    static void MirosBirdUpdate(On.MirosBird.orig_Update orig, MirosBird self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self) || self.room == null)
        {
            return;
        }

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

            if (self.moveDir != self.neutralDir)
            {
                self.remMoveDir = self.moveDir;
            }
            self.lastJawOpen = self.jawOpen;
            if (self.grasps[0] != null)
            {
                self.jawOpen = 0.15f;
            }
            else if (self.jawSlamPause > 0)
            {
                self.jawSlamPause--;
            }
            else
            {
                if (self.jawVel == 0f)
                {
                    self.jawVel = 0.15f;
                }
                if (self.safariControlled && self.jawVel >= 0f && self.jawVel < 1f && !self.controlledJawSnap)
                {
                    self.jawVel = 0f;
                    self.jawOpen = 0f;
                }
                self.jawOpen += self.jawVel;
                if (self.jawKeepOpenPause > 0)
                {
                    self.jawKeepOpenPause--;
                    self.jawOpen = Mathf.Clamp(Mathf.Lerp(self.jawOpen, self.keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
                }
                else if (UnityEngine.Random.value < 1f / (self.Blinded ? 15f : 40f) && !self.safariControlled)
                {
                    self.jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
                    self.keepJawOpenPos = ((UnityEngine.Random.value < 0.5f) ? 0f : 1f);
                    self.jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
                    self.jawOpen = Mathf.Clamp(self.jawOpen, 0f, 1f);
                }
                else if (self.jawOpen <= 0f)
                {
                    self.jawOpen = 0f;
                    if (self.jawVel < -0.4f)
                    {
                        JawSlamShut();
                        self.controlledJawSnap = false;
                    }
                    self.jawVel = 0.15f;
                    self.jawSlamPause = 5;
                }
                else if (self.jawOpen >= 1f)
                {
                    self.jawOpen = 1f;
                    self.jawVel = -0.5f;
                }
            }

            self.lastRunCycle = self.runCycle;
            self.runCycle += Mathf.Sign(self.bodyFlip) / Mathf.Lerp(Mathf.Lerp(30f, 40f, Mathf.Pow(Mathf.Max(self.legs[0].springPower, self.legs[1].springPower), 0.3f)), Custom.LerpMap(Mathf.Abs(self.mainBodyChunk.vel.x), 2f, 10f, 50f, 20f), 1f);

            AIUpdate();

            self.moveDir = new Vector2(0f, 1f);

            if (!self.safariControlled && self.room.aimap.TileAccessibleToCreature(self.room.GetTilePosition(self.mainBodyChunk.pos), self.Template) && self.AI.pathFinder.GetEffectualDestination.TileDefined && self.AI.pathFinder.GetEffectualDestination.room == self.room.abstractRoom.index && Custom.DistLess(self.mainBodyChunk.pos, self.room.MiddleOfTile(self.AI.pathFinder.GetEffectualDestination), 30f))
            {
                self.moveDir = Vector2.ClampMagnitude(self.room.MiddleOfTile(self.AI.pathFinder.GetEffectualDestination) - self.mainBodyChunk.pos, 30f) / 30f;
            }

            if (self.room == null)
            {
                return;
            }

            self.lastBodyFlip = self.bodyFlip;

            if (Mathf.Abs(self.moveDir.x) < 0.3f)
            {
                self.bodyFlip *= 0.7f;
            }
            else if (self.moveDir.x < 0f)
            {
                self.bodyFlip = Mathf.Max(-1f, self.bodyFlip - 0.1f);
            }
            else
            {
                self.bodyFlip = Mathf.Min(1f, self.bodyFlip + 0.1f);
            }

            float num2 = 0f;
            float num3 = self.forwardPower;
            self.forwardPower = 0f;
            bool flag2 = true;

            for (int m = 0; m < self.legs.Length; m++)
            {
                if (self.legs[m].groundContact)
                {
                    num2 = 1f;
                }
                else
                {
                    flag2 = false;
                }
            }

            if (!flag2 && Custom.ManhattanDistance(self.abstractCreature.pos, self.AI.pathFinder.GetEffectualDestination) < 6)
            {
                self.weightDownToStandOnBothLegs = Mathf.Min(1f, self.weightDownToStandOnBothLegs + 0.033333335f);
            }
            else if (Mathf.Abs(self.moveDir.x) > 0.3f)
            {
                self.weightDownToStandOnBothLegs = Mathf.Max(0f, self.weightDownToStandOnBothLegs - 0.1f);
            }

            bool flag3 = false;
            int num4 = self.abstractCreature.pos.y;
            while (num4 >= self.abstractCreature.pos.y - 3 && !flag3)
            {
                flag3 = self.room.aimap.TileAccessibleToCreature(new IntVector2(self.abstractCreature.pos.x, num4), self.Template);
                num4--;
            }

            if (!flag3)
            {
                num2 = 0f;
            }

            self.WeightedPush(1, 2, new Vector2(self.moveDir.x, 0f), Custom.LerpMap(Vector2.Dot(Custom.DirVec(self.bodyChunks[0].pos, self.bodyChunks[1].pos), new Vector2(0f, 1f)), 0f, 1f, 0f, 1f));
            self.WeightedPush(1, 2, new Vector2(0f, 1f), Custom.LerpMap(Vector2.Dot(Custom.DirVec(self.bodyChunks[0].pos, self.bodyChunks[1].pos), new Vector2(0f, 1f)), -1f, 1f, 8f, 0f) * num2);
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void AIUpdate()
        {
            MirosBirdAI ai = self.AI;

            MiscHooks.AIUpdate(ai);

            ai.creatureLooker.Update();

            for (int i = ai.tracker.CreaturesCount - 1; i >= 0; i--)
            {
                if (ai.tracker.GetRep(i).TicksSinceSeen > 160)
                {
                    ai.tracker.ForgetCreature(ai.tracker.GetRep(i).representedCreature);
                }
            }

            if (!ai.enteredRoom && ai.bird.room != null && ai.creature.pos.x > 2 && ai.creature.pos.x < ai.bird.room.TileWidth - 3)
            {
                ai.enteredRoom = true;
            }

            AIModule aimodule = ai.utilityComparer.HighestUtilityModule();
            ai.currentUtility = ai.utilityComparer.HighestUtility();
            if (aimodule != null)
            {
                if (aimodule is ThreatTracker)
                {
                    ai.behavior = MirosBirdAI.Behavior.Flee;
                }
                else if (aimodule is RainTracker)
                {
                    ai.behavior = MirosBirdAI.Behavior.EscapeRain;
                }
                else if (aimodule is PreyTracker)
                {
                    ai.behavior = MirosBirdAI.Behavior.Hunt;
                }
            }
            if (ai.currentUtility < 0.1f)
            {
                ai.behavior = MirosBirdAI.Behavior.Idle;
            }
            if (ai.bird.grasps[0] != null && ai.behavior != MirosBirdAI.Behavior.Flee && ai.behavior != MirosBirdAI.Behavior.EscapeRain)
            {
                ai.behavior = MirosBirdAI.Behavior.ReturnPrey;
            }

            if (ai.behavior == MirosBirdAI.Behavior.Flee)
            {
                if (ai.threatTracker.mostThreateningCreature != null)
                {
                    ai.focusCreature = ai.threatTracker.mostThreateningCreature;
                }
            }
            else if (ai.behavior == MirosBirdAI.Behavior.Hunt)
            {
                ai.focusCreature = ai.preyTracker.MostAttractivePrey;
            }
        }

        void JawSlamShut()
        {
            Vector2 vector = Custom.DirVec(self.neck.Tip.pos, self.Head.pos);
            self.neck.Tip.vel -= vector * 10f;
            self.neck.Tip.pos += vector * 20f;
            self.Head.pos += vector * 20f;

            self.room.PlaySound(SoundID.Miros_Beak_Snap_Miss, self.Head);
        }
    }
    #endregion

    #region MirosBirdGraphics
    static void MirosBirdGraphicsDrawSprites(On.MirosBirdGraphics.orig_DrawSprites orig, MirosBirdGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[self.BodySprite].scaleX = Custom.LerpMap(Vector2.Distance(Vector2.Lerp(self.bird.legs[0].Hip.lastPos, self.bird.legs[0].Hip.pos, timeStacker), Vector2.Lerp(self.bird.legs[1].Hip.lastPos, self.bird.legs[1].Hip.pos, timeStacker)), 5f, 50f, 0.75f, 1.1f) * MiscHooks.ApplyBreath(data, timeStacker);
        sLeaser.sprites[self.BodySprite].scaleY = 1 * MiscHooks.ApplyBreath(data, timeStacker);
    }
    static void MirosBirdGraphicsUpdate(On.MirosBirdGraphics.orig_Update orig, MirosBirdGraphics self)
    {
        orig(self);

        if (BreathCheck(self.bird))
        {
            MiscHooks.UpdateBreath(self);
        }
    }
    #endregion
}