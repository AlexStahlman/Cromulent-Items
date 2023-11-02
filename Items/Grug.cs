using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2.Orbs;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using BepInEx.Configuration;
using UnityEngine.Networking;
using RoR2.Projectile;
using System.Runtime.CompilerServices;
using System.Linq;

namespace CromulentItems
{
    internal class Grug
    {


        //We need our item definition to persist through our functions, and therefore make it a class field.
        public static ItemDef myItemDef;
        public static GameObject RockProjectile;
        public static GameObject RockGhost;


        internal static void Init()
        {
            //Generate the basic information for the item
            CreateItem();
            //CreateProjectile();
            //Now let's turn the tokens we made into actual strings for the game:
            AddTokens();

            //You can add your own display rules here, where the first argument passed are the default display rules: the ones used when no specific display rules for a character are found.
            //For this example, we are omitting them, as they are quite a pain to set up without tools like ItemDisplayPlacementHelper
            var displayRules = new ItemDisplayRuleDict(null);

            //Then finally add it to R2API
            ItemAPI.Add(new CustomItem(myItemDef, displayRules));

            // Initialize the hooks
            hooks();
        }

        private static void CreateItem()
        {
            //First let's define our item
            myItemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Language Tokens, check AddTokens() below.
            myItemDef.name = "Grug";
            myItemDef.nameToken = "Grug";
            myItemDef.pickupToken = "GrugItem";
            myItemDef.descriptionToken = "GrugDesc";
            myItemDef.loreToken = "GrugDesc";
            myItemDef.tags = new ItemTag[1] { ItemTag.Healing };

            //The tier determines what rarity the item is:
            //Tier1=white, Tier2=green, Tier3=red, Lunar=Lunar, Boss=yellow,
            //and finally NoTier is generally used for helper items, like the tonic affliction
#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            myItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001
            //myItemDef.pickupIconSprite = Resources.Load<Sprite>("Assets/CromulentItems/AssetsFolder/icons/JacobBluePNG");
            var icon = Assets.icons.LoadAsset<Sprite>("Grug");
            myItemDef.pickupIconSprite = icon;
            //myItemDef.pickupModelPrefab = Resources.Load<GameObject>("Assets/CromulentItems/AssetsFolder/prefabs/JacobCube");
            var prefab = Assets.prefabs.LoadAsset<GameObject>("GrugCube");
            myItemDef.pickupModelPrefab = prefab;

            RockProjectile = Assets.prefabs.LoadAsset<GameObject>("rock");

            /*May return to this later. The idea was to use a small gray circle prefab (rock) and make it the missile and not the base missile prefab.
            This didnt work as a null reference was called each time, or the rock would not move but it would spawn. 
            This is because "rock" is not a ghost prefab, you have to make it one, but making it one wasnt working.
            PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileProjectile"), "RockGhost").GetComponent<ProjectileController>().ghostPrefab = ProjectileGhostController.Instantiate(RockProjectile);
            RockGhost = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/MissileProjectile"), "rock").GetComponent<ProjectileController>().ghostPrefab;
            //PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile"), "rock").GetComponent<ProjectileController>().ghostPrefab = RockProjectile;
            */
            //Can remove determines if a shrine of order, or a printer can take this item, generally true, except for NoTier items.
            myItemDef.canRemove = true;

            //Hidden means that there will be no pickup notification
            myItemDef.hidden = false;
        }

        private static void hooks()
        {

            On.RoR2.CharacterBody.OnInventoryChanged += (orig, self) =>
            {
                orig(self);
                CharacterBody charBody = self.GetComponent<CharacterBody>();
                charBody.baseCrit += 5f;
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                
                if (damageInfo.attacker && damageInfo.crit)
                {
                    CharacterBody attackerCharacterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimCharacterBody = victim.GetComponent<CharacterBody>();
                    if (attackerCharacterBody?.inventory)
                    {
                        int inventoryCount = attackerCharacterBody.inventory.GetItemCount(myItemDef.itemIndex);
                        var chosenPosition = victimCharacterBody.transform.position;
                        if (inventoryCount > 0)
                        {
                            //here is setting the damage, then the on hit proc damage.
                            float damageCoefficient = (attackerCharacterBody.healthComponent.fullCombinedHealth / 50f);
                            float missileDamage = Util.OnHitProcDamage(damageInfo.damage, attackerCharacterBody.damage, damageCoefficient);

                            //this is firing the missile
                            MissileUtils.FireMissile(attackerCharacterBody.corePosition, attackerCharacterBody, damageInfo.procChainMask, victim, missileDamage, false, GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);

                        }
                    }
                }
                
            };
        }

        //This function adds the tokens from the item using LanguageAPI, the comments in here are a style guide, but is very opiniated. Make your own judgements!
        private static void AddTokens()
        {
            //The Name should be self explanatory
            LanguageAPI.Add("Grug", "grug...");

            //The Pickup is the short text that appears when you first pick this up. This text should be short and to the point, numbers are generally ommited.
            LanguageAPI.Add("GrugItem", "Gain crit. \nOn crit, rock(et)");

            //The Description is where you put the actual numbers and give an advanced description.
            LanguageAPI.Add("GrugDesc", "Gain <style=cIsUtility>+" + 5 + "% crit.</style> \nOn crit, launch a rock that does damage based on about 1/4 your max health. Subsquent items increase crit by 5%");

            //The Lore is, well, flavor. You can write pretty much whatever you want here.
            LanguageAPI.Add("GrugLore", "He's grug you know");
        }
    }
}

