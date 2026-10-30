using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Imperialism_2_SGEditor
{
  public static class DB
  {
    //public static int NotifyCounter = 0;

    //ALL CONST VALUES ARE COUNTED FROM ZERO (so 20th byte is 19 in const value for array use)
    public const byte OWNER1 = 4;
    public const byte OWNER2 = 5;
    public const byte SEA = 255;
    public const byte MAP_VISIBILITY = 31;
    public const byte BUILDINGS = 29;//55 capitol
    public const byte PROD_LEVEL = 16;//0-4 levels
    public const byte RESOURCE_DISCOVERED_BY = 17;
    public const byte LAND_TYPE_RES = 21;//resource that can be used
    public const byte LAND_TYPE_GFX = 22;//visual only
    public const byte PROVINCE_ID = 23;//255 is sea, other numbers are province id
    public const byte TERRAIN_CAT = 1;     // Terrain Category (0=Plains, 1=Forest, 2=Hills, 3=Mountains, 4=Swamp, 5=Water, 6=Desert/Tundra, 7=Farmland)
    public const byte ELEVATION = 2;       // Elevation (1=Mountain peak/high ridge, 0=flat/foothill)
    public const byte TRANS_COAST = 12;    // Coastline direction bitmask (Byte 12-13)
    public const byte TRANS_ELEVATION = 14;// Elevation / slope transition bitmask
    public const byte TRANS_SPRITE = 15;   // Transition sprite index
    public const byte LAND_WATER_FLAG = 24;// 0=Land, 255=Sea

    public const int DIR_NW = 0; // Top-Left
    public const int DIR_NE = 1; // Top-Right
    public const int DIR_E  = 2; // Right
    public const int DIR_SE = 3; // Bottom-Right
    public const int DIR_SW = 4; // Bottom-Left
    public const int DIR_W  = 5; // Left

    public static readonly byte[] DIR_MASKS = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20 };
    public static readonly int[] OPPOSITE_DIRS = new int[] { 3, 4, 5, 0, 1, 2 };

    public static Brush[] COUNTRY_COLORS = new Brush[6] { Brushes.Blue, Brushes.Red, Brushes.Cyan, Brushes.Orange, Brushes.SpringGreen, Brushes.Yellow };
    public static string[] COUNTRY_NAMES = new string[22] { "France", "England", "Sweden", "Holland", "Portugal", "Spain", "Ireland", "Scotland", "Denmark", "Germany", "Italy", "Switzerland", "The Aztecs", "The Incas", "The Mayas", "The Taino", "The Iroquois", "The Sioux", "The Kwakiutl", "The Cherokee", "The Huron", "The Pueblo" };
    public static string[] HEADERS = new string[40] { "0", "1", "2", "3", "Owner1", "Owner2", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "Prod lvl", "Res discovered by", "18", "19", "20", "Type (res)", "Type (vis)", "Sea/ID", "24", "25", "26", "27", "28", "Buildings", "30", "Explored", "32", "33", "34", "35", "36", "37", "38", "39" };

    //CITIES
    public const byte FORT_LEVEL = 3;
    public const byte IS_EXPANDED = 2;
    public const byte CITY_OWNER1 = 0;
    public const byte CITY_OWNER2 = 1;
    public const byte CITY_POS1 = 68;
    public const byte CITY_POS2 = 69;
    //CITIES END

    public static List<Resource> LandTypes = new List<Resource>()
    {
      new Resource("Sea", "sea.jpg", 0),
      new Resource("Plains", "plains.jpg", 1),
      new Resource("Cotton Plantation", "cotton.jpg", 2),
      new Resource("Livestock", "cattle.jpg", 3),
      new Resource("Horse Ranch", "horses.jpg", 4),
      new Resource("Grain Farm", "grain.jpg", 5),
      new Resource("Spice Orchard", "spices.jpg", 6),
      new Resource("Fertile Hills", "sheep.jpg", 7),
      new Resource("Barren Hills", "bhills.jpg", 8),
      new Resource("Mountains", "mountains.jpg", 9),
      new Resource("Swamp", "swamp.jpg", 10),
      new Resource("Desert", "desert.jpg", 11),
      new Resource("Tundra", "tundra.jpg", 12),
      new Resource("Hardwood Forest", "forest.jpg", 13),
      new Resource("Swamp (Tin)", "swamp_tin.jpg", 14),
      new Resource("Scrub Forest", "scrub_forest.jpg", 15),
      new Resource("Sugar Cane", "sugar_cane.jpg", 16),
      new Resource("Tobacco Plantation", "tobacco.jpg", 17),
      new Resource("Furry Tundra", "furs.jpg", 18)
    };

    public static List<Resource> Resources = new List<Resource>()
    {
      new Resource("Wool", "rsheep.jpg", 0),
      new Resource("Timber", "rtimber.jpg", 1),
      new Resource("Tin", "rtin.jpg", 2),
      new Resource("Copper", "rcopper.jpg", 3),
      new Resource("Iron Ore", "riron_ore.jpg", 4),
      new Resource("Coal", "rcoal.jpg", 5),
      new Resource("Cotton", "rcotton.jpg", 6),
      new Resource("Sugar Cane", "rsugar_cane.jpg", 7),
      new Resource("Tobacco", "rtobacco.jpg", 8),
      new Resource("Fur", "rfurs.jpg", 9),
      new Resource("Horses", "rhorses.jpg", 10),
      new Resource("Grain", "rgrain.jpg", 20),
      new Resource("Fish", "rfish.jpg", 21),
      new Resource("Livestock", "rcattle.jpg", 22),
      new Resource("Spices", "rspice.jpg", 23),
      new Resource("Silver", "rsilver.jpg", 24),
      new Resource("Gold", "rgold.jpg", 25),
      new Resource("Gems", "rgems.jpg", 26),
      new Resource("Diamonds", "rdiamonds.jpg", 27),
      new Resource("None", "rnone.png", 255)
    };

    public static Resource GetResourceById(byte id)
    {
      //255 no resource //32,4 res discovered (1/0, 2/1, 4/2, 8/3, 16/4, 32/5, 64/6, 128)
      foreach (var item in Resources) if (item.Id == id) return item;
      return null;
    }

    public static Resource GetLandTypesById(byte id)
    {
      //255 no resource //32,4 res discovered (1/0,2./1,4/2,8/3,16/4,32/5,64/6,128)
      foreach (var item in LandTypes) if (item.Id == id) return item;
      return null;
    }

    internal static byte GetCountryNumber(string name)
    {
      for (int i = 0; i < COUNTRY_NAMES.Length; i++)
      {
        if (COUNTRY_NAMES[i] == name) return (byte)i;
      }
      return 255;
    }

    public static int GetNeighborIndex(int tileIndex, int dir)
    {
      if (tileIndex < 0 || tileIndex >= 6480) return -1;
      int row = (tileIndex / 108) + 1;
      int col = (tileIndex % 108) + 1;
      bool isEvenRow = (row % 2 == 0);

      int targetRow = row;
      int targetCol = col;

      switch (dir)
      {
        case DIR_NW:
          targetRow = row - 1;
          targetCol = isEvenRow ? col : col - 1;
          break;
        case DIR_NE:
          targetRow = row - 1;
          targetCol = isEvenRow ? col + 1 : col;
          break;
        case DIR_E:
          targetRow = row;
          targetCol = col + 1;
          break;
        case DIR_SE:
          targetRow = row + 1;
          targetCol = isEvenRow ? col + 1 : col;
          break;
        case DIR_SW:
          targetRow = row + 1;
          targetCol = isEvenRow ? col : col - 1;
          break;
        case DIR_W:
          targetRow = row;
          targetCol = col - 1;
          break;
        default:
          return -1;
      }

      if (targetRow < 1 || targetRow > 60 || targetCol < 1 || targetCol > 108)
        return -1;

      return (targetRow - 1) * 108 + (targetCol - 1);
    }

    public static void SyncTerrainProfile(byte gfxId, MapField field, MapField[] worldFields)
    {
      if (field == null || field.RawData == null || field.RawData.Length < 40) return;
      byte[] rawData = field.RawData;

      rawData[LAND_TYPE_GFX] = gfxId;

      switch (gfxId)
      {
        case 0: // Sea
          rawData[TERRAIN_CAT] = 5; // Water
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 255;
          rawData[PROVINCE_ID] = SEA; // 255
          rawData[LAND_WATER_FLAG] = 255; // Sea flag
          rawData[BUILDINGS] = 0;
          field.ProvinceId = SEA;
          break;
        case 1: // Plains
          rawData[TERRAIN_CAT] = 0;
          rawData[ELEVATION] = 0;
          if (rawData[LAND_TYPE_RES] == 9 || rawData[LAND_TYPE_RES] == 1 || rawData[LAND_TYPE_RES] == 255) rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 2: // Cotton Plantation
          rawData[TERRAIN_CAT] = 0;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 6; // Cotton
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 3: // Livestock
          rawData[TERRAIN_CAT] = 0;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 22; // Livestock
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 4: // Horse Ranch
          rawData[TERRAIN_CAT] = 0;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 10; // Horses
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 5: // Grain Farm
          rawData[TERRAIN_CAT] = 7; // Farmland
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 20; // Grain
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 6: // Spice Orchard
          rawData[TERRAIN_CAT] = 7;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 23; // Spices
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 7: // Fertile Hills
          rawData[TERRAIN_CAT] = 2; // Hills
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 0; // Wool
          rawData[LAND_WATER_FLAG] = 0;
          if (rawData[BUILDINGS] > 0 && rawData[BUILDINGS] < 4) rawData[BUILDINGS] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 8: // Barren Hills
          rawData[TERRAIN_CAT] = 2; // Hills
          rawData[ELEVATION] = 0;
          bool isHillMineral = (rawData[LAND_TYPE_RES] >= 2 && rawData[LAND_TYPE_RES] <= 5) || (rawData[LAND_TYPE_RES] >= 24 && rawData[LAND_TYPE_RES] <= 27);
          if (!isHillMineral) rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          if (rawData[BUILDINGS] > 0 && rawData[BUILDINGS] < 4) rawData[BUILDINGS] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 9: // Mountains
          rawData[TERRAIN_CAT] = 3; // Mountains
          rawData[ELEVATION] = 1; // High elevation
          bool isMountainMineral = (rawData[LAND_TYPE_RES] >= 2 && rawData[LAND_TYPE_RES] <= 5) || (rawData[LAND_TYPE_RES] >= 24 && rawData[LAND_TYPE_RES] <= 27);
          if (!isMountainMineral) rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          // Clear incompatible improvements like woodcutters/farms
          rawData[BUILDINGS] = 0;
          rawData[PROD_LEVEL] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 10: // Swamp
          rawData[TERRAIN_CAT] = 4; // Swamp
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          if (rawData[BUILDINGS] > 0 && rawData[BUILDINGS] < 4) rawData[BUILDINGS] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 11: // Desert
          rawData[TERRAIN_CAT] = 6; // Desert
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          if (rawData[BUILDINGS] > 0 && rawData[BUILDINGS] < 4) rawData[BUILDINGS] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 12: // Tundra
          rawData[TERRAIN_CAT] = 6; // Tundra
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 255;
          rawData[LAND_WATER_FLAG] = 0;
          if (rawData[BUILDINGS] > 0 && rawData[BUILDINGS] < 4) rawData[BUILDINGS] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 13: // Hardwood Forest
          rawData[TERRAIN_CAT] = 1; // Forest
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 1; // Timber
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 14: // Swamp Tin
          rawData[TERRAIN_CAT] = 4;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 2; // Tin
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 15: // Scrub Forest
          rawData[TERRAIN_CAT] = 1;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 1; // Timber
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 16: // Sugar Cane
          rawData[TERRAIN_CAT] = 7;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 7; // Sugar Cane
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 17: // Tobacco Plantation
          rawData[TERRAIN_CAT] = 7;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 8; // Tobacco
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
        case 18: // Furry Tundra
          rawData[TERRAIN_CAT] = 6;
          rawData[ELEVATION] = 0;
          rawData[LAND_TYPE_RES] = 9; // Fur
          rawData[LAND_WATER_FLAG] = 0;
          EnsureLandProvince(field, worldFields);
          break;
      }
    }

    private static void EnsureLandProvince(MapField field, MapField[] worldFields)
    {
      if (field.RawData[PROVINCE_ID] == SEA && worldFields != null)
      {
        short foundProv = SEA;
        for (int d = 0; d < 6; d++)
        {
          int nIdx = GetNeighborIndex(field.Idx, d);
          if (nIdx >= 0 && nIdx < worldFields.Length)
          {
            MapField n = worldFields[nIdx];
            if (n != null && n.RawData != null && n.RawData[PROVINCE_ID] != SEA)
            {
              foundProv = n.RawData[PROVINCE_ID];
              break;
            }
          }
        }
        if (foundProv == SEA) foundProv = 0;
        field.RawData[PROVINCE_ID] = (byte)foundProv;
        field.ProvinceId = foundProv;
      }
    }

    public static byte RotateRight6(byte val)
    {
      return (byte)(((val >> 1) | ((val & 1) << 5)) & 0x3F);
    }

    public static void RecalculateTileTransitions(MapField field, MapField[] worldFields)
    {
      if (field == null || field.RawData == null || worldFields == null)
        return;

      int tileIdx = field.Idx;
      byte cat = field.RawData[TERRAIN_CAT];

      if (cat == 5) // Water
      {
        byte landMask = 0;
        for (int d = 0; d < 6; d++)
        {
          int nIdx = GetNeighborIndex(tileIdx, d);
          if (nIdx >= 0 && nIdx < worldFields.Length)
          {
            MapField n = worldFields[nIdx];
            if (n != null && n.RawData != null && n.RawData[TERRAIN_CAT] != 5)
            {
              landMask |= DIR_MASKS[d];
            }
          }
        }
        field.RawData[TRANS_SPRITE] = RotateRight6(landMask);
        field.RawData[TRANS_ELEVATION] = 0;
        field.RawData[13] = 0;
      }
      else if (cat == 3) // Mountains
      {
        byte mounMask = 0;
        byte hillMask = 0;
        byte coastMask = 0;
        for (int d = 0; d < 6; d++)
        {
          int nIdx = GetNeighborIndex(tileIdx, d);
          if (nIdx >= 0 && nIdx < worldFields.Length)
          {
            MapField n = worldFields[nIdx];
            if (n != null && n.RawData != null)
            {
              byte nCat = n.RawData[TERRAIN_CAT];
              if (nCat == 3) mounMask |= DIR_MASKS[d];
              else if (nCat == 2) hillMask |= DIR_MASKS[d];
              else if (nCat == 5) coastMask |= DIR_MASKS[d];
            }
          }
        }
        field.RawData[TRANS_ELEVATION] = RotateRight6(mounMask);
        field.RawData[TRANS_SPRITE] = RotateRight6(hillMask);
        field.RawData[13] = RotateRight6(coastMask);
      }
      else if (cat == 2) // Hills
      {
        byte mounMask = 0;
        byte hillMask = 0;
        byte coastMask = 0;
        for (int d = 0; d < 6; d++)
        {
          int nIdx = GetNeighborIndex(tileIdx, d);
          if (nIdx >= 0 && nIdx < worldFields.Length)
          {
            MapField n = worldFields[nIdx];
            if (n != null && n.RawData != null)
            {
              byte nCat = n.RawData[TERRAIN_CAT];
              if (nCat == 2) hillMask |= DIR_MASKS[d];
              else if (nCat == 3) mounMask |= DIR_MASKS[d];
              else if (nCat == 5) coastMask |= DIR_MASKS[d];
            }
          }
        }
        field.RawData[TRANS_ELEVATION] = RotateRight6(hillMask);
        field.RawData[TRANS_SPRITE] = RotateRight6(mounMask);
        field.RawData[13] = RotateRight6(coastMask);
      }
      else // Flat Land (Plains, Forest, Swamp, Desert, Farm)
      {
        byte coastMask = 0;
        for (int d = 0; d < 6; d++)
        {
          int nIdx = GetNeighborIndex(tileIdx, d);
          if (nIdx >= 0 && nIdx < worldFields.Length)
          {
            MapField n = worldFields[nIdx];
            if (n != null && n.RawData != null && n.RawData[TERRAIN_CAT] == 5)
            {
              coastMask |= DIR_MASKS[d];
            }
          }
        }
        field.RawData[TRANS_SPRITE] = 0;
        field.RawData[13] = RotateRight6(coastMask);
      }

      field.Update();
    }

    public static void UpdateTerrainAndTransitions(MapField centerField, MapField[] worldFields)
    {
      if (centerField == null || centerField.RawData == null || worldFields == null)
        return;

      byte gfxId = centerField.RawData[LAND_TYPE_GFX];
      SyncTerrainProfile(gfxId, centerField, worldFields);

      // If center is flat land, clear its elevation transition:
      byte centerCat = centerField.RawData[TERRAIN_CAT];
      if (centerCat != 2 && centerCat != 3 && centerCat != 5)
      {
        centerField.RawData[TRANS_ELEVATION] = 0;
      }

      // Recalculate transitions for the center tile:
      RecalculateTileTransitions(centerField, worldFields);

      // Recalculate transitions for all 6 neighbors of center tile:
      for (int dir = 0; dir < 6; dir++)
      {
        int nIdx = GetNeighborIndex(centerField.Idx, dir);
        if (nIdx >= 0 && nIdx < worldFields.Length)
        {
          RecalculateTileTransitions(worldFields[nIdx], worldFields);
        }
      }
    }
  }
}
