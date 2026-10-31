# Imperialism2 SGEditor

Savegame editor for Imperialism 2.

It's mostly a world editor.

Be patient. There is no loading indicator and map editor loads about 10 seconds on fast computers.

## Acknowledgments & Attribution

* **[Reken41](https://github.com/Reken41)** – Original author and creator of the Imperialism 2 Save Game Editor.
* **[jjalexander](https://github.com/jjalexander)** – Contributor (resource reveal fixes and improvements).

> [!NOTE]
> The recent additions and enhancements in this fork are mostly **vibe coded** (AI-assisted rapid prototyping and exploratory development).

## Features

* Show/hide map for each player
* Edit all cities on map:
  * Fort level, expansion, owner
* Edit world map:
  * Owner, production, resources, and more
* Each element has raw data in bytes so you can modify stuff on your own (see [**Tile Byte Structure Reference**](TILE_STRUCTURE.md) and [**Province & City Byte Structure Reference**](PROVINCE_STRUCTURE.md))

### Recent Fork Additions (Vibe Coded)

* **Interactive Map Zooming**:
  * Smooth zoom controls via slider and dedicated zoom in/out/reset buttons.
  * `Ctrl` + `Mouse Wheel` zooming centered on the mouse cursor position.
  * Keyboard shortcuts: `Ctrl` + `+`, `Ctrl` + `-`, and `Ctrl` + `0` (reset).
* **Smoothed Image Rendering**:
  * Improved rendering quality with high-quality bitmap scaling for map tiles.
* **Automated Terrain & Coastline Transition Engine**:
  * Automatically recalculates directional coastline bitmasks, elevation transitions, and slope graphics for edited tiles and their surrounding neighbors.
  * Synchronizes terrain category profiles, elevation levels, and water/land flags.
  * Handles province assignment when converting sea tiles to land.
  * Added additional visual terrain options such as Sea and Swamp (Tin).
* **Tile Byte Display Labels & Reverse-Engineered Documentation**:
  * Updated the 40 raw data columns at the top of the screen with clear, descriptive headers (State, Terrain Category, Road/Rail masks, River paths, Coast/Elevation transitions, Mineral site IDs, Fort levels, etc.).
  * Added complete technical documentation covering all 40 bytes in [**TILE_STRUCTURE.md**](TILE_STRUCTURE.md).
* **Province & City Raw Byte Descriptors & Technical Reference**:
  * Upgraded all 209 raw data columns in the "Cities/Provinces" sub-grid with detailed descriptors (Sovereign Owners, Fortification, Expanded status, Hex Tile Count, Resource & Terrain Tallies, Militia, Unrest, Explored Fog-of-War masks, Seat Tile position, Neighbor Adjacency table, Border Entry Tiles, Hex list, and Economic valuation).
  * Added comprehensive technical documentation covering all 208 bytes of the structure in [**PROVINCE_STRUCTURE.md**](PROVINCE_STRUCTURE.md).

## Screens

![Imperialism2SGEditor-screen1](https://github.com/Reken41/Imperialism2_SGEditor/raw/master/Screens/screen1.jpg)

![Imperialism2SGEditor-screen2](https://github.com/Reken41/Imperialism2_SGEditor/raw/master/Screens/screen2.jpg)

![Imperialism2SGEditor-screen3](https://github.com/Reken41/Imperialism2_SGEditor/raw/master/Screens/screen3.jpg)
