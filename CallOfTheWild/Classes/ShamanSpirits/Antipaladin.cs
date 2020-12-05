﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using static Kingmaker.UnitLogic.ActivatableAbilities.ActivatableAbilityResourceLogic;
using static Kingmaker.UnitLogic.Commands.Base.UnitCommand;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Blueprints.Root;

namespace CallOfTheWild
{
    public class Antipaladin
    {
        static internal LibraryScriptableObject library => Main.library;
        static internal bool test_mode = false;
        static public BlueprintCharacterClass antipaladin_class;
        static public BlueprintProgression antipaladin_progression;

        static public BlueprintFeatureSelection antipaladin_deity;
        static public BlueprintFeature antipaladin_proficiencies;
        static public BlueprintFeature touch_of_corruption;
        static public BlueprintFeature smite_good;
        static public BlueprintFeature smite_good_extra_use;
        static public BlueprintFeature unholy_resilence;
        static public BlueprintFeature aura_of_cowardice;
        static public BlueprintFeature plague_bringer;
        static public BlueprintFeature channel_negative_energy;
        static public BlueprintFeature[] fiendish_boon;
        static public BlueprintFeature aura_of_despair;
        static public BlueprintFeature aura_of_vengeance;
        static public BlueprintFeature aura_of_sin;
        static public BlueprintFeature aura_of_deparvity;
        static public BlueprintFeature tip_of_spear;
        static public BlueprintFeature antipaladin_alignment;

        static public BlueprintAbilityResource smite_resource;
        static public BlueprintAbilityResource touch_of_corruption_resource;
        static public BlueprintAbilityResource fiendish_boon_resource;

        static public BlueprintFeature extra_touch_of_corruption;
        static public BlueprintFeature extra_channel;
        static public BlueprintAbilityResource extra_channel_resource;

        static public BlueprintFeatureSelection cruelty;

        static public BlueprintArchetype insinuator;
        static public BlueprintFeature invocation;
        static public BlueprintFeature smite_impudence;
        static public BlueprintFeature smite_impudence_extra_use;
        static public BlueprintFeature selfish_healing;
        static public BlueprintFeature aura_of_ego;
        static public BlueprintFeature stubborn_health;
        static public BlueprintFeatureSelection greeds;
        static public BlueprintFeature insinuator_channel_energy;
        static public BlueprintFeature channel_positive_energy;
        static public BlueprintFeature aura_of_ambition;
        static public BlueprintFeature aura_of_glory;
        static public BlueprintFeature aura_of_belief;
        static public BlueprintFeature aura_of_indomitability;
        static public BlueprintFeature personal_champion;
        static public BlueprintFeatureSelection insinuator_bonus_feat;



        internal class CrueltyEntry
        {
            static Dictionary<string, CrueltyEntry> cruelties_map = new Dictionary<string, CrueltyEntry>();
            static Dictionary<string, BlueprintFeature> features_map = new Dictionary<string, BlueprintFeature>();
            string dispaly_name;
            string description;
            BlueprintBuff[] buffs;
            string prerequisite_id;
            int level;
            int divisor;
            SavingThrowType save;

            internal static void addCruelty(string cruelty_name, string cruelty_description, int lvl, string prerequisite_name, int round_divisor, SavingThrowType save_type, params BlueprintBuff[] effects)
            {
                cruelties_map.Add(cruelty_name, new CrueltyEntry(cruelty_name, cruelty_description, lvl, prerequisite_name, round_divisor, save_type, effects));
            }


            CrueltyEntry(string cruelty_name, string cruelty_description, int lvl, string prerequisite_name, int round_divisor, SavingThrowType save_type, params BlueprintBuff[] effects)
            {
                buffs = effects;
                level = lvl;
                prerequisite_id = prerequisite_name;
                divisor = round_divisor;
                dispaly_name = cruelty_name;
                description = cruelty_description;
                save = save_type;
            }


            BlueprintFeature createCrueltyFeature(BlueprintAbility base_ability)
            {
                var abilities_touch = new List<BlueprintAbility>();
                var abilities_hit = new List<BlueprintAbility>();

                var feature = Helpers.CreateFeature(dispaly_name + "CrueltyFeature",
                                                    "Cruelty: " + dispaly_name,
                                                    description,
                                                    "",
                                                    buffs[0].Icon,
                                                    FeatureGroup.None);

                if (level > 0)
                {
                    feature.AddComponent(Helpers.PrerequisiteClassLevel(antipaladin_class, level));
                }
                if (!prerequisite_id.Empty())
                {
                    feature.AddComponent(Helpers.PrerequisiteFeature(features_map[prerequisite_id]));
                }
                foreach (var b in buffs)
                {
                    var touch_ability = base_ability.Variants[0].GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility;
                    var touch_cruelty = library.CopyAndAdd(touch_ability, b.name + touch_ability.name, "");
                    touch_cruelty.SetNameDescriptionIcon("Cruelty: " + b.Name,
                                                         description +"\n" + b.Name + ": " + b.Description,
                                                         b.Icon);

                    var duration = Helpers.CreateContextDuration();
                    if (divisor < 0)
                    {
                        duration = Helpers.CreateContextDuration(divisor);
                    }
                    else if (divisor > 0)
                    {
                        duration = Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.StatBonus));
                    }

                    var effect_action = Helpers.CreateActionSavingThrow(save,
                                                                        new GameAction[] { Helpers.CreateConditionalSaved(Common.createContextActionApplyBuff(b, duration, is_permanent: divisor == 0, dispellable: false), null) }
                                                                        );

                    touch_cruelty.ReplaceComponent<AbilityEffectRunAction>(a => a.Actions = Helpers.CreateActionList(a.Actions.Actions.AddToArray(effect_action)));
                    if (divisor > 0)
                    {
                        touch_cruelty.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.DivStep, AbilityRankType.StatBonus,
                                                                                   stepLevel: divisor, classes: getAntipaladinArray()));
                    }
                    var spell_descriptor_component = b.GetComponent<SpellDescriptorComponent>();

                    var cast_cruelty = Helpers.CreateTouchSpellCast(touch_cruelty);
                    cast_cruelty.AddComponent(base_ability.Variants[0].GetComponent<AbilityResourceLogic>());
                    cast_cruelty.AddComponent(Common.createAbilityShowIfCasterHasFact(feature));
                    cast_cruelty.AddComponents(Common.createAbilityTargetHasFact(true, Common.construct), Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
                    cast_cruelty.Parent = base_ability;
                    cast_cruelty.Parent.addToAbilityVariants(cast_cruelty);
                    if (spell_descriptor_component != null)
                    {
                        touch_cruelty.AddComponent(spell_descriptor_component);
                        cast_cruelty.AddComponent(spell_descriptor_component);
                    }
                }
                return feature;
            }


            internal static BlueprintFeature[] createCruelties(BlueprintAbility base_ability)
            {
                foreach (var kv in cruelties_map)
                {
                    features_map.Add(kv.Key, kv.Value.createCrueltyFeature(base_ability));
                }

                return features_map.Values.ToArray();
            }
        }

        internal static void creatAntipaldinClass()
        {
            Main.logger.Log("Antipaladin class test mode: " + test_mode.ToString());
            var fighter_class = library.Get<BlueprintCharacterClass>("48ac8db94d5de7645906c7d0ad3bcfbd");
            var paladin_class = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            var inquisitor_class = library.Get<BlueprintCharacterClass>("f1a70d9e1b0b41e49874e1fa9052a1ce");

            antipaladin_class = Helpers.Create<BlueprintCharacterClass>();
            antipaladin_class.name = "AntipaladinClass";
            library.AddAsset(antipaladin_class, "");

            antipaladin_class.LocalizedName = Helpers.CreateString("Antipaladin.Name", "Antipaladin");
            antipaladin_class.LocalizedDescription = Helpers.CreateString("Antipaladin.Description",
                                                                         "Although it is a rare occurrence, paladins do sometimes stray from the path of righteousness. Most of these wayward holy warriors seek out redemption and forgiveness for their misdeeds, regaining their powers through piety, charity, and powerful magic. Yet there are others, the dark and disturbed few, who turn actively to evil, courting the dark powers they once railed against in order to take vengeance on their former brothers. It’s said that those who climb the farthest have the farthest to fall, and antipaladins are living proof of this fact, their pride and hatred blinding them to the glory of their forsaken patrons.\n"
                                                                         + "Antipaladins become the antithesis of their former selves. They make pacts with fiends, take the lives of the innocent, and put nothing ahead of their personal power and wealth. Champions of evil, they often lead armies of evil creatures and work with other villains to bring ruin to the holy and tyranny to the weak. Not surprisingly, paladins stop at nothing to put an end to such nefarious antiheroes.\n"
                                                                         + "Role: Antipaladins are villains at their most dangerous. They care nothing for the lives of others and actively seek to bring death and destruction to ordered society. They rarely travel with those that they do not subjugate, unless as part of a ruse to bring ruin from within."
                                                                         );
            antipaladin_class.m_Icon = fighter_class.Icon;
            antipaladin_class.SkillPoints = paladin_class.SkillPoints;
            antipaladin_class.HitDie = DiceType.D10;
            antipaladin_class.BaseAttackBonus = paladin_class.BaseAttackBonus;
            antipaladin_class.FortitudeSave = paladin_class.FortitudeSave;
            antipaladin_class.ReflexSave = paladin_class.ReflexSave;
            antipaladin_class.WillSave = paladin_class.WillSave;
            antipaladin_class.Spellbook = createAntipaladinSpellbook();
            antipaladin_class.ClassSkills = paladin_class.ClassSkills;
            antipaladin_class.IsDivineCaster = true;
            antipaladin_class.IsArcaneCaster = false;
            antipaladin_class.StartingGold = paladin_class.StartingGold;
            antipaladin_class.PrimaryColor = inquisitor_class.PrimaryColor;
            antipaladin_class.SecondaryColor = inquisitor_class.SecondaryColor;
            antipaladin_class.RecommendedAttributes = paladin_class.RecommendedAttributes;
            antipaladin_class.NotRecommendedAttributes = paladin_class.NotRecommendedAttributes;
            antipaladin_class.EquipmentEntities = paladin_class.EquipmentEntities;
            antipaladin_class.MaleEquipmentEntities = paladin_class.MaleEquipmentEntities;
            antipaladin_class.FemaleEquipmentEntities = paladin_class.FemaleEquipmentEntities;
            antipaladin_class.ComponentsArray = paladin_class.ComponentsArray.ToArray();
            antipaladin_class.ReplaceComponent<PrerequisiteAlignment>(p => p.Alignment = Kingmaker.UnitLogic.Alignments.AlignmentMaskType.Evil);
            antipaladin_class.StartingItems = paladin_class.StartingItems;
            createAntipaladinProgression();
            antipaladin_class.Progression = antipaladin_progression;

            createInsinuator();
            antipaladin_class.Archetypes = new BlueprintArchetype[] {insinuator }; //blighted myrmidon, insinuator, dread vanguard, seal breaker, iron tyrant
            Helpers.RegisterClass(antipaladin_class);
            fixAntipaladinFeats();

            antipaladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = paladin_class));
            antipaladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = VindicativeBastard.vindicative_bastard_class));
            paladin_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = antipaladin_class));
            VindicativeBastard.vindicative_bastard_class.AddComponent(Helpers.Create<PrerequisiteNoClassLevel>(p => p.CharacterClass = antipaladin_class));

            Common.addMTDivineSpellbookProgression(antipaladin_class, antipaladin_class.Spellbook, "MysticTheurgeAntipaladin",
                                                   Common.createPrerequisiteClassSpellLevel(antipaladin_class, 2));
        }


        static void createInsinuator()
        {
            insinuator = Helpers.Create<BlueprintArchetype>(a =>
            {
                a.name = "InsinuatorArchetype";
                a.LocalizedName = Helpers.CreateString($"{a.name}.Name", "Insinuator");
                a.LocalizedDescription = Helpers.CreateString($"{a.name}.Description", "Between the selfless nobility of paladins and the chaotic menace of antipaladins, there exists a path of dedicated self-interest.\nShunning the ties that bind them to a single deity, insinuators embrace whatever forces help them achieve their own agenda and glory, borrowing power to emulate divine warriors.");
            });
            Helpers.SetField(insinuator, "m_ParentClass", antipaladin_class);
            library.AddAsset(insinuator, "");

            createInvocation(); //also aura of belief, aura of indomitobility, aura of ego, aura of ambition and personal champion (dr part)
            createSmiteImpudence(); //also personal champion smite part and aura of glory
            createSelfishHealing(); //and greeds
            createInsinuatorChannelEnergy(); 
            createBonusFeats();
            //ambitious bond(?)

            insinuator.RemoveFeatures = new LevelEntry[] {Helpers.LevelEntry(1, smite_good, antipaladin_deity),
                                                          Helpers.LevelEntry(2, touch_of_corruption),
                                                          Helpers.LevelEntry(3, cruelty, aura_of_cowardice),
                                                          Helpers.LevelEntry(4, smite_good_extra_use, channel_negative_energy),
                                                          Helpers.LevelEntry(6, cruelty),
                                                          Helpers.LevelEntry(7, smite_good_extra_use),
                                                          Helpers.LevelEntry(8, aura_of_despair),
                                                          Helpers.LevelEntry(9, cruelty),
                                                          Helpers.LevelEntry(10, smite_good_extra_use),
                                                          Helpers.LevelEntry(11, aura_of_vengeance),
                                                          Helpers.LevelEntry(12, cruelty),
                                                          Helpers.LevelEntry(13, smite_good_extra_use),
                                                          Helpers.LevelEntry(14, aura_of_sin),
                                                          Helpers.LevelEntry(15, cruelty),
                                                          Helpers.LevelEntry(16, smite_good_extra_use),
                                                          Helpers.LevelEntry(17, aura_of_deparvity),
                                                          Helpers.LevelEntry(18, cruelty),
                                                          Helpers.LevelEntry(19, smite_good_extra_use),
                                                          Helpers.LevelEntry(20, tip_of_spear)
                                                         };

            insinuator.AddFeatures = new LevelEntry[] {Helpers.LevelEntry(1, invocation, smite_impudence),
                                                       Helpers.LevelEntry(2, selfish_healing),
                                                       Helpers.LevelEntry(3, greeds, aura_of_ego),
                                                       Helpers.LevelEntry(4, insinuator_channel_energy, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(6, greeds),
                                                       Helpers.LevelEntry(7, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(8, aura_of_ambition),
                                                       Helpers.LevelEntry(9, greeds),
                                                       Helpers.LevelEntry(10, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(11, aura_of_glory),
                                                       Helpers.LevelEntry(12, greeds),
                                                       Helpers.LevelEntry(13, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(14, aura_of_belief),
                                                       Helpers.LevelEntry(15, greeds),
                                                       Helpers.LevelEntry(16, insinuator_bonus_feat, smite_impudence_extra_use),
                                                       Helpers.LevelEntry(17, aura_of_indomitability),
                                                       Helpers.LevelEntry(18, greeds),
                                                       Helpers.LevelEntry(19, insinuator_bonus_feat,smite_impudence_extra_use),
                                                       Helpers.LevelEntry(20, personal_champion),
                                                      };

            antipaladin_progression.UIGroups[0].Features.Add(stubborn_health);
            antipaladin_progression.UIGroups[1].Features.Add(smite_impudence);
            antipaladin_progression.UIGroups[1].Features.Add(smite_impudence_extra_use);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_ego);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_ambition);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_glory);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_belief);
            antipaladin_progression.UIGroups[2].Features.Add(aura_of_indomitability);
            antipaladin_progression.UIGroups[2].Features.Add(personal_champion);
            antipaladin_progression.UIGroups[3].Features.Add(selfish_healing);
            antipaladin_progression.UIGroups[3].Features.Add(greeds);
            antipaladin_progression.UIGroups[3].Features.Add(insinuator_channel_energy);
            antipaladin_progression.UIGroups = antipaladin_progression.UIGroups.AddToArray(Helpers.CreateUIGroup(insinuator_bonus_feat));
            antipaladin_progression.UIDeterminatorsGroup = antipaladin_progression.UIDeterminatorsGroup.AddToArray(invocation);

            insinuator.RemoveSpellbook = true;
        }

        static void createBonusFeats()
        {
            insinuator_bonus_feat = library.CopyAndAdd<BlueprintFeatureSelection>("41c8486641f7d6d4283ca9dae4147a9f", "InsinuatorBonusFeat", ""); //combat feat
            insinuator_bonus_feat.AllFeatures = insinuator_bonus_feat.AllFeatures.AddToArray(library.Get<BlueprintFeatureSelection>("c9629ef9eebb88b479b2fbc5e836656a"));
            insinuator_bonus_feat.SetNameDescription("Bonus Feat",
                                                     "At 4th level, an insinuator gains one bonus feat, which must be selected from the list of combat feats or Skill Focus. At 7th level and every 3 antipaladin levels thereafter, the insinuator gains one additional combat or Skill Focus feat.");
        }

        static void createInsinuatorChannelEnergy()
        {
            var positive_energy_feature = library.Get<BlueprintFeature>("a79013ff4bcd4864cb669622a29ddafb");
            var context_rank_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(), progression: ContextRankProgression.StartPlusDivStep, stepLevel: 4, startLevel: 2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getAntipaladinArray(), StatType.Charisma);
            channel_positive_energy = Helpers.CreateFeature("AntipaladinChannelPositiveEnergyFeature",
                                                            "Channel Positive Energy",
                                                            "When an insinuator reaches 4th level, he gains the supernatural ability to channel positive energy like a cleric if she invokes a neutral outsider. Using this ability consumes two uses of his touch of corruption ability. An Insinuator uses half his level as his effective cleric level when channeling positive energy. This is a Charisma-based ability.",
                                                            "",
                                                            positive_energy_feature.Icon,
                                                            FeatureGroup.None);

            var heal_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                                      "AntipaladinChannelEnergyHealLiving",
                                                                      "",
                                                                      "Channeling positive energy causes a burst that heals all living creatures in a 30-foot radius centered on the insinuator. The amount of damage healed is equal to 1d6 points of damage plus 1d6 points of damage for every four insinuator levels beyond 2nd (2d6 at 6th, 3d6 at 10th, and so on).",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));
            var harm_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                                                        "AntipaladinChannelEnergyHarmUndead",
                                                                        "",
                                                                        "Channeling positive energy causes a burst that damages all undead creatures in a 30-foot radius centered on the insinuator. The amount of damage inflicted is equal to 1d6 points of damage plus 1d6 points of damage for every four insinuator levels beyond 2nd (2d6 at 6th, 3d6 at 10th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1/2 the insinuator's level + the insinuator's Charisma modifier.",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));

            var heal_living_base = Common.createVariantWrapper("AntipaladinPositiveHealBase", "", heal_living);
            var harm_undead_base = Common.createVariantWrapper("AntipaladinPositiveHarmBase", "", harm_undead);
            heal_living.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_living.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());
            harm_undead.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            harm_undead.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());

            ChannelEnergyEngine.storeChannel(heal_living, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(heal_living_base, harm_undead_base));

            var heal_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHeal,
                                                          "AntipaladinChannelEnergyHealLivingExtra",
                                                          heal_living.Name + " (Extra)",
                                                          heal_living.Description,
                                                          "",
                                                          heal_living.GetComponent<ContextRankConfig>(),
                                                          heal_living.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                          Helpers.CreateResourceLogic(extra_channel_resource, true, 1));

            var harm_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.PositiveHarm,
                                              "AntipaladinChannelEnergyHarmUndeadExtra",
                                              harm_undead.Name + " (Extra)",
                                              harm_undead.Description,
                                              "",
                                              harm_undead.GetComponent<ContextRankConfig>(),
                                              harm_undead.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                              Helpers.CreateResourceLogic(extra_channel_resource, true, 1));


            heal_living_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_living_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());
            harm_undead_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            harm_undead_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInsinuatorAlignmentNeutral>());

            heal_living_base.addToAbilityVariants(heal_living_extra);
            harm_undead_base.addToAbilityVariants(harm_undead_extra);
            ChannelEnergyEngine.storeChannel(heal_living_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHeal);
            ChannelEnergyEngine.storeChannel(harm_undead_extra, channel_positive_energy, ChannelEnergyEngine.ChannelType.PositiveHarm);


            insinuator_channel_energy = Helpers.CreateFeature("InsinuatorChannelEnergy",
                                                              "Channel Energy",
                                                              "At 4th level, an insinuator can channel negative energy, treating his antipaladin level as his effective cleric level. If he invokes a neutral outsider for the day, he may instead chose to channel positive energy, but treats his effective cleric level as half his antipaladin level. Using this ability consumes two uses of his selfish healing ability. This is a Charisma-based ability.",
                                                              "",
                                                              LoadIcons.Image2Sprite.Create(@"AbilityIcons/ChannelEnergy.png"),
                                                              FeatureGroup.None,
                                                              Helpers.CreateAddFacts(channel_negative_energy, channel_positive_energy)
                                                              );
        }


        static void createSelfishHealing()
        {
            var ability = library.CopyAndAdd<BlueprintAbility>("8d6073201e5395d458b8251386d72df1", "SelfishHealingAbility", "");
            ability.ReplaceComponent<ContextRankConfig>(c => Helpers.SetField(c, "m_Class", getAntipaladinArray()));
            ability.SetNameDescription("Selfish Healing",
                                       "Beginning at 2nd level, an insinuator can heal his wounds by touch. This is treated exactly like the paladin’s lay on hands class feature, except it can be used only to heal the insinuator and cannot be used on other creatures."
                                       );

            ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            selfish_healing = Common.AbilityToFeature(ability, false);
            selfish_healing.AddComponent(touch_of_corruption_resource.CreateAddAbilityResource());

            greeds = library.CopyAndAdd<BlueprintFeatureSelection>("02b187038a8dce545bb34bbfb346428d", "GreedsSelection", "");
            greeds.SetNameDescription("Greeds",
                                      "Beginning at 3rd level, an insinuator can heal himself of certain conditions. This functions as the mercy paladin class ability, but these mercies can only be applied to the insinuator himself."
                                      );
            foreach (var f in greeds.AllFeatures)
            {
                var prereq = f.GetComponent<PrerequisiteClassLevel>();
                if (prereq == null)
                {
                    continue;
                }
                prereq.Group = Prerequisite.GroupType.Any;
                f.AddComponent(Common.createPrerequisiteArchetypeLevel(insinuator, prereq.Level, any: true));
            }
        }



        static void createSmiteImpudence()
        {
            var smite_target_buff = library.CopyAndAdd<BlueprintBuff>("b6570b8cbb32eaf4ca8255d0ec3310b0", "SmiteImpudenceBuff", "");
            smite_target_buff.RemoveComponents<ACBonusAgainstTarget>();
            var smite_target_buff2 = library.CopyAndAdd<BlueprintBuff>(smite_target_buff, "SmiteImpudence2Buff", "");
            smite_target_buff2.SetNameDescriptionIcon(personal_champion);

            var aura_of_glory_buff = library.CopyAndAdd<BlueprintBuff>("ac3c66782859eb84692a8782320ffd2c", "AuaOfGloryBuff", "");
            aura_of_glory_buff.RemoveComponents<ACBonusAgainstTarget>();
            var aura_of_glory_buff2 = library.CopyAndAdd<BlueprintBuff>(aura_of_glory_buff, "AuraOfGlory2Buff", "");
            aura_of_glory_buff2.SetNameDescriptionIcon(personal_champion);

            var hp_buff = Helpers.CreateBuff("SmiteImpudenceHpBuff",
                                                "",
                                                "",
                                                "",
                                                null,
                                                null,
                                                Helpers.Create<TemporaryHitPointsFromAbilityValue>(t => 
                                                { t.Descriptor = ModifierDescriptor.UntypedStackable;
                                                  t.RemoveWhenHitPointsEnd = true;
                                                  t.Value = Helpers.CreateContextValue(AbilityRankType.Default);
                                                }),
                                                Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray())
                                                );

            hp_buff.SetBuffFlags(BuffFlags.HiddenInUi);

            var smite_impudence_ability = library.CopyAndAdd<BlueprintAbility>("7bb9eb2042e67bf489ccd1374423cdec", "SmiteImpudenceAbility", "");
            smite_impudence_ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            smite_impudence_ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(smite_target_buff, false),
                                                                                                Helpers.Create<InsinuatorMechanics.ContextConditionNonOutsiderAlignment>()
                                                                                               },
                                                                               new GameAction[] { Common.createContextActionApplyBuff(smite_target_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                                                                  Common.createContextActionApplyBuffToCaster(hp_buff, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true),
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(smite_target_buff2, Helpers.CreateContextDuration(), dispellable: false, is_permanent: true) )
                                                                                                }
                                                                               )
                                                     );
            }
            );
            smite_impudence_ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            smite_impudence_ability.SetNameDescriptionIcon("Smite Impudence",
                                                   "Once per day, an insinuator can beseech the forces empowering him to punish their shared enemies. As a swift action, the insinuator chooses one target within sight to smite.\n"
                                                   + "An insinuator cannot use smite against a target that shares an alignment with the outsider he has invoked for the day. The insinuator adds his Charisma bonus on his attack rolls and half his insinuator level on all damage rolls made against the target of his smite.\n"
                                                   + "Regardless of the target, the smite attack automatically bypasses any damage reduction the creature might possess. In addition, each time the insinuator declares a smite, he gains a number of temporary hit points equal to his antipaladin level.\n"
                                                   + "The smite effect remains until the target is defeated or the next time the insinuator rests and regains his uses of this ability.\n"
                                                   + "At 4th level and at every 3 levels thereafter, the insinuator can use smite one additional time per day, to a maximum of seven times per day at 19th level.",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteImpudence.png")
                                                   );

            var config_dmg = smite_impudence_ability.GetComponents<ContextRankConfig>().Where(c => c.IsBasedOnClassLevel).FirstOrDefault();
            smite_impudence_ability.ReplaceComponent(config_dmg, config_dmg.CreateCopy(c => Helpers.SetField(c, "m_Progression", ContextRankProgression.Div2)));


            smite_impudence = Common.AbilityToFeature(smite_impudence_ability, false);
            smite_impudence.AddComponent(smite_resource.CreateAddAbilityResource());

            smite_impudence_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteImpudenceAdditionalUse", "");
            smite_impudence_extra_use.SetNameDescriptionIcon("Smite Impudence — Additional Use", smite_impudence.Description, smite_impudence.Icon);




            var aura_of_glory_ability = library.CopyAndAdd<BlueprintAbility>("7a4f0c48829952e47bb1fd1e4e9da83a", "AuraOfGloryAbility", "");
            aura_of_glory_ability.ReplaceComponent<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil);
            aura_of_glory_ability.ReplaceComponent<AbilityEffectRunAction>(a =>
            {
                a.Actions = Helpers.CreateActionList(Helpers.CreateConditional(new Condition[] {Common.createContextConditionHasFact(aura_of_glory_buff, false),
                                                                                                Helpers.Create<InsinuatorMechanics.ContextConditionNonOutsiderAlignment>()
                                                                                               },
                                                                               new GameAction[] { Common.createContextActionApplyBuff(smite_target_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                  Common.createContextActionApplyBuff(aura_of_glory_buff,Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false),
                                                                                                  Helpers.Create<TeamworkMechanics.ContextActionOnUnitsWithinRadius>(c =>
                                                                                                  {
                                                                                                      c.around_caster = true;
                                                                                                      c.ignore_target = false;
                                                                                                      c.Radius = 13.Feet();
                                                                                                      c.actions = Helpers.CreateActionList(Helpers.CreateConditional(Helpers.Create<ContextConditionIsAlly>(),
                                                                                                                                                                     Common.createContextActionApplyBuff(hp_buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)
                                                                                                                                                                    )
                                                                                                                                          );
                                                                                                  }
                                                                                                  ),                       
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(smite_target_buff2, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                                                                  Helpers.CreateConditional(Common.createContextConditionCasterHasFact(personal_champion),
                                                                                                                            Common.createContextActionApplyBuff(aura_of_glory_buff2, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false) )
                                                                                                }
                                                                               )
                                                     );
            }
            );
            aura_of_glory_ability.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterInvokedOutsider>());
            aura_of_glory_ability.SetNameDescriptionIcon("Aura of Glory",
                                                   "At 11th level, an insinuator can expend two uses of his smite impudence ability to grant the ability to smite impudence to all allies within 10 feet, using his bonuses. Allies must use this smite impudence ability before the start of the insinuator’s next turn, and the bonuses last for 1 minute. Using this ability is a free action.",
                                                   Helpers.GetIcon("7a4f0c48829952e47bb1fd1e4e9da83a")//aura of justice
                                                   );
            aura_of_glory = Common.AbilityToFeature(aura_of_glory_ability, false);           
        }


        static void createInvocation()
        {
            invocation = Helpers.CreateFeature("InsinuatorInvocationFeature",
                                               "Invocation",
                                               "At the start of each day, an insinuator can meditate to contact and barter with an outsider to empower him for a day. An insinuator can freely invoke an outsider of his own alignment. He can instead invoke an outsider within one step of his own alignment by succeeding at a Diplomacy or Knowledge (religion) skill check (DC = 15 + the insinuator’s antipaladin level). While invoking the power of an outsider, the insinuator radiates an alignment aura that matches that of the outsider’s, and becomes vulnerable to alignment-based effects that target that outsider’s alignment (such as smite evil or chaos hammer). None of an insinuator’s supernatural or spell-like class abilities function unless he has invoked the power of an outsider, and the alignment of the being invoked may affect how some abilities function.",
                                               "",
                                               Helpers.GetIcon("d3a4cb7be97a6694290f0dcfbd147113"),
                                               FeatureGroup.None
                                               );

            aura_of_ego = Helpers.CreateFeature("AuraOfEgoFeature",
                                               "Aura of Ego",
                                               "At 3rd level, an insinuator radiates an aura that bolsters allies and deters enemies. Each ally within 10 feet gains a +2 morale bonus on saving throws against fear effects. Enemies within 10 feet take a –2 penalty on saving throws against fear effects. This ability functions only while the insinuator is conscious, not if he is unconscious or dead.",
                                               "",
                                               Helpers.GetIcon("e45ab30f49215054e83b4ea12165409f"), //aura of courage
                                               FeatureGroup.None
                                               );

            var aura_of_ego_enemy_buff = Helpers.CreateBuff("AuraOfEgoEnemyBuff",
                                                            aura_of_ego.Name,
                                                            aura_of_ego.Description,
                                                            "",
                                                            aura_of_cowardice.Icon,
                                                            null,
                                                            Common.createSavingThrowBonusAgainstDescriptor(-2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                            );

            var aura_of_ego_ally_buff = Helpers.CreateBuff("AuraOfEgoAllyBuff",
                                                            aura_of_ego.Name,
                                                            aura_of_ego.Description,
                                                            "",
                                                            aura_of_ego.Icon,
                                                            null,
                                                            Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.Morale, SpellDescriptor.Fear | SpellDescriptor.Shaken)
                                                            );
            var aura_of_ego_enemy = Common.createAuraEffectBuff(aura_of_ego_enemy_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()));
            var aura_of_ego_ally = Common.createAuraEffectBuff(aura_of_ego_ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));

            aura_of_ambition = Helpers.CreateFeature("AuraOfAmbitionFeature",
                                                   "Aura of Ambition",
                                                   "At 8th level, enemies within 10 feet of an insinuator take a –1 penalty on all saving throws. All allies within 10 feet gain a +1 bonus on all saving throws.\n"
                                                   + "This penalty does not stack with the penalty from aura of ego. This ability functions only while the insinuator is conscious, not if he is unconscious or dead.",
                                                   "",
                                                   LoadIcons.Image2Sprite.Create(@"AbilityIcons/AuraOfAmbition.png"),
                                                   FeatureGroup.None
                                                   );

            var aura_of_ambition_enemy_buff = Helpers.CreateBuff("AuraOfAmbitionEnemyBuff",
                                                aura_of_ambition.Name,
                                                aura_of_ambition.Description,
                                                "",
                                                aura_of_despair.Icon,
                                                null,
                                                Common.createSavingThrowBonusAgainstDescriptor(1, ModifierDescriptor.UntypedStackable, SpellDescriptor.Fear | SpellDescriptor.Shaken),
                                                Helpers.Create<BuffAllSavesBonus>(b => { b.Value = -1; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                                );

            var aura_of_ambition_ally_buff = Helpers.CreateBuff("AuraOfAmbitionAllyBuff",
                                                            aura_of_ambition.Name,
                                                            aura_of_ambition.Description,
                                                            "",
                                                            aura_of_ambition.Icon,
                                                            null,
                                                            Helpers.Create<BuffAllSavesBonus>(b => { b.Value = 1; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                                            );
            var aura_of_ambition_enemy = Common.createAuraEffectBuff(aura_of_ambition_enemy_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>()));
            var aura_of_ambition_ally = Common.createAuraEffectBuff(aura_of_ambition_ally_buff, 13.Feet(), Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsAlly>()));


            aura_of_belief = Helpers.CreateFeature("AuraOfBeliefFeature",
                                                   "Aura of Belief",
                                                   "At 14th level, an insinuator’s weapons are treated as chaos-aligned while he invokes a chaotic outsider, law-aligned when he invokes a lawful outsider, or evil-aligned while he invokes an evil outsider.",
                                                   "",
                                                   Helpers.GetIcon("90e59f4a4ada87243b7b3535a06d0638"), //bless
                                                   FeatureGroup.None
                                                   );

            aura_of_indomitability = Helpers.CreateFeature("AuraOfIndomitabilityFeature",
                                                           "Aura of Indomitability",
                                                           "At 17th level, an insinuator gains DR 10 that is bypassed by the alignment opposite of the outsider he has invoked for the day, or DR 5/— while invoking a neutral outsider.",
                                                           "",
                                                           Helpers.GetIcon("2a6a2f8e492ab174eb3f01acf5b7c90a"), //defensive stance
                                                           FeatureGroup.None
                                                           );



            personal_champion = Helpers.CreateFeature("PersonalChampionFeature",
                                                       "Personal Champion",
                                                       "At 20th level, an insinuator becomes a living embodiment of his selfish desires. His damage reduction from aura of indomitability increases to 15 (or 10 while invoking a neutral outsider). Whenever he uses smite impudence, he adds twice his full Charisma bonus to the attack roll and doubles his effective bonus damage gained from the smite. In addition, he can invoke a new outsider patron by meditating again.",
                                                       "",
                                                       Helpers.GetIcon("5ab0d42fb68c9e34abae4921822b9d63"), //heroism
                                                       FeatureGroup.None
                                                       );

            var alignments_map = new Dictionary<AlignmentMaskType, AlignmentMaskType>
            {
                {AlignmentMaskType.LawfulEvil, AlignmentMaskType.NeutralEvil | AlignmentMaskType.LawfulEvil },
                {AlignmentMaskType.NeutralEvil, AlignmentMaskType.Evil },
                {AlignmentMaskType.ChaoticEvil, AlignmentMaskType.NeutralEvil | AlignmentMaskType.ChaoticEvil },
                {AlignmentMaskType.LawfulNeutral,  AlignmentMaskType.LawfulEvil },
                {AlignmentMaskType.TrueNeutral,  AlignmentMaskType.NeutralEvil },
                {AlignmentMaskType.ChaoticNeutral, AlignmentMaskType.ChaoticEvil }
            };

            List<BlueprintAbility> abilities = new List<BlueprintAbility>();
            List<BlueprintBuff> buffs = new List<BlueprintBuff>();
            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);

            var cooldown_buff = Helpers.CreateBuff("InvocationCooldownBuff",
                                                   "Invocation Cooldown",
                                                   "Insinuator is no longer able to contact outsider to barter for power.",
                                                   "",
                                                   invocation.Icon,
                                                   null
                                                   );
            cooldown_buff.SetBuffFlags(BuffFlags.RemoveOnRest | BuffFlags.HiddenInUi);

            foreach (var kv in alignments_map)
            {
                var buff = Helpers.CreateBuff(kv.Key.ToString() + "InvocationBuff",
                                              invocation.Name + ": " + UIUtility.GetAlignmentText(kv.Key),
                                              invocation.Description,
                                              "",
                                              invocation.Icon,
                                              null,
                                              Helpers.Create<InsinuatorMechanics.InsinuatorOutsiderAlignment>(i => i.alignment = kv.Key)
                                              );
                buff.SetBuffFlags(BuffFlags.RemoveOnRest);

                remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);

                var apply_buff = Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false);
                var ability = Helpers.CreateAbility(kv.Key.ToString() + "InvocationAbility",
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Extraordinary,
                                                    CommandType.Standard,
                                                    AbilityRange.Personal,
                                                    "",
                                                    "",
                                                    Helpers.CreateRunActions(remove_buffs,
                                                                             Helpers.CreateConditional(Helpers.Create<NewMechanics.ContextConditionAlignmentStrict>(c => c.Alignment = kv.Key),
                                                                                                       apply_buff,
                                                                                                       Helpers.Create<SkillMechanics.ContextActionCasterSkillCheck>(c =>
                                                                                                       {
                                                                                                           c.CustomDC = Helpers.CreateContextValue(AbilityRankType.Default);
                                                                                                           c.Success = Helpers.CreateActionList(apply_buff);
                                                                                                           c.Stat = StatType.CheckDiplomacy;
                                                                                                       })
                                                                                                       ),
                                                                             Helpers.CreateConditional(Common.createContextConditionHasFact(personal_champion, false),
                                                                                                       Common.createContextActionApplyBuff(cooldown_buff, Helpers.CreateContextDuration(), is_permanent: true, dispellable: false)
                                                                                                       )
                                                                             ),
                                                    Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, ContextRankProgression.BonusValue, stepLevel: 15,
                                                                                    classes: getAntipaladinArray()
                                                                                    ),
                                                    Common.createAbilitySpawnFx("c4d861e816edd6f4eab73c55a18fdadd", anchor: AbilitySpawnFxAnchor.SelectedTarget),
                                                    Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = kv.Value),
                                                    Helpers.Create<NewMechanics.AbilityShowIfCasterHasAlignment>(a => a.alignment = kv.Value),
                                                    Common.createAbilityCasterHasNoFacts(cooldown_buff)
                                                    );
                Common.setAsFullRoundAction(ability);
                ability.setMiscAbilityParametersSelfOnly();
                abilities.Add(ability);

                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ego_ally, aura_of_ego);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ego_enemy, aura_of_ego);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ambition_ally, aura_of_ambition);
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_ambition_enemy, aura_of_ambition);

                if (kv.Key != AlignmentMaskType.TrueNeutral)
                {
                    var aura_of_belief_buff = Helpers.CreateBuff(kv.Key.ToString() + "AuraOfBeliefBuff",
                                                     aura_of_belief.Name,
                                                     aura_of_belief.Description,
                                                     "",
                                                     aura_of_belief.Icon,
                                                     null,
                                                     Common.createAddOutgoingAlignmentFromAlignment(kv.Key)
                                                     );
                    Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_belief_buff, aura_of_belief);
                }
                var aura_of_indomitability_buff = Helpers.CreateBuff(kv.Key.ToString() + "AuraOfIndomitabilityBuff",
                                                                     aura_of_indomitability.Name,
                                                                     aura_of_indomitability.Description,
                                                                     "",
                                                                     aura_of_indomitability.Icon,
                                                                     null,
                                                                     Common.createContextDRFromAlignment(Helpers.CreateContextValue(AbilityRankType.Default), kv.Key),
                                                                     Helpers.CreateContextRankConfig(ContextRankBaseValueType.FeatureListRanks, ContextRankProgression.MultiplyByModifier, stepLevel: 5,
                                                                                                     featureList: Enumerable.Repeat(invocation, kv.Key == AlignmentMaskType.TrueNeutral ? 1 : 2).ToArray().AddToArray(personal_champion)
                                                                                                     )
                                                                     );
                Common.addContextActionApplyBuffOnFactsToActivatedAbilityBuffNoRemove(buff, aura_of_indomitability_buff, aura_of_indomitability);
            }
        }


        static void fixAntipaladinFeats()
        {
            extra_touch_of_corruption = library.CopyAndAdd<BlueprintFeature>("a2b2f20dfb4d3ed40b9198e22be82030", "ExtraTouchOfCorruption", "");
            extra_touch_of_corruption.ReplaceComponent<PrerequisiteFeature>(p => p.Feature = touch_of_corruption);
            extra_touch_of_corruption.SetNameDescription("Extra Touch of Corruption",
                                                         "You can use your touch of corruption two additional times per day.\nSpecial: You can gain Extra Touch of Corruption multiple times.Its effects stack.");
            library.AddFeats(extra_touch_of_corruption);
        }


        public static BlueprintCharacterClass[] getAntipaladinArray()
        {
            return new BlueprintCharacterClass[] { antipaladin_class };
        }

        static void createAntipaladinProgression()
        {
            antipaladin_proficiencies = library.CopyAndAdd<BlueprintFeature>("b10ff88c03308b649b50c31611c2fefb", "AntipaladinProficiencies", "");
            antipaladin_proficiencies.SetNameDescription("Antipaladin Proficiencies",
                                                         "Antipaladins are proficient with all simple and martial weapons, with all types of armor (heavy, medium, and light), and with shields (except tower shields).");
            unholy_resilence = library.CopyAndAdd<BlueprintFeature>("8a5b5e272e5c34e41aa8b4facbb746d3", "UnholyResilence", ""); //from divine grace
            unholy_resilence.SetNameDescription("Unholy Resilience",
                                                "At 2nd level, an antipaladin gains a bonus equal to his Charisma bonus (if any) on all saving throws.");

            plague_bringer = library.CopyAndAdd<BlueprintFeature>("41d1d0de15e672349bf4262a5acf06ce", "PlagueBearer", ""); //from divine health
            plague_bringer.SetNameDescription("Plague Bringer",
                                              "At 3rd level, the powers of darkness make an antipaladin a beacon of corruption and disease. An antipaladin does not take any damage or take any penalty from diseases. He can still contract diseases and spread them to others, but he is otherwise immune to their effects.");
            plague_bringer.ComponentsArray = new BlueprintComponent[] { Helpers.Create<SuppressBuffs>(s => s.Descriptor = SpellDescriptor.Disease) };

            antipaladin_alignment = library.CopyAndAdd<BlueprintFeature>("f8c91c0135d5fc3458fcc131c4b77e96", "AntipaladinAlignmentRestriction", "");
            antipaladin_alignment.SetIcon(NewSpells.aura_of_doom.Icon);
            antipaladin_alignment.SetDescription("An antipaladin who ceases to be evil loses all antipaladin spells and class features. She cannot thereafter gain levels as an antipaladin until she changes the alignment back.");
            antipaladin_alignment.ReplaceComponent<ForbidSpellbookOnAlignmentDeviation>(f =>
            {
                f.Alignment = AlignmentMaskType.Evil;
                f.Spellbooks = new BlueprintSpellbook[] { antipaladin_class.Spellbook };
            });

            createAntipaladinDeitySelection();
            createSmiteGood();
            createTouchOfCorruption();
            createChannelEnergy();
            createAuras();
            createFiendishBoon();

            antipaladin_progression = Helpers.CreateProgression("AntpladinProgression",
                                                              antipaladin_class.Name,
                                                              antipaladin_class.Description,
                                                              "",
                                                              antipaladin_class.Icon,
                                                              FeatureGroup.None);

            antipaladin_progression.Classes = getAntipaladinArray();



            antipaladin_progression.LevelEntries = new LevelEntry[] {Helpers.LevelEntry(1, antipaladin_proficiencies, antipaladin_deity, smite_good, antipaladin_alignment,
                                                                                        library.Get<BlueprintFeature>("d3e6275cfa6e7a04b9213b7b292a011c"), // ray calculate feature
                                                                                        library.Get<BlueprintFeature>("62ef1cdb90f1d654d996556669caf7fa")), // touch calculate feature                                                                                      
                                                                    Helpers.LevelEntry(2, touch_of_corruption, unholy_resilence),
                                                                    Helpers.LevelEntry(3, aura_of_cowardice, plague_bringer, cruelty),
                                                                    Helpers.LevelEntry(4, smite_good_extra_use, channel_negative_energy),
                                                                    Helpers.LevelEntry(5, fiendish_boon[0]),
                                                                    Helpers.LevelEntry(6, cruelty),
                                                                    Helpers.LevelEntry(7, smite_good_extra_use),
                                                                    Helpers.LevelEntry(8, aura_of_despair, fiendish_boon[1]),
                                                                    Helpers.LevelEntry(9, cruelty),
                                                                    Helpers.LevelEntry(10, smite_good_extra_use),
                                                                    Helpers.LevelEntry(11, aura_of_vengeance, fiendish_boon[2]),
                                                                    Helpers.LevelEntry(12, cruelty),
                                                                    Helpers.LevelEntry(13, smite_good_extra_use),
                                                                    Helpers.LevelEntry(14, aura_of_sin, fiendish_boon[3]),
                                                                    Helpers.LevelEntry(15, cruelty),
                                                                    Helpers.LevelEntry(16, smite_good_extra_use),
                                                                    Helpers.LevelEntry(17, aura_of_deparvity, fiendish_boon[4]),
                                                                    Helpers.LevelEntry(18, cruelty),
                                                                    Helpers.LevelEntry(19, smite_good_extra_use),
                                                                    Helpers.LevelEntry(20, tip_of_spear, fiendish_boon[5])
                                                                    };

            antipaladin_progression.UIDeterminatorsGroup = new BlueprintFeatureBase[] { antipaladin_proficiencies, antipaladin_deity, antipaladin_alignment};
            antipaladin_progression.UIGroups = new UIGroup[]  {Helpers.CreateUIGroup(fiendish_boon.AddToArray(unholy_resilence, plague_bringer)),
                                                                Helpers.CreateUIGroup(smite_good, smite_good_extra_use),
                                                                Helpers.CreateUIGroup(aura_of_cowardice, aura_of_despair, aura_of_vengeance, aura_of_sin, aura_of_deparvity, tip_of_spear),
                                                                Helpers.CreateUIGroup(touch_of_corruption, channel_negative_energy, cruelty),
                                                           };
        }


        static void createAuras()
        {
            var cowardice_buff = Helpers.CreateBuff("AuraOfCowardiceEffectBuff",
                                                    "Aura of Cowardice",
                                                    "At 3rd level, an antipaladin radiates a palpably daunting aura that causes all enemies within 10 feet to take a –4 penalty on saving throws against fear effects. Creatures that are normally immune to fear lose that immunity while within 10 feet of an antipaladin with this ability. This ability functions only while the antipaladin remains conscious, not if he is unconscious or dead.",
                                                    "",
                                                    Helpers.GetIcon("08cb5f4c3b2695e44971bf5c45205df0"),
                                                    null,
                                                    Common.createSavingThrowBonusAgainstDescriptor(-4, ModifierDescriptor.UntypedStackable, SpellDescriptor.Shaken | SpellDescriptor.Fear)
                                                    );

            aura_of_cowardice = Common.createAuraEffectFeature(cowardice_buff.Name,
                                                               cowardice_buff.Description,
                                                               cowardice_buff.Icon,
                                                               cowardice_buff,
                                                               13.Feet(),
                                                               Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                               );

            var despair_buff = Helpers.CreateBuff("AuraOfDespairEffectBuff",
                                        "Aura of Despair",
                                        "At 8th level, enemies within 10 feet of an antipaladin take a –2 penalty on all saving throws. This penalty does not stack with the penalty from aura of cowardice.\nThis ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                        "",
                                        Helpers.GetIcon("4baf4109145de4345861fe0f2209d903"), //crushing despair
                                        null,
                                        Common.createSavingThrowBonusAgainstDescriptor(2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Shaken | SpellDescriptor.Fear),
                                        Helpers.Create<BuffAllSavesBonus>(b => { b.Value = -2; b.Descriptor = ModifierDescriptor.UntypedStackable; })
                                        );

            aura_of_despair = Common.createAuraEffectFeature(despair_buff.Name,
                                                             despair_buff.Description,
                                                             despair_buff.Icon,
                                                             despair_buff,
                                                             13.Feet(),
                                                             Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                             );

            var deparvity_buff = Helpers.CreateBuff("AuraOfDeparvityEffectBuff",
                                                    "Aura of Deparvity",
                                                    "At 17th level, an antipaladin gains DR 5/good. Each enemy within 10 feet takes a –4 penalty on saving throws against compulsion effects. This ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                                    "",
                                                    Helpers.GetIcon("41cf93453b027b94886901dbfc680cb9"), //overwhelming presence
                                                    null,
                                                    Common.createSavingThrowBonusAgainstDescriptor(-2, ModifierDescriptor.UntypedStackable, SpellDescriptor.Compulsion)
                                                    );

            aura_of_deparvity = Common.createAuraEffectFeature(deparvity_buff.Name,
                                                             deparvity_buff.Description,
                                                             deparvity_buff.Icon,
                                                             deparvity_buff,
                                                             13.Feet(),
                                                             Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                             );

            aura_of_deparvity.AddComponent(Common.createAlignmentDR(5, DamageAlignment.Good));

            var sin_buff = Helpers.CreateBuff("AuraOfSinEffectBuff",
                                        "Aura of Sin",
                                        "At 14th level, an antipaladin’s weapons are treated as evil-aligned for the purposes of overcoming damage reduction. Any attack made against an enemy within 10 feet of him is treated as evil-aligned for the purposes of overcoming damage reduction. This ability functions only while the antipaladin is conscious, not if he is unconscious or dead.",
                                        "",
                                        Helpers.GetIcon("8bc64d869456b004b9db255cdd1ea734"), //bane
                                        null,
                                        library.Get<BlueprintBuff>("f84a39e55230f5e499588c5cd19548cd").GetComponent<AddIncomingDamageWeaponProperty>().CreateCopy(a => a.Alignment = DamageAlignment.Evil)
                                        );

            aura_of_sin = Common.createAuraEffectFeature(sin_buff.Name,
                                                         sin_buff.Description,
                                                         sin_buff.Icon,
                                                         sin_buff,
                                                         13.Feet(),
                                                         Helpers.CreateConditionsCheckerAnd(Helpers.Create<ContextConditionIsEnemy>())
                                                         );
            aura_of_sin.AddComponent(library.Get<BlueprintFeature>("0437f4af5ad49b544bccf48aa7a51319").GetComponent<AddOutgoingPhysicalDamageProperty>().CreateCopy(a => a.Alignment = DamageAlignment.Evil));
        }


        static void createFiendishBoon()
        {
            fiendish_boon_resource = Helpers.CreateAbilityResource("FiendishBoonResource", "", "", "", null);
            fiendish_boon_resource.SetIncreasedByLevelStartPlusDivStep(1, 9, 1, 4, 1, 0, 0.0f, getAntipaladinArray());

            var divine_weapon = library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4");
            var enchants = WeaponEnchantments.temporary_enchants;

            var enhancement_buff = Helpers.CreateBuff("FiendishBondEnchancementBaseBuff",
                                         "",
                                         "",
                                         "",
                                         null,
                                         null,
                                         Common.createBuffRemainingGroupsSizeEnchantPrimaryHandWeapon(ActivatableAbilityGroup.DivineWeaponProperty,
                                                                                                      false, true,
                                                                                                      enchants
                                                                                                      )
                                         );
            var fiendish_boon_enhancement_buff = Helpers.CreateBuff("FiendishBondEnchancementSwitchBuff",
                                                                 "Fiendish Boon",
                                                                 "Upon reaching 5th level, an antipaladin forms a divine bond with her weapon. As a standard action, she can call upon the aid of a fiendish spirit for 1 minute per antipaladin level.\nAt 5th level, this spirit grants the weapon a +1 enhancement bonus. For every three levels beyond 5th, the weapon gains another +1 enhancement bonus, to a maximum of +6 at 20th level. These bonuses can be added to the weapon, stacking with existing weapon bonuses to a maximum of +5.\nAlternatively, they can be used to add any of the following weapon properties: anarchic, axiomatic, flaming, keen, speed, unholy, vicious and vorpal. Adding these properties consumes an amount of bonus equal to the property's cost. These bonuses are added to any properties the weapon already has, but duplicate abilities do not stack.\nAn antipaladin can use this ability once per day at 5th level, and one additional time per day for every four levels beyond 5th, to a total of four times per day at 17th level.",
                                                                 "",
                                                                 NewSpells.magic_weapon_greater.Icon,
                                                                 null,
                                                                 Helpers.CreateAddFactContextActions(activated: Common.createContextActionApplyBuff(enhancement_buff, Helpers.CreateContextDuration(),
                                                                                                                is_child: true, is_permanent: true, dispellable: false)
                                                                                                     )
                                                                 );
            enhancement_buff.SetBuffFlags(BuffFlags.HiddenInUi);
            var vicious_enchant = library.Get<BlueprintWeaponEnchantment>("a1455a289da208144981e4b1ef92cc56");
            var vicious = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementVicious",
                                                                        "Fiendish Boon - Vicious",
                                                                        "An antipaladin can add the vicious property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\n" + vicious_enchant.Description,
                                                                        library.Get<BlueprintActivatableAbility>("8c714fbd564461e4588330aeed2fbe1d").Icon, //disruption
                                                                        fiendish_boon_enhancement_buff,
                                                                        vicious_enchant,
                                                                        1, ActivatableAbilityGroup.DivineWeaponProperty);

            var vorpal = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementVorpal",
                                                            "Fiendish Boon - Vorpal",
                                                            "An antipaladin can add the vorpal property to a weapon enhanced with her fiendish boon, but this consumes 5 points of enhancement bonus granted to this weapon.\n" + WeaponEnchantments.vorpal.Description,
                                                            Helpers.GetIcon("2c38da66e5a599347ac95b3294acbe00"), //true strike
                                                            fiendish_boon_enhancement_buff,
                                                            WeaponEnchantments.vorpal,
                                                            5, ActivatableAbilityGroup.DivineWeaponProperty);

            var speed_enchant = library.Get<BlueprintWeaponEnchantment>("f1c0c50108025d546b2554674ea1c006");
            var speed = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementSpeed",
                                                "Fiendish Boon - Speed",
                                                "An antipaladin can add the speed property to a weapon enhanced with her fiendish boon, but this consumes 3 points of enhancement bonus granted to this weapon.\n" + speed_enchant.Description,
                                                library.Get<BlueprintActivatableAbility>("ed1ef581af9d9014fa1386216b31cdae").Icon, //disruption
                                                fiendish_boon_enhancement_buff,
                                                speed_enchant,
                                                3, ActivatableAbilityGroup.DivineWeaponProperty);

            var flaming = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementFlaming",
                                                                "Fiendish Boon - Flaming",
                                                                "An antipaladin can add the flaming property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\nA flaming weapon is sheathed in fire that deals an extra 1d6 points of fire damage on a successful hit. The fire does not harm the wielder.",
                                                                library.Get<BlueprintActivatableAbility>("7902941ef70a0dc44bcfc174d6193386").Icon,
                                                                fiendish_boon_enhancement_buff,
                                                                library.Get<BlueprintWeaponEnchantment>("30f90becaaac51f41bf56641966c4121"),
                                                                1, ActivatableAbilityGroup.DivineWeaponProperty);

            var keen = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementKeen",
                                                            "Fiendish Boon - Keen",
                                                            "An antipaladin can add the keen property to a weapon enhanced with her fiendish boon, but this consumes 1 point of enhancement bonus granted to this weapon.\nThe keen property doubles the threat range of a weapon. This benefit doesn't stack with any other effects that expand the threat range of a weapon (such as the keen edge spell or the Improved Critical feat).",
                                                            library.Get<BlueprintActivatableAbility>("27d76f1afda08a64d897cc81201b5218").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("102a9c8c9b7a75e4fb5844e79deaf4c0"),
                                                            1, ActivatableAbilityGroup.DivineWeaponProperty);

            var unholy = Common.createEnchantmentAbility("FiendishBoonWeaponEnchancementUnholy",
                                                            "Fiendish Boon - Unholy",
                                                            "An antipaladin can add the unholy property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn unholy weapon is imbued with unholy power. This power makes the weapon evil-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of good alignment.",
                                                            library.Get<BlueprintActivatableAbility>("561803a819460f34ea1fe079edabecce").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("d05753b8df780fc4bb55b318f06af453"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Evil);

            var axiomatic = Common.createEnchantmentAbility("FiendishBoonEnchancementAxiomatic",
                                                            "Fiendish Boon - Axiomatic",
                                                            "An antipaladin can add the axiomatic property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn axiomatic weapon is infused with lawful power. It makes the weapon lawful-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against chaotic creatures.",
                                                            library.Get<BlueprintActivatableAbility>("d76e8a80ab14ac942b6a9b8aaa5860b1").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("0ca43051edefcad4b9b2240aa36dc8d4"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Lawful);

            var anarchic = Common.createEnchantmentAbility("FiendishBoonEnchancementAnarchic",
                                                            "Fiendish Boon - Anarchic",
                                                            "An antipaladin can add the anarchic property to a weapon enhanced with her fiendish boon, but this consumes 2 points of enhancement bonus granted to this weapon.\nAn anarchic weapon is infused with the power of chaos. It makes the weapon chaotic-aligned and thus overcomes the corresponding damage reduction. It deals an extra 2d6 points of damage against all creatures of lawful alignment.",
                                                            library.Get<BlueprintActivatableAbility>("8ed07b0cc56223c46953348f849f3309").Icon,
                                                            fiendish_boon_enhancement_buff,
                                                            library.Get<BlueprintWeaponEnchantment>("57315bc1e1f62a741be0efde688087e9"),
                                                            2, ActivatableAbilityGroup.DivineWeaponProperty,
                                                            AlignmentMaskType.Chaotic);

           
            var ability = Helpers.CreateAbility("FiendishBoonSwitchAbility",
                                                 fiendish_boon_enhancement_buff.Name,
                                                 fiendish_boon_enhancement_buff.Description,
                                                 "",
                                                 fiendish_boon_enhancement_buff.Icon,
                                                 AbilityType.Supernatural,
                                                 CommandType.Standard,
                                                 AbilityRange.Personal,
                                                 Helpers.minutesPerLevelDuration,
                                                 "",
                                                 Helpers.CreateRunActions(Common.createContextActionApplyBuff(fiendish_boon_enhancement_buff, Helpers.CreateContextDuration(Helpers.CreateContextValue(AbilityRankType.Default), DurationRate.Minutes), dispellable: false)),
                                                 Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil),
                                                 fiendish_boon_resource.CreateResourceLogic(),
                                                 Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>()
                                                 );
            ability.setMiscAbilityParametersSelfOnly(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.EnchantWeapon);
            ability.NeedEquipWeapons = true;
            ability.AddComponents(library.Get<BlueprintAbility>("7ff088ab58c69854b82ea95c2b0e35b4").GetComponent<AbilitySpawnFx>());

            fiendish_boon = new BlueprintFeature[6];
            fiendish_boon[0] = Helpers.CreateFeature("FiendishBoonWeaponEnchancementFeature",
                                                    "Fiendish Boon +1",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Helpers.CreateAddAbilityResource(fiendish_boon_resource),
                                                    Helpers.CreateAddFacts(ability, flaming, keen, vicious)
                                                    );

            fiendish_boon[1] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement2Feature",
                                                    "Fiendish Boon +2",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(anarchic, axiomatic, unholy)
                                                    );

            fiendish_boon[2] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement3Feature",
                                                    "Fiendish Boon +3",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(speed)
                                                    );

            fiendish_boon[3] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement4Feature",
                                                    "Fiendish Boon +4",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty)
                                                    );

            fiendish_boon[4] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement5Feature",
                                                    "Fiendish Boon +5",
                                                    fiendish_boon_enhancement_buff.Description,
                                                    "",
                                                    fiendish_boon_enhancement_buff.Icon,
                                                    FeatureGroup.None,
                                                    Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty),
                                                    Helpers.CreateAddFacts(vorpal)
                                                    );

            fiendish_boon[5] = Helpers.CreateFeature("FiendishBoonWeaponEnchancement6Feature",
                                                        "Fiendish Boon +6",
                                                        fiendish_boon_enhancement_buff.Description,
                                                        "",
                                                        fiendish_boon_enhancement_buff.Icon,
                                                        FeatureGroup.None,
                                                        Common.createIncreaseActivatableAbilityGroupSize(ActivatableAbilityGroup.DivineWeaponProperty)
                                                        );
        }


        static void createTouchOfCorruption()
        {
            var bestow_curse = library.Get<BlueprintAbility>("989ab5c44240907489aba0a8568d0603");
            var contagion = library.Get<BlueprintAbility>("48e2744846ed04b4580be1a3343a5d3d");

            var fatigued = library.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            var shaken = library.Get<BlueprintBuff>("25ec6cb6ab1845c48a95f9c20b034220");
            var sickened = library.Get<BlueprintBuff>("4e42460798665fd4cb9173ffa7ada323");
            //6
            var dazed = Common.dazed_non_mind_affecting;
            var staggered = library.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
            var diseased = contagion.Variants.Select(b => Common.extractActions<ContextActionApplyBuff>(b.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility.GetComponent<AbilityEffectRunAction>().Actions.Actions).First().Buff).ToArray();
            //9
            //cursed
            var cursed = bestow_curse.Variants.Select(b => Common.extractActions<ContextActionApplyBuff>(b.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility.GetComponent<AbilityEffectRunAction>().Actions.Actions).First().Buff).ToArray();
            var exahusted = library.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");
            var frightened = library.Get<BlueprintBuff>("f08a7239aa961f34c8301518e71d4cdf");
            var nauseated = library.Get<BlueprintBuff>("956331dba5125ef48afe41875a00ca0e");
            var poisoned = library.Get<BlueprintBuff>("ba1ae42c58e228c4da28328ea6b4ae34");

            //12
            var blinded = library.Get<BlueprintBuff>("0ec36e7596a4928489d2049e1e1c76a7");
            var deafened = Common.deafened;
            var paralyzed = library.Get<BlueprintBuff>("af1e2d232ebbb334aaf25e2a46a92591");
            var stunned = library.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");

            CrueltyEntry.addCruelty("Fatigued", "The target is fatigued if Fortitude save is failed.", 0, "", 0, SavingThrowType.Fortitude, fatigued);
            CrueltyEntry.addCruelty("Shaken", "The target is shaken for 1 round per level of the antipaladin if Will save is failed.", 0, "", 1, SavingThrowType.Will, shaken);
            CrueltyEntry.addCruelty("Sickened", "The target is sickened for 1 round per level of the antipaladin if Fortitude save is failed.", 0, "", 1, SavingThrowType.Fortitude, sickened);
            CrueltyEntry.addCruelty("Disiesed", "The target contracts a disease, as if the antipaladin had cast contagion, using his antipaladin level as his caster level if Fortitude save is failed.", 6, "", 0, SavingThrowType.Fortitude, diseased);
            CrueltyEntry.addCruelty("Dazed", "The target is dazed for 1 round if Will save is failed.", 6, "", -1, SavingThrowType.Will, dazed);
            CrueltyEntry.addCruelty("Staggered", "The target is staggered for 1 round per two levels of the antipaladin if Fortitude save is failed.", 6, "", 2, SavingThrowType.Fortitude, staggered);
            CrueltyEntry.addCruelty("Cursed", "The target is cursed, as if the antipaladin had cast bestow curse, using his antipaladin level as his caster level if Will save is failed.", 9, "", 0, SavingThrowType.Will, cursed);
            CrueltyEntry.addCruelty("Exhausted", "The target is exhausted if Fortitude save is failed.", 9, "Fatigued", 9, SavingThrowType.Fortitude, exahusted);
            CrueltyEntry.addCruelty("Frightened", "The target is frightened for 1 round per three levels of the antipaladin if Will save is failed.", 9, "Shaken", 2, SavingThrowType.Will, frightened);
            CrueltyEntry.addCruelty("Nauseated", "The target is nauseated for 1 round per three levels of the antipaladin if Fortitude save is failed.", 9, "Sickened", 3, SavingThrowType.Fortitude, nauseated);
            CrueltyEntry.addCruelty("Poisoned", "The target is poisoned, as if the antipaladin had cast poison, using the antipaladin’s level as the caster level if Fortitude save is failed.", 9, "", 0, SavingThrowType.Fortitude, poisoned);
            CrueltyEntry.addCruelty("Blinded", "The target is blinded for 1 round per level of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, blinded);
            CrueltyEntry.addCruelty("Deafened", "The target is deafened for 1 round per level of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, deafened);
            CrueltyEntry.addCruelty("Paralyzed", " The target is paralyzed for 1 round. if Will save is failed.", 12, "", -1, SavingThrowType.Will, paralyzed);
            CrueltyEntry.addCruelty("Stunned", "The target is stunned for 1 round per four levels of the antipaladin if Fortitude save is failed.", 12, "", 1, SavingThrowType.Fortitude, stunned);


            var paladin = library.Get<BlueprintCharacterClass>("bfa11238e7ae3544bbeb4d0b92e897ec");
            touch_of_corruption_resource = library.Get<BlueprintAbilityResource>("9dedf41d995ff4446a181f143c3db98c");
            ClassToProgression.addClassToResource(antipaladin_class, new BlueprintArchetype[0], touch_of_corruption_resource, paladin);

            var dice = Helpers.CreateContextDiceValue(DiceType.D6, Helpers.CreateContextValue(AbilityRankType.DamageDice), 0);
            var heal_action = Common.createContextActionHealTarget(dice);
            var damage_undead_action = Helpers.CreateActionDealDamage(DamageEnergyType.PositiveEnergy, dice);
            var damage_living_action = Helpers.CreateActionDealDamage(DamageEnergyType.NegativeEnergy, dice);
            var context_rank_config = Helpers.CreateContextRankConfig(baseValueType: ContextRankBaseValueType.ClassLevel, progression: ContextRankProgression.Div2,
                                                          type: AbilityRankType.DamageDice, classes: getAntipaladinArray());

            var deaths_embrace_living = library.Get<BlueprintFeature>("fd7c08ccd3c7773458eb9613db3e93ad");

            var inflict_light_wounds = library.Get<BlueprintAbility>("244a214d3b0188e4eb43d3a72108b67b");
            var ability = Helpers.CreateAbility("TouchOfCorruptionAbility",
                                                "Touch of Corruption",
                                                "Beginning at 2nd level, an antipaladin surrounds his hand with a fiendish flame, causing terrible wounds to open on those he touches. Each day he can use this ability a number of times equal to 1/2 his antipaladin level + his Charisma modifier. As a touch attack, an antipaladin can cause 1d6 points of damage for every two antipaladin levels he possesses. Using this ability is a standard action that does not provoke attacks of opportunity.\n"
                                                + "An antipaladin can also chose to channel corruption into a melee weapon by spending 2 uses of this ability as a swift action. The next enemy struck with this weapon will suffer the effects of this ability.\n"
                                                + "Alternatively, an antipaladin can use this power to heal undead creatures, restoring 1d6 hit points for every two levels the antipaladin possesses. This ability is modified by any feat, spell, or effect that specifically works with the lay on hands paladin class feature. For example, the Extra Lay On Hands feat grants an antipaladin 2 additional uses of the touch of corruption class feature.",
                                                "",
                                                Helpers.GetIcon("989ab5c44240907489aba0a8568d0603"), //bestow curse
                                                AbilityType.Supernatural,
                                                CommandType.Standard,
                                                AbilityRange.Touch,
                                                "",
                                                Helpers.savingThrowNone,
                                                Helpers.CreateRunActions(Helpers.CreateConditional(Helpers.CreateConditionsCheckerOr(Helpers.Create<UndeadMechanics.ContextConditionHasNegativeEnergyAffinity>(),
                                                                                                                                        Common.createContextConditionHasFact(deaths_embrace_living)
                                                                                                                                    ),
                                                                                                    heal_action,
                                                                                                    damage_living_action)),
                                                inflict_light_wounds.GetComponent<AbilityDeliverTouch>(),
                                                inflict_light_wounds.GetComponent<AbilitySpawnFx>(),
                                                context_rank_config,
                                                Helpers.Create<AbilityUseOnRest>(c => c.Type = AbilityUseOnRestType.HealUndead),
                                                Common.createContextCalculateAbilityParamsBasedOnClass(antipaladin_class, StatType.Charisma)
                                                );
            ability.setMiscAbilityParametersTouchHarmful();
            var ability_cast = Helpers.CreateTouchSpellCast(ability, touch_of_corruption_resource);

            ability_cast.AddComponents(Common.createAbilityTargetHasFact(true, Common.construct),
                                       Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil),
                                       Helpers.CreateResourceLogic(touch_of_corruption_resource));
            var wrapper = Common.createVariantWrapper("AntipladinCrueltyBaseAbility", "", ability_cast);

            touch_of_corruption = Common.AbilityToFeature(wrapper, false);

            cruelty = Helpers.CreateFeatureSelection("CrueltiesFeatureSelection",
                                                     "Cruelty",
                                                     "At 3rd level, and every three levels thereafter, an antipaladin can select one cruelty. Each cruelty adds an effect to the antipaladin’s touch of corruption ability. Whenever the antipaladin uses touch of corruption to deal damage to one target, the target also receives the additional effect from one of the cruelties possessed by the antipaladin. This choice is made when the touch is used. The target receives a save to avoid this cruelty. If the save is successful, the target takes the damage as normal, but not the effects of the cruelty. The DC of this save is equal to 10 + 1/2 the antipaladin’s level + the antipaladin’s Charisma modifier.",
                                                     "",
                                                     null,
                                                     FeatureGroup.None);

            cruelty.AllFeatures = CrueltyEntry.createCruelties(wrapper);


            //create channel corruption
            var channels = new List<BlueprintAbility>();
            var remove_buffs = Helpers.Create<NewMechanics.ContextActionRemoveBuffs>(c => c.Buffs = new BlueprintBuff[0]);
            foreach (var a in wrapper.Variants)
            {
                var touch_ability = a.GetComponent<AbilityEffectStickyTouch>().TouchDeliveryAbility;
                var actions = new GameAction[] { Helpers.Create<ContextActionCastSpell>(c => c.Spell = touch_ability), Helpers.Create<ContextActionRemoveSelf>() };
                var buff = Helpers.CreateBuff("Channel" + touch_ability.name +"Buff",
                                              "Channel " + touch_ability.Name,
                                              touch_ability.Description,
                                              "",
                                              touch_ability.Icon,
                                              null,
                                              Common.createAddInitiatorAttackWithWeaponTrigger(Helpers.CreateActionList(actions), check_weapon_range_type: true, range_type: AttackTypeAttackBonus.WeaponRangeType.Melee)
                                              );
                remove_buffs.Buffs = remove_buffs.Buffs.AddToArray(buff);

                var channel = Helpers.CreateAbility("Channel" + touch_ability.name,
                                                    buff.Name,
                                                    buff.Description,
                                                    "",
                                                    buff.Icon,
                                                    AbilityType.Supernatural,
                                                    CommandType.Swift,
                                                    AbilityRange.Personal,
                                                    Helpers.oneMinuteDuration,
                                                    "",
                                                    Helpers.CreateRunActions(remove_buffs,
                                                                             Common.createContextActionApplyBuff(buff, Helpers.CreateContextDuration(1, DurationRate.Minutes), dispellable: false)),
                                                    Helpers.Create<AbilityCasterMainWeaponIsMelee>(),
                                                    touch_of_corruption_resource.CreateResourceLogic(amount: 2),
                                                    touch_ability.GetComponent<ContextCalculateAbilityParamsBasedOnClass>()
                                                    );
                var requirement = a.GetComponent<AbilityShowIfCasterHasFact>();
                if (requirement != null)
                {
                    channel.AddComponent(requirement);
                }
                channel.setMiscAbilityParametersSelfOnly();
                channels.Add(channel);
            }

            var channel_wrapper = Common.createVariantWrapper("ChannelTouchOfCorruptionBase", "", channels.ToArray());
            channel_wrapper.SetIcon(LoadIcons.Image2Sprite.Create(@"AbilityIcons/WeaponEvil.png"));
            touch_of_corruption.AddComponent(Helpers.CreateAddFacts(channel_wrapper));
        }


        static void createChannelEnergy()
        {
            var negative_energy_feature = library.Get<BlueprintFeature>("3adb2c906e031ee41a01bfc1d5fb7eea");
            var context_rank_config = Helpers.CreateContextRankConfig(ContextRankBaseValueType.ClassLevel, classes: getAntipaladinArray(), progression: ContextRankProgression.OnePlusDiv2);
            var dc_scaling = Common.createContextCalculateAbilityParamsBasedOnClasses(getAntipaladinArray(), StatType.Charisma);
            channel_negative_energy = Helpers.CreateFeature("AntipaladinChannelNegativeEnergyFeature",
                                                            "Channel Negative Energy",
                                                            "When an antipaladin reaches 4th level, he gains the supernatural ability to channel negative energy like a cleric. Using this ability consumes two uses of his touch of corruption ability. An antipaladin uses his level as his effective cleric level when channeling negative energy. This is a Charisma-based ability.",
                                                            "",
                                                            negative_energy_feature.Icon,
                                                            FeatureGroup.None);

            var harm_living = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                                      "AntipaladinChannelEnergyHarmLiving",
                                                                      "",
                                                                      "Channeling negative energy causes a burst that damages all living creatures in a 30 - foot radius centered on the antipaladin. The amount of damage inflicted is equal to 1d6 points of damage plus 1d6 points of damage for every two antipaladin levels beyond 1st (3d6 at 5th, and so on). Creatures that take damage from channeled energy receive a Will save to halve the damage. The DC of this save is equal to 10 + 1 / 2 the antipaladin's level + the antipaladin's Charisma modifier.",
                                                                      "",
                                                                      context_rank_config,
                                                                      dc_scaling,
                                                                      Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));
            var heal_undead = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                                                        "AntipaladinChannelEnergyHealUndead",
                                                                        "",
                                                                        "Channeling negative energy causes a burst that heals all undead creatures in a 30-foot radius centered on the antipaladin. The amount of damage healed is equal to 1d6 points of damage plus 1d6 points of damage for every two antipaladin levels beyond 1st (3d6 at 5th, and so on).",
                                                                        "",
                                                                        context_rank_config,
                                                                        dc_scaling,
                                                                        Helpers.CreateResourceLogic(touch_of_corruption_resource, amount: 2));

            var harm_living_base = Common.createVariantWrapper("AntipaladinNegativeHarmBase", "", harm_living);
            var heal_undead_base = Common.createVariantWrapper("AntipaladinNegativeHealBase", "", heal_undead);
            harm_living.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());
            harm_living.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());

            ChannelEnergyEngine.storeChannel(harm_living, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddFacts(harm_living_base, heal_undead_base));

            //add extra channel
            extra_channel_resource = Helpers.CreateAbilityResource("AntipaladinExtraChannelResource", "", "", "", null);
            extra_channel_resource.SetFixedResource(0);


            var harm_living_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHarm,
                                                          "AntipaladinChannelEnergyHarmLivingExtra",
                                                          harm_living.Name + " (Extra)",
                                                          harm_living.Description,
                                                          "",
                                                          harm_living.GetComponent<ContextRankConfig>(),
                                                          harm_living.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                                          Helpers.CreateResourceLogic(extra_channel_resource, true, 1));

            var heal_undead_extra = ChannelEnergyEngine.createChannelEnergy(ChannelEnergyEngine.ChannelType.NegativeHeal,
                                              "AntipaladinChannelEnergyHealUndeadExtra",
                                              heal_undead.Name + " (Extra)",
                                              heal_undead.Description,
                                              "",
                                              heal_undead.GetComponent<ContextRankConfig>(),
                                              heal_undead.GetComponent<NewMechanics.ContextCalculateAbilityParamsBasedOnClasses>(),
                                              Helpers.CreateResourceLogic(extra_channel_resource, true, 1));


            harm_living_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead_extra.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
            heal_undead_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());
            harm_living_extra.AddComponent(Helpers.Create<InsinuatorMechanics.AbilityCasterMaybeInvokedOutsider>());

            harm_living_base.addToAbilityVariants(harm_living_extra);
            heal_undead_base.addToAbilityVariants(heal_undead_extra);
            ChannelEnergyEngine.storeChannel(harm_living_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHarm);
            ChannelEnergyEngine.storeChannel(heal_undead_extra, channel_negative_energy, ChannelEnergyEngine.ChannelType.NegativeHeal);

            channel_negative_energy.AddComponent(Helpers.CreateAddAbilityResource(extra_channel_resource));
            extra_channel = ChannelEnergyEngine.createExtraChannelFeat(harm_living_extra, channel_negative_energy, "ExtraChannelAntipaladin", "Extra Channel (Antipaladin)", "");
        }

        static void createSmiteGood()
        {
            var resource = library.Get<BlueprintAbilityResource>("b4274c5bb0bf2ad4190eb7c44859048b");
            tip_of_spear = Helpers.CreateFeature("TipOfTheSpearFeature",
                                                 "Tip of the Spear",
                                                 "At 20th level, the antipaladin tears through heroes and rival villains alike. The antipaladin gains three additional uses of smite good per day and can smite foes regardless of their alignment.",
                                                 "",
                                                 LoadIcons.Image2Sprite.Create(@"AbilityIcons/TipOfTheSpear.png"),
                                                 FeatureGroup.None,
                                                 resource.CreateIncreaseResourceAmount(3)
                                                 );

            smite_good = Common.createSmite("AntipaladinSmiteGood",
                                            "Smite Good",
                                            "Once per day, an antipaladin can call out to the dark powers to crush the forces of good. As a swift action, the antipaladin chooses one target within sight to smite. If this target is good, the antipaladin adds his Charisma bonus (if any) on his attack rolls and adds his antipaladin level on all damage rolls made against the target of his smite, smite good attacks automatically bypass any DR the creature might possess.\n"
                                            + "In addition, while smite good is in effect, the antipaladin gains a deflection bonus equal to his Charisma modifier (if any) to his AC against attacks made by the target of the smite. If the antipaladin targets a creature that is not good, the smite is wasted with no effect.\n"
                                            + "The smite evil lasts until the target dies or the paladin selects a new target. At 4th level, and at every three levels thereafter, the paladin may smite evil one additional time per day.",
                                            "",
                                            "",
                                            LoadIcons.Image2Sprite.Create(@"AbilityIcons/SmiteGood.png"),
                                            getAntipaladinArray(),
                                            Helpers.Create<NewMechanics.ContextConditionAlignmentUnlessCasterHasFact>(c => { c.Alignment = AlignmentComponent.Good; c.fact = tip_of_spear; })
                                            );

            smite_good_extra_use = library.CopyAndAdd<BlueprintFeature>("0f5c99ffb9c084545bbbe960b825d137", "SmiteGoodAdditionalUse", "");
            smite_good_extra_use.SetNameDescriptionIcon("Smite Good — Additional Use",
                                              smite_good.Description,
                                              smite_good.Icon);

            smite_resource = resource;

            var smite_good_ability = smite_good.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            smite_good_ability.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));


            aura_of_vengeance = Common.createSmite("AntipaladinAuraOfVengeance",
                                "Aura of Vengeance",
                                "At 11th level, an antipaladin can expend two uses of his smite good ability to grant the ability to smite good to all allies within 10 feet, using his bonuses. Allies must use this smite good ability by the start of the antipaladin’s next turn and the bonuses last for 1 minute. Using this ability is a free action.",
                                "",
                                "",
                                NewSpells.command.Icon,
                                getAntipaladinArray(),
                                Helpers.Create<NewMechanics.ContextConditionAlignmentUnlessCasterHasFact>(c => { c.Alignment = AlignmentComponent.Good; c.fact = tip_of_spear; })
                                );

            var aura_of_vengeance_ability = aura_of_vengeance.GetComponent<AddFacts>().Facts[0] as BlueprintAbility;
            aura_of_vengeance_ability.AddComponent(Helpers.Create<AbilityCasterAlignment>(a => a.Alignment = AlignmentMaskType.Evil));
        }


        static void createAntipaladinDeitySelection()
        {
            antipaladin_deity = library.CopyAndAdd<BlueprintFeatureSelection>("a7c8b73528d34c2479b4bd638503da1d", "AntipaladinDeitySelection", "");
            antipaladin_deity.AllFeatures = new BlueprintFeature[0];
            antipaladin_deity.Group = FeatureGroup.Deities;

            var deities = library.Get<BlueprintFeatureSelection>("59e7a76987fe3b547b9cce045f4db3e4").AllFeatures;

            var allow_evil = library.Get<BlueprintFeature>("351235ac5fc2b7e47801f63d117b656c");

            antipaladin_deity.AllFeatures = deities.Where(d => d.GetComponents<AddFacts>().Aggregate(false, (val, af) => val = val || af.Facts.Contains(allow_evil))).ToArray();
            antipaladin_deity.Features = antipaladin_deity.AllFeatures;
        }


        static BlueprintSpellbook createAntipaladinSpellbook()
        {
            var paladin_spellook = library.Get<BlueprintSpellbook>("bce4989b070ce924b986bf346f59e885");
            var antipaladin_spellbook = library.CopyAndAdd(paladin_spellook, "AntipaladinSpellbook", "");
            antipaladin_spellbook.Name = antipaladin_class.LocalizedName;
            antipaladin_spellbook.CharacterClass = antipaladin_class;          
            antipaladin_spellbook.SpellList = Helpers.Create<BlueprintSpellList>();
            antipaladin_spellbook.SpellList.name = "AntipaladinSpelllist";
            library.AddAsset(antipaladin_spellbook.SpellList, "");
            antipaladin_spellbook.SpellList.SpellsByLevel = new SpellLevelList[10];
            for (int i = 0; i < antipaladin_spellbook.SpellList.SpellsByLevel.Length; i++)
            {
                antipaladin_spellbook.SpellList.SpellsByLevel[i] = new SpellLevelList(i);
            }

            Common.SpellId[] spells = new Common.SpellId[]
            {
                new Common.SpellId( "8bc64d869456b004b9db255cdd1ea734", 1), //bane
                new Common.SpellId( "b7731c2b4fa1c9844a092329177be4c3", 1), //boneshaker
                new Common.SpellId( "bd81a3931aa285a4f9844585b5d97e51", 1), //cause fear
                new Common.SpellId( NewSpells.command.AssetGuid, 1),
                new Common.SpellId( "fbdd8c455ac4cde4a9a3e18c84af9485", 1), //doom
                new Common.SpellId( "e5af3674bb241f14b9a9f6b0c7dc3d27", 1), //inflict light wounds
                new Common.SpellId( NewSpells.magic_weapon.AssetGuid, 1),
                new Common.SpellId( "433b1faf4d02cc34abb0ade5ceda47c4", 1), //protection from alignment
                new Common.SpellId( NewSpells.savage_maw.AssetGuid, 1), 
                //shadow claws
                //touch of blindness
                new Common.SpellId( "8fd74eddd9b6c224693d9ab241f25e84", 1), //summon monster I
                
                new Common.SpellId( NewSpells.blade_tutor.AssetGuid, 2), //acid arrow
                new Common.SpellId( "46fd02ad56c35224c9c91c88cd457791", 2), //blindness
                new Common.SpellId( "4c3d08935262b6544ae97599b3a9556d", 2), //bulls's strength
                new Common.SpellId( NewSpells.desecrate.AssetGuid, 2),
                new Common.SpellId( "446f7bf201dc1934f96ac0a26e324803", 2), //eagles splendor
                new Common.SpellId( "c7104f7526c4c524f91474614054547e", 2), //hold person
                new Common.SpellId( NewSpells.inflict_pain.AssetGuid, 2),
                new Common.SpellId( "89940cde01689fb46946b2f8cd7b66b7", 2), //invisibility
                new Common.SpellId( "c9198d9dfd2515d4ba98335b57bb66c7", 2), //litany of eloqeunce
                new Common.SpellId( "16f7754287811724abe1e0ead88f74ca", 2), //litany of entanglement
                new Common.SpellId( "dee3074b2fbfb064b80b973f9b56319e", 2), //pernicious poison
                new Common.SpellId( "08cb5f4c3b2695e44971bf5c45205df0", 2), //scare
                new Common.SpellId( "82962a820ebc0e7408b8582fdc3f4c0c", 2), //sense vitals
                new Common.SpellId( NewSpells.silence.AssetGuid, 2),
                new Common.SpellId( "1724061e89c667045a6891179ee2e8e7", 2), //summon monster 2
                new Common.SpellId( NewSpells.touch_of_blood_letting.AssetGuid, 2),              
                new Common.SpellId( NewSpells.vine_strike.AssetGuid, 2),
                
                new Common.SpellId( NewSpells.accursed_glare.AssetGuid, 3), 
                new Common.SpellId( "4b76d32feb089ad4499c3a1ce8e1ac27", 3), //animate dead
                new Common.SpellId( "989ab5c44240907489aba0a8568d0603", 3), //bestow curse
                new Common.SpellId( "48e2744846ed04b4580be1a3343a5d3d", 3), //contagion
                new Common.SpellId( NewSpells.deadly_juggernaut.AssetGuid, 3),
                new Common.SpellId( "92681f181b507b34ea87018e8f7a528a", 3), //dispel magic
                new Common.SpellId( "903092f6488f9ce45a80943923576ab3", 3), //diplacement instead of shield of darkness
                new Common.SpellId( "65f0b63c45ea82a4f8b8325768a3832d", 3), //inflict moderate wounds        
                new Common.SpellId( NewSpells.magic_weapon_greater.AssetGuid, 3),
                //second wind
                new Common.SpellId( "5d61dde0020bbf54ba1521f7ca0229dc", 3), //summon monster 3
                new Common.SpellId( "8a28a811ca5d20d49a863e832c31cce1", 3), //vampiric touch

                //banishing blade        
                new Common.SpellId( "d2aeac47450c76347aebbc02e4f463e0", 4), //fear
                new Common.SpellId( NewSpells.inflict_pain_mass.AssetGuid, 4),
                new Common.SpellId( "bd5da98859cf2b3418f6d68ea66cabbe", 4), //inflict serious wounds
                new Common.SpellId( "ecaa0def35b38f949bd1976a6c9539e0", 4), //invisibility greater
                new Common.SpellId( "435e73bcff18f304293484f9511b4672", 4), //lithany of madness
                new Common.SpellId( "4fbd47525382517419c66fb548fe9a67", 4), //slay living
                new Common.SpellId( "b56521d58f996cd4299dab3f38d5fe31", 4), //profane nimbus
                new Common.SpellId( "9047cb1797639924487ec0ad566a3fea", 4), //resounding blow
                new Common.SpellId( "7ed74a3ec8c458d4fb50b192fd7be6ef", 4), //summon monster 4
                //unholy sword
            };

            foreach (var spell_id in spells)
            {
                var spell = library.Get<BlueprintAbility>(spell_id.guid);
                spell.AddToSpellList(antipaladin_spellbook.SpellList, spell_id.level);
            }

            return antipaladin_spellbook;
        }
    }
}
