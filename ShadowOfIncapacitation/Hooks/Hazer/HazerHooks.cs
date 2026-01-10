using RWCustom;
using System;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.HazerHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Hazer.Update += HazerUpdate;

        On.HazerGraphics.DrawSprites += HazerGraphicsDrawSprites;
        On.HazerGraphics.Update += HazerGraphicsUpdate;
    }

    #region Hazer
    static void HazerUpdate(On.Hazer.orig_Update orig, Hazer self, bool eu)
    {
        orig(self, eu);

        if (!inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !IsInconBase(self))
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

            if (self.Stunned || self.grabbedBy.Count > 0)
            {
                return;
            }

            if (!self.spraying)
            {
                Act();
            }
            else
            {
                self.swim = Mathf.Max(0f, self.swim - 0.033333335f);
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }

        void Act()
        {
            if (self.grabbedBy.Count > 0)
            {
                return;
            }

            self.moveCounter++;

            if (self.Submersion < 0.8f || (self.room.GetTile(self.mainBodyChunk.pos).WaterSurface && self.room.GetTile(self.mainBodyChunk.pos + new Vector2(0f, -20f)).Solid))
            {
                self.swim = Mathf.Max(0f, self.swim - 0.033333335f);
                
                if (self.room.GetTile(self.mainBodyChunk.pos).AnyWater)
                {
                    for (int i = 0; i < self.bodyChunks.Length; i++)
                    {
                        self.bodyChunks[i].vel.y -= 0.3f;
                    }
                }
                if (self.bodyChunks[1].ContactPoint.y > -1 && self.bodyChunks[0].ContactPoint.y > -1)
                {
                    return;
                }
                if (self.moveCounter > 0 && self.moveCounter % 6 == 0)
                {
                    if (self.room.readyForAI && (self.room.aimap.getAItile(self.room.GetTilePosition(self.mainBodyChunk.pos) + new IntVector2(self.hopDir, 0)).floorAltitude > 2 || self.room.aimap.getAItile(self.room.GetTilePosition(self.mainBodyChunk.pos) + new IntVector2(self.hopDir * 2, 0)).floorAltitude > 2))
                    {
                        self.hopDir = -self.hopDir;
                        self.moveCounter = -UnityEngine.Random.Range(60, 120);
                    }
                    else
                    {
                        self.WeightedPush(2, 1, new Vector2((float)self.hopDir, 0f), 4f);
                        //self.bodyChunks[1].vel += new Vector2((float)(-(float)self.hopDir), 0f);
                        //self.bodyChunks[0].vel += new Vector2((float)self.hopDir * 3f, 4f);
                        //self.bodyChunks[2].vel += new Vector2((float)self.hopDir * 3f, 4f);
                        self.room.PlaySound(SoundID.Hazer_Shuffle, self.mainBodyChunk);
                        if (self.moveCounter > UnityEngine.Random.Range(30, 400))
                        {
                            if (UnityEngine.Random.value < 0.33333334f)
                            {
                                self.hopDir = -self.hopDir;
                            }
                            self.moveCounter = -UnityEngine.Random.Range(120, 2500);
                        }
                    }
                }
                if (UnityEngine.Random.value < 0.1f && self.bodyChunks[1].ContactPoint.x != 0)
                {
                    self.hopDir = -self.bodyChunks[1].ContactPoint.x;
                }
                if (UnityEngine.Random.value < 0.1f && self.bodyChunks[2].ContactPoint.x != 0)
                {
                    self.hopDir = -self.bodyChunks[1].ContactPoint.x;
                    return;
                }
                
            }
            else
            {
                if (self.moveCounter > 0)
                {
                    self.swim = Mathf.Min(1f, self.swim + 0.033333335f);
                    self.swimCycle += self.swim / 18f;
                    self.swimDir = (self.swimDir + Custom.RNV() * (UnityEngine.Random.value * 0.1f)).normalized;
                    /*
                    if (self.room.readyForAI)
                    {
                        Vector2 vector = new Vector2(0f, 0f);
                        IntVector2 tilePosition = self.room.GetTilePosition(self.bodyChunks[2].pos);
                        int terrainProximity = self.room.aimap.getTerrainProximity(self.bodyChunks[2].pos);
                        for (int j = 1; j < 3; j++)
                        {
                            for (int k = 0; k < 8; k++)
                            {
                                if (terrainProximity < 3 && self.room.aimap.getTerrainProximity(tilePosition + Custom.eightDirections[k] * j) > terrainProximity)
                                {
                                    vector += Custom.eightDirections[k].ToVector2() * UnityEngine.Random.value / (float)j;
                                }
                                else if (!self.room.GetTile(tilePosition + Custom.eightDirections[k] * j).AnyWater)
                                {
                                    vector -= Custom.eightDirections[k].ToVector2() * (0.1f * UnityEngine.Random.value) / (float)j;
                                }
                            }
                        }
                        self.swimDir = (self.swimDir + Vector2.ClampMagnitude(vector, 1f) * UnityEngine.Random.value).normalized;
                    }
                    */
                    //self.bodyChunks[2].vel += self.swimDir;

                    if (self.moveCounter > UnityEngine.Random.Range(120, 8000))
                    {
                        self.moveCounter = -UnityEngine.Random.Range(120, 800);
                    }

                    self.floatHeight = Mathf.Max(30f, Mathf.Abs((float)self.room.defaultWaterLevel * 20f - self.bodyChunks[2].pos.y));
                    return;
                }

                self.swim = Mathf.Max(0f, self.swim - 0.0076923077f);
                self.swimCycle += 0.00625f;
                float num = (float)self.room.defaultWaterLevel * 20f - self.floatHeight + Mathf.Sin(self.swimCycle * 3.1415927f * 2f) * 10f;

                self.ChunkInOrder(0).vel.y += 0.25f * (1f - self.swim);
                self.ChunkInOrder(1).vel.y -= Mathf.Clamp(self.bodyChunks[0].pos.y - num, -0.6f, 0.6f) * (1f - self.swim);
                self.ChunkInOrder(2).vel.y -= 0.25f * (1f - self.swim);

                if (self.swimDir.y < 0.5f)
                {
                    self.swimDir = (self.swimDir + new Vector2(0f, 0.1f)).normalized;
                }
            }
        }
    }
    #endregion

    #region HazerGraphics
    static void HazerGraphicsDrawSprites(On.HazerGraphics.orig_DrawSprites orig, HazerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);

        if (!breathstorage.TryGetValue(self, out BreathData data) || self.culled)
        {
            return;
        }

        sLeaser.sprites[self.BodySprite].scaleX = 0.45f * MiscHooks.ApplyBreath(data, timeStacker);
        sLeaser.sprites[self.BodySprite].scaleY = 0.6f * MiscHooks.ApplyBreath(data, timeStacker);
    }
    static void HazerGraphicsUpdate(On.HazerGraphics.orig_Update orig, HazerGraphics self)
    {
        orig(self);

        if (BreathCheck(self.bug))
        {
            MiscHooks.UpdateBreath(self);
        }
    }
    #endregion
}