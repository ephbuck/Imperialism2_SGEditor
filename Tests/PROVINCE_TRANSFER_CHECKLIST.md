# Province transfer verification

Use copies of saves. The editor's Save button overwrites the opened file.
This feature is experimental until the in-game checks below pass.

## Automated checks

- [x] Debug build succeeds (existing targeting-pack warnings remain).
- [x] Reference-save transfer and reverse transfer succeed in memory.
- [x] Tile province IDs, province lists, and tile counts agree after a move.
- [x] Province adjacency remains reciprocal and entry tiles belong to the neighbor.
- [x] Rejected transfers leave all tile and province bytes unchanged (14 cases in
  the reference-save search).
- [x] Independently calculated province-border bytes match all 6,480 reference
  tiles before edits. This validates province borders, not coastal graphics.
- [ ] Add explicit fixtures for each rejection: sea, nonadjacent destination,
  different country, occupation, buildings, county seat, city attachments,
  disconnected source, empty source, 32-tile destination, 12-neighbor overflow,
  invalid membership, and resource tally underflow/overflow.
- [ ] Verify successful resource transfers change only the relevant resource tally
  in each province; test grain, timber, minerals, and no-resource terrain.
- [ ] Exercise first/last columns, polar rows, and both staggered row parities.
- [ ] Test creating/removing a province adjacency and replacing an invalidated
  border-entry tile with another valid crossing.
- [ ] Add a serialized save round-trip check, comparing all unrelated bytes.

Run from the repository root with Windows PowerShell:

```powershell
powershell.exe -NoProfile -STA -File Tests\ProvinceTransfers.ps1 -SavePath "path\to\reference.imp"
powershell.exe -NoProfile -STA -File Tests\TerrainConnections.ps1
powershell.exe -NoProfile -STA -File Tests\GrainTransitions.ps1
powershell.exe -NoProfile -STA -File Tests\BuildingOverlays.ps1
```

The province script never writes the supplied save. It tests one permitted move,
its reversal, and rejected candidates encountered before that move.

## Editor checks

- [x] Automated sea-to-land conversion displays land without assigning a province
  prematurely; neighboring provinces appear and saving is blocked until assigned.
- [x] Assigning new grain land adds one tile and one grain tally to the destination,
  adopts its ownership, and maintains reciprocal adjacency and entry tables.
- [x] Converting ocean to land assigns Fish to adjacent water and refreshes its
  icons; land, distant water, and later ordinary land edits retain their resources.
  Regression script: `Tests/CoastalFishing.ps1`.
- [ ] In the UI, convert a coastal water tile to land, assign it to each bordering
  country in separate save copies, then save/reopen and load in-game.
- [ ] Check full destinations and isolated new islands: failed assignments leave
  province records unchanged and saving stays blocked until resolved.
- [ ] Verify coastline graphics, naval access, former sea-zone/port metadata, and
  movement around reclaimed land in-game. Changing terrain alone does not rebuild
  every naval-access cache.
- [X] Select a border tile: only provinces directly touching that tile are listed.
- [ ] Select an interior tile, sea, or no destination: no transfer occurs.
- [X] Perform a permitted transfer: province name and border lines refresh without
  reopening; resource, terrain, building, and ownership displays remain correct.
- [X] A blocked move gives a useful status message and changes nothing.
- [ ] Repeat transfers between several provinces; save, close, and reopen the copy.
  Tile membership, counts, names, borders, and entry tiles must persist.
- [ ] Repeat with cities tab open to confirm raw province records refresh.

## Required in-game checks

- [X] Load the edited copy successfully and inspect the changed border.
- [ ] Move armies in both directions across the changed boundary. Test old and
  newly created province adjacencies and check destination/entry positions.
- [ ] Verify production and resource availability in both provinces, advance a
  turn, save in-game, and reload. Compare totals and look for lost resources.
- [ ] Check roads/rail and civilian units crossing the moved tile.
- [ ] Check adjacent ports, shipping access, expanded towns, fog of war, and
  territorial highlighting remain correct.
- [ ] Verify province purchase/economic valuation. The editor currently preserves
  economic/strategic cached fields; their recalculation rules are not established.
- [ ] Confirm game-generated saves retain the new membership, tile counts,
  adjacency, and resource tallies after advancing a turn.

## Current limits

Transfers are limited to adjacent land provinces with matching owner records and
a tile controlled by that country. Buildings, county seats, and recorded city
attachments are blocked. Province sizes and neighbor arrays retain their existing
32-tile and 12-neighbor capacities. Moves that disconnect either province fail.
Terrain/coastline graphics and country ownership are not changed by a transfer.
Port-access and economic caches require the in-game checks above.
