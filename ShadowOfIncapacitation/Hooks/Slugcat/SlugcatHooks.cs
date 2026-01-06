using MonoMod.RuntimeDetour;
using System;
using UnityEngine;

using static Incapacitation.Incapacitation;

namespace Incapacitation.SlugcatHooks;

internal class Hooks
{
    public static void Apply()
    {
        On.Player.CanEatMeat += PlayerCanEatMeat;
        On.Player.CanIPutDeadSlugOnBack += PlayerCanIPutDeadSlugOnBack;

        On.PlayerGraphics.Update += PlayerGraphicsUpdate;

        new Hook(
            typeof(Player).GetProperty(nameof(Player.Wounded)).GetGetMethod(),
            typeof(Hooks).GetMethod(nameof(PlayerWounded)));
    }

    #region Slugcat
    static bool PlayerCanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
    {
        return orig(self, crit) && (crit.abstractCreature.creatureTemplate.TopAncestor().type != MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC || !IsComa(crit));
    }
    static bool PlayerCanIPutDeadSlugOnBack(On.Player.orig_CanIPutDeadSlugOnBack orig, Player self, Player pickUpCandidate)
    {
        if (ModManager.MSC && pickUpCandidate != null && IsComa(pickUpCandidate))
        {
            return true;
        }
        return orig(self, pickUpCandidate);
    }
    #endregion

    #region SlugcatGraphics
    static void PlayerGraphicsUpdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        orig(self);

        if (!inconstorage.TryGetValue(self.player.abstractCreature, out InconData data) || !IsComa(self.player))
        {
            return;
        }

        self.breath += (IsIncon(self.player) && !self.player.Sleeping) ? (1f / Mathf.Lerp(60f, 15f, Mathf.Pow(self.player.aerobicLevel, 1.5f))) : 0.0125f;

        self.player.standing = false;
    }
    #endregion

    #region SlugcatManual
    public static bool PlayerWounded(Func<Player, bool> orig, Player self)
    {
        try
        {
            if (IsIncon(self) && (self.State as PlayerState).permanentDamageTracking > 0.4)
            {
                return true;
            }
        }
        catch (Exception e) { Incapacitation.Logger.LogError(e); }
        return orig(self);
    }
    #endregion
}