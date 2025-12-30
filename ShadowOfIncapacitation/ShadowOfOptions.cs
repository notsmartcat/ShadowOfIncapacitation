using System.Collections.Generic;
using System.Globalization;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Incapacitation;

public class ShadowOfOptions : OptionInterface
{
    public ShadowOfOptions(Incapacitation plugin)
    {
        #region Main
        debug_keys = config.Bind("debug_keys", false, new ConfigurableInfo("If turned On N kills all creatures that can be affected by this mod in the Players room putting them in the 'Incapacitated' state. (Default = false)", null, "", new object[1] { "Debug Keys" }));
        debug_logs = config.Bind("debug_logs", false, new ConfigurableInfo("If turned On Messages that include info about Creatures will show up when you turn on Debug Logs, these will also appear in the 'consoleLog.txt' all logs from this mod start with 'IncapacitationOf:' for easy locating. (Default = false)", null, "", new object[1] { "Debug Logs" }));
        chance_logs = config.Bind("chance_logs", false, new ConfigurableInfo("If turned On Messages that include exact chance numbers Lizards rolled compared to the target number to succed a check, these will also appear in the 'consoleLog.txt' all logs from this mod start with 'ShadowOf:' for easy locating. (Default = false)", null, "", new object[1] { "Chance Debug Logs" }));

        #region Chances
        uncon_chance_bleed = config.Bind("uncon_chance_bleed", 10, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_bleed = config.Bind("incon_chance_bleed", 60, new ConfigurableInfo("", null, "", new object[1] { "" }));

        uncon_chance_blunt = config.Bind("uncon_chance_blunt", 80, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_blunt = config.Bind("incon_chance_blunt", 95, new ConfigurableInfo("", null, "", new object[1] { "" }));

        uncon_chance_stab = config.Bind("uncon_chance_stab", 10, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_stab = config.Bind("incon_chance_stab", 50, new ConfigurableInfo("", null, "", new object[1] { "" }));

        uncon_chance_electric = config.Bind("uncon_chance_electric", 20, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_electric = config.Bind("incon_chance_electric", 40, new ConfigurableInfo("", null, "", new object[1] { "" }));

        uncon_chance_explosion = config.Bind("uncon_chance_explosion", 20, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_explosion = config.Bind("incon_chance_explosion", 25, new ConfigurableInfo("", null, "", new object[1] { "" }));
        #endregion

        insta_die_threshold = config.Bind("insta_die_threshold", -1f, new ConfigurableInfo("Whenever a creatures health goes below this threshold the creature will not go into either the Unconscious or Incapacitated State (Default = -1)", new ConfigAcceptableRange<float>(-5f, -0.5f), "", new object[1] { "Static Death Threshold" }));
        die_threshold_min = config.Bind("die_threshold_min", 0.2f, new ConfigurableInfo("", new ConfigAcceptableRange<float>(0.1f, 3f), "", new object[1] { "" }));
        die_threshold_max = config.Bind("die_threshold_max", 0.5f, new ConfigurableInfo("", new ConfigAcceptableRange<float>(0.1f, 3f), "", new object[1] { "" }));

        cheat_death = config.Bind("cheat_death", false, new ConfigurableInfo("If On Creatures affected by this mod can Cheat Death, which means they will not be replaced on the next cycle. (Default = false)", null, "", new object[1] { "Cheat Death" }));
        den_revive = config.Bind("den_revive", false, new ConfigurableInfo("If On Creatures that are either Unconscious or Incapacitated will be revived if brought into the player's Den. After being revived the creature will go to it's den (if it has a den) and the creature will be neutral to the Player unless attacked. (Default = false)", null, "", new object[1] { "Den Revive" }));

        blunt_uncon_guaranteed = config.Bind("blunt_uncon_guaranteed", true, new ConfigurableInfo("If On Incapacitated Creatures will be guaranteed to be knocked Unconscious state if hit in the head and their health if less then half (if applicable) (Default = true)", null, "", new object[1] { "Guaranteed Unconscious" }));

        uncon_chance_cheat_death = config.Bind("uncon_chance_cheat_death", 30, new ConfigurableInfo("", null, "", new object[1] { "" }));
        incon_chance_cheat_death = config.Bind("incon_chance_cheat_death", 60, new ConfigurableInfo("", null, "", new object[1] { "" }));
        #endregion

        #region Creatures
        #region Base Game
        #region BigSpider
        spid_state = config.Bind("spid_state", "Incapacitation, Cheating Death and Den Revive", new ConfigurableInfo("Big Spider"));

        spid_mother = config.Bind("spid_mother", false, new ConfigurableInfo("If On Big Spider Mothers will not spew babies when it is Incapacitated or Unconscious. (Default = false)", null, "", new object[1] { "Mother" }));
        spid_cling = config.Bind("spid_cling", false, new ConfigurableInfo("If On Incapacitated Big Spiders will cling onto creatures that touch it's front. This will attack the Spider to the Creature, most likely moving the Spider. (Default = false)", null, "", new object[1] { "Cling" }));
        spid_collide = config.Bind("spid_collide", false, new ConfigurableInfo("If On Incapacitated Big Spiders will move themselves away whenever they are touched fron behind or the side, if 'Spider Jump' is on some Spiders will jump away. This will move the Spider. (Default = false)", null, "", new object[1] { "Collide" }));
        spid_attack = config.Bind("spid_attack", false, new ConfigurableInfo("If On Incapacitated Big Spiders will attempt to bite prey near them, if 'Spider Jump' is on the Spider will leap towards the prey. (Default = false)", null, "", new object[1] { "Attack" }));
        spid_jump = config.Bind("spid_jump", false, new ConfigurableInfo("If On Incapacitated Big Spiders will be capable of jumping if other spider settings that include jumping are On. This will move the Spider. (Default = false)", null, "", new object[1] { "Jump" }));
        spid_dodge = config.Bind("spid_dodge", false, new ConfigurableInfo("If this and 'Spider Jump' are On Incapacitated Big Spiders will attempt to dodge flying weapons by Jumping. This will move the Spider. (Default = false)", null, "", new object[1] { "Dodge" }));
        #endregion

        #region Centipede
        centi_state = config.Bind("centi_state", "Incapacitation, Cheating Death and Den Revive", new ConfigurableInfo("Centipede"));

        centi_shock = config.Bind("centi_shock", false, new ConfigurableInfo("If On Incapacitated baby Centipedes will shock anything that holds them. (Default = false)", null, "", new object[1] { "Baby Shock" }));
        centi_grab = config.Bind("centi_grab", false, new ConfigurableInfo("If On Incapacitated Centipedes will attempt to grab onto prey, the Centipedes are incapable of Shocking. (Default = false)", null, "", new object[1] { "Grab" }));
        #endregion

        #region Cicada
        cic_state = config.Bind("cic_state", "Incapacitation, Cheating Death and Den Revive", new ConfigurableInfo("Cicada"));

        cic_eat = config.Bind("cic_eat", false, new ConfigurableInfo("If On Incapacitated Cicadas will grab and hold any valid edibles. (Default = false)", null, "", new object[1] { "Eat" }));
        cic_attack = config.Bind("cic_attack", false, new ConfigurableInfo("If On Incapacitated Cicadas will sometimes try to Bump Creatures, this will move the Cicada. (Default = false)", null, "", new object[1] { "Bump" }));
        #endregion

        #region Lizard
        liz_state = config.Bind("liz_state", "Incapacitation, Cheating Death and Den Revive", new ConfigurableInfo("Lizard"));

        liz_spit = config.Bind("liz_spit", false, new ConfigurableInfo("(Default = false)", null, "", new object[1] { "Spit" }));
        liz_blizzard = config.Bind("liz_blizzard", false, new ConfigurableInfo("(Default = false)", null, "", new object[1] { "Blizzard Ability" }));
        liz_rot = config.Bind("liz_rot", false, new ConfigurableInfo("(Default = false)", null, "", new object[1] { "Rot Ability" }));
        liz_jump = config.Bind("liz_jump", false, new ConfigurableInfo("(Default = false)", null, "", new object[1] { "Jump Ability" }));

        liz_attack = config.Bind("liz_attack", false, new ConfigurableInfo("If On Incapacitated Lizards will attempt to bite prey near them. The bites will never actually damage anything (Default = false)", null, "", new object[1] { "Attempt Bite" }));
        liz_attack_move = config.Bind("liz_attack_move", false, new ConfigurableInfo("If On and Lizard Attempt Bite is On Incapacitated Lizards will Lunge whenever appropierate, this will move the Lizard. (Default = false)", null, "", new object[1] { "Lunge" }));
        liz_voice = config.Bind("liz_voice", false, new ConfigurableInfo("If On Incapacitated Lizards will make sounds such as pain and fear. These sounds can be heard by other creatures in the game. (Default = false)", null, "", new object[1] { "Voice" }));
        liz_fear_move = config.Bind("liz_fear_move", false, new ConfigurableInfo("If On Incapacitated Lizards will attempt to scoot away from their predators. (Default = false)", null, "", new object[1] { "Fear Movement" }));
        liz_friend = config.Bind("liz_friend", false, new ConfigurableInfo("If this and 'Den Revive' are On Lizards that have been Revived by the players den will always be tamed. (Default = false)", null, "", new object[1] { "Den Tame" }));
        #endregion

        #region Vulture
        vul_state = config.Bind("vul_state", "Incapacitation and Cheating Death", new ConfigurableInfo("Vulture"));

        vul_attack = config.Bind("vul_attack", false, new ConfigurableInfo("If On Incapacitated Vultures will attempt to bite prey near them. The bites will never actually damage anything (Default = false)", null, "", new object[1] { "Attempt Bite" }));
        vul_attack_move = config.Bind("vul_attack_move", false, new ConfigurableInfo("If On and Vulture Attempt Bite is On Incapacitated Vultures will slightly move themselves forward during bites, this will move the Vulture. (Default = false)", null, "", new object[1] { "Lunge" }));
        #endregion

        #region SlugPup
        slug_state = config.Bind("slug_state", "Incapacitation and Den Revive", new ConfigurableInfo("Slugcat"));

        incon_slugplayer = config.Bind("incon_slugplayer", true, new ConfigurableInfo("If On Slugcats (Players) will be affected by the Incapacitation if it is Enabled for Slugcats. Cheating Death is disabled no matter what. Incapacitated Players cannot move or do anything. (Default = true)", null, "", new object[1] { "SlugCat" }));
        incon_slugpup = config.Bind("incon_slugpup", true, new ConfigurableInfo("If On SlugPups will be affected by the Incapacitation and Cheating Death depending on if they are Enabled for Slugcats. (Default = true)", null, "", new object[1] { "SlugPup" }));
        slugpup_never_die = config.Bind("slugpup_never_die", false, new ConfigurableInfo("If On SlugPups will always be incapacitated and will not die in this state. Bringing the SlugPup into the den and hibernating will revive it. (Default = false)", null, "", new object[1] { "SlugPup Never Die" }));
        #endregion

        #region Scavenger
        scav_state = config.Bind("scav_state", "Incapacitation, Cheating Death and Den Revive", new ConfigurableInfo("Scavenger"));

        scav_back_spear = config.Bind("scav_back_spear", false, new ConfigurableInfo("If On Scavengers will not drop any items that are attacked to them (such as back-spears) when they are Incapacitated or Unconscious. (Default = false)", null, "", new object[1] { "Drop Items" }));
        #endregion
        #endregion
        #endregion
    }

    #region Misc Values
    readonly float font_height = 20f;
    float spacing = 20f;
    readonly int number_of_check_boxes = 3;
    readonly float check_box_size = 24f;

    Vector2 margin_x = default;
    Vector2 position = default;
    readonly List<OpLabel> text_labels = new();
    readonly List<float> box_end_positions = new();

    readonly List<Configurable<bool>> check_box_configurables = new();
    readonly List<OpLabel> check_boxes_text_labels = new();

    readonly List<Configurable<int>> slider_configurables = new();
    readonly List<Configurable<float>> float_slider_configurables = new();
    readonly List<string> slider_main_text_labels = new();
    readonly List<OpLabel> slider_text_labels_left = new();
    readonly List<OpLabel> slider_text_labels_right = new();

    float Check_Box_With_Spacing => check_box_size + 0.25f * spacing;
    #endregion

    public override void Initialize()
    {
        base.Initialize();
        Tabs = new OpTab[3];

        #region Main Options
        Tabs[0] = new OpTab(this, "Main Options");
        InitializeMarginAndPos();

        //AddNewLine();
        AddBox();
        AddCheckBox(debug_keys, (string)debug_keys.info.Tags[0]);
        AddCheckBox(debug_logs, (string)debug_logs.info.Tags[0]);
        //AddCheckBox(chance_logs, (string)chance_logs.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[0]);

        AddNewLine();
        AddSlider(uncon_chance_bleed, (string)uncon_chance_bleed.info.Tags[0], "0%", "100%");
        AddSecondarySlider(incon_chance_bleed, "100%");
        DrawDualSliders(ref Tabs[0], 0);

        //AddNewLine();
        AddSlider(uncon_chance_blunt, (string)uncon_chance_blunt.info.Tags[0], "0%", "100%");
        AddSecondarySlider(incon_chance_blunt, "100%");
        DrawDualSliders(ref Tabs[0], 1);

        //AddNewLine();
        AddSlider(uncon_chance_stab, (string)uncon_chance_stab.info.Tags[0], "0%", "100%");
        AddSecondarySlider(incon_chance_stab, "100%");
        DrawDualSliders(ref Tabs[0], 2);

        //AddNewLine();
        AddSlider(uncon_chance_electric, (string)uncon_chance_electric.info.Tags[0], "0%", "100%");
        AddSecondarySlider(incon_chance_electric, "100%");
        DrawDualSliders(ref Tabs[0], 3);

        //AddNewLine();
        AddSlider(uncon_chance_explosion, (string)uncon_chance_explosion.info.Tags[0], "0%", "100%");
        AddSecondarySlider(incon_chance_explosion, "100%");
        DrawDualSliders(ref Tabs[0], 4);
        DrawBox(ref Tabs[0]);

        AddNewLine();
        AddBox();
        AddCheckBox(cheat_death, (string)cheat_death.info.Tags[0]);
        AddCheckBox(den_revive, (string)den_revive.info.Tags[0]);
        AddCheckBox(blunt_uncon_guaranteed, (string)blunt_uncon_guaranteed.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[0]);

        AddNewLine();
        DrawFloatSliders(ref Tabs[0]);

        AddNewLine();
        DrawFloatDualSliders(ref Tabs[0]);
        DrawBox(ref Tabs[0]);
        #endregion

        #region Base Game Creatures 1
        Tabs[1] = new OpTab(this, " Base Creatures 1");
        InitializeMarginAndPos();

        #region BigSpider
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[1], spid_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death", "Incapacitation, Cheating Death and Den Revive" });
        AddNewLine();
        AddCheckBox(spid_mother, (string)spid_mother.info.Tags[0]);
        AddCheckBox(spid_cling, (string)spid_cling.info.Tags[0]);
        AddCheckBox(spid_collide, (string)spid_collide.info.Tags[0]);
        AddCheckBox(spid_attack, (string)spid_attack.info.Tags[0]);
        AddCheckBox(spid_jump, (string)spid_jump.info.Tags[0]);
        AddCheckBox(spid_dodge, (string)spid_dodge.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[1]);
        DrawBox(ref Tabs[1]);
        #endregion

        #region Centipede
        AddNewLine();
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[1], centi_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death", "Incapacitation, Cheating Death and Den Revive" });
        AddNewLine();
        AddCheckBox(centi_shock, (string)centi_shock.info.Tags[0]);
        AddCheckBox(centi_grab, (string)centi_grab.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[1]);
        DrawBox(ref Tabs[1]);
        #endregion

        #region Cicada
        AddNewLine();
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[1], cic_state, new List<string>{ "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death", "Incapacitation, Cheating Death and Den Revive" });
        AddNewLine();
        AddCheckBox(cic_eat, (string)cic_eat.info.Tags[0]);
        AddCheckBox(cic_attack, (string)cic_attack.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[1]);
        DrawBox(ref Tabs[1]);
        #endregion

        #region Lizard
        AddNewLine();
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[1], liz_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death", "Incapacitation, Cheating Death and Den Revive" });
        AddNewLine();
        AddCheckBox(liz_attack, (string)liz_attack.info.Tags[0]);
        AddCheckBox(liz_attack_move, (string)liz_attack_move.info.Tags[0]);
        AddCheckBox(liz_voice, (string)liz_voice.info.Tags[0]);
        AddCheckBox(liz_fear_move, (string)liz_fear_move.info.Tags[0]);
        AddCheckBox(liz_friend, (string)liz_friend.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[1]);
        DrawBox(ref Tabs[1]);
        #endregion
        #endregion

        #region Base Game Creatures 2
        Tabs[2] = new OpTab(this, "Base Creatures 2");
        InitializeMarginAndPos();

        #region Slugcat
        AddNewLine();
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[2], slug_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Den Revive" });
        AddNewLine();
        AddCheckBox(incon_slugplayer, (string)incon_slugplayer.info.Tags[0]);
        AddCheckBox(incon_slugpup, (string)incon_slugpup.info.Tags[0]);
        AddCheckBox(slugpup_never_die, (string)slugpup_never_die.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[2]);
        DrawBox(ref Tabs[2]);
        #endregion

        #region Vulture
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[2], vul_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death" });
        AddNewLine();
        AddCheckBox(vul_attack, (string)vul_attack.info.Tags[0]);
        AddCheckBox(vul_attack_move, (string)vul_attack_move.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[2]);
        DrawBox(ref Tabs[2]);
        #endregion

        #region Scavenger
        AddNewLine();
        AddBox();
        AddNewLine();
        DrawComboBox(ref Tabs[2], scav_state, new List<string> { "Disabled", "Incapacitation Only", "Incapacitation and Cheating Death", "Incapacitation, Cheating Death and Den Revive" });
        AddNewLine();
        AddCheckBox(scav_back_spear, (string)scav_back_spear.info.Tags[0]);
        DrawCheckBoxes(ref Tabs[2]);
        DrawBox(ref Tabs[2]);
        #endregion
        #endregion
    }

    public override void Update()
    {
        int bleed = int.Parse(((UIconfig)OPunconbleedSlider).value);
        int bleed2 = int.Parse(((UIconfig)OPinconbleedSlider).value);
        int bleed3 = Mathf.Clamp(100 - bleed, 0, 100);

        int blunt = int.Parse(((UIconfig)OPunconbluntSlider).value);
        int blunt2 = int.Parse(((UIconfig)OPinconbluntSlider).value);
        int blunt3 = Mathf.Clamp(100 - blunt, 0, 100);

        int stab = int.Parse(((UIconfig)OPunconstabSlider).value);
        int stab2 = int.Parse(((UIconfig)OPinconstabSlider).value);
        int stab3 = Mathf.Clamp(100 - stab, 0, 100);

        int electric = int.Parse(((UIconfig)OPunconelectricSlider).value);
        int electric2 = int.Parse(((UIconfig)OPinconelectricSlider).value);
        int electric3 = Mathf.Clamp(100 - electric, 0, 100);

        int explosion = int.Parse(((UIconfig)OPunconexplosionSlider).value);
        int explosion2 = int.Parse(((UIconfig)OPinconexplosionSlider).value);
        int explosion3 = Mathf.Clamp(100 - explosion, 0, 100);

        float thresholdMin = Mathf.Floor(float.Parse(((UIconfig)OPdiethresholdminSlider).value) * 10f) / 10f;
        float thresholdMax = Mathf.Floor(float.Parse(((UIconfig)OPdiethresholdmaxSlider).value) * 10f) / 10f;

        if (bleed2 - bleed <= 0)
        {
            ((UIconfig)OPinconbleedSlider).value = bleed.ToString();
        }
        if (blunt2 - blunt <= 0)
        {
            ((UIconfig)OPinconbluntSlider).value = blunt.ToString();
        }
        if (stab2 - stab <= 0)
        {
            ((UIconfig)OPinconstabSlider).value = stab.ToString();
        }
        if (electric2 - electric <= 0)
        {
            ((UIconfig)OPinconelectricSlider).value = electric.ToString();
        }
        if (explosion2 - explosion <= 0)
        {
            ((UIconfig)OPinconexplosionSlider).value = explosion.ToString();
        }
        if (thresholdMax - thresholdMin <= 0)
        {
            ((UIconfig)OPdiethresholdmaxSlider).value = thresholdMin.ToString();
        }

        OPinconbleedSlider._label.text = Mathf.Clamp(bleed2 - bleed, 0, bleed3) + "%";
        OPbleedChance.text = "Bleed; Unconscious = " + bleed + "%; Incapacitation = " + Mathf.Clamp(bleed2 - bleed, 0, bleed3) + "%; Death = " + (100 - (bleed + Mathf.Clamp(bleed2 - bleed, 0, bleed3))) + "%";

        OPinconbluntSlider._label.text = Mathf.Clamp(blunt2 - blunt, 0, blunt3) + "%";
        OPbluntChance.text = "Blunt; Unconscious = " + blunt + "%; Incapacitation = " + Mathf.Clamp(blunt2 - blunt, 0, blunt3) + "%; Death = " + (100 - (blunt + Mathf.Clamp(blunt2 - blunt, 0, blunt3))) + "%";

        OPinconstabSlider._label.text = Mathf.Clamp(stab2 - stab, 0, stab3) + "%";
        OPstabChance.text = "Stab and Bite; Unconscious = " + stab + "%; Incapacitation = " + Mathf.Clamp(stab2 - stab, 0, stab3) + "%; Death = " + (100 - (stab + Mathf.Clamp(stab2 - stab, 0, stab3))) + "%";

        OPinconelectricSlider._label.text = Mathf.Clamp(electric2 - electric, 0, electric3) + "%";
        OPelectricChance.text = "Electric; Unconscious = " + electric + "%; Incapacitation = " + Mathf.Clamp(electric2 - electric, 0, electric3) + "%; Death = " + (100 - (electric + Mathf.Clamp(electric2 - electric, 0, electric3))) + "%";

        OPinconexplosionSlider._label.text = Mathf.Clamp(explosion2 - explosion, 0, explosion3) + "%";
        OPexplosionChance.text = "Explosion; Unconscious = " + explosion + "%; Incapacitation = " + Mathf.Clamp(explosion2 - explosion, 0, explosion3) + "%; Death = " + (100 - (explosion + Mathf.Clamp(explosion2 - explosion, 0, explosion3))) + "%";

        OPdiethresholdmaxSlider._label.text = Mathf.Clamp(thresholdMax, thresholdMin, 3) + "";
        OPthreshold.text = "Whenever a Creature gets Incapacitated a new Threshold between " + thresholdMin + ((thresholdMin % 1) == 0 ? ".0" : "") + " and " + thresholdMax + ((thresholdMax % 1) == 0 ? ".0" : "") + " lower then the Creatures current health will be randomly selected, the creature will die when it gets below this Threshold.";
    }

    void InitializeMarginAndPos()
    {
        margin_x = new Vector2(20f, 550f);
        position = new Vector2(20f, 600f);
    }

    void AddNewLine(float spacingModifier = 1f)
    {
        position.x = margin_x.x;
        position.y -= spacingModifier * spacing;
    }

    void AddBox()
    {
        margin_x += new Vector2(spacing, 0f - spacing);
        box_end_positions.Add(position.y);
        AddNewLine();
    }

    void DrawBox(ref OpTab tab)
    {
        margin_x += new Vector2(0f - spacing, spacing);
        AddNewLine();
        float num = margin_x.y - margin_x.x + 20;
        int index = box_end_positions.Count - 1;
        tab.AddItems((UIelement[])(object)new UIelement[1] { new OpRect(position, new Vector2(num, box_end_positions[index] - position.y), 0.3f) });
        box_end_positions.RemoveAt(index);
    }

    void AddCheckBox(Configurable<bool> configurable, string text)
    {
        check_box_configurables.Add(configurable);
        check_boxes_text_labels.Add(new OpLabel(default, default, text, (FLabelAlignment)1, false, null));
    }

    void DrawCheckBoxes(ref OpTab tab)
    {
        if (check_box_configurables.Count != check_boxes_text_labels.Count)
        {
            return;
        }
        float num = margin_x.y - margin_x.x;
        float num2 = (num - (number_of_check_boxes - 1) * 0.5f * spacing) / number_of_check_boxes;
        position.y -= check_box_size;
        float num3 = position.x;
        for (int i = 0; i < check_box_configurables.Count; i++)
        {
            Configurable<bool> val = check_box_configurables[i];
            OpCheckBox val2 = new(val, new Vector2(num3, position.y))
            {
                description = (val.info?.description ?? "")
            };
            tab.AddItems((UIelement[])(object)new UIelement[1] { val2 });
            num3 += Check_Box_With_Spacing;
            OpLabel val3 = check_boxes_text_labels[i];
            ((UIelement)val3).pos = new Vector2(num3, position.y + 2f);
            val3.size = new Vector2(num2 - Check_Box_With_Spacing, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });
            if (i < check_box_configurables.Count - 1)
            {
                if ((i + 1) % number_of_check_boxes == 0)
                {
                    AddNewLine();
                    position.y -= check_box_size;
                    num3 = position.x;
                }
                else
                {
                    num3 += num2 - Check_Box_With_Spacing + 0.5f * spacing;
                }
            }
        }
        check_box_configurables.Clear();
        check_boxes_text_labels.Clear();
    }

    void DrawComboBox(ref OpTab tab, Configurable<string> config, List<string> list)
    {
        float num = margin_x.y - margin_x.x;
        float num2 = num * 0.5f * spacing;
        float num3 = position.x;

        List<ListItem> items = new();

        for (int i = 0; i < list.Count; i++)
        {
            items.Add(new ListItem(list[i], i));
        }

        if (items.Count == 0)
        {
            return;
        }

        OpLabel val3 = new(default, default, config.info.description, (FLabelAlignment)1, true, null);
        ((UIelement)val3).pos = new Vector2(num3, position.y + 4f);
        val3.size = new Vector2(num2, font_height);
        num3 += 150;
        tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });

        ComboBox val2 = new(config, new Vector2(num3, position.y), 350, items);
        tab.AddItems((UIelement[])(object)new UIelement[1] { val2 });
    }

    void DrawCheckBoxAndSliderCombo(ref OpTab tab)
    {
        if (check_box_configurables.Count != check_boxes_text_labels.Count)
        {
            return;
        }
        float num = margin_x.y - margin_x.x;
        float num2 = (num - (number_of_check_boxes - 1) * 0.5f * spacing) / number_of_check_boxes;
        position.y -= check_box_size;
        float num3 = position.x;
        for (int i = 0; i < check_box_configurables.Count; i++)
        {
            Configurable<bool> val = check_box_configurables[i];
            OpCheckBox val2 = new(val, new Vector2(num3, position.y))
            {
                description = (val.info?.description ?? "")
            };
            tab.AddItems((UIelement[])(object)new UIelement[1] { val2 });
            num3 += Check_Box_With_Spacing;
            OpLabel val3 = check_boxes_text_labels[i];
            ((UIelement)val3).pos = new Vector2(num3, position.y + 2f);
            val3.size = new Vector2(num2 - Check_Box_With_Spacing, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });
            if (i < check_box_configurables.Count - 1)
            {
                if ((i + 1) % number_of_check_boxes == 0)
                {
                    AddNewLine();
                    position.y -= check_box_size;
                    num3 = position.x;
                }
                else
                {
                    num3 += num2 - Check_Box_With_Spacing + 0.5f * spacing;
                }
            }
        }
        check_box_configurables.Clear();
        check_boxes_text_labels.Clear();

        if (slider_configurables.Count != slider_main_text_labels.Count || slider_configurables.Count != slider_text_labels_left.Count || slider_configurables.Count != slider_text_labels_right.Count)
        {
            return;
        }
        num = margin_x.y - margin_x.x;
        num2 = margin_x.x + 0.7f * num;
        num3 = 0.2f * num;
        float num4 = num - 2f * num3 - 20;
        for (int i = 0; i < slider_configurables.Count; i++)
        {
            //AddNewLine(2f);
            OpLabel val = slider_text_labels_left[i];
            ((UIelement)val).pos = new Vector2(margin_x.x + 90f, position.y);
            val.size = new Vector2(num3, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val });
            Configurable<int> val2 = slider_configurables[i];
            OpSlider val3 = new(val2, new Vector2(num2 - 0.5f * num4, position.y - 5f), (int)num4, false)
            {
                size = new Vector2(num4, font_height),
                description = (val2.info?.description ?? "")
            };
            tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });
            val = slider_text_labels_right[i];
            ((UIelement)val).pos = new Vector2(num2 + 0.5f * num4 + 0.5f * 20, position.y);
            val.size = new Vector2(num3, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val });
            AddTextLabel(slider_main_text_labels[i], 0);
            DrawTextLabel(ref tab);
            if (i < slider_configurables.Count - 1)
            {
                AddNewLine();
            }
        }
        slider_configurables.Clear();
        slider_main_text_labels.Clear();
        slider_text_labels_left.Clear();
        slider_text_labels_right.Clear();

        void DrawTextLabel(ref OpTab tab)
        {
            if (text_labels.Count == 0)
            {
                return;
            }
            float num = (margin_x.y - margin_x.x) / text_labels.Count;
            foreach (OpLabel text_label in text_labels)
            {
                text_label.pos = new Vector2(num2 - 0.5f * num4, position.y);
                text_label.size = new Vector2(num4, font_height);
                tab.AddItems((UIelement[])(object)new UIelement[1] { text_label });
                position.x += num;
            }
            position.x = margin_x.x;
            text_labels.Clear();
        }
    }

    void AddSlider(Configurable<int> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
    {
        slider_configurables.Add(configurable);
        slider_main_text_labels.Add(text);
        slider_text_labels_left.Add(new OpLabel(default, default, sliderTextLeft, (FLabelAlignment)2, false, null));
        slider_text_labels_right.Add(new OpLabel(default, default, sliderTextRight, (FLabelAlignment)1, false, null));
    }
    void AddSecondarySlider(Configurable<int> configurable, string sliderTextRight = "")
    {
        slider_configurables.Add(configurable);
        slider_main_text_labels.Add("");
        slider_text_labels_left.Add(new OpLabel(default, default, "", (FLabelAlignment)2, false, null));
        slider_text_labels_right.Add(new OpLabel(default, default, sliderTextRight, (FLabelAlignment)1, false, null));
    }
    void AddFloatSlider(Configurable<float> configurable, string text, string sliderTextLeft = "", string sliderTextRight = "")
    {
        float_slider_configurables.Add(configurable);
        slider_main_text_labels.Add(text);
        slider_text_labels_left.Add(new OpLabel(default, default, sliderTextLeft, (FLabelAlignment)2, false, null));
        slider_text_labels_right.Add(new OpLabel(default, default, sliderTextRight, (FLabelAlignment)1, false, null));
    }

    void DrawDualSliders(ref OpTab tab, int variant)
    {
        if (slider_configurables.Count != slider_main_text_labels.Count || slider_configurables.Count != slider_text_labels_left.Count || slider_configurables.Count != slider_text_labels_right.Count)
        {
            return;
        }
        float num = margin_x.y - margin_x.x;
        float num2 = margin_x.x + 0.5f * num;
        float num3 = 0.2f * num;
        float num4 = num - 2f * num3 - spacing;
        float num5 = num4 / 2;

        if (variant != 0)
        {
            AddNewLine(2f);
        }
        else
        {
            AddNewLine();
        }

        for (int i = 0; i < slider_configurables.Count; i++)
        {
            if (i == 0)
            {
                OpLabel val = slider_text_labels_left[i];
                ((UIelement)val).pos = new Vector2(margin_x.x, position.y + 5f);
                val.size = new Vector2(num3, font_height);
                tab.AddItems((UIelement[])(object)new UIelement[1] { val });

                tab.AddItems((UIelement[])(object)new UIelement[1] { NewSLider(i) });
            }
            else
            {
                tab.AddItems((UIelement[])(object)new UIelement[1] { NewSLider(i) });
                OpLabel val = slider_text_labels_right[i];
                ((UIelement)val).pos = new Vector2(num2 + 0.5f * num4 + 0.5f * spacing, position.y + 5f);
                val.size = new Vector2(num3, font_height);
                tab.AddItems((UIelement[])(object)new UIelement[1] { val });
            }
        }
        AddChanceLabel();
        DrawTextLabels(ref tab);
        slider_configurables.Clear();
        slider_main_text_labels.Clear();
        slider_text_labels_left.Clear();
        slider_text_labels_right.Clear();

        OpSlider NewSLider(int i)
        {
            Configurable<int> val2 = slider_configurables[i];
            if (i == 0)
            {
                if (variant == 0)
                {
                    OPunconbleedSlider = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPunconbleedSlider;
                }
                else if (variant == 1)
                {
                    OPunconbluntSlider = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPunconbluntSlider;
                }
                else if (variant == 2)
                {
                    OPunconstabSlider = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPunconstabSlider;
                }
                else if (variant == 3)
                {
                    OPunconelectricSlider = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPunconelectricSlider;
                }
                else
                {
                    OPunconexplosionSlider = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPunconexplosionSlider;
                }
            }
            else
            {
                if (variant == 0)
                {
                    OPinconbleedSlider = new(val2, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPinconbleedSlider;
                }
                else if (variant == 1)
                {
                    OPinconbluntSlider = new(val2, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPinconbluntSlider;
                }
                else if (variant == 2)
                {
                    OPinconstabSlider = new(val2, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPinconstabSlider;
                }
                else if (variant == 3)
                {
                    OPinconelectricSlider = new(val2, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPinconelectricSlider;
                }
                else
                {
                    OPinconexplosionSlider = new(val2, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, false)
                    {
                        size = new Vector2(num5, font_height),
                    };

                    return OPinconexplosionSlider;
                }
            }
        }

        void AddChanceLabel(FLabelAlignment alignment = 0, bool bigText = false)
        {
            float num = (bigText ? 2f : 1f) * font_height;
            if (text_labels.Count == 0)
            {
                position.y -= num;
            }
            text_labels.Add(NewLabel());

            OpLabel NewLabel()
            {
                if (variant == 0)
                {
                    OPbleedChance = new(default, new Vector2(20f, num), "", alignment, bigText, null)
                    {
                        autoWrap = true
                    };

                    return OPbleedChance;
                }
                else if (variant == 1)
                {
                    OPbluntChance = new(default, new Vector2(20f, num), "", alignment, bigText, null)
                    {
                        autoWrap = true
                    };

                    return OPbluntChance;
                }
                else if (variant == 2)
                {
                    OPstabChance = new(default, new Vector2(20f, num), "", alignment, bigText, null)
                    {
                        autoWrap = true
                    };

                    return OPstabChance;
                }
                else if (variant == 3)
                {
                    OPelectricChance = new(default, new Vector2(20f, num), "", alignment, bigText, null)
                    {
                        autoWrap = true
                    };

                    return OPelectricChance;
                }
                else
                {
                    OPexplosionChance = new(default, new Vector2(20f, num), "", alignment, bigText, null)
                    {
                        autoWrap = true
                    };

                    return OPexplosionChance;
                }
            }
        }
    }

    void DrawSliders(ref OpTab tab)
    {
        if (slider_configurables.Count != slider_main_text_labels.Count || slider_configurables.Count != slider_text_labels_left.Count || slider_configurables.Count != slider_text_labels_right.Count)
        {
            return;
        }
        float num = margin_x.y - margin_x.x;
        float num2 = margin_x.x + 0.5f * num;
        float num3 = 0.2f * num;
        float num4 = num - 2f * num3 - spacing;
        for (int i = 0; i < slider_configurables.Count; i++)
        {
            AddNewLine(2f);
            OpLabel val = slider_text_labels_left[i];
            ((UIelement)val).pos = new Vector2(margin_x.x, position.y + 5f);
            val.size = new Vector2(num3, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val });
            Configurable<int> val2 = slider_configurables[i];
            OpSlider val3 = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num4, false)
            {
                size = new Vector2(num4, font_height),
                description = (val2.info?.description ?? "")
            };
            tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });
            val = slider_text_labels_right[i];
            ((UIelement)val).pos = new Vector2(num2 + 0.5f * num4 + 0.5f * spacing, position.y + 5f);
            val.size = new Vector2(num3, font_height);
            tab.AddItems((UIelement[])(object)new UIelement[1] { val });
            AddTextLabel(slider_main_text_labels[i], 0);
            DrawTextLabels(ref tab);
            if (i < slider_configurables.Count - 1)
            {
                AddNewLine();
            }
        }
        slider_configurables.Clear();
        slider_main_text_labels.Clear();
        slider_text_labels_left.Clear();
        slider_text_labels_right.Clear();
    }

    void DrawFloatSliders(ref OpTab tab)
    {
        float num = margin_x.y - margin_x.x;
        float num2 = margin_x.x + 0.5f * num;
        float num3 = 0.2f * num;
        float num4 = num - 2f * num3 - spacing;


        AddNewLine(1f);
        OpLabel val = new(default, default, "-5", (FLabelAlignment)2, false, null);
        ((UIelement)val).pos = new Vector2(margin_x.x, position.y + 5f);
        val.size = new Vector2(num3, font_height);
        tab.AddItems((UIelement[])(object)new UIelement[1] { val });
        Configurable<float> val2 = insta_die_threshold;
        OpFloatSlider val3 = new(val2, new Vector2(num2 - 0.5f * num4, position.y), (int)num4, 1)
        {
            size = new Vector2(num4, font_height),
            description = (val2.info?.description ?? "")
        };
        tab.AddItems((UIelement[])(object)new UIelement[1] { val3 });
        val = new(default, default, "-0.5", (FLabelAlignment)1, false, null);
        ((UIelement)val).pos = new Vector2(num2 + 0.5f * num4 + 0.5f * spacing, position.y + 5f);
        val.size = new Vector2(num3, font_height);
        tab.AddItems((UIelement[])(object)new UIelement[1] { val });
        AddTextLabel((string)insta_die_threshold.info.Tags[0], 0);
        DrawTextLabels(ref tab);
    }

    void DrawFloatDualSliders(ref OpTab tab)
    {
        float num = margin_x.y - margin_x.x;
        float num2 = margin_x.x + 0.5f * num;
        float num3 = 0.2f * num;
        float num4 = num - 2f * num3 - spacing;

        float num5 = num4 / 2;

        AddNewLine();

        OpLabel val = new(default, default, "0.1", (FLabelAlignment)2, false, null);
        ((UIelement)val).pos = new Vector2(margin_x.x, position.y + 5f);
        val.size = new Vector2(num3, font_height);
        tab.AddItems((UIelement[])(object)new UIelement[1] { val });

        OPdiethresholdminSlider = new(die_threshold_min, new Vector2(num2 - 0.5f * num4, position.y), (int)num5, 1)
        {
            size = new Vector2(num5, font_height),
        };
        tab.AddItems((UIelement[])(object)new UIelement[1] { OPdiethresholdminSlider });

        OPdiethresholdmaxSlider = new(die_threshold_max, new Vector2(num2 - 0.5f * num4 + num5, position.y), (int)num5, 1)
        {
            size = new Vector2(num5, font_height),
        };
        tab.AddItems((UIelement[])(object)new UIelement[1] { OPdiethresholdmaxSlider });
        val = new(default, default, "3", (FLabelAlignment)1, false, null);
        ((UIelement)val).pos = new Vector2(num2 + 0.5f * num4 + 0.5f * spacing, position.y + 5f);
        val.size = new Vector2(num3, font_height);
        tab.AddItems((UIelement[])(object)new UIelement[1] { val });

        AddNewLine(1.5f);

        AddText();
        DrawTextLabels(ref tab);

        void AddText()
        {
            if (text_labels.Count == 0)
            {
                position.y -= 1;
            }
            OPthreshold = new(default, new Vector2(20f, 1), "", 0, false, null)
            {
                autoWrap = true
            };
            text_labels.Add(OPthreshold);
        }
    }

    void AddTextLabel(string text, FLabelAlignment alignment = 0, bool bigText = false)
    {
        float num = (bigText ? 2f : 1f) * font_height;
        if (text_labels.Count == 0)
        {
            position.y -= num;
        }
        OpLabel label = new(default, new Vector2(20f, num), text, alignment, bigText, null)
        {
            autoWrap = true
        };
        text_labels.Add(label);
    }

    void DrawTextLabels(ref OpTab tab)
    {
        if (text_labels.Count == 0)
        {
            return;
        }
        float num = (margin_x.y - margin_x.x) / text_labels.Count;
        foreach (OpLabel text_label in text_labels)
        {
            text_label.pos = position;
            text_label.size += new Vector2(num - 20f, 0f);
            tab.AddItems((UIelement[])(object)new UIelement[1] { text_label });
            position.x += num;
        }
        position.x = margin_x.x;
        text_labels.Clear();
    }

    #region Main
    public static Configurable<bool> debug_keys;
    public static Configurable<bool> debug_logs;
    public static Configurable<bool> chance_logs;

    #region Chances
    public static Configurable<int> uncon_chance_bleed;
    public static Configurable<int> incon_chance_bleed;

    public static Configurable<int> uncon_chance_cheat_death;
    public static Configurable<int> incon_chance_cheat_death;

    public OpSlider OPunconbleedSlider;
    public OpSlider OPinconbleedSlider;
    public OpLabel OPbleedChance;

    public static Configurable<int> uncon_chance_blunt;
    public static Configurable<int> incon_chance_blunt;

    public OpSlider OPunconbluntSlider;
    public OpSlider OPinconbluntSlider;
    public OpLabel OPbluntChance;

    public static Configurable<int> uncon_chance_stab;
    public static Configurable<int> incon_chance_stab;

    public OpSlider OPunconstabSlider;
    public OpSlider OPinconstabSlider;
    public OpLabel OPstabChance;

    public static Configurable<int> uncon_chance_electric;
    public static Configurable<int> incon_chance_electric;

    public OpSlider OPunconelectricSlider;
    public OpSlider OPinconelectricSlider;
    public OpLabel OPelectricChance;

    public static Configurable<int> uncon_chance_explosion;
    public static Configurable<int> incon_chance_explosion;

    public OpSlider OPunconexplosionSlider;
    public OpSlider OPinconexplosionSlider;
    public OpLabel OPexplosionChance;

    public static Configurable<float> insta_die_threshold;
    public static Configurable<float> die_threshold_min;
    public static Configurable<float> die_threshold_max;

    public OpFloatSlider OPdiethresholdminSlider;
    public OpFloatSlider OPdiethresholdmaxSlider;
    public OpLabel OPthreshold;
    #endregion

    public static Configurable<bool> cheat_death;
    public static Configurable<bool> den_revive;

    public static Configurable<bool> blunt_uncon_guaranteed;
    #endregion

    #region Creatures
    #region Base Game
    #region BigSpider
    public static Configurable<string> spid_state;

    public static Configurable<bool> spid_mother;
    public static Configurable<bool> spid_cling;
    public static Configurable<bool> spid_collide;
    public static Configurable<bool> spid_attack;
    public static Configurable<bool> spid_jump;
    public static Configurable<bool> spid_dodge;
    #endregion

    #region Centipede
    public static Configurable<string> centi_state;

    public static Configurable<bool> centi_shock;
    public static Configurable<bool> centi_grab;
    #endregion

    #region Cicada
    public static Configurable<string> cic_state;

    public static Configurable<bool> cic_eat;
    public static Configurable<bool> cic_attack;
    #endregion

    #region Lizard
    public static Configurable<string> liz_state;

    public static Configurable<bool> liz_attack;
    public static Configurable<bool> liz_attack_move;
    public static Configurable<bool> liz_spit;
    public static Configurable<bool> liz_blizzard;
    public static Configurable<bool> liz_rot;
    public static Configurable<bool> liz_jump;

    public static Configurable<bool> liz_voice;
    public static Configurable<bool> liz_fear_move;
    public static Configurable<bool> liz_friend;
    #endregion

    #region Vulture
    public static Configurable<string> vul_state;

    public static Configurable<bool> vul_attack;
    public static Configurable<bool> vul_attack_move;
    #endregion

    #region SlugNPC
    public static Configurable<string> slug_state;
    public static Configurable<bool> incon_slugplayer;
    public static Configurable<bool> incon_slugpup;

    public static Configurable<bool> slugpup_never_die;
    #endregion

    #region Scav
    public static Configurable<string> scav_state;

    public static Configurable<bool> scav_back_spear;
    #endregion
    #endregion
    #endregion
}

internal class ComboBox : OpComboBox
{
    public const int BG_SPRITE_INDEX_RANGE = 9;

    public ComboBox(Configurable<string> config, Vector2 pos, float width, string[] array) : base(config, pos, width, array)
    {
    }

    public ComboBox(Configurable<string> config, Vector2 pos, float width, List<ListItem> list) : base(config, pos, width, list)
    {
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        if (_rectList != null && !_rectList.isHidden)
        {
            myContainer.MoveToFront();

            for (int i = 0; i < BG_SPRITE_INDEX_RANGE; i++)
            {
                _rectList.sprites[i].alpha = 1;
            }
        }
    }
}