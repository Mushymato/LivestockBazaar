# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
