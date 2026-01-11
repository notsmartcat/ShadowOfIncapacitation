using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

using static Incapacitation.Incapacitation;

namespace Incapacitation.LanternMouseHooks;

internal class ILHooks
{
    public static void Apply()
    {
        IL.LanternMouse.Update += ILLanternMouseUpdate;

        IL.MouseGraphics.DrawSprites += ILMouseGraphicsDrawSprites;
        IL.MouseGraphics.Update += ILMouseGraphicsUpdate;
    }

    #region LanternMouse
    static void ILLanternMouseUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Bleed
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCallvirt<Creature>("Die")
        }))
        {
            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(delegate (Creature creature)
            {
                ILHooksMisc.TryAddKillFeedEntry(creature, "Bleed");
            });
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate Bleed!");
        }
        #endregion

        #region Battery
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdfld<MouseState>("battery"),
        })){}
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate Battery skip!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<Creature>("get_dead"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate Battery!");
        }
        #endregion

        #region NoAct
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<LanternMouse>("get_Footing")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate NoAct Target!");
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
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target); ;
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate NoAct!");
        }
        #endregion

        #region NoFooting
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<LanternMouse>("get_Footing"),
            x => x.MatchBrfalse(out target)
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILLanternMouseUpdate NoFooting!");
        }
        #endregion
    }
    #endregion

    #region LanterMouseGraphics
    static void ILMouseGraphicsDrawSprites(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Vector4
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdfld<LanternMouse>("voiceCounter")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match LanterMouseGraphics Vector4 Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match LanterMouseGraphics Vector4!");
        }
        #endregion

        #region num14
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[2]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<MouseGraphics>("ouchEyes"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match LanterMouseGraphics num14 Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match LanterMouseGraphics num14!");
        }
        #endregion

        #region num14Dead
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_dead"),
            x => x.MatchBrfalse(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match LanterMouseGraphics num14Dead!");
        }
        #endregion
    }
    static void ILMouseGraphicsUpdate(ILContext il)
    {
        ILCursor val = new(il);
        ILLabel target = null;

        #region Con
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdfld<LanternMouse>("profileFac")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate Con Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate Con!");
        }
        #endregion

        #region NotDead
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdfld<LanternMouse>("carried")
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate NotDead Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate NotDead!");
        }
        #endregion

        /*
        #region lastConsious
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[1]
        {
            x => x.MatchLdcI4(1),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate lastConsious Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate lastConsious!");
        }
        #endregion
        */

        #region flickeringFac
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.value)).GetGetMethod()),
            x => x.MatchLdcR4(0.0033333334f),
            x => x.MatchBgeUn(out _),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate flickeringFac Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate flickeringFac!");
        }
        #endregion

        #region breath
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<MouseGraphics>("breath"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate breath Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_dead"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsComa);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate breath!");
        }
        #endregion

        #region lookDir
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<MouseGraphics>("creatureLooker"),
            x => x.MatchLdfld<CreatureLooker>("lookCreature"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate lookDir Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate lookDir!");
        }
        #endregion

        #region headVel
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[4]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
            x => x.MatchBrtrue(out target),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate headVel!");
        }
        #endregion

        #region tailPos
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdfld<LanternMouse>("sitting"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate tailPos Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate tailPos!");
        }
        #endregion

        #region limbs
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdflda<LanternMouse>("ropeAttatchedPos"),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate limbs Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate limbs!");
        }
        #endregion

        #region limbsDangle
        if (val.TryGotoNext(MoveType.Before, new Func<Instruction, bool>[5]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchLdfld<LanternMouse>("AI"),
            x => x.MatchLdfld<MouseAI>("fear"),
            x => x.MatchLdcR4(0.1f),
        }))
        {
            val.MoveAfterLabels();

            target = val.MarkLabel();
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate limbsDangle Target!");
        }

        if (val.TryGotoPrev(MoveType.Before, new Func<Instruction, bool>[3]
        {
            x => x.MatchLdarg(0),
            x => x.MatchCall<MouseGraphics>("get_mouse"),
            x => x.MatchCallvirt<Creature>("get_Consious"),
        }))
        {
            val.MoveAfterLabels();

            val.Emit(OpCodes.Ldarg_0);
            val.Emit<MouseGraphics>(OpCodes.Call, "get_mouse");
            val.EmitDelegate(IsIncon);
            val.Emit(OpCodes.Brtrue_S, target);
        }
        else
        {
            Incapacitation.Logger.LogInfo(all + "Could not find match ILMouseGraphicsUpdate limbsDangle!");
        }
        #endregion
    }
    #endregion
}