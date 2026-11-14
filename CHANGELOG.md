# Changelog

## 2026-11-14

- Added zoomable building overlays for towns, capitals, forts, and ports, with
  building-name tooltips and live refresh after tile edits. Unknown building codes
  receive a generic building marker. WPF binding checks cover 12 building codes
  and prevent fort-level bytes alone from marking empty tiles as buildings.
- Fixed the Production dropdown remaining on the previous selection when changing
  a tile's visual type assigns a new default resource.
- Removed the non-tin Swamp option from the visual dropdown. Existing plain swamp
  tiles retain their rendering definition.
- Fixed grain and other flat-terrain connection masks, updating the edited tile and
  its neighbors when matching terrain is added or removed.
- Corrected desert/tundra matching: desert matches desert, while tundra and furry
  tundra can connect to each other.
- Updated the byte-14 reference and added PowerShell regression checks for all
  18 selectable tile options, mixed terrain, and reference-save grain connections.
- Rebuilt the local Debug executable. Build outputs and the local game installation
  remain excluded from version control.

Validation: Debug build succeeded with existing framework warnings; 216 terrain
cases and 12 mixed-terrain cases passed. Grain checks matched all 167 grain tiles
in the reference `slot3.imp` save. In-game visual validation and coastal blending
remain open; see [the running TODO list](TODO.md).
