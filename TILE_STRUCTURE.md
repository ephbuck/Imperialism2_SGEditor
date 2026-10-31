# Imperialism II Map Tile Byte Structure (`TileStruct`)

This document provides a comprehensive specification of the 40-byte tile data structure (`TileStruct`) used in *Imperialism II: Age of Exploration* save games (`.imp`).

---

## Overview & Architecture

* **World Dimensions**: 60 rows × 108 columns = **6,480 hex tiles**.
* **Storage**: Serialized as a contiguous binary stream of **259,200 bytes** (6,480 tiles × 40 bytes) immediately following the map key header in the `.imp` save file.
* **Hex Grid Directions**:
  * Hex neighbors correspond to 6 directional bitmask flags (`DIR_MASKS`):
    * `0x01` (Bit 0): North-West (`DIR_NW`)
    * `0x02` (Bit 1): North-East (`DIR_NE`)
    * `0x04` (Bit 2): East (`DIR_E`)
    * `0x08` (Bit 3): South-East (`DIR_SE`)
    * `0x10` (Bit 4): South-West (`DIR_SW`)
    * `0x20` (Bit 5): West (`DIR_W`)

---

## Reverse-Engineering Sources

This specification was reverse-engineered from:
1. **Game Executable (`Imperialism II.exe`) & Symbol Map (`Imperialism II.map`)**:
   * Analyzed original engine routines from `UMap.cpp`, `UMapper.cpp`, and `MapIO.cpp`: `TMapMgr::VerifyMap`, `TMapMgr::ReadFrom`, `TMapMgr::WriteTo`, `TMapMgr::SetToPlainTerrain`, `BitmaskToRiver`, `CreateMapDrawing`, and diagnostic map dumpers.
2. **Forensic Save File Verification**:
   * Cross-verified across multiple campaign and multiplayer saves (`slot0.imp` – `slot8.imp`, `mult0.imp`).

---

## Complete 40-Byte Reference Table

| Byte | Field Name / Editor Label | Data Type | Value Range | Description & Engine Usage |
|:---:|:---|:---:|:---:|:---|
| **0** | `[0] State` | `uint8` | `0, 2, 3` | **Tile Military / Combat State**: `0` for peaceful land/sea, `2` on active/disputed/strategic province border tiles or contested zones, `3` during active multiplayer combat. |
| **1** | `[1] Terrain` | `uint8` | `0 – 7` | **Terrain Category (`TERRAIN_CAT`)**: Basic engine terrain class:<br>• `0` = Plains<br>• `1` = Forest<br>• `2` = Hills<br>• `3` = Mountains<br>• `4` = Swamp<br>• `5` = Water / Sea<br>• `6` = Desert / Tundra<br>• `7` = Farmland |
| **2** | `[2] Elev Mask` | `uint8` | `0 – 62` | **Elevation / Slope Mask**: Directional 6-bit mask indicating adjacent slope transitions across hex borders. |
| **3** | `[3] River Exit` | `uint8` | `0 – 58` | **River Exit Direction Bitmask**: Directional exit bits indicating which edges a river flows across. Non-zero only on tiles with rivers; checked against `gRiverTable`. |
| **4** | `[4] Owner 1` | `uint8` | `0 – 99` | **Sovereign Nation / Sea Zone**: Country index (`0`–`21`) for Great Powers and Minor Nations. Numbers `22`–`99` represent Sea Zone IDs on ocean tiles. |
| **5** | `[5] Owner 2` | `uint8` | `0 – 99` | **Controlling Nation / Occupant**: Current occupying or controlling nation. Differs from Owner 1 when a province is under military occupation. |
| **6** | `[6] Region` | `uint8` | `1 – 12, 255` | **World Macro-Region / Zone**: Continent or geographic sector identifier; `255` for open ocean. |
| **7** | `[7] Road Dir` | `uint8` | `0 – 63` | **Road Connections Bitmask**: 6-bit direction flags (`1, 2, 4, 8, 16, 32`) indicating adjacent hex edges connected by roads. |
| **8** | `[8] Rail Dir` | `uint8` | `0 – 63` | **Rail Connections Bitmask**: 6-bit direction flags indicating adjacent hex edges connected by railroads. |
| **9** | `[9] River Delta` | `uint8` | `0 – 60` | **River Branch / Delta Index**: Graphic index for river junctions, forks, and coastal deltas. |
| **10** | `[10] Canal/Coast` | `uint8` | `0 – 52` | **Water Edge / Canal Transition**: Blending index for canals, estuaries, and inland water interfaces. |
| **11** | `[11] Coast N` | `uint8` | `0 – 207` | **Northern Coastline Transition**: Directional coastline bitmask for North, NW, and NE coast transitions. |
| **12** | `[12] Coast S` | `uint8` | `0 – 183` | **Southern Coastline Transition**: Directional coastline bitmask for South, SW, and SE coast transitions. |
| **13** | `[13] Slope Trans` | `uint8` | `0 – 255` | **Hill / Mountain Slope Mask**: Transition bitmask between elevated terrain (hills/mountains) and flat ground. |
| **14** | `[14] Peak Trans` | `uint8` | `0 – 63` | **Mountain Peak Transition Mask**: Blending bitmask for high mountain ridges and alpine peaks. |
| **15** | `[15] Trans Sprite`| `uint8` | `0 – 63` | **Transition Sprite Variant**: Graphic sprite variation index for corner blending and shore tiles. |
| **16** | `[16] Prod Lvl` | `uint8` | `0 – 4` | **Production / Development Level**: Infrastructure level of the tile's resource (`0` = undeveloped wild land, `1`–`4` = upgraded output levels). |
| **17** | `[17] Disc By` | `uint8` | `0 – 127` | **Discovered By Player Bitmask**: Bitmask (`1 << playerID`) tracking which countries have prospected and discovered the mineral or resource on this tile. |
| **18** | `[18] Surveyed` | `uint8` | `0, 1` | **Prospected Flag**: `1` if the tile has been surveyed by an engineer/prospector; `0` if uncharted/hidden. |
| **19** | `[19] Yield Bonus`| `uint8` | `0 – 12` | **Resource Yield Multiplier**: Bonus extraction yield or deposit density level. |
| **20** | `[20] Mineral ID` | `uint8` | `0 – 89, 255` | **Deposit / Mine Site Table Index**: Sequential index (`0`..`89`) into the game's global mineral table; `255` if no deposit. |
| **21** | `[21] Type (res)` | `uint8` | `0 – 27, 255` | **Resource Type**: Active exploitable resource on this hex (see Resource ID table below); `255` = None. |
| **22** | `[22] Type (vis)` | `uint8` | `0 – 18` | **Visual Terrain Graphic**: Visual terrain sprite displayed on the world map (see Visual Terrain table below). |
| **23** | `[23] Province ID`| `uint8` | `0 – 151, 255` | **Province ID**: Province number (`0`–`151`) for land tiles; `255` for sea. |
| **24** | `[24] Land/Sea` | `uint8` | `0, 255` | **Land / Water Flag**: `0` = Land, `255` = Water. |
| **25** | `[25] Port Zone` | `uint8` | `2 – 255` | **Naval Access / Port Zone ID**: Only non-`255` on coastal water tiles providing port entry/exit to adjacent land provinces. |
| **26** | `[26] Port Country`| `uint8` | `0 – 21, 255` | **Port Sovereign Link**: Country ID for tiles carrying an active naval trade port link; `255` = None. |
| **27** | `[27] Reserved` | `uint8` | `255` | **Reserved / Struct Padding 1**: Always `255`. |
| **28** | `[28] Infra Mask` | `uint8` | Bitmask | **Infrastructure Facility Bitmask**: Checked directly by pathfinding and engine routines:<br>• Bit 1 (`0x02`): Town / City link<br>• Bit 2 (`0x04`): Port facility<br>• Bit 4 (`0x10`): Railroad network<br>• Bit 5 (`0x20`): Road network |
| **29** | `[29] Building` | `uint8` | `0 – 63` | **Building / Settlement Type**:<br>• `0` = None<br>• `4`, `34`, `38`, `42`, `46` = Town architectural styles<br>• `54` = Minor Nation Capitol<br>• `55` = Great Power Capitol<br>• `62` = Fort<br>• `63` = Port / Naval Base |
| **30** | `[30] Reserved` | `uint8` | `0` | **Reserved / Struct Padding 2**: Always `0`. |
| **31** | `[31] Explored` | `uint8` | `0 – 255` | **Fog of War / Visibility Mask**: Bitmask of Great Powers (`1 << playerID`) that have explored this tile. |
| **32** | `[32] Fort Lvl` | `uint8` | `0, 2, 4` | **Fortification Level**: `0` = None, `2` = Fort, `4` = Fortress / Citadel. |
| **33** | `[33] Expand City`| `uint8` | `0, 1` | **City Expansion State**: `0` = Standard town, `1` = Expanded settlement / metropolis. |
| **34** | `[34] Garrison` | `uint8` | `0 – 4` | **Stationed Garrison / Militia Strength**: Garrison troops stationed inside the fort/city. |
| **35** | `[35] Reserved` | `uint8` | `0` | **Reserved / Struct Padding 3**: Always `0`. |
| **36** | `[36] Reserved` | `uint8` | `0` | **Reserved / Struct Padding 4**: Always `0`. |
| **37** | `[37] Civ Unit L` | `uint8` | `0 – 255` | **Civilian Unit Reference (Low Byte)**: Reference index to active engineer / merchant / civilian unit on the hex. |
| **38** | `[38] Civ Unit H` | `uint8` | `0 – 255` | **Civilian Unit Reference (High Byte)**: High byte of civilian unit reference. |
| **39** | `[39] Army Ref` | `uint8` | `0 – 255` | **Military Regiment / Fleet Reference**: Reference to military army or naval fleet stationed on the hex. |

---

## Lookup Tables

### Resource ID (`Byte 21`)

| ID | Resource | ID | Resource | ID | Resource |
|:---:|:---|:---:|:---|:---:|:---|
| `0` | Wool | `7` | Sugar Cane | `21` | Fish |
| `1` | Timber | `8` | Tobacco | `22` | Livestock |
| `2` | Tin | `9` | Fur | `23` | Spices |
| `3` | Copper | `10` | Horses | `24` | Silver |
| `4` | Iron Ore | `20` | Grain | `25` | Gold |
| `5` | Coal | | | `26` | Gems |
| `6` | Cotton | | | `27` | Diamonds |
| | | | | `255`| None |

### Visual Terrain (`Byte 22`)

| ID | Terrain Graphic | ID | Terrain Graphic |
|:---:|:---|:---:|:---|
| `0` | Sea | `10` | Swamp |
| `1` | Plains | `11` | Desert |
| `2` | Cotton Plantation | `12` | Tundra |
| `3` | Livestock | `13` | Hardwood Forest |
| `4` | Horse Ranch | `14` | Swamp (Tin) |
| `5` | Grain Farm | `15` | Scrub Forest |
| `6` | Spice Orchard | `16` | Sugar Cane |
| `7` | Fertile Hills (Sheep) | `17` | Tobacco Plantation |
| `8` | Barren Hills | `18` | Furry Tundra |
| `9` | Mountains | | |

