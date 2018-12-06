const string ORE_CONTAINER = "Контейнер руди";
const string INGOT_CONTAINER = "Контейнер злитків";
const string COMPONENT_CONTAINER = "Контейнер компонентів";
const string AMMO_CONTAINER = "Контейнер зброї";
const string LOCKED_CONTAINER = "Locked";
bool AUTO_RENAME = false;
List<IMyTerminalBlock> containers;
List<IMyTerminalBlock> oreContainers;
List<IMyTerminalBlock> ingotContainers;
List<IMyTerminalBlock> componentContainers;
List<IMyTerminalBlock> ammoContainers;
List<IMyTerminalBlock> gasContainers;

// destination inventory items are being sent to
IMyInventory containerDestination;

public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

void Main(string argument)
{
	if (argument == "ren") AUTO_RENAME = true;
	containers = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(containers);

    oreContainers = new List<IMyTerminalBlock>();
    ingotContainers = new List<IMyTerminalBlock>();
    componentContainers = new List<IMyTerminalBlock>();
    ammoContainers = new List<IMyTerminalBlock>();
    gasContainers = new List<IMyTerminalBlock>();


    // find all the containers with our custom name and store them
    GridTerminalSystem.SearchBlocksOfName(ORE_CONTAINER, oreContainers);
    GridTerminalSystem.SearchBlocksOfName(INGOT_CONTAINER, ingotContainers);
    GridTerminalSystem.SearchBlocksOfName(COMPONENT_CONTAINER, componentContainers);
    GridTerminalSystem.SearchBlocksOfName(AMMO_CONTAINER, ammoContainers);


    
    // sort everything into the respective container
    sort(ORE_CONTAINER, "Ore", oreContainers);
    sort(INGOT_CONTAINER, "Ingot", ingotContainers);
    sort(COMPONENT_CONTAINER, "Component", componentContainers);
    sort(AMMO_CONTAINER, "Ammo", ammoContainers);
    sort(AMMO_CONTAINER, "Gun", ammoContainers);// for drills, grinders, etc.
    sort(AMMO_CONTAINER, "Gas", gasContainers);// for hydro tanks
    sort(AMMO_CONTAINER, "Oxygen", gasContainers);// for oxygen tanks

    
}

void sort(string containerName, string type, List<IMyTerminalBlock> typeContainers)
{
    if (typeContainers == null) return;
    containerDestination = null;
    for (int n = 0; n < typeContainers.Count; n++)
    {
        var _container = typeContainers[n];
		if (_container.GetInventory(0) == null) continue;
        var containerInv = _container.GetInventory(0);
        if (!IsFull(containerInv))
        {
            containerDestination = containerInv;
            break;
        }
    }
    if (containerDestination == null) return;



    // search all containers
    foreach (IMyTerminalBlock container in containers)
    {
		IMyInventory containerInv = null;
		if (container as IMyReactor != null) continue;
		if (container as IMyGasGenerator != null) continue;
        if (container.CustomName.Contains(LOCKED_CONTAINER)) continue;
        if (container.GetInventory(1) != null) {containerInv = container.GetInventory(1);}
		else if (container.GetInventory(0) != null) {containerInv = container.GetInventory(0);}
        if (container.GetInventory(0) == null & container.GetInventory(1) == null) continue;
		var containerItems = containerInv.GetItems();

        for (int j = containerItems.Count - 1; j >= 0; j--)
        {            
            if (containerItems[j].Content.ToString().Contains(type) && !container.CustomName.Contains(containerName))
            {
                containerInv.TransferItemTo(containerDestination, j, null, true, null);
            }
        }
		if (container as IMyCargoContainer != null & AUTO_RENAME & !container.CustomName.Contains("Контейнер")) container.SetCustomName(COMPONENT_CONTAINER);
        // add percentages to names
        if (container.CustomName.Contains(containerName))
        {
            // get rid of the % symbol the player added
            container.SetCustomName(container.CustomName.Replace("%", "").Replace("  ", " "));
            // get the container's name without the precentage
            string[] delim = { " - " };
            string[] fullName = container.CustomName.Split(delim, StringSplitOptions.RemoveEmptyEntries);
            string name = fullName[0];
            string percentage = " - " + getPercent(containerInv).ToString("0.##") + "%";
            container.SetCustomName((name + percentage).Replace("  ", " "));
        }
    }

}

float getPercent(IMyInventory inv)
{
    return ((float)inv.CurrentVolume / (float)inv.MaxVolume) * 100f;
}


bool IsFull(IMyInventory inv)
{
    if (getPercent(inv) >= 99)
        return true;
    else
        return false;
}