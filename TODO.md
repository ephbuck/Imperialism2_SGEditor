# Running TODO

Keep unresolved findings here as work progresses. Mark completed items and add the
verification used when closing them.

## Terrain rendering and save compatibility

- [ ] Complete [road/rail in-game verification](Tests/TRANSPORT_CHECKLIST.md),
  especially resource transport, movement, rivers, ports, borders, and planned routes.
- [ ] Verify reclaimed land in-game, including coastline transitions, naval access,
  and old sea-zone/port metadata. Also reconcile province records for land-to-water
  conversion; that reverse operation has not received the new assignment workflow.
- [ ] Complete [province transfer verification](Tests/PROVINCE_TRANSFER_CHECKLIST.md),
  especially save round trips, movement entry tiles, resource accounting, port
  access, and economic valuation caches. The first implementation is experimental.
- [ ] Investigate coastal blending before treating shoreline edits as verified.
  Saved bytes 11–13 contain values the current transition routine does not fully
  reproduce. Confirm their encoding and interaction with province borders, rivers,
  and shoreline variants before changing the algorithm.
- [ ] Verify edited terrain visually in-game after saving and reloading. Cover
  farmland clusters, forests, swamps, desert/tundra boundaries, hills/mountains,
  and coastlines. Automated connection checks do not verify rendered pixels.
- [ ] Confirm visual type 14's identity and default profile. The editor labels it
  `Swamp (Tin)` and assigns swamp terrain/tin production, while reference saves
  also contain type 14 with terrain category 0 and no resource. Establish the
  game's actual rule before changing this behavior.
- [ ] Investigate existing connection-mask inconsistencies in `slot0.imp` and
  `slot2.imp`, and the special type-14 cases. Determine whether they reflect prior
  edits, game behavior, or an incomplete rule. Any future repair operation should
  preview its changes and preserve the original save.
- [ ] Verify neighbor behavior at the first/last map columns and polar rows against
  the game, including whether east/west connections wrap.
- [ ] Reconcile transition-byte names and descriptions in code, raw-grid headers,
  and `TILE_STRUCTURE.md` as their meanings are verified. Byte 14's description
  has been corrected; other transition descriptions remain provisional.

## Build and regression coverage

- [ ] Configure a reproducible build with the appropriate .NET Framework reference
  assemblies. Current legacy MSBuild succeeds using installed framework/GAC
  assemblies but reports targeting-pack and architecture warnings.
- [ ] Add a repeatable WPF binding regression check for the Production dropdown.
  A temporary binding harness passed during development; it is not yet checked in.

## Completed — 2026-11-14

- [x] Display buildings on map tiles with distinct town, capital, fort, and port
  icons. Verified rendered vector icons and 12 building-code/binding cases.
- [x] Notify tile data bindings after updates so Production follows terrain defaults.
- [x] Remove plain Swamp from visual choices while retaining its rendering definition
  for existing saves.
- [x] Recalculate matching-terrain connections for flat terrain and adjacent tiles
  instead of clearing byte 14 on farmland.
- [x] Keep desert connections separate from tundra; allow tundra/furry-tundra links.
- [x] Add regression scripts: 216 option/direction/parity cases, 12 mixed-terrain
  cases, and grain comparisons matching all 167 grain tiles in `slot3.imp`.
