# Livestock Bazaar

Allow alternate livestock (farm animal) vendors, revamp livestock menu.

## Custom Livestock Vendor

To add a custom livestock vendor, add entry to `Data/FarmAnimals` normally, then:

1. Add `Data/FarmAnimals` CustomFields entry `"mushymato.LivestockBazaar/BuyFrom.{{ModId}}_MyVendor": true` to the farm animal. `{{ModId}}_MyVendor` is value that will be referred to as `shopName` in following parts of this guide. Prefix this with your mod id when your vendor is a vanilla NPC.
2. *(Optional)* Add CustomFields entry `"mushymato.LivestockBazaar/BuyFrom.Marnie": false` to prevent Marnie from selling this animal. Marnie is the special hardcoded `shopName` this mod uses to refer to the vanilla animal shop. Unlike all other `BuyFrom` fields, `BuyFrom.Marnie` defaults to `true` and need to be explicitly set to false.
3. Use map tile action `mushymato.LivestockBazaar_Shop` to create a shop.

#### TileAction: mushymato.LivestockBazaar_Shop

```
Usage: mushymato.LivestockBazaar_Shop \<shopName\> [direction] [openTime] [closeTime] [ownerRect: [X] [Y] [Width] [Height]]
```

The arguments of this tile action is identical to "OpenShop" from vanilla.

- `shopName`: Name of shop
- `direction`: Which direction of the tile is valid for interaction, one of `down`, `up`, `left`, `right` for direction, or `any` to allow interaction from all directions.
- `openTime`: `0600` time code format for shop open time, or -1 to skip.
- `closeTime`: `2200` time code format for shop close time, or -1 to skip
- `ownerRect`: 4 consecutive number arguments for `X`, `Y`, `Width`, `Height` of a rectangle. If defined, there must be a `mushymato.LivestockBazaar/Shops` entry for `"shopName"`, and that NPC must be within this rectangle in order to open the shop. Must specify all 4 arguments, or none of them.

#### Custom Asset: mushymato.LivestockBazaar/Shops

Asset defining the properties of a LivestockBazaar shop. Accepts all fields in [Data/Shops](https://stardewvalleywiki.com/Modding:Shops), as well as:

| Property | Type | Default | Notes |
| -------- | ---- | ------- | ----- |
| `BazaarOpenFlag` | OpenFlagType | "Stat" | What flag will allow this shop to ignore open/close time and owner checks.<ul><li>`None`: Always check.</li><li>`Stat`: Skip check if the stat named in `BazaarOpenKey` is set.</li><li>Mail: Skip check if the mailflag named in `BazaarOpenKey` is set.</li></ul> |
| `BazaarOpenKey` | string | "Book_AnimalCatalogue" | Key to use with `BazaarOpenFlag`. The default values combined makes the shop always open once the player has read the [Animal Catalogue](https://stardewvalleywiki.com/Animal_Catalogue) book. |

Making one of these is mandatory if your shop owner is a real NPC and need to be checked for with `ownerRect`.
