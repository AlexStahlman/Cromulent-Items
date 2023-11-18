using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BepInEx;


namespace CromulentItems
{
    internal class Respect
    {


        //We need our item definition to persist through our functions, and therefore make it a class field.
        public static ItemDef myItemDef;


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
            myItemDef.name = "Respect";
            myItemDef.nameToken = "Respect";
            myItemDef.pickupToken = "RespectItem";
            myItemDef.descriptionToken = "RespectDesc";
            myItemDef.loreToken = "RespectDesc";
            myItemDef.tags = new ItemTag[1] { ItemTag.Utility };

            //The tier determines what rarity the item is:
            //Tier1=white, Tier2=green, Tier3=red, Lunar=Lunar, Boss=yellow,
            //and finally NoTier is generally used for helper items, like the tonic affliction
#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
            myItemDef._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
#pragma warning restore Publicizer001
            var icon = Assets.icons.LoadAsset<Sprite>("respectPNG");
            myItemDef.pickupIconSprite = icon;
            var prefab = Assets.prefabs.LoadAsset<GameObject>("RespectOBJ");
            myItemDef.pickupModelPrefab = prefab;

            //Can remove determines if a shrine of order, or a printer can take this item, generally true, except for NoTier items.
            myItemDef.canRemove = true;

            //Hidden means that there will be no pickup notification
            myItemDef.hidden = false;
        }


        private static void hooks()
        {

            On.RoR2.GlobalEventManager.OnHitEnemy += (orig, self, damageInfo, victim) =>
            {
                if (damageInfo.attacker)
                {
                    CharacterBody attackerCharacterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    CharacterBody victimCharacterBody = victim.GetComponent<CharacterBody>();

                    if (attackerCharacterBody?.inventory)
                    {
                        int inventoryCount = attackerCharacterBody.inventory.GetItemCount(myItemDef.itemIndex);
                        if (inventoryCount > 0 && damageInfo.procCoefficient > 0)
                        {
                            float extraTotal = .03f * inventoryCount;
                            damageInfo.procCoefficient += extraTotal;
                        }
                    }
                }
                orig(self, damageInfo, victim);
            };
        }

        //This function adds the tokens from the item using LanguageAPI, the comments in here are a style guide, but is very opiniated. Make your own judgements!
        private static void AddTokens()
        {
            //The Name should be self explanatory
            LanguageAPI.Add("Respect", "Respect");

            //The Pickup is the short text that appears when you first pick this up. This text should be short and to the point, numbers are generally ommited.
            LanguageAPI.Add("RespectItem", "Slightly increase proc coeficcient");

            //The Description is where you put the actual numbers and give an advanced description.
            LanguageAPI.Add("RespectDesc", "Gain <style=cIsUtility>+" + 3 + "%</style><style=cStack>(+" + 3 + "%)</style> proc coeficcient");

            //The Lore is, well, flavor. You can write pretty much whatever you want here.
            LanguageAPI.Add("RespectLore", "#1 Star of Thats Respect Compilations");
        }
    }
}

