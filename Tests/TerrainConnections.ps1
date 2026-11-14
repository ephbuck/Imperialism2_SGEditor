param([string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe")

# Run with Windows PowerShell -STA. These fixtures do not read or write saves.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
[void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $EditorPath).Path)

function New-Tile([int]$Index, [byte]$Visual, $World) {
  $tile = New-Object Imperialism_2_SGEditor.MapField
  $tile.Idx = $Index
  $bytes = New-Object byte[] 40
  $bytes[21] = 255
  $bytes[22] = $Visual
  $tile.RawData = $bytes
  $World[$Index] = $tile
  [Imperialism_2_SGEditor.DB]::SyncTerrainProfile($Visual, $tile, $World)
  return $tile
}

function Assert-Mask($Tile, [int]$Offset, [int]$Expected, $Context) {
  if ($Tile.RawData[$Offset] -ne $Expected) {
    throw "$Context byte ${Offset}: expected $Expected, got $($Tile.RawData[$Offset])"
  }
}

$count = 0
foreach ($choice in [Imperialism_2_SGEditor.DB]::VisualLandTypes) {
  foreach ($centerIndex in @(218, 326)) {
    foreach ($direction in 0..5) {
      $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
      $center = New-Tile $centerIndex $choice.Id $world
      $neighborIndex = [Imperialism_2_SGEditor.DB]::GetNeighborIndex($centerIndex, $direction)
      $neighbor = New-Tile $neighborIndex $choice.Id $world
      [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center, $world)
      $bit = 1 -shl (($direction + 5) % 6)
      $oppositeBit = 1 -shl (($direction + 2) % 6)
      $isSea = $choice.Id -eq 0
      Assert-Mask $center 14 $(if ($isSea) { 0 } else { $bit }) $choice.Name
      Assert-Mask $neighbor 14 $(if ($isSea) { 0 } else { $oppositeBit }) $choice.Name

      $neighbor.RawData[22] = $(if ($isSea) { 1 } else { 0 })
      [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($neighbor, $world)
      Assert-Mask $center 14 0 "$($choice.Name) removal"
      if ($isSea) { Assert-Mask $center 15 $bit 'Sea-to-land connection' }
      $count++
    }
  }
}

# Verify the game's exceptions to exact visual matching, and category transitions.
foreach ($pair in @(@(7,8,14), @(8,7,14), @(12,18,14), @(18,12,14), @(7,9,15), @(9,7,15))) {
  $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
  $center = New-Tile 218 $pair[0] $world
  $neighbor = New-Tile 219 $pair[1] $world
  [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center, $world)
  Assert-Mask $center $pair[2] 2 "Mixed terrain $($pair[0])/$($pair[1])"
}
foreach ($pair in @(@(11,12), @(12,11), @(11,18), @(18,11), @(5,16), @(13,15))) {
  $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
  $center = New-Tile 218 $pair[0] $world
  $neighbor = New-Tile 219 $pair[1] $world
  [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center, $world)
  Assert-Mask $center 14 0 "Separate graphics $($pair[0])/$($pair[1])"
}
Write-Output "Passed $count tile-option/direction/parity cases and 12 mixed-terrain cases."
