/* ############################################# 
Stellt automatisch einen Grundstock an Komponenten her der hier hinterlegt wird. 

Script by n30nl1ght 

added vivalenta
intercept all errors, now working with all assembler and all containers, but use to create 1 (if no assembler whith name thisAssemblerName - then use any)
CustomData move to Program Block
add autorestart, not need timer (only program block)
add list to copy-paste in CustomData
add if no customdata load default
translate Ukrainian (comment in Eng)
#############################################*/

/* Copy list to CustomData in Program Block, Select --> Ctrl+C --> Ctrl+V
BulletproofGlass=500
Computer=500
Construction=1000
Detector=10
Display=50
Explosives=2
Girder=150
GravityGenerator=1
InteriorPlate=500
LargeTube=100
Medical=1
MetalGrid=500
Motor=100
PowerCell=10
RadioCommunication=4
Reactor=100
SmallTube=200
SolarCell=32
SteelPlate=2000
Thrust=10
NATO_5p56x45mm=50
Superconductor=10
*/
// Configure
public string ThisAssemblerName = "Збирач"; // Name Assembler to use


public Program()
{
Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument)
{
Dictionary<String, VRage.MyFixedPoint> materialQuota = new Dictionary<String, VRage.MyFixedPoint>();
Dictionary<String, VRage.MyFixedPoint> materialCurrent = new Dictionary<String, VRage.MyFixedPoint>();

List<IMyAssembler> assemblers = new List<IMyAssembler>();
GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers);

IMyAssembler thisAssembler = GridTerminalSystem.GetBlockWithName(ThisAssemblerName) as IMyAssembler;
if (thisAssembler == null || !thisAssembler.IsFunctional)
{
if (assemblers.Count == 0) Echo("Немає Збирачів"); //No assembler
else
{
foreach (IMyAssembler currentAssembler in assemblers)
{
if (currentAssembler.IsFunctional) thisAssembler = currentAssembler;
}
if (thisAssembler == null) Echo("Немає збирачів \nабо вони не функціонують"); // No Functional Assembler
else Echo("Немає збирача " + ThisAssemblerName + "\nВикористаю " + thisAssembler.CustomName + "\n"); // No need Assembler, use any
}
}

List<IMyTerminalBlock> containers = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(containers);

foreach (IMyTerminalBlock container in containers)
{
if (container.GetInventory(0) == null) continue;
var containerInventory = container.GetInventory(0);
var containerItems = containerInventory.GetItems();
foreach (IMyInventoryItem containerItem in containerItems)
{
string containerItemName = containerItem.Content.SubtypeId.ToString();
containerItemName = CreateBlueprintName(containerItemName);

if (materialCurrent.ContainsKey(containerItemName))
{
materialCurrent[containerItemName] += containerItem.Amount;
}
else
{
materialCurrent.Add(containerItemName, containerItem.Amount);
}
}
}

string inputs = Me.CustomData;
try
{
string[] quotaComponent = inputs.Split('\n');
foreach (string s in quotaComponent)
{
string[] quota = s.Split('=');
materialQuota.Add(CreateBlueprintName(quota[0]), Convert.ToInt32(quota[1]));
}
}
catch (Exception)
{
materialQuota.Add(CreateBlueprintName("BulletproofGlass"), 50);
materialQuota.Add(CreateBlueprintName("Computer"), 20);
materialQuota.Add(CreateBlueprintName("Construction"), 100);
materialQuota.Add(CreateBlueprintName("Detector"), 1);
materialQuota.Add(CreateBlueprintName("Display"), 5);
materialQuota.Add(CreateBlueprintName("Girder"), 15);
materialQuota.Add(CreateBlueprintName("InteriorPlate"), 50);
materialQuota.Add(CreateBlueprintName("LargeTube"), 10);
materialQuota.Add(CreateBlueprintName("MetalGrid"), 50);
materialQuota.Add(CreateBlueprintName("Motor"), 10);
materialQuota.Add(CreateBlueprintName("PowerCell"), 1);
materialQuota.Add(CreateBlueprintName("RadioCommunication"), 4);
materialQuota.Add(CreateBlueprintName("Reactor"), 9);
materialQuota.Add(CreateBlueprintName("SmallTube"), 20);
materialQuota.Add(CreateBlueprintName("SolarCell"), 2);
materialQuota.Add(CreateBlueprintName("SteelPlate"), 200);
materialQuota.Add(CreateBlueprintName("Thrust"), 1);
Echo("Немає вказівок, див. опис\nБуде завантажено за умовчанням"); // No CustomData, load default

}

foreach (IMyAssembler thisAssembler1 in assemblers)
{
IMyInventory assemblerInventory = thisAssembler1.GetInventory(1);
List<IMyInventoryItem> assemblerItems = assemblerInventory.GetItems();
List<MyProductionItem> prodList = new List<MyProductionItem>();

foreach (IMyInventoryItem assemblerItem in assemblerItems)
{
string assemblerItemName = assemblerItem.Content.SubtypeId.ToString();
assemblerItemName = CreateBlueprintName(assemblerItemName);

if (materialCurrent.ContainsKey(assemblerItemName))
{
materialCurrent[assemblerItemName] += assemblerItem.Amount;
}
else
{
materialCurrent.Add(assemblerItemName, assemblerItem.Amount);
}
}


if (thisAssembler1.IsQueueEmpty)
{
Echo(thisAssembler1.CustomName + " : Очікує"); //Wait
}
else
{
Echo(thisAssembler1.CustomName + " : Збирання"); //Work

thisAssembler1.GetQueue(prodList);
foreach (MyProductionItem prod in prodList)
{
string componentName = prod.BlueprintId.SubtypeName;
componentName = CreateBlueprintName(componentName);

if (materialCurrent.ContainsKey(componentName))
{
materialCurrent[componentName] += prod.Amount;
}
else
{
materialCurrent.Add(componentName, prod.Amount);
}
}

}
}
if (thisAssembler != null)
{
foreach (KeyValuePair<string, VRage.MyFixedPoint> s in materialQuota)
{
VRage.MyFixedPoint componentsToBuild;
VRage.MyFixedPoint componentCurrent = 0;
VRage.MyFixedPoint componentQuota = s.Value;
if (materialCurrent.ContainsKey(s.Key))
{
componentCurrent = materialCurrent[s.Key];
componentsToBuild = componentQuota - componentCurrent;
}
else
{
componentsToBuild = componentQuota;
}

if (componentsToBuild > 0)
{
var objectIdToAdd = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + s.Key);
thisAssembler.AddQueueItem(objectIdToAdd, componentsToBuild);


Echo(s.Key + " : " + componentCurrent.ToString() + " / " + componentQuota.ToString() + " = " + componentsToBuild.ToString());

}
}
}
}

public string CreateBlueprintName(string name)
{
switch (name)
{
case "RadioCommunication": name += "Component"; break;
case "Computer": name += "Component"; break;
case "Reactor": name += "Component"; break;
case "Detector": name += "Component"; break;
case "Construction": name += "Component"; break;
case "Thrust": name += "Component"; break;
case "Motor": name += "Component"; break;
case "Explosives": name += "Component"; break;
case "Girder": name += "Component"; break;
case "GravityGenerator": name += "Component"; break;
case "Medical": name += "Component"; break;
case "NATO_25x184mm": name += "Magazine"; break;
case "NATO_5p56x45mm": name += "Magazine"; break;
case "Superconductor": name += "Component"; break;
}
return name;
}