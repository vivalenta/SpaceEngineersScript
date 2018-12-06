/** 
* Скрипт для виводу на текстову панель заповненності гріндерів та ящиків 
* Виводить інфо на текстову панель
* Автор ідеї та початкового коду Greyhunt 
* Автор вдосконалення та оптимізації коду vivalenta 
* v1.2 Оптимізація коду 
* v1.2 Додано опис параметрів на ангійській мові 
* v2.0 Оновлення коду
* ================================= EN ======================================== 
* Script for output to a text panel occupancy Grinder and boxes 
* Displays info text on the panel
* The author of the idea and source code Greyhunt 
* By improving and optimizing code vivalenta 
* v1.2 Optimization Package 
* v1.2 add English comments 
* v2.0 renew code
*/ 
string panelName = "Дисплей";	// Частина або повне ім'я текстової панелі.	[EN] Part or full name of the text panel. 
string langWaring = "Стій";		// Текст попередження про заповненість		[EN] Text warning occupancy 

void Main() {
	string output = "";
	List<IMyTerminalBlock> Grinders = new List<IMyTerminalBlock>();
	List<IMyTerminalBlock> Cargos = new List<IMyTerminalBlock>();
	List<IMyTerminalBlock> Drils = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(Grinders);
	GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Cargos);	
	GridTerminalSystem.GetBlocksOfType<IMyShipDrill>(Drils);
	
	float curr = 0.0f;
	float max = 0.0f;
	float percent = 0.0f;
	foreach (IMyShipDrill thisDril in Drils) {		
		var sourceInventoryG =  thisDril.GetInventory(0);
		float curriG = (float)sourceInventoryG.CurrentVolume;
		float maxiG = (float)sourceInventoryG.MaxVolume;
		curr = curr + curriG;
		max = max + maxiG;
		percent = (curriG/maxiG)*100;
		output += thisDril.CustomName + "\n" + encodeProgress(percent) + "\n"; 
	}
	
	foreach (IMyShipGrinder thisGrinder in Grinders) {		
		var sourceInventoryG =  thisGrinder.GetInventory(0);
		float curriG = (float)sourceInventoryG.CurrentVolume;
		float maxiG = (float)sourceInventoryG.MaxVolume;
		curr = curr + curriG;
		max = max + maxiG;
		percent = (curriG/maxiG)*100;
		output += thisGrinder.CustomName + "\n" + encodeProgress(percent) + "\n"; 
	}
	
	
	foreach (IMyCargoContainer thisCargo in Cargos) {		
		var sourceInventoryG =  thisCargo.GetInventory(0);
		float curriG = (float)sourceInventoryG.CurrentVolume;
		float maxiG = (float)sourceInventoryG.MaxVolume;
		curr = curr + curriG;
		max = max + maxiG;		
	}
	percent = (curr/max)*100;
	output += "Всьго: \n" + encodeProgress(percent) + "\n"; 
	WriteToPanel(output);
}

public string encodeProgress(float progressbar) {
	int i;
	string bar = "", war = "";
	if (progressbar > 100.0f || progressbar < 0.0f) progressbar = 100.0f;
	if (progressbar > 90.0f) {war = langWaring;}
	for (i=1; i < progressbar/2; i++) {bar += "|";}
	for (i=0; i < (100.0f-progressbar)/2.0f; i++) {bar += "'";}
	return string.Format ("[{0}] {1:0.0}% {2}", bar, progressbar, war);
}

public void WriteToPanel (string Message, float font = 1.2f){ 
	IMyTextPanel thisPanel = GridTerminalSystem.GetBlockWithName(panelName) as IMyTextPanel;
	if (thisPanel == null) throw new Exception("Немає текстової панелі з і`ям " + panelName);
	thisPanel.SetValueFloat("FontSize", font); 
	thisPanel.SetValue("FontColor", new Color(204,255,0));  
	thisPanel.SetValue("BackgroundColor", new Color(0,0,0)); 
	thisPanel.ShowPublicTextOnScreen(); 
	thisPanel.WritePublicText(Message);
}
