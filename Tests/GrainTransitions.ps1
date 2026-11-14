param(
  [string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe",
  [string]$SavePath
)

# Run with Windows PowerShell -STA after building the editor.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
[void][Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $EditorPath).Path)

function Assert-Equal($Actual, $Expected, $Message) {
  if ($Actual -ne $Expected) { throw "${Message}: expected $Expected, got $Actual" }
}

# Exercise both staggered row parities and all six edges independently.
foreach ($centerIndex in @(218, 326)) {
  foreach ($direction in 0..5) {
    $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
    foreach ($index in 0..6479) {
      $field = New-Object Imperialism_2_SGEditor.MapField
      $field.Idx = $index
      $bytes = New-Object byte[] 40
      $bytes[21] = 255
      $bytes[22] = 1
      $field.RawData = $bytes
      $world[$index] = $field
    }
    $center = $world[$centerIndex]
    $neighborIndex = [Imperialism_2_SGEditor.DB]::GetNeighborIndex($centerIndex, $direction)
    $neighbor = $world[$neighborIndex]
    $neighbor.RawData[22] = 5
    [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($neighbor, $world)
    $center.RawData[22] = 5
    [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center, $world)
    $bit = 1 -shl (($direction + 5) % 6)
    $oppositeBit = 1 -shl (($direction + 2) % 6)
    Assert-Equal $center.RawData[14] $bit 'New grain connection'
    Assert-Equal $neighbor.RawData[14] $oppositeBit 'Neighbor grain connection'
    Assert-Equal $center.RawData[21] 20 'Default grain production'

    # Another farm graphic must not connect to grain just because both are farmland.
    $center.RawData[22] = 16
    [Imperialism_2_SGEditor.DB]::UpdateTerrainAndTransitions($center, $world)
    Assert-Equal $center.RawData[14] 0 'Different farm graphic'
    Assert-Equal $neighbor.RawData[14] 0 'Removed neighbor connection'
  }
}
Write-Output 'Passed grain addition/removal in all six directions on both row parities.'

# Optional comparison against a game-generated save; never writes to the save.
if ($SavePath) {
  $data = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $SavePath).Path)
  $start = -1
  for ($i = 0; $i -lt $data.Length - 259200; $i++) {
    if ($data[$i] -eq 26 -and $data[$i+1] -eq 3 -and $data[$i+2] -eq 0 -and
        $data[$i+3] -eq 12 -and $data[$i+4] -eq 3) {
      $start = $i + 20 + $data[$i+19]
      break
    }
  }
  if ($start -lt 0) { throw 'Map header not found' }
  $world = New-Object 'Imperialism_2_SGEditor.MapField[]' 6480
  foreach ($index in 0..6479) {
    $field = New-Object Imperialism_2_SGEditor.MapField
    $field.Idx = $index
    $bytes = New-Object byte[] 40
    [Array]::Copy($data, $start + 40 * $index, $bytes, 0, 40)
    $field.RawData = $bytes
    $world[$index] = $field
  }
  $count = 0
  foreach ($field in $world) {
    if ($field.RawData[22] -ne 5) { continue }
    $expected = $field.RawData[14]
    [Imperialism_2_SGEditor.DB]::RecalculateTileTransitions($field, $world)
    Assert-Equal $field.RawData[14] $expected "Saved grain tile $($field.Idx)"
    $count++
  }
  if ($count -eq 0) { throw 'Save contains no grain tiles' }
  Write-Output "Matched all $count grain connections in the reference save."
}
