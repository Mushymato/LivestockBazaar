# Livestock Bazaar

Revamp the menu for purchasing farm animals, and provide ways for mod authors to create their own custom animal shop.

## New Menu Overview

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

Once you have finished the choices, press the purchase button to complete buying your new animal. The menu will stick around until you run out of money or space for the animal, so you can buy as many as desired.

## Installation

1. Download and install SMAPI.
2. Download and install StardewUI.
3. Download this mod and install to the Mods folder.

## Compatibility

Because this mod completely replace the vanilla animal purchase menu, thus any mods that work by changing the vanilla menu will not take effect. I 

## Configuration

- `Vanilla Marnie Stock`: Do not override Marnie's vanilla stock or shop menu, allowing Marnie to sell all animals. This option is intended as a backup in case of error or incompatibility.

- `Livestock Sort Mode`: Current sort mode of the livestock portion of shop, can also be changed either in GMCM or in the custom animal shop menu.

- `Livestock Sort Is Ascending`: Current sort direction of the livestock portion of shop, can also be changed from the custom animal shop menu directly.

## Mod Author Guide

Livestock Bazaar provides a framework to create custom animal vendors besides Marnie, with just a few extra custom fields and a tile action.

Please refer to the [author's guide](author-guide.md) for detailed guide.
