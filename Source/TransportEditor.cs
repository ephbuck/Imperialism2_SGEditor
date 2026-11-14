using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace Imperialism_2_SGEditor
{
  public static class TransportEditor
  {
    public const int RailMask = DB.RAIL_DIR, RoadMask = DB.ROAD_DIR;

    public static void Set(MapField tile, MapField[] world, bool road, bool rail)
    {
      if (tile == null || world == null) return;
      if (tile.IsSea && (road || rail)) throw new InvalidOperationException("Roads and railways require land.");
      road = road && !rail;
      tile.SetTransportPresence(road, rail);
      for (int d = 0; d < 6; d++)
      {
        int index = DB.GetNeighborIndex(tile.Idx, d);
        MapField neighbor = index >= 0 && index < world.Length ? world[index] : null;
        int bit = 1 << ((d + 5) % 6), opposite = 1 << ((d + 2) % 6);
        bool connected = neighbor != null && !tile.IsSea && !neighbor.IsSea &&
            (road || rail) && (neighbor.HasRoad || neighbor.HasRail);
        bool railEdge = connected && rail && neighbor.HasRail;
        bool roadEdge = connected && !railEdge;
        SetBit(tile, RoadMask, bit, roadEdge);
        SetBit(tile, RailMask, bit, railEdge);
        if (neighbor != null)
        {
          // Capture presence before removing the last incoming connection.
          neighbor.SetTransportPresence(neighbor.HasRoad, neighbor.HasRail);
          SetBit(neighbor, RoadMask, opposite, roadEdge);
          SetBit(neighbor, RailMask, opposite, railEdge);
          neighbor.Update();
        }
      }
      tile.Update();
    }

    private static void SetBit(MapField tile, int offset, int bit, bool enabled)
    {
      tile.RawData[offset] = (byte)((tile.RawData[offset] & ~bit) | (enabled ? bit : 0));
    }

    private static string Digest(byte[] data)
    {
      using (var hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(data));
    }

    public static void SaveSelections(string savePath, byte[] data, MapField[] world)
    {
      var root = new XElement("TransportSelections", new XAttribute("saveHash", Digest(data)));
      foreach (var tile in world)
      {
        bool road = tile.HasRoad, rail = tile.HasRail;
        bool inferredRoad = (tile.RawData[RoadMask] & 63) != 0, inferredRail = (tile.RawData[RailMask] & 63) != 0;
        if (!tile.IsSea && (road != inferredRoad || rail != inferredRail)) root.Add(new XElement("Tile",
            new XAttribute("id", tile.Idx), new XAttribute("road", road), new XAttribute("rail", rail)));
      }
      new XDocument(root).Save(savePath + ".sgeditor.xml");
    }

    public static void LoadSelections(string savePath, byte[] data, MapField[] world)
    {
      string path = savePath + ".sgeditor.xml";
      if (!File.Exists(path)) return;
      var root = XDocument.Load(path).Root;
      if (root == null || (string)root.Attribute("saveHash") != Digest(data)) return;
      foreach (var element in root.Elements("Tile"))
      {
        int index;
        bool road, rail;
        if (!int.TryParse((string)element.Attribute("id"), out index) || index < 0 || index >= world.Length ||
            !bool.TryParse((string)element.Attribute("road"), out road) ||
            !bool.TryParse((string)element.Attribute("rail"), out rail)) continue;
        var tile = world[index];
        if (tile.IsSea) continue;
        tile.SetTransportPresence(road, rail);
        tile.Update();
      }
    }
  }
}
