/** 
* Автор ідеї та початкового коду Greyhunt 
* Автор вдосконалення та оптимізації коду vivalenta 
* v1.1 Оптимізація коду 
* v1.2 Тепер программа керує всіма заводами та всіма сховищами кисню. 
* v1.3 Додано введення на дисплей 
* ================================= EN ======================================== 
* The author of the idea and source code Greyhunt 
* By improving and optimizing code vivalenta 
* v1.1 Optimization Package 
* v1.2 now program control all Oxygen Generators & all Oxygen Tanks. 
* v1.3 add debug display 
*/ 
 
string panelName = "Дисплей";	// Частина або повне ім'я текстової панелі.	[EN] Part or full name of the text panel.  
float percentDisable = 0.6f; 
 
bool ShowInfo = true; 
void Main() { 
	string text = ""; 
	List<IMyTerminalBlock> tanks = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks); 
	float ratio = 0.0f; 
	int tankCouner = 0; 
	foreach (IMyGasTank thisTank in tanks){ 
		if (thisTank.IsWorking && !thisTank.BlockDefinition.SubtypeId.Contains("Hydro")){ 
			ratio += thisTank.FilledRatio; 
			tankCouner++; 
			text += thisTank.CustomName + "\n" + encodeProgress(thisTank.FilledRatio * 100.0f) + "\n";  
		} 
	} 
	float ratioAll = ratio / tankCouner; 
	if(ratioAll > percentDisable) GasGenOnOff("OnOff_Off"); else GasGenOnOff("OnOff_On"); 
	text += "Всьго: \n" + encodeProgress(ratioAll * 100.0f) + "\n";  
	if (ShowInfo) WriteToPanel(text); 
} 
 
public string encodeProgress(float progressbar) { 
	int i; 
	string bar = ""; 
	if (progressbar > 100.0f || progressbar < 0.0f) progressbar = 100.0f; 
	for (i=1; i < progressbar/2; i++) {bar += "|";} 
	for (i=0; i < (100.0f-progressbar)/2.0f; i++) {bar += "'";} 
	return string.Format ("[{0}] {1:0.0}%", bar, progressbar); 
} 
 
public void GasGenOnOff (string state){ 
	List<IMyTerminalBlock> generators = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(generators); 
	foreach (IMyGasGenerator thisGasGen in generators) { thisGasGen.ApplyAction(state); } 
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