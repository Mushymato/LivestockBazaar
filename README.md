# Livestock Bazaar

Revamp the menu for purchasing farm animals, and provide ways for mod authors to create their own custom animal shop.

## New in 1.3.0: Animal Manage

You can access a new menu through the hay icon top right of shop: the animal manage menu.

In this menu, you can move animals between buildings conveniently, it also allows moving animals across locations.

## Menu Overview

The primary player facing feature of Livestock Bazaar is the custom animal shop menu, which replaces Marnie's animal purchase menu.

### Animal Selection

Animals are displayed in a grid with prices listed. Clicking on one goes to the next page for building and skin selection.

If you cannot afford an animal, they will be greyed out. If you lack the required buildng, they will be shown as a silhouette.

To help you find an animal, there is a button to change the sorting mode, and a search box to find by name.

Sort Modes:

- Name: alphabetical name sort.
- Price: price sort, will sort by currency first, then value.
- House: house (e.g. coop, barn) sort.

Each sort mod has ascending and descending modes.

### Target Building

Once an animal is selected, the menu shows a list of farm buildings (sorted by location) that the animal can live in. You must choose one in order to enable the purchase button.

### Alternate Purchase & Skins

Some animals like cows have alternate purchase variants. Instead of randomly choosing one, this mod allows you to decide whether you want a brown cow or a white cow.

Mods can also add alternate skins for an animal (the ones shown here is Elle's Cuter Barn Animals), you can select these with the arrow buttons. There is an option to have the game pick a random skin, as it would in vanilla.

### Animal Name

The animal name is set through a text input. You can press the dice icon to randomize the name.

### Purchase

Once you have finished the choices, press the purchase button to complete buying your new animal. The menu will stick around until you run out of money or space for the animal, so you can buy as many as desired. A new random name is generated with each purchase.

## Installation

1. Download and install SMAPI.
2. Download and install StardewUI.
3. Download this mod and install to the Mods folder.

## Compatibility

Because this mod completely replace the vanilla animal purchase menu, any mods that work by changing the vanilla menu will either not take effect, or prevent this mod from replacing Marnie's shop. There is a configuration to disable the menu for Marnie's shop specifically, but that essentially makes this mod useless on its own. You can suggest new features for the custom shop menu.

All sprites used for UI elements are from the vanilla game, if a UI interface mod seems incompatible, then that recolour missed some vanilla assets and would need to be updated.

There is no change to any animal mechanics.

## Configuration

- `Vanilla Marnie Stock`: Do not override Marnie's vanilla stock or shop menu, allowing Marnie to sell all animals regardless of custom vendor settings. This option is intended as a backup in case of error or incompatibility, and essentially disables this mod unless you have custom animal vendors from other mods using this mod as a framework.

- `Livestock Sort Mode`: Current sort mode of the livestock portion of shop, can also be changed either in GMCM or in the custom animal shop menu.

- `Livestock Sort Is Ascending`: Current sort direction of the livestock portion of shop, can also be changed from the custom animal shop menu directly.

## Translations

- English

- 简体中文

- Español (by [Diorenis](https://next.nexusmods.com/profile/Diorenis))

- Русский (by [ellatuk](https://github.com/ellatuk))

## Mod Author Guide

Livestock Bazaar provides a framework to create custom animal vendors besides Marnie, with just a few extra custom fields and a tile action.

Please refer to the [author's guide](author-guide.md) for detailed guide.
