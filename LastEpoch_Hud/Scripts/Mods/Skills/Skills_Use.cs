using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Skills
{
    public class Skills_Use
    {
        public static bool CanRun()
        {
            bool result = false;
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed()) { result = true; }
            }

            return result;
        }

        [HarmonyPatch(typeof(CharacterMutator), "OnAbilityUse")]
        public class OnAbilityUse
        {
            [HarmonyPrefix]
            static void Prefix(CharacterMutator __instance, AbilityInfo __0, ref AbilityMutator __1, float __2, UnityEngine.Vector3 __3, bool __4)
            {
                if (CanRun())
                {
                    Ability ability = null;
                    try { ability = __1.getAbility(); }
                    catch { Main.logger_instance.Error("OnAbilityUse Prefix : Can't get Ability"); }

                    if (!ability.IsNullOrDestroyed())
                    {
                        if (Save_Manager.instance.data.Skills.Enable_RemoveChannelCost) { ability.channelCost = 0f; }
                        if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                        {
                            ability.manaCost = 0f;
                            ability.minimumManaCost = 0f;
                            ability.manaCostPerDistance = 0f;
                        }
                        if (Save_Manager.instance.data.Skills.Enable_NoManaRegenWhileChanneling) { ability.noManaRegenWhileChanneling = false; }
                        if (Save_Manager.instance.data.Skills.Enable_StopWhenOutOfMana) { ability.stopWhenOutOfMana = false; }
                        //if (Config.Data.mods_config.character.characterstats.Enable_attack_rate) { ability.speedMultiplier = Config.Data.mods_config.character.characterstats.attack_rate; }
                        if ((ability.moveOrAttackCompatible) && (ability.moveOrAttackFallback == Ability.MoveOrAttackFallback.Move))
                        {
                            if (Save_Manager.instance.data.Skills.MovementSkills.Enable_NoTarget)
                            {
                                ability.playerRequiresTarget = false;
                                ability.requiredEnemyTargetMustBeAccessible = false;
                            }
                            if (Save_Manager.instance.data.Skills.MovementSkills.Enable_ImmuneDuringMovement)
                            {
                                ability.immuneDuringMovement = true;
                                ability.cannotDieDuringMovement = true;
                            }
                            if (Save_Manager.instance.data.Skills.MovementSkills.Disable_SimplePath)
                            {
                                ability.limitRangeForPlayers = false;
                                ability.requireSimplePath = false;
                            }
                        }
                    }

                    Il2CppSystem.Type il2cpp_type = null;
                    try { il2cpp_type = __1.GetIl2CppType(); }
                    catch { Main.logger_instance.Error("OnAbilityUse Prefix : Can't get Mutator type"); }

                    //Use Switch(il2cpp_type.ToString()) instead of if for better result (== is bad)

                    if (!il2cpp_type.IsNullOrDestroyed())
                    {
                        //Holy Aura : Fix ManaCost
                        if (il2cpp_type.ToString() == "HolyAuraMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                            {
                                __1.TryCast<HolyAuraMutator>().ability.manaCost = 0f;
                                __1.TryCast<HolyAuraMutator>().ability.minimumManaCost = 0f;
                                __1.TryCast<HolyAuraMutator>().ability.manaCostPerDistance = 0f;
                            }
                        }

                        //Warpath : Fix Channel Cost
                        if (il2cpp_type.ToString() == "WarpathMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveChannelCost)
                            {
                                __1.TryCast<WarpathMutator>().addedChannelCost = 0f;
                                __1.TryCast<WarpathMutator>().addedChannelCostPerSecond = 0f;
                            }
                        }

                        //Smite : Fix ManaCost
                        if (il2cpp_type.ToString() == "SmiteMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                            {
                                __1.TryCast<SmiteMutator>().addedManaCost = 0f;
                                __1.TryCast<SmiteMutator>().increasedManaCost = 0f;
                            }
                        }

                        //Sigil of Hope : Fix ManaCost
                        if (il2cpp_type.ToString() == "SigilsOfHopeMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                            {
                                __1.TryCast<SigilsOfHopeMutator>().addedManaCost = 0f;
                                __1.TryCast<SigilsOfHopeMutator>().increasedManaCost = 0f;
                            }
                        }

                        //Meteor
                        if (il2cpp_type.ToString() == "MeteorMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                            {
                                __1.TryCast<MeteorMutator>().addedManaCost = 0f;
                            }
                        }

                        //Companions
                        else if (il2cpp_type.ToString() == "SummonWolfMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Companion.Wolf.Enable_SummonLimit)
                            {
                                __1.TryCast<SummonWolfMutator>().wolfLimit = Save_Manager.instance.data.Skills.Companion.Wolf.SummonLimit;
                            }
                        }
                        else if (il2cpp_type.ToString() == "SummonScorpionMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Companion.Scorpion.Enable_BabyQuantity)
                            {
                                SummonScorpionMutator mutator = __1.TryCast<SummonScorpionMutator>();
                                mutator.babyScorpionQuantity = Save_Manager.instance.data.Skills.Companion.Scorpion.BabyQuantity;
                                mutator.babyScorpionsToSpawnOnAbilityActivation = Save_Manager.instance.data.Skills.Companion.Scorpion.BabyQuantity;
                                mutator.increasedBabySpawnRate = 1;
                            }
                        }

                        //Minions
                        else if (il2cpp_type.ToString() == "SummonSkeletonMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromPassives)
                            {
                                __1.TryCast<SummonSkeletonMutator>().additionalSkeletonsFromPassives = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromPassives;
                                __1.TryCast<SummonSkeletonMutator>().onlySummonOneWarrior = false;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsFromSkillTree)
                            {
                                __1.TryCast<SummonSkeletonMutator>().additionalSkeletonsFromSkillTree = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsFromSkillTree;
                                __1.TryCast<SummonSkeletonMutator>().onlySummonOneWarrior = false;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_additionalSkeletonsPerCast)
                            {
                                __1.TryCast<SummonSkeletonMutator>().additionalSkeletonsPerCast = Save_Manager.instance.data.Skills.Minions.Skeletons.additionalSkeletonsPerCast;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_chanceToResummonOnDeath)
                            {
                                __1.TryCast<SummonSkeletonMutator>().chanceToResummonOnDeath = Save_Manager.instance.data.Skills.Minions.Skeletons.chanceToResummonOnDeath;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceArcher)
                            {
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonArchers = false;
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonWarriors = true;
                                __1.TryCast<SummonSkeletonMutator>().canSummonRogues = false;

                                __1.TryCast<SummonSkeletonMutator>().forceBrawler = false;
                                __1.TryCast<SummonSkeletonMutator>().forceRogue = false;
                                __1.TryCast<SummonSkeletonMutator>().forceWarrior = false;
                                __1.TryCast<SummonSkeletonMutator>().forceArcher = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceBrawler)
                            {
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonArchers = true;
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonWarriors = true;
                                __1.TryCast<SummonSkeletonMutator>().canSummonRogues = false;

                                __1.TryCast<SummonSkeletonMutator>().forceRogue = false;
                                __1.TryCast<SummonSkeletonMutator>().forceWarrior = false;
                                __1.TryCast<SummonSkeletonMutator>().forceArcher = false;
                                __1.TryCast<SummonSkeletonMutator>().forceBrawler = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceRogue)
                            {
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonArchers = true;
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonWarriors = true;
                                __1.TryCast<SummonSkeletonMutator>().canSummonRogues = true;

                                __1.TryCast<SummonSkeletonMutator>().forceWarrior = false;
                                __1.TryCast<SummonSkeletonMutator>().forceArcher = false;
                                __1.TryCast<SummonSkeletonMutator>().forceBrawler = false;
                                __1.TryCast<SummonSkeletonMutator>().forceRogue = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Skeletons.Enable_forceWarrior)
                            {
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonArchers = true;
                                __1.TryCast<SummonSkeletonMutator>().cannotSummonWarriors = false;
                                __1.TryCast<SummonSkeletonMutator>().canSummonRogues = false;

                                __1.TryCast<SummonSkeletonMutator>().forceArcher = false;
                                __1.TryCast<SummonSkeletonMutator>().forceBrawler = false;
                                __1.TryCast<SummonSkeletonMutator>().forceRogue = false;
                                __1.TryCast<SummonSkeletonMutator>().forceWarrior = true;
                            }
                        }
                        else if (il2cpp_type.ToString() == "SummonWraithMutator")
                        //else if (type == typeof(SummonWraithMutator))
                        {
                            if (Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_additionalMaxWraiths)
                            {
                                __1.TryCast<SummonWraithMutator>().additionalMaxWraiths = Save_Manager.instance.data.Skills.Minions.Wraiths.additionalMaxWraiths;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_delayedWraiths)
                            {
                                __1.TryCast<SummonWraithMutator>().delayedWraiths = Save_Manager.instance.data.Skills.Minions.Wraiths.delayedWraiths; //Wraiths per cast
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_limitedTo2Wraiths)
                            {
                                __1.TryCast<SummonWraithMutator>().limitedTo2Wraiths = false;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_wraithsDoNotDecay)
                            {
                                __1.TryCast<SummonWraithMutator>().wraithsDoNotDecay = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Wraiths.Enable_increasedCastSpeed)
                            {
                                __1.TryCast<SummonWraithMutator>().increasedCastSpeed = Save_Manager.instance.data.Skills.Minions.Wraiths.increasedCastSpeed;
                            }
                        }
                        else if (il2cpp_type.ToString() == "SummonMageMutator")
                        {
                            //remove in LastEpoch 1.2
                            /*if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromItems)
                            {
                                __1.TryCast<SummonMageMutator>().additionalSkeletonsFromItems = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromItems;
                            }*/
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromPassives)
                            {
                                __1.TryCast<SummonMageMutator>().additionalSkeletonsFromPassives = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromPassives;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsFromSkillTree)
                            {
                                __1.TryCast<SummonMageMutator>().additionalSkeletonsFromSkillTree = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsFromSkillTree;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_additionalSkeletonsPerCast)
                            {
                                __1.TryCast<SummonMageMutator>().additionalSkeletonsPerCast = Save_Manager.instance.data.Skills.Minions.Mages.additionalSkeletonsPerCast;
                            }
                            //if (Config.Data.mods_config.character.minions.mage.Enable_onlySummonOneMage)
                            //{
                            //    ability_mutator.TryCast<SummonMageMutator>().onlySummonOneMage = false;                                
                            //}
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_singleSummon)
                            {
                                __1.TryCast<SummonMageMutator>().singleSummon = false;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceCryomancer)
                            {
                                __1.TryCast<SummonMageMutator>().forceDeathKnight = false;
                                __1.TryCast<SummonMageMutator>().forcePyromancer = false;
                                __1.TryCast<SummonMageMutator>().forceNoCryo = false;
                                __1.TryCast<SummonMageMutator>().forceCryomancer = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_forceDeathKnight)
                            {
                                __1.TryCast<SummonMageMutator>().forcePyromancer = false;
                                __1.TryCast<SummonMageMutator>().forceCryomancer = false;
                                __1.TryCast<SummonMageMutator>().forceDeathKnight = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_forcePyromancer)
                            {
                                __1.TryCast<SummonMageMutator>().forceCryomancer = false;
                                __1.TryCast<SummonMageMutator>().forceDeathKnight = false;
                                __1.TryCast<SummonMageMutator>().forceNoPyro = false;
                                __1.TryCast<SummonMageMutator>().forcePyromancer = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_chanceForTwoExtraProjectiles)
                            {
                                __1.TryCast<SummonMageMutator>().chanceForTwoExtraProjectiles = Save_Manager.instance.data.Skills.Minions.Mages.chanceForTwoExtraProjectiles;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.Mages.Enable_doubleProjectiles)
                            {
                                __1.TryCast<SummonMageMutator>().doubleProjectiles = true;
                            }
                            //ability_mutator.TryCast<SummonMageMutator>().additionalWarlords = 50;
                        }
                        else if (il2cpp_type.ToString() == "SummonBoneGolemMutator")
                        //else if (type == typeof(SummonBoneGolemMutator))
                        {
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_selfResurrectChance)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().selfResurrectChance = Save_Manager.instance.data.Skills.Minions.BoneGolems.selfResurrectChance;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedFireAuraArea)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().increasedFireAuraArea = Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedFireAuraArea;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_increasedMoveSpeed)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().increasedMoveSpeed = Save_Manager.instance.data.Skills.Minions.BoneGolems.increasedMoveSpeed;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_twins)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().twins = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_hasSlamAttack)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().hasSlamAttack = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadArmorAura)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().undeadArmorAura = Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadArmorAura;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.BoneGolems.Enable_undeadMovespeedAura)
                            {
                                __1.TryCast<SummonBoneGolemMutator>().undeadMovespeedAura = Save_Manager.instance.data.Skills.Minions.BoneGolems.undeadMovespeedAura;
                            }
                        }
                        else if (il2cpp_type.ToString() == "SummonVolatileZombieMutator")
                        //else if (type == typeof(SummonVolatileZombieMutator))
                        {
                            if (Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastFromMinionDeath)
                            {
                                __1.TryCast<SummonVolatileZombieMutator>().chanceToCastFromMinionDeath = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastFromMinionDeath;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastInfernalShadeOnDeath)
                            {
                                __1.TryCast<SummonVolatileZombieMutator>().chanceToCastInfernalShadeOnDeath = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastInfernalShadeOnDeath;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.VolatileZombies.Enable_chanceToCastMarrowShardsOnDeath)
                            {
                                __1.TryCast<SummonVolatileZombieMutator>().chanceToCastMarrowShardsOnDeath = Save_Manager.instance.data.Skills.Minions.VolatileZombies.chanceToCastMarrowShardsOnDeath;
                            }
                        }
                        else if (il2cpp_type.ToString() == "DreadShadeMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableLimit)
                            {
                                __1.TryCast<DreadShadeMutator>().limitTo1DreadShade = false;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Duration)
                            {
                                __1.TryCast<DreadShadeMutator>().increasedDuration = Save_Manager.instance.data.Skills.Minions.DreadShades.Duration;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_DisableHealthDrain)
                            {
                                __1.TryCast<DreadShadeMutator>().noHealthDrain = true;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Max)
                            {
                                __1.TryCast<DreadShadeMutator>().addedMaxShades = Save_Manager.instance.data.Skills.Minions.DreadShades.max;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_ReduceDecay)
                            {
                                __1.TryCast<DreadShadeMutator>().reducedDecayRate = Save_Manager.instance.data.Skills.Minions.DreadShades.decay;
                            }
                            if (Save_Manager.instance.data.Skills.Minions.DreadShades.Enable_Radius)
                            {
                                __1.TryCast<DreadShadeMutator>().increasedRadius = Save_Manager.instance.data.Skills.Minions.DreadShades.radius;
                            }
                        }
                        else if (il2cpp_type.ToString() == "FlameWardMutator")
                        {
                            if (Save_Manager.instance.data.Skills.Enable_RemoveManaCost)
                            {
                                __1.TryCast<FlameWardMutator>().addedManaCost = 0f;
                            }
                        }
                    }
                }
            }

            [HarmonyPostfix]
            static void PostFix(CharacterMutator __instance, AbilityInfo __0, ref AbilityMutator __1, float __2, UnityEngine.Vector3 __3, bool __4)
            {
                if ((CanRun()) && (!__1.IsNullOrDestroyed()))
                {
                    if (Save_Manager.instance.data.Skills.Enable_RemoveCooldown) { __1.RemoveCooldown(); }
                }
            }
        }
    }
}
