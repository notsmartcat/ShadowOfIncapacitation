using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.LizardHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.Lizard.ActAnimation += ILLizardActAnimation;

        IL.LizardAI.AggressiveBehavior += ILLizardAIAggressiveBehavior;

        IL.LizardGraphics.DrawSprites += ILLizardGraphicsDrawSprites;
        IL.LizardGraphics.Update += ILLizardGraphicsUpdate;

        IL.LizardVoice.MakeSound_Emotion_float += ILLizardVoiceMakeSound;


        //IL.Watcher.LizardBlizzardModule.Update += ILLizardBlizzardModuleUpdate;
        //IL.Watcher.LizardRotModule.Update += ILLizardRotModuleUpdate;
        //IL.Watcher.LizardRotModule.Violence += ILLizardRotModuleViolence;
        //IL.LizardJumpModule.Update += ILLizardJumpModuleUpdate;
    }

    #region Lizard
    static void ILLizardActAnimation(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region JawReadyForBite
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Lizard>("AI"),
            x => x.MatchLdfld<LizardAI>("focusCreature"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(LizardActAnimation);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardActAnimation!");
        }
        #endregion
    }
    public static bool LizardActAnimation(Creature self)
    {
        return IsIncon(self) && !ShadowOfOptions.liz_fear_move.Value;
    }
    #endregion

    #region LizardAI
    static void ILLizardAIAggressiveBehavior(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardAI>("focusCreature"),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("age"),
            x => x.MatchLdcI4(120),
            x => x.MatchBle(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardAIAggressiveBehavior target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<LizardAI>("get_lizard"),
            x => x.MatchLdfld<Lizard>("loungeDelay"),
            x => x.MatchLdcI4(1),
            x => x.MatchBge(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardAI>(OpCodes.Call, "get_lizard");
            val.EmitDelegate(LizardAIAggressiveBehavior);
            val.Emit(OpCodes.Brtrue, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardAIAggressiveBehavior!");
        }
    }
    public static bool LizardAIAggressiveBehavior(Creature self)
    {
        return IsIncon(self) && ShadowOfOptions.liz_attack.Value && ShadowOfOptions.liz_attack_move.Value;
    }
    #endregion

    #region LizardGraphics
    static void ILLizardGraphicsDrawSprites(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region JawReadyForBite
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdloc(2),
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.2f),
            x => x.MatchMul(),
            x => x.MatchAdd(),
            x => x.MatchStloc(2),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsDrawSprites JawReadyForBite target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardGraphicsDrawSprites JawReadyForBite!");
        }
        #endregion

        #region bubble
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchLdfld<Lizard>("bubble"),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsDrawSprites bubble target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsDrawSprites bubble!");
        }
        #endregion
    }
    static void ILLizardGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region creatureLooker
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdloc(0),
            x => x.MatchLdcR4(80),
            x => x.MatchSub(),
            x => x.MatchStloc(0)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate creatureLooker target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate creatureLooker!");
        }
        #endregion

        #region blink
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lightSource"),
            x => x.MatchBrfalse(out _)
        }))
        {
            //val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate blink target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate blink!");
        }
        #endregion

        /*
        #region breath
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("breath"),
            x => x.MatchLdcR4(0.0125f),
            x => x.MatchAdd(),
            x => x.MatchStfld<LizardGraphics>("breath"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for breath target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(ShadowOfLizardGraphicsUpdate);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for breath!");
        }
        #endregion
        */

        #region tailDirection
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.05f),
            x => x.MatchBgeUn(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate tailDirection target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate tailDirection!");
        }
        #endregion

        #region Head Rotation
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("head"),
            x => x.MatchCallvirt<BodyPart>("Update")
        })) { }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate head!");
        }

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("drawPositions"),
            x => x.MatchLdcI4(0),
            x => x.MatchLdcI4(0)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate headrotation target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardGraphics>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardGraphicsUpdate headrotation!");
        }
        #endregion
    }
    #endregion

    #region LizardVoice
    static void ILLizardVoiceMakeSound(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardVoice>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardVoice>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(LizardVoiceMakeSound);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardVoiceMakeSound!");
        }
    }
    public static bool LizardVoiceMakeSound(Creature self)
    {
        return ShadowOfOptions.liz_voice.Value && IsIncon(self);
    }
    #endregion

    #region LizardUnused
    static void ILLizardJumpModuleUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Start
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardJumpModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardJumpModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brfalse_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleViolence target!");
        }
        #endregion

        #region Start
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[6]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardJumpModule>("lizard"),
            x => x.MatchLdfld<Lizard>("animation"),
            x => x.MatchLdsfld<Lizard.Animation>("PrepareToJump"),
            x => x.MatchCall("ExtEnum`1<Lizard/Animation>", "op_Equality"),
            x => x.MatchBrtrue(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleViolence target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardJumpModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<LizardJumpModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleViolence target!");
        }
        #endregion
    }
    static void ILLizardRotModuleViolence(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Start
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("vocalizeSoundCooldown"),
            x => x.MatchLdcI4(0),
            x => x.MatchBgt(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleViolence target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Watcher.LizardRotModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(ILLizardRotModule);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardRotModuleViolence!");
        }
        #endregion
    }
    static void ILLizardRotModuleUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Start
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("transforming"),
            x => x.MatchBrfalse(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleUpdate target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Watcher.LizardRotModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(ILLizardRotModule);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleUpdate!");
        }
        #endregion

        #region bubble
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[7]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("lizard"),
            x => x.MatchCallvirt<Lizard>("get_LizardState"),
            x => x.MatchLdfld<LizardState>("rotType"),
            x => x.MatchLdsfld<LizardState.RotType>("Full"),
            x => x.MatchCall("ExtEnum`1<LizardState/RotType>", "op_Equality"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleUpdate sound target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardRotModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrtrue(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Watcher.LizardRotModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(ILLizardRotModule);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match for ILLizardRotModuleUpdate sound!");
        }
        #endregion
    }
    static bool ILLizardRotModule(Creature self)
    {
        if (ShadowOfOptions.liz_rot.Value && IsIncon(self))
        {
            return true;
        }

        return false;
    }
    static void ILLizardBlizzardModuleUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region beamTimer
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardBlizzardModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Watcher.LizardBlizzardModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardBlizzardModuleUpdate beamTimer!");
        }
        #endregion

        #region beamTimer+5
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardBlizzardModule>("lizard"),
            x => x.MatchLdfld<Lizard>("AI"),
            x => x.MatchBrfalse(out _)
        }))
        {
            //val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardBlizzardModuleUpdate beamTimer+5 target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Watcher.LizardBlizzardModule>("lizard"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrfalse(out _)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<Watcher.LizardBlizzardModule>(OpCodes.Ldfld, "lizard");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLizardBlizzardModuleUpdate beamTimer+5!");
        }
        #endregion
    }
    #endregion
}