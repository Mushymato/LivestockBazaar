# Author Guide

This page covers how to add a new livestock bazaar shop. It is assumed that you already know how to [add a new farm animal](https://stardewvalleywiki.com/Modding:Animal_data) and how to [add a tile property](https://stardewvalleywiki.com/Modding:Maps). Shop data is also relevant for adding owner portraits.

## Quick Start

To add a custom livestock bazaar shop, add entry to `Data/FarmAnimals` normally and ensure that the animal functions correctly when bought from Marnie's shop.
Then, follow these steps to make your farm animal appear for sale in a custom shop:

1. Add `Data/FarmAnimals` CustomFields entry `"mushymato.LivestockBazaar/BuyFrom.{{ModId}}_MyVendor": true` to the farm animal. `{{ModId}}_MyVendor` is value that will be referred to as `shopName` or `<shopName>` in following parts of this guide. It's recommended to prefix this name with your mod id, even when your vendor is a vanilla NPC.
    - Note: `PurchasePrice` of the farm animal must be non-zero, even if you intend to use `TradeItemId` for the actual purchase. This behavior is kept since there aren't any other ways to mark a farm animal as "not for sale".
2. *(Optional)* Add CustomFields entry `"mushymato.LivestockBazaar/BuyFrom.Marnie": false` to prevent Marnie from selling this animal. Marnie is the special hardcoded `shopName` this mod uses to refer to the vanilla animal shop. Unlike all other `BuyFrom` fields, `BuyFrom.Marnie` defaults to `true` and need to be explicitly set to false.
3. *(Optional)* Add a entry named `shopName` to `mushymato.LivestockBazaar/Shops`, to provide an owner portrait and other advanced settings.
4. Use map tile action `mushymato.LivestockBazaar_Shop` to create a shop.

#### Why is my animal still purchasable at Marnie's?

Livestock bazaar only applies it's rules about animal availability when it gets to apply the custom menu. If you are seeing the vanilla animal purchase menu at Marnie's for any reason (e.g. config option, mod incompatibility), then she will get to sell all animals as if livestock bazaar is not installed.

Custom shops will always use livestock bazaar menu.

### Data/FarmAnimals CustomFields

When using these for a particular shop, replace `<shopName>` with the shop's actual ID, e.g. `mushymato.LivestockBazaar/BuyFrom.{{ModId}}_MyVendor`.

A special shop `Marnie` is always available and represents the animal shop at Marnie's ranch. Fields can be applied to this shop in same way as custom shops.

#### BuyFrom

| Field | Type | Notes |
| ----- | ---- | ----- |
| `mushymato.LivestockBazaar/BuyFrom.<shopName>` | bool | If `true`, the animal will be available in `<shopName>` |
| `mushymato.LivestockBazaar/BuyFrom.<shopName>.Condition` | string | A game state query used to conditionally offer an animal. |

These 2 fields control whether an animal is available for a given shop. An animal will be available for `Marnie` by default unless `mushymato.LivestockBazaar/BuyFrom.Marnie` is set to false.

#### TradeItemId and TradeItemAmount

| Field | Type | Notes |
| ----- | ---- | ----- |
| `mushymato.LivestockBazaar/TradeItemId.<shopName>` | string | Let you purchase the animal with something besides money, for `<shopName>` only. |
| `mushymato.LivestockBazaar/TradeItemId` | string| Let you purchase the animal with an item, for all shops including Marnie's. |
| `mushymato.LivestockBazaar/TradeItemAmount.<shopName>` | int | Amount of trade items needed, for `<shopName>` only. |
| `mushymato.LivestockBazaar/TradeItemAmount` | string | Amount of trade items needed, for all shops including Marnie's. |

These fields allows an animal to be purchased with items instead of money.

Special TradeItemId values:
- `(O)858`: Qi Gems.
- `(O)73`: Golden Walnuts. WARNING: This mod does not add extra ways to get golden walnuts, use with caution.
- `(O)GoldCoin`: This item's icon will be used for money.

This has been tested to work with [Unlockable Bundles](https://www.nexusmods.com/stardewvalley/mods/17265) custom currency.

If you set a `TradeItemId` without setting a `TradeItemAmount`, the shop will require `PurchasePrice` number of items. For the reverse scenario of `TradeItemAmount` without `TradeItemId`, the animal will be sold for `TradeItemAmount` amount of money.

### TileAction: mushymato.LivestockBazaar_Shop

```
Usage: mushymato.LivestockBazaar_Shop \<shopName\> [direction] [openTime] [closeTime] [ownerRect: [X] [Y] [Width] [Height]]
```

The arguments of this tile action is identical to "OpenShop" from vanilla, every argument after `shopName` is optional.

- `shopName`: Name of livestock bazaar shop
- `direction`: Which direction of the tile is valid for interaction, one of `down`, `up`, `left`, `right` for direction, or `any` to allow interaction from all directions.
- `openTime`: `0600` time code format for shop open time, or -1 to skip.
- `closeTime`: `2200` time code format for shop close time, or -1 to skip
- `ownerRect`: 4 consecutive number arguments for `X`, `Y`, `Width`, `Height` of a rectangle. If defined, there must be a `mushymato.LivestockBazaar/Shops` entry for `shopName`, and that NPC must be within this rectangle in order to open the shop. Must specify all 4 arguments, or none of them.

The vanilla TileAction `"AnimalShop"` at Marnie's is equivalent to:
```
mushymato.LivestockBazaar_Shop Marnie down -1 -1 12 14 2 1
```
But for compatibility reasons, Marnie shop override is implemented as a Harmony prefix rather than change of tile action.

When the shop has a `mushymato.LivestockBazaar/Shops` entry with `ShopwShopDialog` set to true and valid `ShopId` set, this tile action will open dialogue box that allows you to choose between the item shop or the livestock bazaar shop. A similar pair of options named `ShowPetShopDialog` and `PetShopId` exist for pet shops.

#### Action: mushymato.LivestockBazaar_Shop

```
Usage: mushymato.LivestockBazaar_Shop \<shopName\>
```

This is the trigger action action way to open a livestock bazaar shop. It can be used from TriggerActions, dialogue, and more. It only accepts the shop name and directly opens the livestock bazaar shop without any question dialog.

### Custom Asset: mushymato.LivestockBazaar/Shops

This is a custom asset that let you provide some additional configurations to a livestock bazaar shop. Each entry is keyed by `shopName`.
Marnie's shop only uses `Owners` from this asset, as her other services are not overwritten by this mod.

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `Owners` | List\<ShopOwnerData\> | _null_ | A list of shop owners, identical to the Owners property on Data/Shops. |
| `ShopId` | string | _null_ | String ID to an entry in `Data/Shops`. |
| `PetShopId` | string | _null_ | String ID to an entry in `Data/Shops`, this one is meant to be used with a shop similar to `PetAdoption` but there's no strict check. |
| `OpenFlag` | OpenFlagType | "Stat" | One of `"None", "Stat", "Mail"`, used in conjunction with `OpenKey` to determine if the NPC shop's open/close hours and NPC prescence can be ignored. This setting is ignored for Marnie, who always use vanilla `Book_AnimalCatalogue` logic. |
| `OpenKey` | string | `"Book_AnimalCatalogue"` | String name of the stat or mailflag. If this is set, then the usual open/close time and NPC prescence will not be checked. The default value `Book_AnimalCatalogue` refers to the Animal Catalogue book that grants 24/7 access to animal shop in vanilla. |
| `ShowShopDialog` | bool | true | If true and `ShopId` is a valid shop, show a dialog option to let player open the supplies shop. |
| `ShowPetShopDialog` | bool | true | If true and `PetShopId` is a valid shop, show a dialog option to let player open the pet shop. |
| `ShopDialogSupplies` | string | _ | Display string for the supplies shop. |
| `ShopDialogAnimals` | string | _ | Display string for the animal shop. |
| `ShopDialogAdopt` | string | _ | Display string for the pet adoption shop. |
| `ShopDialogLeave` | string | _ | Display string for exiting the dialog. |

#### Owners

There are up to 3 possible lists of ShopOwnerData in this custom asset, they are picked in this order.
1. `Owners`
2. `Data/Shops[ShopId].Owners`
3. `Data/Shops[PetShopId].Owners`

The first non null list will be used. No attempt is made to "fall" further down the list should none of the owners in the chosen list match a given condition.

#### ShopId vs PetShopId

The main distinction between the supplies shop (`ShopId`) and the pet adoption shop (`PetShopId`) is that the player cannot access the shop behind `PetShopId` until they are eligible for a second pet (first pet at max hearts, or no pets and year 2).

Livestock Bazaar makes no changes to either shop's mechanics, they both use the vanilla shop menu.

#### OpenFlag and OpenKey

To make a custom book that acts similar to `Book_AnimalCatalogue` for your livestock bazaar shop, make a book item and then put that item's unqualified id into `OpenKey`.

Mail flags are offered because there are more ways to set it compared to game stat, change `OpenFlags` to `"Mail"` to use mail flag.

#### Visual Theme

The Bazaar menu respects `"VisualTheme"` in either `ShopId` or `PetShopId`. For example of vanilla menus with alternate themes, check Mr Qi's shop.

### ShopDialog Default Values

The default values are:

- ShopDialogSupplies: `"[LocalizedText Strings\\Locations:AnimalShop_Marnie_Supplies]"` (Supplies Shop)
- ShopDialogAnimals: `"[LocalizedText Strings\\Locations:AnimalShop_Marnie_Animals]"` (Purchase Animals)
- ShopDialogAdopt: `"[LocalizedText Strings\\1_6_Strings:AdoptPets]"` (Adopt Pets)
- ShopDialogLeave: `"[LocalizedText Strings\\Locations:AnimalShop_Marnie_Leave]"` (Leave)

The default values are [tokenizable strings](https://stardewvalleywiki.com/Modding:Tokenizable_strings), modded shops may use tokenizable strings or direct strings (like i18n keys).

### Extras

#### Conversation Topic: purchasedAnimal_{animalType}

Helps fix issue of some dialogue never showing up in other languages, because the translated name is used in the vanilla conversation topic.
This version of the topic uses the animal's internal Id.

#### MailFlag: mushymato.LivestockBazaar_purchasedAnimal_{animalType}

Mail flag indicating an animal had been purchased at least once.

#### Trigger: mushymato.LivestockBazaar_purchasedAnimal

A trigger raised when the player purchases an animal, passes 2 triggerArgs AnimalHouse and FarmAnimal.

#### InteractMethod: LivestockBazaar.OpenBazaar, LivestockBazaar: InteractShowLivestockShop

This interact method can be set via Data/Machines to open a shop by interacting with a big craftable.

CustomFields can be used to control the behavior:
- mushymato.LivestockBazaar_ShopTile: all the arguments for TileAction is valid through this field and will behave similarly, this version spawns the dialogue.
- mushymato.LivestockBazaar_Shop: directly opens animal shop of this ID.

Quirk with using this is that, should the machine have any item processing rules and you attempt to drop in an item, the menu **WILL** appear even after the item is dropped into machine.

#### Item Query: mushymato.LivestockBazaar_PET_ADOPTION [petId] [breedId] [ignoreBaseProce] [ignoreCanBeAdoptedFromMarnie]

Item query for usage in custom pet shops (NOT animal shops!), works similar to vanilla `PET_ADOPTION` item query but allows filtering.
- petId (default `T`): this is the top level pet id (i.e. Cat, Dog, Turtle), T means any pet.
- breedId (default `T`): this is the breed id for particular appearance for pet, T means any breed.
- ignoreBasePrice (default `false`): breed price is normally sourced from `Data/Pets`, setting this to `true` allows the item query's price field to take effect, otherwise use `false`.
- ignoreCanBeAdoptedFromMarnie (default `false`): setting this to `true` makes this query ignore the CanBeAdoptedFromMarnie field, which allows you to ban this pet from Marnie's PetAdoption shop but still access it through this item query.
If no arguments are given then this behaves identical to `PET_ADOPTION`.

#### Trigger Action Action: mushymato.LivestockBazaar_AdoptPet \<petId\> \<breedId\> \<petName\>

Instantly sends a pet to the farm with a specific name, skipping the naming step. All 3 arguments are required.
- petId: this is the top level pet id (i.e. Cat, Dog, Turtle), must provide specific pet.
- breedId: this is the breed id for particular appearance for pet, must provide specific breed.
- petName: this will be the name of the new pet.
