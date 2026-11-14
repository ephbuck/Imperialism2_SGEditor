param([string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
[void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $EditorPath).Path)

foreach ($index in @(218,326)) {
  $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
  foreach ($i in 0..6479) {
    $tile = New-Object Imperialism_2_SGEditor.MapField
    $tile.Idx = $i
    $raw = New-Object byte[] 40
    $raw[1] = 5
    $raw[21] = 255
    $raw[23] = 255
    $raw[24] = 255
    $tile.RawData = $raw
    $tile.ProvinceId = 255
    $world[$i] = $tile
  }
  $neighbors = @(0..5 | ForEach-Object { [Imperialism_2_SGEditor.DB]::GetNeighborIndex($index,$_) })
  $land = $world[$neighbors[0]]
  [Imperialism_2_SGEditor.DB]::SyncTerrainProfile(5,$land,$world)
  $land.RawData[23] = 0
  $land.ProvinceId = 0
  $center = $world[$index]
  $center.RawData[22] = 5
  [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center,$world)
  foreach ($i in 0..6479) {
    $expected = if ($i -eq $index -or $i -eq $land.Idx) { 20 }
                elseif ($neighbors -contains $i) { 21 } else { 255 }
    if ($world[$i].RawData[21] -ne $expected) { throw "Unexpected resource on tile $i" }
    if ($expected -eq 21 -and ($world[$i].ImgResource -ne 'Resources/rfish.jpg' -or
        $world[$i].ResourceIconVis -ne [Windows.Visibility]::Visible)) { throw 'Fish icon did not refresh' }
  }
  # Later land edits must preserve manually selected neighboring water resources.
  $water = $world[$neighbors[1]]
  $water.RawData[21] = 255
  $center.RawData[22] = 2
  [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center,$world)
  if ($water.RawData[21] -ne 255) { throw 'Ordinary land edit replaced a neighboring resource' }
}
Write-Output 'Passed coastal fish assignment, unchanged land/distant water, icon refresh, and ordinary land-edit checks on both row parities.'
