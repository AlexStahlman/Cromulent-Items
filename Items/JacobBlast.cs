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

namespace CromulentItems
{
    internal class JacobBlast
    {


        //We need our item definition to persist through our functions, and therefore make it a class field.
        public static ItemDef myItemDef;

        // Set luck amount in one location
        public static float luckAmount = 10f;
        public static float numItems = 0f;

        // Set equipment cd in one location
        public static float cooldownAmount = 1f - (30f * .01f);

        internal static void Init()
        {
            //Generate the basic information for the item
            CreateItem();

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
            myItemDef.name = "JacobBlast";
            myItemDef.nameToken = "JacobBlast";
            myItemDef.pickupToken = "JacobBlastItem";
            myItemDef.descriptionToken = "JacobBlastDesc";
            myItemDef.loreToken = "JacobBlastDesc";
            myItemDef.tags = new ItemTag[1] { ItemTag.Utility };

            //The tier determines what rarity the item is:
            //Tier1=white, Tier2=green, Tier3=red, Lunar=Lunar, Boss=yellow,
            //and finally NoTier is generally used for helper items, like the tonic affliction
#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            myItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001
            //myItemDef.pickupIconSprite = Resources.Load<Sprite>("Assets/CromulentItems/AssetsFolder/icons/JacobBluePNG");
            var icon = Assets.icons.LoadAsset<Sprite>("JacobBluePNG");
            myItemDef.pickupIconSprite = icon;
            //myItemDef.pickupModelPrefab = Resources.Load<GameObject>("Assets/CromulentItems/AssetsFolder/prefabs/JacobCube");
            var prefab = Assets.prefabs.LoadAsset<GameObject>("JacobCube");
            myItemDef.pickupModelPrefab = prefab;
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
            };

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                orig(self, damageInfo, victim);
                if (damageInfo.attacker && damageInfo.procCoefficient > 0)
                {
                    CharacterBody attackerCharacterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimCharacterBody = victim.GetComponent<CharacterBody>();

                    if (attackerCharacterBody?.inventory)
                    {
                        int inventoryCount = attackerCharacterBody.inventory.GetItemCount(myItemDef.itemIndex);
                        if (inventoryCount > 0 && Util.CheckRoll(inventoryCount/2, attackerCharacterBody.master))
                        {
                            attackerCharacterBody.AddTimedBuff(RoR2Content.Buffs.TonicBuff, 2);
                        }
                    }
                }

            };
        }

        //This function adds the tokens from the item using LanguageAPI, the comments in here are a style guide, but is very opiniated. Make your own judgements!
        private static void AddTokens()
        {
            //The Name should be self explanatory
            LanguageAPI.Add("JacobBlast", "Jacob Blast");

            //The Pickup is the short text that appears when you first pick this up. This text should be short and to the point, numbers are generally ommited.
            LanguageAPI.Add("JacobBlastItem", "Chance to activate 'Tonic'");

            //The Description is where you put the actual numbers and give an advanced description.
            LanguageAPI.Add("JacobBlastDesc", " <style=cIsUtility>" + numItems/2 + "</style> chance to Activate a Tonic Effect.");

            //The Lore is, well, flavor. You can write pretty much whatever you want here.
            LanguageAPI.Add("JacobBlastLore", "Blue Liquid Amalgomation");
        }
    }
}
