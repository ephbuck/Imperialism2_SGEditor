param(
  [string]$EditorPath = "$PSScriptRoot\..\Source\bin\Debug\Imperialism 2 SGEditor.exe",
  [string]$SavePath = "$PSScriptRoot\..\Imperialism II - Patched\Save\slot3.imp"
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationFramework
$path = (Resolve-Path -LiteralPath $EditorPath).Path
[void][Reflection.Assembly]::LoadFrom($path)
$code = @'
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Imperialism_2_SGEditor;
public static class TransferChecks {
 public static string Run(string path, bool crossCountry, bool newLand = false) {
  byte[] data=File.ReadAllBytes(path); int start=-1;
  for(int i=0;i<data.Length-259200;i++)
   if(data[i]==26 && data[i+1]==3 && data[i+2]==0 && data[i+3]==12 && data[i+4]==3){start=i+20+data[i+19];break;}
  if(start<0)throw new Exception("Map header missing");
  var world=new MapField[6480];
  for(int i=0;i<world.Length;i++){
   byte[] raw=new byte[40];Array.Copy(data,start+40*i,raw,0,40);
   world[i]=new MapField{RawData=raw,Idx=(short)i,ProvinceId=raw[23]};
  }
  int p=start+259201;var cities=new List<City>();
  for(int i=0;i<152;i++){
   byte[] raw=new byte[209+data[p+208]];Array.Copy(data,p,raw,0,raw.Length);p+=raw.Length;
   cities.Add(new City{RawData=raw,ProvinceId=(short)i,Name="Province "+i});
  }
  int rejected=0;bool moved=false;
  foreach(var tile in world){
   if(newLand ? !tile.IsSea : tile.IsSea)continue;
   foreach(int d in Enumerable.Range(0,6)){
    int j=DB.GetNeighborIndex(tile.Idx,d);if(j<0 || world[j].IsSea || world[j].ProvinceId==tile.ProvinceId)continue;
    var dest=cities[world[j].ProvinceId];
    if (!newLand && (cities[tile.ProvinceId].RawData[0] != dest.RawData[0]) != crossCountry) continue;
    byte[] originalTile=(byte[])tile.RawData.Clone();
    int destinationCount=dest.RawData[4], destinationGrain=dest.RawData[28];
    if(newLand){
     DB.SyncTerrainProfile(5,tile,world);
     tile.Update();
     if(tile.IsSea || tile.RawData[23]!=255 || tile.Img!="Resources/grain.jpg")throw new Exception("Converted land not displayed as unassigned grain");
     var vm=new MainWindowVM{WorldFields=world,Cities=cities,SelectedMapField=tile};
     vm.RefreshProvinceTargets();
     if(!vm.ProvinceTargets.Contains(dest))throw new Exception("Neighbor province missing after conversion");
     vm.SaveSavedGameCommand.Execute(null);
     if(vm.Status!="Assign all new land tiles to a province before saving.")throw new Exception("Unassigned land save was not blocked");
    }
    var before=world.SelectMany(t=>t.RawData).Concat(cities.SelectMany(c=>c.RawData)).ToArray();
    int old=tile.ProvinceId;
    try{ProvinceEditor.Move(tile,dest,world,cities);moved=true;}
    catch(InvalidOperationException){
     var after=world.SelectMany(t=>t.RawData).Concat(cities.SelectMany(c=>c.RawData));
     if(!before.SequenceEqual(after))throw new Exception("Rejected transfer mutated the save");
     if(newLand){tile.RawData=originalTile;tile.ProvinceId=255;}
     rejected++;continue;
    }
    foreach(var c in cities){
     var members=world.Where(t=>!t.IsSea && t.ProvinceId==c.ProvinceId).Select(t=>t.Idx).ToArray();
     if(members.Length!=c.RawData[4] || !new HashSet<short>(members).SetEquals(Enumerable.Range(0,members.Length).Select(k=>BitConverter.ToInt16(c.RawData,124+2*k))))throw new Exception("Membership mismatch");
     for(int k=0;k<12;k++){
      int other=BitConverter.ToInt16(c.RawData,72+2*k),entry=BitConverter.ToInt16(c.RawData,96+2*k);
      if(other<0)continue;
      if(entry<0 || entry>=6480 || world[entry].ProvinceId!=other)throw new Exception("Bad entry tile");
      if(!Enumerable.Range(0,12).Any(q=>BitConverter.ToInt16(cities[other].RawData,72+2*q)==c.ProvinceId))throw new Exception("Asymmetric adjacency");
     }
    }
    if(tile.RawData[23]!=dest.ProvinceId || tile.ProvinceId!=dest.ProvinceId)throw new Exception("Tile province mismatch");
    if(tile.RawData[DB.OWNER1]!=dest.RawData[0] || tile.RawData[DB.OWNER2]!=dest.RawData[1])throw new Exception("Destination ownership not applied");
    if(!world.Where(t=>t!=tile).All(t=>t.RawData[DB.OWNER1]==before[t.Idx*40+DB.OWNER1] && t.RawData[DB.OWNER2]==before[t.Idx*40+DB.OWNER2]))throw new Exception("Unrelated tile ownership changed");
    // A reverse move must remain valid.
    if(newLand){
     if(dest.RawData[4]!=destinationCount+1 || dest.RawData[28]!=destinationGrain+1)throw new Exception("New land did not increase membership/resource count");
     return "Passed sea-to-land conversion, neighboring province choices, save guard, assignment, ownership, membership, grain tally, and adjacency. Original save untouched.";
    }
    ProvinceEditor.Move(tile,cities[old],world,cities);
    return "Passed "+(crossCountry ? "cross-country" : "same-country")+" transfer, ownership, reversal, membership, reciprocal adjacency, entry tiles, and "+rejected+" atomic rejection checks. Original save untouched.";
   }
  }
  if(!moved)throw new Exception("No permitted transfer found in reference save");
  return "";
 }
}
'@
$reference = Join-Path $env:TEMP 'Imperialism2-ProvinceTestReference.dll'
Copy-Item -LiteralPath $path -Destination $reference -Force
Add-Type -TypeDefinition $code -ReferencedAssemblies @($reference,'WindowsBase','PresentationCore','PresentationFramework','System.Core')
[TransferChecks]::Run((Resolve-Path -LiteralPath $SavePath).Path, $false)
[TransferChecks]::Run((Resolve-Path -LiteralPath $SavePath).Path, $true)
[TransferChecks]::Run((Resolve-Path -LiteralPath $SavePath).Path, $false, $true)
