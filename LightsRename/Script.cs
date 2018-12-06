int count = 12;
string name = "Експерементальна";

List<IMyTerminalBlock> array = new List<IMyTerminalBlock>();

void Main() {
	array.Clear();
	GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(array);
	int counted = array.Count;
	if (count != 0) {
		int s = counted - count;
		for(int k = array.Count; s < counted; s++) {
			array[s].SetCustomName("[Світло] " + name);
			array[s].GetActionWithName("OnOff_Off").Apply(array[s]);
		}
	} else {
		for(int q = 0; q < array.Count; q++) {
			array[q].SetCustomName("[Світло] " + name);
			array[q].SetValue("Color", new Color(225,255,0)); 

		}
		throw new Exception ("Всьго доступно: " + counted);
	}
}