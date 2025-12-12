using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using static Incapacitation.Incapacitation;

namespace Incapacitation.PlayerHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Player.ClassMechanicsSaint += ILPlayerClassMechanicsSaint;

        IL.Player.LungUpdate += ILPlayerLungUpdate;

        IL.Player.TerrainImpact += ILPlayerTerrainImpact;

        IL.Player.Tongue.Update += ILPlayerTongueUpdate;

        IL.Player.EatMeatUpdate += ILPlayerEatMeatUpdate;

        IL.PlayerGraphics.Update += ILPlayerGraphicsUpdate;

        IL.PlayerGraphics.DrawSprites += ILPlayerGraphicsDrawSprites;

        IL.Player.Update += ILPlayerUpdate;
    }

    static void ILPlayerUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region SleepUpdate
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Player>("SleepUpdate")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate SleepUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate SleepUpdate!");
        }
        #endregion

        #region LungUpdate
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Player>("LungUpdate")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate LungUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate LungUpdate!");
        }
        #endregion

        /*
        #region DeadAnim
        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate DeadAnim!");
        }
        #endregion

        #region StunnedAnim
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player>("bodyMode"),
            x => x.MatchLdsfld<Player.BodyModeIndex>("Swimming")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate StunnedAnim target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_stun"),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brfalse_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerUpdate StunnedAnim!");
        }
        #endregion
        */
    }

    static void ILPlayerGraphicsDrawSprites(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region LookCreature
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[12]
        {
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RoomCamera.SpriteLeaser>("sprites"),
            x => x.MatchLdcI4(9),
            x => x.MatchLdelemRef(),
            x => x.MatchLdsfld<Futile>("atlasManager"),
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RoomCamera.SpriteLeaser>("sprites"),
            x => x.MatchLdcI4(9),
            x => x.MatchLdelemRef(),
            x => x.MatchCallvirt<FNode>("get_scaleX"),
            x => x.MatchLdcI4(0),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites 0g target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites 0g!");
        }

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchCallvirt<FAtlasManager>("GetElementWithName"),
            x => x.MatchCallvirt<FFacetElementNode>("set_element"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_1);
            val.Emit(OpCodes.Ldc_I4_0);
            val.EmitDelegate(PlayerGraphicsDrawSprites);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites 0g Sprite!");
        }
        #endregion

        #region Regular
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchLdfld<Player>("bodyMode"),
            x => x.MatchLdsfld<Player.BodyModeIndex>("Stand"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites!");
        }

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchCallvirt<FAtlasManager>("GetElementWithName"),
            x => x.MatchCallvirt<FFacetElementNode>("set_element"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_1);
            val.Emit(OpCodes.Ldc_I4, 4);
            val.EmitDelegate(PlayerGraphicsDrawSprites);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites Sprite!");
        }

        if (val.TryGotoNext(MoveType.After, new Func<Instruction, bool>[2]
        {
            x => x.MatchCallvirt<FAtlasManager>("GetElementWithName"),
            x => x.MatchCallvirt<FFacetElementNode>("set_element"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_1);
            val.Emit(OpCodes.Ldloc, 20);
            val.EmitDelegate(PlayerGraphicsDrawSpritesVector);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites Sprite!");
        }
        #endregion

        #region Weird
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[8]
        {
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RoomCamera.SpriteLeaser>("sprites"),
            x => x.MatchLdcI4(9),
            x => x.MatchLdelemRef(),
            x => x.MatchLdloc(8),
            x => x.MatchLdcR4(0.2f),
            x => x.MatchAdd(),
            x => x.MatchCallvirt<FNode>("set_rotation"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_1);
            val.Emit(OpCodes.Ldc_I4, 5);
            val.EmitDelegate(PlayerGraphicsDrawSprites);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsDrawSprites Weird Sprite!");
        }
        #endregion
    }

    public static void PlayerGraphicsDrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, int imgIndex)
    {
        if (true && !inconstorage.TryGetValue(self.player.abstractCreature, out InconData data) || !IsComa(self.player))
        {
            return;
        }

        sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(self.DefaultFaceSprite(sLeaser.sprites[9].scaleX, imgIndex));
    }
    public static void PlayerGraphicsDrawSpritesVector(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, Vector2 imgIndex)
    {
        if (true && !inconstorage.TryGetValue(self.player.abstractCreature, out InconData data) || !IsComa(self.player))
        {
            return;
        }

        sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName(self.DefaultFaceSprite(sLeaser.sprites[9].scaleX, Mathf.RoundToInt(Mathf.Abs(RWCustom.Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), imgIndex) / 22.5f))));
    }

    static void ILPlayerGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region LookCreature
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("objectLooker"),
            x => x.MatchLdfld<PlayerGraphics.PlayerObjectLooker>("currentMostInteresting"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate LookCreature target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate LookCreature!");
        }
        #endregion

        #region LookAtNothing
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate LookAtNothing!");
        }
        #endregion

        #region Exhausted
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchLdfld<Player>("exhausted"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate Exhausted target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate Exhausted!");
        }
        #endregion

        #region LookDirection
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("objectLooker"),
            x => x.MatchCallvirt<PlayerGraphics.PlayerObjectLooker>("get_looking"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate LookDirection target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PlayerGraphics>("player"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<PlayerGraphics>(OpCodes.Ldfld, "player");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerGraphicsUpdate LookDirection!");
        }
        #endregion
    }

    static void ILPlayerTongueUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Player.Tongue>("player"),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Player.Tongue>(OpCodes.Ldfld, "player");
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Electric");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerTongueUpdate!");
        }
    }

    static void ILPlayerTerrainImpact(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Blunt");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerTerrainImpact!");
        }
    }

    static void ILPlayerLungUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerLungUpdate Arti!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die"),
            x => x.MatchBr(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerLungUpdate Arti!");
        }
    }

    static void ILPlayerClassMechanicsSaint(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdloc(18),
            x => x.MatchIsinst(typeof(Creature)),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldloc, 18);
            val.Emit(OpCodes.Isinst, typeof(Creature));
            val.EmitDelegate(ActuallyKill);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerClassMechanicsSaint!");
        }
    }

    static void ILPlayerEatMeatUpdate(ILContext il)
    {
        ILCursor val = new(il);

        if (val.TryGotoNext(MoveType.AfterLabel, new Func<Instruction, bool>[2]
        {
            x => x.MatchSub(),
            x => x.MatchStfld<CreatureState>("meatLeft")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.Emit(OpCodes.Ldarg_1);
            val.EmitDelegate(PlayerEatMeatUpdate);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILPlayerEatMeatUpdate!");
        }
    }

    public static void PlayerEatMeatUpdate(Player player, int graspIndex)
    {
        if (player.grasps[graspIndex].grabbed is not Creature self || !inconstorage.TryGetValue(self.abstractCreature, out InconData data) || !data.isAlive)
        {
            return;
        }

        if (self.State.meatLeft <= 0)
        {
            ActuallyKill(self);
            return;
        }

        int chance = UnityEngine.Random.Range(0, 101);

        if (IsIncon(self))
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
            else if (chance < 50)
            {
                data.isUncon = true;
            }
        }
        else
        {
            if (chance < 25)
            {
                ActuallyKill(self);
            }
        }
    }
}
