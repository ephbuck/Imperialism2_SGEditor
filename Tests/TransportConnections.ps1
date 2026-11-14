param([string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe")
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
$path = (Resolve-Path -LiteralPath $EditorPath).Path
[void][Reflection.Assembly]::LoadFrom($path)
$code = @'
using System;
using System.IO;
using Imperialism_2_SGEditor;
public static class TransportChecks {
 static MapField Tile(int index) {
  var raw=new byte[40];raw[21]=255;raw[22]=1;raw[8]=42;raw[10]=17;
  return new MapField{Idx=(short)index,RawData=raw};
 }
 static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
 public static string Run() {
  int cases=0;
  foreach(int centerIndex in new[]{218,326})foreach(int direction in new[]{0,1,2,3,4,5})
  foreach(int a in new[]{1,2})foreach(int b in new[]{1,2}){
   var world=new MapField[6480];var center=Tile(centerIndex);
   int neighborIndex=DB.GetNeighborIndex(centerIndex,direction);var neighbor=Tile(neighborIndex);
   world[centerIndex]=center;world[neighborIndex]=neighbor;
   TransportEditor.Set(neighbor,world,(b&1)!=0,(b&2)!=0);
   TransportEditor.Set(center,world,(a&1)!=0,(a&2)!=0);
   int bit=1<<((direction+5)%6),opposite=1<<((direction+2)%6);
   bool rail=(a&2)!=0 && (b&2)!=0;
   bool road=!rail;
   Assert(center.RawData[7]==(rail?bit:0) && neighbor.RawData[7]==(rail?opposite:0),"Rail reciprocity");
   Assert(center.RawData[9]==(road?bit:0) && neighbor.RawData[9]==(road?opposite:0),"Road reciprocity");
   Assert(center.HasRoad==((a&1)!=0) && center.HasRail==((a&2)!=0),"Checkbox intent changed");
   Assert(center.RawData[8]==42 && center.RawData[10]==17 && center.RawData[29]==0,"Unrelated data changed");
   TransportEditor.Set(center,world,a==2,a==1);
   Assert(center.HasRoad==(a==2) && center.HasRail==(a==1),"Switch did not deselect previous type");
   bool switchedRail=a==1 && b==2;
   Assert(center.RawData[7]==(switchedRail?bit:0) && center.RawData[9]==(switchedRail?0:bit),"Switch did not update edges");
   Assert(neighbor.RawData[7]==(switchedRail?opposite:0) && neighbor.RawData[9]==(switchedRail?0:opposite),"Switch did not update neighbor");
   TransportEditor.Set(center,world,false,false);
   Assert(center.RawData[7]==0 && center.RawData[9]==0 && neighbor.RawData[7]==0 && neighbor.RawData[9]==0,"Removal left dangling segment");
   Assert(neighbor.HasRoad==((b&1)!=0) && neighbor.HasRail==((b&2)!=0),"Isolated neighbor lost selection");
   cases++;
  }
  var map=new MapField[6480];map[218]=Tile(218);map[219]=Tile(219);
  TransportEditor.Set(map[218],map,true,true);
  Assert(map[218].HasRail && !map[218].HasRoad,"Legacy both-type selection not normalized");
  TransportEditor.Set(map[218],map,true,false);
  Assert(map[218].HasRoad && map[218].RawData[9]==0,"Isolated road");
  TransportEditor.Set(map[219],map,false,true);
  Assert(map[218].RawData[9]==2 && map[219].RawData[9]==16 && !map[219].HasRoad,"Mixed edge");
  string folder=Path.Combine(Path.GetTempPath(),"imp-transport-"+Guid.NewGuid());Directory.CreateDirectory(folder);
  string save=Path.Combine(folder,"test.imp");byte[] data={1,2,3};
  // SaveSelections normally receives a complete world, so supply sea for unused slots.
  for(int i=0;i<map.Length;i++)if(map[i]==null){map[i]=Tile(i);map[i].RawData[1]=5;}
  try{
   TransportEditor.SaveSelections(save,data,map);
   var loaded=new MapField[6480];
   for(int i=0;i<map.Length;i++)loaded[i]=new MapField{Idx=(short)i,RawData=(byte[])map[i].RawData.Clone()};
   TransportEditor.LoadSelections(save,data,loaded);
   Assert(loaded[219].HasRail && !loaded[219].HasRoad,"Mixed checkbox intent not restored");
   TransportEditor.Set(map[218],map,false,false);
   TransportEditor.SaveSelections(save,data,map);
   var isolated=new MapField[6480];
   for(int i=0;i<map.Length;i++)isolated[i]=new MapField{Idx=(short)i,RawData=(byte[])map[i].RawData.Clone()};
   TransportEditor.LoadSelections(save,data,isolated);
   Assert(isolated[219].HasRail && isolated[219].RawData[7]==0,"Isolated selection not restored");
   var stale=new MapField[6480];
   for(int i=0;i<map.Length;i++)stale[i]=new MapField{Idx=(short)i,RawData=(byte[])map[i].RawData.Clone()};
   TransportEditor.LoadSelections(save,new byte[]{9},stale);
   Assert(!stale[219].HasRail,"Stale companion file applied");
  }finally{File.Delete(save+".sgeditor.xml");Directory.Delete(folder);}
  TransportEditor.Set(map[218],map,true,true);
  map[218].RawData[22]=0;
  DB.UpdateTerrainAndTransitions(map[218],map);
  Assert(!map[218].HasRoad && !map[218].HasRail && map[218].RawData[7]==0 && map[218].RawData[9]==0,"Sea retained transport");
  Assert(map[219].RawData[7]==0 && map[219].RawData[9]==0 && map[219].HasRail,"Sea conversion left connection");
  try{TransportEditor.Set(map[218],map,true,false);throw new Exception("Water accepted road");}catch(InvalidOperationException){}
  return "Passed "+cases+" directional/mixed transport cases; removal, planned-route preservation, isolated selections, companion-file reload/hash checks, and sea conversion.";
 }
}
'@
$reference = Join-Path $env:TEMP 'Imperialism2-TransportTestReference.dll'
Copy-Item -LiteralPath $path -Destination $reference -Force
Add-Type -TypeDefinition $code -ReferencedAssemblies @($reference,'WindowsBase','PresentationCore','PresentationFramework','System.Core','System.Xml.Linq')
[TransportChecks]::Run()
