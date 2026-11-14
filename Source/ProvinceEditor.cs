using System;
using System.Collections.Generic;
using System.Linq;

namespace Imperialism_2_SGEditor
{
  public static class ProvinceEditor
  {
    private static IEnumerable<MapField> Neighbors(MapField tile, MapField[] world)
    {
      for (int d = 0; d < 6; d++)
      {
        int i = DB.GetNeighborIndex(tile.Idx, d);
        if (i >= 0 && i < world.Length && world[i] != null) yield return world[i];
      }
    }

    private static bool Connected(List<MapField> tiles, MapField[] world)
    {
      if (tiles.Count == 0) return false;
      var remaining = new HashSet<short>(tiles.Select(t => t.Idx));
      var queue = new Queue<MapField>();
      queue.Enqueue(tiles[0]);
      remaining.Remove(tiles[0].Idx);
      while (queue.Count > 0)
        foreach (var next in Neighbors(queue.Dequeue(), world))
          if (remaining.Remove(next.Idx)) queue.Enqueue(next);
      return remaining.Count == 0;
    }

    public static void Move(MapField tile, City destination, MapField[] world, List<City> cities)
    {
      if (tile == null || destination == null || world == null || cities == null)
        throw new InvalidOperationException("Select a land tile and a neighboring province.");
      int oldId = tile.RawData[DB.PROVINCE_ID], newId = destination.ProvinceId;
      City source = cities.FirstOrDefault(c => c.ProvinceId == oldId);
      bool unassigned = oldId == DB.SEA && !tile.IsSea;
      if (tile.IsSea || (source == null && !unassigned) || newId == oldId || newId < 0 || newId >= 152 || !cities.Contains(destination))
        throw new InvalidOperationException("Choose a different land province.");
      if (!Neighbors(tile, world).Any(n => !n.IsSea && n.RawData[DB.PROVINCE_ID] == newId))
        throw new InvalidOperationException("The destination must touch this tile.");
      if (tile.RawData[DB.BUILDINGS] != 0 || cities.Any(c => c.Idx == tile.Idx ||
          BitConverter.ToInt16(c.RawData, 120) == tile.Idx || BitConverter.ToInt16(c.RawData, 122) == tile.Idx))
        throw new InvalidOperationException("Town, building, and city attachment tiles cannot be transferred.");

      // Prepare copies first. A validation failure must leave the entire map untouched.
      var membership = new Dictionary<int, List<MapField>>();
      var records = new Dictionary<int, byte[]>();
      foreach (var city in cities)
      {
        int id = city.ProvinceId;
        if (membership.ContainsKey(id)) throw new InvalidOperationException("Duplicate province IDs in the save.");
        var members = world.Where(t => t != null && !t.IsSea && t.RawData[DB.PROVINCE_ID] == id).ToList();
        if (members.Count != city.RawData[4] || members.Count > 32 ||
            !new HashSet<short>(members.Select(t => t.Idx)).SetEquals(
              Enumerable.Range(0, members.Count).Select(i => BitConverter.ToInt16(city.RawData, 124 + i * 2))))
          throw new InvalidOperationException("Province tile lists are inconsistent; transfer cancelled.");
        membership.Add(id, members);
        records.Add(id, (byte[])city.RawData.Clone());
      }
      if (!unassigned) membership[oldId].Remove(tile);
      membership[newId].Add(tile);
      if (membership[newId].Count > 32) throw new InvalidOperationException("The destination already contains 32 tiles.");
      if ((!unassigned && !Connected(membership[oldId], world)) || !Connected(membership[newId], world))
        throw new InvalidOperationException("This move would leave a province disconnected or empty.");

      Func<MapField, int> province = t => t == tile ? newId : t.RawData[DB.PROVINCE_ID];
      int resource = tile.RawData[DB.LAND_TYPE_RES];
      if (resource < 28)
      {
        int offset = 8 + resource;
        if ((!unassigned && records[oldId][offset] == 0) || records[newId][offset] == 255)
          throw new InvalidOperationException("Resource tallies cannot accommodate this transfer.");
        if (!unassigned) records[oldId][offset]--;
        records[newId][offset]++;
      }
      foreach (var city in cities)
      {
        int id = city.ProvinceId;
        var raw = records[id];
        var members = membership[id];
        if (id == oldId || id == newId)
        {
          raw[4] = (byte)members.Count;
          for (int i = 0; i < 32; i++) WriteShort(raw, 124 + i * 2, i < members.Count ? members[i].Idx : -1);
        }
        var borders = new SortedDictionary<int, short>();
        foreach (var member in members)
          foreach (var neighbor in Neighbors(member, world))
          {
            int other = province(neighbor);
            if (other != DB.SEA && other != id && !borders.ContainsKey(other)) borders.Add(other, neighbor.Idx);
          }
        if (borders.Count > 12) throw new InvalidOperationException("A province would exceed its 12-neighbor capacity.");
        // Preserve working entry tiles where that edge still exists.
        var ordered = new List<KeyValuePair<int, short>>();
        for (int i = 0; i < 12; i++)
        {
          int other = BitConverter.ToInt16(city.RawData, 72 + i * 2);
          short entry = BitConverter.ToInt16(city.RawData, 96 + i * 2);
          if (!borders.ContainsKey(other)) continue;
          bool valid = entry >= 0 && entry < world.Length && world[entry] != null && province(world[entry]) == other &&
              Neighbors(world[entry], world).Any(n => province(n) == id);
          ordered.Add(new KeyValuePair<int, short>(other, valid ? entry : borders[other]));
          borders.Remove(other);
        }
        ordered.AddRange(borders);
        for (int i = 0; i < 12; i++)
        {
          WriteShort(raw, 72 + i * 2, i < ordered.Count ? ordered[i].Key : -1);
          WriteShort(raw, 96 + i * 2, i < ordered.Count ? ordered[i].Value : -1);
        }
      }
      tile.RawData[DB.PROVINCE_ID] = (byte)newId;
      tile.ProvinceId = (short)newId;
      tile.RawData[DB.OWNER1] = destination.RawData[0];
      tile.RawData[DB.OWNER2] = destination.RawData[1];
      foreach (var city in cities)
      {
        city.RawData = records[city.ProvinceId];
        city.Fill();
      }
      foreach (var field in world.Where(t => t != null))
      {
        RefreshBorder(field, world);
        field.ProvinceName = cities.Where(c => c.ProvinceId == field.RawData[DB.PROVINCE_ID]).Select(c => c.Name).FirstOrDefault();
        field.Update();
      }
    }

    private static void WriteShort(byte[] raw, int offset, int value)
    {
      BitConverter.GetBytes((short)value).CopyTo(raw, offset);
    }

    private static void RefreshBorder(MapField tile, MapField[] world)
    {
      // Engine order: NE, E, SE, SW, W, NW. Includes border corner flags.
      var near = new MapField[6];
      for (int d = 0; d < 6; d++)
      {
        int index = DB.GetNeighborIndex(tile.Idx, (d + 1) % 6);
        if (index >= 0 && index < world.Length) near[d] = world[index];
      }
      int mask = 0;
      for (int d = 0; d < 6; d++)
      {
        var a = near[d]; var b = near[(d + 1) % 6];
        if (a != null && !a.IsSea && ((!tile.IsSea && a.ProvinceId != tile.ProvinceId) ||
            (tile.IsSea && b != null && !b.IsSea && a.ProvinceId != b.ProvinceId))) mask |= 1 << d;
      }
      if ((mask & 3) == 3 && near[0] != null && near[1] != null && near[0].ProvinceId != near[1].ProvinceId) mask |= 64;
      if ((mask & 6) == 6 && near[1] != null && near[2] != null && near[1].ProvinceId != near[2].ProvinceId) mask |= 128;
      tile.RawData[12] = (byte)mask;
      // Country borders follow the controlling-owner byte, including sea-zone edges.
      // Keep existing sea-zone edges: transferring land does not alter sea zones.
      int countryMask = tile.IsSea ? tile.RawData[11] & 63 : 0;
      for (int d = 0; d < 6; d++)
      {
        var a = near[d]; var b = near[(d + 1) % 6];
        if (a == null) continue;
        if (!tile.IsSea && !a.IsSea && a.RawData[DB.OWNER2] != tile.RawData[DB.OWNER2]) countryMask |= 1 << d;
        if (tile.IsSea && !a.IsSea && b != null && !b.IsSea)
        {
          countryMask &= ~(1 << d);
          if (a.RawData[DB.OWNER2] != b.RawData[DB.OWNER2]) countryMask |= 1 << d;
        }
      }
      if ((countryMask & 3) == 3 && near[0] != null && near[1] != null && near[0].RawData[DB.OWNER2] != near[1].RawData[DB.OWNER2]) countryMask |= 64;
      if ((countryMask & 6) == 6 && near[1] != null && near[2] != null && near[1].RawData[DB.OWNER2] != near[2].RawData[DB.OWNER2]) countryMask |= 128;
      tile.RawData[11] = (byte)countryMask;
      tile.Border = new System.Windows.Thickness(near[4] != null && near[4].ProvinceId != tile.ProvinceId ? 1 : 0, 0, 0, 0);
      tile.TopLeftBorder = near[5] != null && near[5].ProvinceId != tile.ProvinceId ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
      tile.TopRightBorder = near[0] != null && near[0].ProvinceId != tile.ProvinceId ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }
  }
}
