# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.3] - 2025-12-06

### Added
- Display for days to mature and days to produce.

## [1.5.2] - 2025-11-23

### Fixed
- Alt purchases not showing specific name when selected.

## [1.5.1] - 2025-11-02

### Fixed
- Alt purchases not showing special description when selected.

## [1.5.0] - 2025-10-13

### Added
- The animal manage menu can now access the vanilla animal details menu via right-click/tool use button. You can remotely sell animals through here.

## [1.4.0] - 2025-07-22

### Added
- Item query mushymato.LivestockBazaar_PET_ADOPTION for filtering for specific pet/breed
- Trigger action mushymato.LivestockBazaar_AdoptPet for adopting a pet

### Changed
- Backend changes for android compat

## [1.3.0] - 2025-07-04

### Added

- New menu for managing animals, access through the shop or through Iconic Framework
- More logging for broken textures.

### Fixed

- Marnie portrait disappearing in the shop.

## [1.2.2] - 2025-05-29

### Fixed

- Support for extra animal config produce overrides + extra drops
- Fixed compatiblity problem with Animal Squeeze Through + EAC extra houses

## [1.2.1] - 2025-04-13

### Added

- Add an indicator for number of animals owned on the animal select page
- List what animals are in a building in the tooltip
- New interact method `LivestockBazaar.OpenBazaar, LivestockBazaar: InteractShowLivestockShop`

## [1.2.0] - 2025-03-25

### Changed

- Rearrange position of alt purchase.

### Fixed

- Handle shop icon case when there there is no source rect set.
- Actually fix the skin purchase thing.

## [1.1.6] - 2025-03-21

### Fixed

- Null check for produced items

## [1.1.5] - 2025-03-12a

### Added

- Show produce items (vanilla).

### Fixed

- Let default skin be pickable.

## [1.1.4] - 2025-03-12

### Fixed

- Bug with null required house.

## [1.1.3] - 2025-03-12

### Fixed

- Null text issues.

## [1.1.2] - 2025-01-30

### Fixed

- Add display for required building.
- Switch shop dialogues over to tokenized text (e.g. [LocalizedText Key]).
- Shop displaying when no animal is purchasable

## [1.1.1] - 2025-01-29

### Fixed

- Fix a vanilla bug related to the conversation topic not working in any language besides english.
    - Also add a mail flag `mushymato.LivestockBazaar_purchasedAnimal_{animalType}` and trigger `mushymato.LivestockBazaar_purchasedAnimal`.
- Sort Modes not translating properly
- Price sort mode should take account into whether animal is buyable

## [1.1.0] - 2024-12-26

### Added

- Translation to español by [Diorenis](https://next.nexusmods.com/profile/Diorenis)
- Revised menu page 2 to add display for currency
- Use vanilla method to check for building is upgrade for SVE compat reasons

## [1.0.0] - 2024-12-26

### Added

- Initial release
