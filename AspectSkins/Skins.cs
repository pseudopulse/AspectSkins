using System;
using RoR2.ContentManagement;
using RoR2.SurvivorMannequins;

namespace AspectSkins {
    public class EliteSkins {
        public static List<EliteSkillDef> skins = new();
        public static void Setup() {
            On.RoR2.ContentManagement.ContentManager.SetContentPacks += CreateSkins;
            On.RoR2.CharacterModel.UpdateMaterials += ApplyOverlay;
            On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.ApplyLoadoutToMannequinInstance += UpdateLobbyModel;
            On.RoR2.CharacterModel.SetEquipmentDisplay += UpdateEquipmentDisplay;
            On.RoR2.Language.LoadStrings += SetupNames;
        }

        internal static void SetupNames(On.RoR2.Language.orig_LoadStrings orig, Language self) {
            orig(self);

            foreach (EliteSkillDef sd in skins) {
                if (sd.elite == null) {
                    continue;
                }

                string name = Language.GetString(sd.skillNameToken);
                name = name.Split(' ')[0];
                string nameToken = sd.skillNameToken + "_AS_NAME";
                string descToken = sd.skillDescriptionToken + "_AS_DESC";
                string desc = $"Transform your skin into a {name} elite!";
                sd.skillNameToken = nameToken;
                sd.skillDescriptionToken = descToken;
                LanguageAPI.Add(nameToken, name);
                LanguageAPI.Add(descToken, desc);
            }
        }

        internal static void UpdateLobbyModel(On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.orig_ApplyLoadoutToMannequinInstance orig, SurvivorMannequinSlotController self) {
            orig(self);
            if (!self.mannequinInstanceTransform) {
                return;
            }

            try {
                Loadout.BodyLoadoutManager.BodyInfo info = Loadout.BodyLoadoutManager.allBodyInfos[(int)self._networkUser.bodyIndexPreference];
                int index = -1;
                for (int i = 0; i < info.prefabSkillSlots.Length; i++) {
                    if (info.prefabSkillSlots[i].skillName != null && info.prefabSkillSlots[i].skillName == "EliteSkin") {
                        index = i;
                    }
                }

                if (index == -1) {
                    return;
                }

                uint variant = self.currentLoadout.bodyLoadoutManager.GetSkillVariant(self._networkUser.bodyIndexPreference, index);
                EliteSkillDef def = info.prefabSkillSlots[index]._skillFamily.variants[variant].skillDef as EliteSkillDef;
                
                CharacterModel model = self.mannequinInstanceTransform.GetComponentInChildren<CharacterModel>();
                
                LobbyMarker marker = model.gameObject.GetComponent<LobbyMarker>();

                if (!marker) {
                    marker = model.gameObject.AddComponent<LobbyMarker>();
                }
                marker.shaderIndex = def.eliteRampIndex;

                if (def.elite != null) {
                    model.SetEquipmentDisplay(def.elite.eliteEquipmentDef.equipmentIndex);
                }

            }
            catch (Exception err) {
                throw err;
            }
        }

        internal static void ApplyOverlay(On.RoR2.CharacterModel.orig_UpdateMaterials orig, CharacterModel self) {                    
            GenericSkill slot = self.body?.skillLocator?.FindSkill("EliteSkin") ?? null;

            if (slot) {
                EliteSkillDef def = slot.skillDef as EliteSkillDef;
                self.shaderEliteRampIndex = def.eliteRampIndex;
            }

            if (self.GetComponent<LobbyMarker>()) {
                self.shaderEliteRampIndex = self.GetComponent<LobbyMarker>().shaderIndex;
            }
            orig(self);
        }

        internal static void UpdateEquipmentDisplay(On.RoR2.CharacterModel.orig_SetEquipmentDisplay orig, CharacterModel self, EquipmentIndex index) {
            GenericSkill slot = self.body?.skillLocator?.FindSkill("EliteSkin") ?? null;

            if (slot) {
                EliteSkillDef def = slot.skillDef as EliteSkillDef;
                index = def.elite?.eliteEquipmentDef?.equipmentIndex ?? index;
            }

            orig(self, index);
        }

        internal class LobbyMarker : MonoBehaviour {
            public int shaderIndex;
        }

        internal static void CreateSkins(On.RoR2.ContentManagement.ContentManager.orig_SetContentPacks orig, List<ReadOnlyContentPack> packs) {
            orig(packs);

            EliteDef[] elites = ContentManager._eliteDefs;
            
            foreach (SurvivorDef def in ContentManager._survivorDefs) {
                GameObject body = def.bodyPrefab;
                if (body) {
                    GenericSkill skill = body.AddComponent<GenericSkill>();
                    skill.skillName = "EliteSkin";
                    skill.hideInCharacterSelect = true;

                    SkillFamily family = ScriptableObject.CreateInstance<SkillFamily>();
                    (family as ScriptableObject).name = "EliteSkins";
                    List<SkillFamily.Variant> variants = new();

                    EliteSkillDef sdd = ScriptableObject.CreateInstance<EliteSkillDef>();
                    sdd.activationState = new(typeof(EntityStates.Idle));
                    sdd.activationStateMachineName = "Weapon";
                    sdd.canceledFromSprinting = false;
                    sdd.cancelSprintingOnActivation = false;

                    string nameToken = "DEFAULT_ESNAME";
                    string descToken = "DEFAULT_ESDESC";
                    string desc = "Standard survivor skin.";
                    LanguageAPI.Add(nameToken, "None");
                    LanguageAPI.Add(descToken, desc);
                    sdd.skillNameToken = nameToken;
                    sdd.skillDescriptionToken = descToken;
                    sdd.icon = null;
                    sdd.eliteRampIndex = -1;

                    variants.Add(new SkillFamily.Variant {
                        skillDef = sdd,
                        viewableNode = new(nameToken, false, null),
                    });

                    int total = 0;

                    for (int i = 0; i < elites.Length; i++) {
                        if (elites[i].name.ToLower().Contains("honor")) {
                            continue;
                        }

                        EliteDef elite = elites[i];

                        if (elite.modifierToken == null || elite.modifierToken.Contains("SECRETSPEED") || elite.modifierToken.Contains("GOLD")) {
                            continue;
                        }

                        EliteSkillDef sd = ScriptableObject.CreateInstance<EliteSkillDef>();
                        sd.activationState = new(typeof(EntityStates.Idle));
                        sd.activationStateMachineName = "Weapon";
                        sd.canceledFromSprinting = false;
                        sd.cancelSprintingOnActivation = false;

                        sd.skillNameToken = elite.modifierToken;
                        sd.skillDescriptionToken = elite.modifierToken;

                        nameToken = elite.modifierToken;
                        
                        sd.skillNameToken = nameToken;
                        sd.skillDescriptionToken = nameToken;
                        sd.icon = elite.eliteEquipmentDef?.passiveBuffDef?.iconSprite ?? null;
                        sd.eliteRampIndex = elite.shaderEliteRampIndex;
                        sd.elite = elites[i];
                        (sd as ScriptableObject).name = nameToken + "_SKILL_" + total;
                        total++;

                        variants.Add(new SkillFamily.Variant {
                            skillDef = sd,
                            viewableNode = new(nameToken, false, null),
                        });

                        skins.Add(sd);
                    }

                    family.variants = variants.ToArray();
                    ContentAddition.AddSkillFamily(family);
                    skill._skillFamily = family;
                }
            }
        }
    }
}