# Changelog

## 2026-11-14

- Road and Rail selections are now mutually exclusive. Checking either deselects
  the other and recalculates both ends of adjacent connections.
- Added left-button drag panning while retaining right-button dragging and normal
  click selection. Drag releases do not select tiles or paint ownership; scrollbars
  keep their standard behavior and mouse capture/cursor reset after dragging.
- Added Road/Rail checkboxes, automatic reciprocal neighbor connections, route
  overlays, and companion-file persistence for isolated/mixed checkbox selections.
  Corrected completed/planned transport byte labels and added transport regression checks.
  The properties panel now scrolls so its controls remain reachable.
- Added a selected-tile development dropdown (0–4) after Production and before the
  province/owner controls, synchronized with the raw development byte.
- Ocean-to-land edits now assign Fish to adjacent water tiles and refresh their
  resource icons. Existing land and nonadjacent water keep their resources.
- Converted water tiles now display as unassigned land and can be assigned to a
  neighboring province through the transfer control. Province membership, resource
  counts, and ownership update together; saving is blocked until new land is assigned.
- Added small province-name labels beneath town and capital icons, with full-name
  tooltips and zoom scaling. Labels extend past the tile so names stay readable.
- Added experimental transfer of ordinary border tiles to adjacent provinces of
  either country, with destination ownership, connectivity/capacity checks, province membership and
  resource tally updates, reciprocal adjacency/entry tables, and border refresh.
  See `Tests/PROVINCE_TRANSFER_CHECKLIST.md` for remaining compatibility checks.
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
