param(
  [string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe",
  [string]$PreviewPath
)

# Run with Windows PowerShell -STA. Exercise actual WPF overlay bindings.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
$assembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $EditorPath).Path)
if (-not [Windows.Application]::Current) { $app = New-Object Windows.Application }
[Windows.Application]::ResourceAssembly = $assembly
$window = New-Object Imperialism_2_SGEditor.MainWindow
$style = $window.FindResource('TileBuildingImageStyle')
$tile = New-Object Imperialism_2_SGEditor.MapField
$bytes = New-Object byte[] 40
$bytes[21] = 255
$bytes[22] = 1
$tile.RawData = $bytes
$image = New-Object Windows.Controls.Image
$image.Style = $style
$image.DataContext = $tile
$gallery = New-Object Windows.Controls.StackPanel
$gallery.Orientation = 'Horizontal'
$gallery.Background = [Windows.Media.Brushes]::DarkOliveGreen

$cases = @(@(0,'None'), @(4,'Town'), @(34,'Town'), @(38,'Town'), @(42,'Town'),
           @(46,'Town'), @(54,'Capital'), @(55,'Capital'), @(62,'Fort'),
           @(63,'Port'), @(1,'Building'), @(255,'None'))
foreach ($case in $cases) {
  $tile.RawData[29] = [byte]$case[0]
  $tile.Update()
  [Windows.Threading.Dispatcher]::CurrentDispatcher.Invoke(
    [Windows.Threading.DispatcherPriority]::Background, [Action]{})
  if ($tile.BuildingKind -ne $case[1]) { throw "Incorrect building kind for $($case[0])" }
  $expectedVisibility = if ($case[1] -eq 'None') { 'Collapsed' } else { 'Visible' }
  if ($image.Visibility.ToString() -ne $expectedVisibility) { throw 'Overlay visibility did not refresh' }
  if ($case[1] -ne 'None') {
    $key = switch ($case[1]) {
      'Capital' { 'CapitalBuildingImage' }
      'Fort' { 'FortBuildingImage' }
      'Port' { 'PortBuildingImage' }
      default { 'TownBuildingImage' }
    }
    if ($image.Source -ne $window.FindResource($key)) { throw 'Incorrect building image' }
  }
}
# Fort-level bytes alone must not put buildings on otherwise empty tiles.
$tile.RawData[29] = 0
$tile.RawData[32] = 4
$tile.Update()
if ($tile.BuildingKind -ne 'None') { throw 'Empty tile received a building overlay' }

if ($PreviewPath) {
  foreach ($name in @('Town','Capital','Fort','Port')) {
    $panel = New-Object Windows.Controls.StackPanel
    $panel.Margin = New-Object Windows.Thickness 12
    $icon = New-Object Windows.Controls.Image
    $icon.Source = $window.FindResource($name + 'BuildingImage')
    $icon.Width = 66
    $icon.Height = 66
    [void]$panel.Children.Add($icon)
    $label = New-Object Windows.Controls.TextBlock
    $label.Text = $name
    $label.Foreground = [Windows.Media.Brushes]::White
    $label.HorizontalAlignment = 'Center'
    [void]$panel.Children.Add($label)
    [void]$gallery.Children.Add($panel)
  }
  $gallery.Measure((New-Object Windows.Size 360,120))
  $gallery.Arrange((New-Object Windows.Rect 0,0,360,120))
  $bitmap = New-Object Windows.Media.Imaging.RenderTargetBitmap 360,120,96,96,([Windows.Media.PixelFormats]::Pbgra32)
  $bitmap.Render($gallery)
  $encoder = New-Object Windows.Media.Imaging.PngBitmapEncoder
  $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
  $stream = [IO.File]::Create($PreviewPath)
  try { $encoder.Save($stream) } finally { $stream.Dispose() }
}
$window.Close()
Write-Output 'Passed 12 building-code cases, live binding updates, and empty-tile checks.'
