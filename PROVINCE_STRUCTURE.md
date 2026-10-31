# Imperialism II Province & City Byte Structure (`Province`)

This document provides a comprehensive technical specification of the 208-byte City / Province data structure (`Province`) used in *Imperialism II: Age of Exploration* save games (`.imp`).

---

## Overview & Architecture

* **Total Provinces**: Exactly **152 provinces** (`0x98`), encompassing all Old World and New World land territories.
* **Storage Location**: Serialized immediately after the 6,480 map tiles (259,200 bytes) in the `.imp` save stream.
* **Record Structure**:
  * **Fixed Binary Payload**: Exactly **208 bytes** (`0x00`–`0xCF`).
  * **Variable-Length Pascal Name**:
    * **Length Byte** (offset 208): 1 byte storing the string length $L$ (0–32).
    * **ASCII Character Array** (offset 209 to $208+L$): String characters of the province/city name.
* **Termination**: The province list is terminated in the save file stream by the 2-byte marker `0xFFFF` (`255, 255`).
* **In-Memory Size**: In the original engine executable, `sizeof(Province)` is **212 bytes** (`0xD4`), because a 4-byte runtime object pointer (`TCity*`) is stored at offset 188 (`0xBC`).

---

## Reverse-Engineering Sources

This specification was established through disassembly and forensic reverse engineering:
1. **Game Executable (`Imperialism II.exe`) & Symbol Map (`Imperialism II.map`)**:
   * Analyzed original engine routines in `UMap.obj`, `UCity.obj`, and `UCountry.obj`:
     * `TMapMgr::ReadFrom` (province reading loop, clamping fort levels to 3, loading Pascal strings).
     * `TMapMgr::ReverseProvinceEnds` (endianness word/dword byte swapping routine detailing all multi-byte fields).
     * `TMapMgr::SetCountySeat`, `TMapMgr::ConvertCapitolToTown`, `TMapMgr::GetProvinceNeighborTile`, `TMapMgr::CalculateProvinceValues`, `TMapMgr::GetProvinceResources`, `TMapMgr::LandPrice`, and `TMapMgr::CountTilesInProvince`.
2. **Forensic Save File Verification**:
   * Statistical correlation and verification across all 152 provinces in campaign save files (`slot0.imp` – `slot8.imp`).

---

## Complete 208-Byte Field Reference Table

| Offset | Field Name / Editor Label | Data Type | Value Range | Description & Engine Usage |
|:---:|:---|:---:|:---:|:---|
| **0** | `[0] Owner 1` | `uint8` | `0 – 21, 255` | **Current Sovereign Owner**: Country ID governing the province (`0`=France, `1`=England, `2`=Sweden, `3`=Holland, `4`=Portugal, `5`=Spain, `6`–`21`=Minor nations / Native tribes, `255`=Wilderness). |
| **1** | `[1] Owner 2` | `uint8` | `0 – 21, 255` | **Original / Legal Sovereign**: Original native owner or sovereign legal nation before conquest or occupation. |
| **2** | `[2] Expanded` | `uint8` | `0, 1` | **City Expanded Flag (`IS_EXPANDED`)**: `1` if town has expanded its urban footprint and outer fortifications; `0` for normal. |
| **3** | `[3] Fort Lvl` | `uint8` | `0 – 3` | **Fortification Level (`FORT_LEVEL`)**: Clamped to `0`–`3` by engine:<br>• `0` = None / Unfortified<br>• `1` = Palisade / Wooden Fort<br>• `2` = Stone Fort<br>• `3` = Fortress / Citadel |
| **4** | `[4] Tile Count` | `uint8` | `11 – 21` | **Province Hex Count**: Total number of map hexes belonging to this province. Controls how many active entries exist in the `TileIndices` array (offset 124). |
| **5** | `[5] Reserved` | `uint8` | `0` | **Reserved / Struct Padding**: Always `0`. |
| **6** | `[6] Town Size` | `uint8` | `0 – 6` | **Settlement Tier / City Size**: Size of the county seat (`0`=uncolonized, `1`=village, `2`=town, `3`=city, `4`–`6`=major capital/metropolis). Set via `SetTownSize`. |
| **7** | `[7] Port Status` | `uint8` | `0, 1, 2, 255` | **Harbor / Port Facility**: `0` = inland / no port, `1` = active sea port / naval base, `2` = tribal / minor coastal anchorage. Evaluated by `HasPortInProvince`. |
| **8** | `[8] Sugar` | `uint8` | `0 – 5` | **Sugar Cane Tile Tally**: Total number of tiles containing Sugar Cane (Resource 7) in this province. |
| **9** | `[9] Grain` | `uint8` | `0 – 11` | **Grain / Farmland Tile Tally**: Total number of arable crop / grain farm hexes in this province. |
| **10** | `[10] Slope` | `uint8` | `0 – 2` | **Hill / Mountain Slope Tally**: Count of elevated hill/slope hexes in this province. |
| **11** | `[11] Forest` | `uint8` | `0 – 2` | **Forest / Timber Tile Tally**: Count of woodland/timber extraction tiles in this province. |
| **12** | `[12] Wool` | `uint8` | `0 – 5` | **Pasture / Wool Tile Tally**: Count of sheep pasture / livestock hexes in this province. |
| **13** | `[13] Swamp` | `uint8` | `0 – 4` | **Swamp / Marsh Tile Tally**: Count of marsh/wetland tiles in this province. |
| **14** | `[14] Tin` | `uint8` | `0 – 5` | **Tin Deposit Tally**: Number of mineral tiles containing Tin deposits (Resource 2). |
| **15** | `[15] Silver` | `uint8` | `0 – 3` | **Silver Deposit Tally**: Number of mineral tiles containing Silver deposits. |
| **16** | `[16] Gold` | `uint8` | `0 – 4` | **Gold Deposit Tally**: Number of mineral tiles containing Gold deposits. |
| **17** | `[17] Gems` | `uint8` | `0 – 2` | **Gems / Diamonds Deposit Tally**: Number of precious stone / diamond hexes in this province. |
| **18** | `[18] Iron` | `uint8` | `0 – 2` | **Iron Ore Deposit Tally**: Number of mineral tiles containing Iron Ore (Resource 4). |
| **19** | `[19] Copper` | `uint8` | `0 – 1` | **Copper Deposit Tally**: Number of mineral tiles containing Copper deposits (Resource 3). |
| **20** | `[20] Horses` | `uint8` | `0 – 4` | **Horse Ranch / Livestock Tally**: Number of tiles with horse breeding or pastoral livestock. |
| **21** | `[21] Res 21` | `uint8` | `0` | **Reserved Resource Slot**: Internal resource slot (unused, always `0`). |
| **22** | `[22] Spices` | `uint8` | `0 – 1` | **Spice Orchard Tally**: Number of tiles producing Spices (Resource 23). |
| **23** | `[23] Tobacco` | `uint8` | `0 – 1` | **Tobacco Plantation Tally**: Number of tiles producing Tobacco (Resource 8). |
| **24** | `[24] Cotton` | `uint8` | `0 – 3` | **Cotton Plantation Tally**: Number of tiles producing Cotton (Resource 6). |
| **25** | `[25] Fur` | `uint8` | `0 – 3` | **Fur Trapping Tally**: Number of wilderness tiles producing Fur (Resource 9). |
| **26** | `[26] Mine Sites`| `uint8` | `0 – 1` | **Mineral / Mine Extraction Sites**: Total active mines operating in this province. |
| **27** | `[27] Res 27` | `uint8` | `0` | **Reserved Resource Slot**: Internal resource slot (unused, always `0`). |
| **28** | `[28] Coal` | `uint8` | `0 – 8` | **Coal Deposit Tally**: Number of mineral tiles containing Coal deposits (Resource 5). |
| **29** | `[29] Res 29` | `uint8` | `0` | **Reserved Resource Slot**: Internal resource slot (unused, always `0`). |
| **30** | `[30] Quarry` | `uint8` | `0 – 5` | **Quarry / Stone Tally**: Count of stone quarry or rock formation tiles. |
| **31** | `[31] Fiber` | `uint8` | `0 – 2` | **Plant Fiber / Hemp Tally**: Count of textile fiber cultivation tiles. |
| **32 – 35** | `[32-35] Res` | `uint8` | `0 – 2` | **Secondary Resource Tallies**: Additional commodity and raw material distribution counts. |
| **36 – 46** | `[36-46] Pad` | `uint8` | `0` | **Reserved Administrative Padding**: Reserved by game engine (always `0`). |
| **47** | `[47] Unrest` | `uint8` | `0, 1` | **Rebellion / Unrest Status**: `1` if the province is experiencing civil unrest, revolt, or resistance to foreign occupation; `0` if peaceful. |
| **48** | `[48] Militia` | `uint8` | `0 – 4` | **Local Militia / Garrison Strength**: Stationed local defense militia unit rating. |
| **49** | `[49] Pad` | `uint8` | `0` | **Reserved Padding**: Always `0`. |
| **50** | `[50] Blockade`| `uint8` | `0, 1` | **Naval Blockade / Siege Status**: `1` if province ports are blockaded by hostile warships. |
| **51** | `[51] Defense` | `uint8` | `0, 1` | **Defensive Brutishness Modifier**: Terrain defensive posture used by `AssessDefensiveBrutishnessOfProvince`. |
| **52** | `[52] Threat` | `uint8` | `0 – 3` | **Native Hostility / Threat Rating**: Native warrior threat level assessed by `AssessImmediateThreatToProvince`. |
| **53** | `[53] Roughness`| `uint8` | `0 – 3` | **Province Terrain Roughness**: Average movement cost / difficulty penalty retrieved by `GetProvinceRoughness`. |
| **54** | `[54] Trade Post`| `uint8` | `0, 1` | **Merchant Company / Trading Post**: `1` if a foreign trading post or chartered company is established; checked by `WhoHasCompaniesIn`. |
| **55 – 64** | `[55-64] Pad` | `uint8` | `0` | **Reserved Engine Padding**: Reserved by simulation engine (always `0`). |
| **65** | `[65] Explored`| `uint8` | `0 – 255` | **Province Fog-of-War Bitmask**: Great Power visibility mask (`1 << playerID`). `255` indicates the province is fully mapped and explored by all nations. |
| **66** | `[66] Pad` | `uint8` | `0` | **Reserved Padding**: Always `0`. |
| **67** | `[67] Strait` | `uint8` | `0, 1` | **Strait / Canal Crossing Flag**: `1` on provinces with narrow water straits, dikes, or canal links (e.g., Zeeland, Copenhagen, Holstein, Friesland). |
| **68 – 69** | `[68-69] Seat Pos`| `int16` | `0 – 6479` | **County Seat / City Tile Index**: 16-bit tile index (`Pos1` low byte at 68, `Pos2` high byte at 69) of the city center or capital building on the 6,480-tile world map. |
| **70 – 71** | `[70-71] Capitol` | `int16` | `0, 999` | **Great Power Capital Status**: `0` for the 6 primary Great Power home capitals (London, Paris, Madrid, Lisbon, Amsterdam, Stockholm); `999` for all other provincial cities and minor nations. |
| **72 – 95** | `[72-95] Neighbors`| `int16[12]`| `-1, 0 – 151` | **Adjacent Neighbor Provinces**: Array of up to 12 16-bit Province IDs that share land borders with this province. Terminated with `-1` (`0xFFFF`). Evaluated by `AreProvinceNeighbors`. |
| **96 – 119**| `[96-119] Entry` | `int16[12]`| `-1, 0 – 6479`| **Border Entry / Transit Tiles**: Array of up to 12 16-bit Tile Indices corresponding directly to each adjacent province in the `Neighbors` array. Specifies the border hex an army enters when marching into that neighbor. Evaluated by `GetProvinceNeighborTile`. |
| **120 – 121**| `[120-121] Aux 1`| `int16` | `0 – 6479` | **Auxiliary / Attachment Tile 1**: Neighboring hex tile used for city graphic expansion, citadel bastions, or port connections. |
| **122 – 123**| `[122-123] Aux 2`| `int16` | `0 – 6479` | **Auxiliary / Attachment Tile 2**: Second neighboring hex tile used for urban footprint rendering. |
| **124 – 187**| `[124-187] Tiles` | `int16[32]`| `-1, 0 – 6479`| **Province Hex Tile Table**: Array of up to 32 16-bit Tile Indices defining all hexes belonging to this province. The number of valid tiles is specified by `TileCount` (Byte 4); unused slots are filled with `-1` (`0xFFFF`). |
| **188 – 191**| `[188-191] Runtime`| `int32` | `0` | **Runtime Object Pointer (`TCity*`)**: Internal memory pointer zeroed out upon loading (`mov dword ptr [eax + 0x75], 0` in `ReverseProvinceEnds`). |
| **192 – 195**| `[192-195] Value` | `int32` | `0 – 25,000` | **Economic Valuation (`ProvinceValue`)**: Calculated economic and strategic value of the province in gold ducats. Computed by `CalculateProvinceValues` and checked by `LandPrice`. |
| **196 – 199**| `[196-199] Riches`| `int32` | `0 – 500` | **Tribal Riches / Plunder Value**: Tribute and treasure value lootable when conquering native New World tribal territories. Evaluated by `GetUnfortifiedRichesProvince`. |
| **200 – 203**| `[200-203] Tier` | `int32` | `0 – 3` | **Urban Development Tier**: Provincial development level (`0` = rural/colony, `1`–`3` = developed provincial center / capital). |
| **204 – 207**| `[204-207] Gov ID` | `int32` | `0 – 21` | **Governing Country ID**: Full 32-bit Sovereign Country ID actively governing the province and collecting trade revenue. |
| **208** | `[208] Name Len` | `uint8` | `1 – 32` | **Province Name String Length**: Number of characters in the following Pascal string. |
| **209+** | `[209+] Name` | `char[]` | ASCII string | **Province / City Name**: Human-readable name (e.g., `"London"`, `"Madrid"`, `"Florence"`, `"Texcoco"`, `"Pachacamac"`). |

---

## Endianness Handling (`ReverseProvinceEnds`)

*Imperialism II* was originally engineered as a cross-platform title (Apple Macintosh PPC & Windows x86). Save games store integers in **Little-Endian (Intel x86)** format.

When loading or saving, the routine `TMapMgr::ReverseProvinceEnds` at `0x0054ff60` (`0x156d40` in patched binary) handles byte-swapping for all multi-byte fields:
* **16-bit Words (2-byte swap)**:
  * Offset `68–69`: County Seat Tile Index
  * Offset `70–71`: Capital Status
  * Offset `72–95`: 12 Neighbor Province IDs (`short[12]`)
  * Offset `96–119`: 12 Neighbor Entry Tile Indices (`short[12]`)
  * Offset `120–121`: Auxiliary Tile 1
  * Offset `122–123`: Auxiliary Tile 2
  * Offset `124–187`: 32 Province Tile Indices (`short[32]`)
* **32-bit Dwords (4-byte swap)**:
  * Offset `188–191`: Zeroed out in memory (`mov dword ptr [eax + 0x75], 0`)
  * Offset `192–195`: Economic Value (`int32`)
  * Offset `196–199`: Tribal Riches (`int32`)
  * Offset `200–203`: Urban Development Tier (`int32`)
  * Offset `204–207`: Governing Country ID (`int32`)
* **8-bit Bytes (no swap)**:
  * Offsets `0–67` are single-byte integers and bitmasks, requiring no endian conversion.

