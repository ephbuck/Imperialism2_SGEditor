# Road and railway verification

## Automated checks

- [x] 48 Road/Rail neighbor combinations over six directions and both row parities,
  including switching types and updating both ends. Legacy both-type selections
  normalize to Rail; existing completed segments are preserved on load.
- [x] Both ends agree for road, rail, and mixed edges.
- [x] Removing transport clears the opposite edge without clearing the neighbor's selection.
- [x] Planned route bytes and building data remain unchanged.
- [x] Isolated selections survive companion-file reload; mixed-edge checkbox intent is preserved.
- [x] Companion files with a different save hash are ignored.
- [x] Converting a connected tile to water removes routes at both ends; water rejects new routes.
- [x] WPF fixture: actual click events update routes, tile switching restores checkbox
  values, unchecking removes the shared edge, and water disables both controls.
- [x] Rendered the actual map tile template and inspected a mixed road/rail connection.
- [ ] Validate UI clicks, tile switching, and overlays at different zoom levels on a full save.
- [ ] Test corners, polar rows, and first/last map columns against game neighbor behavior.
- [ ] Test multiple simultaneous neighbors, junctions, loops, and upgrades/removals in real saves.

Run `powershell.exe -NoProfile -STA -File Tests\TransportConnections.ps1` after building.

## In-game verification still required

- [ ] Save/reopen an edited copy in the editor, then load it in the game.
- [ ] Check road/rail graphics and movement in both directions across each edge type.
- [ ] Confirm resource transport, railway benefits, town/port access, rivers, and mountains.
- [ ] Verify cross-country connections respect the game's diplomatic/access rules.
- [ ] Advance a turn, save in-game, and reload without losing completed routes.
- [ ] Confirm preserved planned routes remain valid when completed routes change.

## Persistence

Completed segments are saved in the `.imp` data. The game has no connection bits
for an isolated tile, so checkbox intent that cannot be reconstructed from segments
is stored in `<save>.sgeditor.xml`. Keep that file with the save when moving it.
Its hash must match the save exactly; an in-game save change invalidates the old
editor selections. Completed routes remain in the game save even without the
companion file. A missing companion file makes the editor infer checkboxes from
completed segments. Checkbox edits reconnect the edited tile's six edges only;
opening a save does not automatically rewrite its entire transport network.

## Encoding evidence

Static inspection with the installed Capstone library of the supplied patched
executable identified completed-rail and completed-road routines at `0x551c60`
and `0x551cf0`, using in-memory offsets 6 and 8 (serialized offsets 7 and 9).
Ghost-route routines use in-memory offsets 7 and 9 (serialized offsets 8 and 10).
Direction tables at `0x74c010` and `0x74c020` contain six successive powers of two.
This is static evidence, not an observed in-game transport test. Existing field
labels for these four bytes were corrected; no infrastructure/building flags are
repurposed to represent checkbox selections.
