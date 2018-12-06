/** 
* Скрипт для виводу на текстову панель заповненності гріндерів та ящиків 
* Виводить інфо на першу текстову панель та \ або оверлеем на камеру (є глюк гри, не оновлюеться автоматично) 
* Автор ідеї та початкового коду Greyhunt 
* Автор вдосконалення та оптимізації коду vivalenta 
* v1.2 Оптимізація коду 
* v1.2 Додано опис параметрів на ангійській мові 
* ================================= EN ======================================== 
* Script for output to a text panel occupancy Grinder and boxes 
* Displays info text on the first panel and \ or overlay the camera (a bug the game, camera name not up automatically) 
* The author of the idea and source code Greyhunt 
* By improving and optimizing code vivalenta 
* v1.2 Optimization Package 
* v1.2 add English comments 
*/ 
string PanelName = "Текстова панель";	// Частина або повне ім'я текстової панелі.	[EN] Part or full name of the text panel. 
bool CameraEnable = true;				// Оверлей на камеру вкл./викл.				[EN] Overlay on camera On / Off 
string langName = "Контейнери";			// Рядок зверху над всім					[EN] The line was over all 
string langWaring = "Стій";				// Текст попередження про заповненість		[EN] Text warning occupancy 
 
// ================================ Тіло программи, бажано не чіпати! ======================================= 
// ================================ [EN] The body programs, preferably not touch! =========================== 
void Main() { 
	float max = 0.0f, curr = 0.0f, maxG = 0.0f, currG = 0.0f, percenti = 0.0f, percentG = 0.0f; 
	string output = langName + "\n"; 
 
	var Containers = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(Containers); 
 
	var Grinders = new List<IMyTerminalBlock>();	 
	GridTerminalSystem.GetBlocksOfType<IMyShipGrinder>(Grinders);  
 
	for ( int i = 0; i <Containers.Count; i++ ) { 
		IMyCargoContainer thisContainer = Containers[i] as IMyCargoContainer;  
		var sourceInventory =  thisContainer.GetInventory(0); 
		var curri = sourceInventory.CurrentVolume; 
		var maxi = sourceInventory.MaxVolume; 
		curr += (float)curri;  
		max += (float)maxi;  
		percenti = (curr/max)*100;  
		output += encodeProgress(percenti) + "\n";  
		 
	} 
	for ( int i = 0; i <Grinders.Count; i++ ){ 
		IMyShipGrinder thisGrinder = Grinders[i] as IMyShipGrinder; 
		var sourceInventoryG =  thisGrinder.GetInventory(0); 
		var curriG = sourceInventoryG.CurrentVolume; 
		var maxiG = sourceInventoryG.MaxVolume; 
		currG = (float)curriG; 
		maxG = (float)maxiG; 
		curr += (float)currG; 
		max += (float)maxG; 
		percentG = (currG/maxG)*100; 
		output += thisGrinder.CustomName + "\n" + encodeProgress(percentG) + "\n";  
	} 
	WriteToPanel(output); 
	double percent = Convert.ToDouble((curr/max)*100); 
	if (CameraEnable) { CameraOverlay(percent); } 
} 
 
public void CameraOverlay (double percent_C){ 
	IMyCameraBlock Camera = null; 
	var Cameras = new List<IMyTerminalBlock>();	 
	GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(Cameras);  
	Camera = Cameras[0] as IMyCameraBlock; 
	string CameraText = encodeProgress(percent_C); 
	Camera.SetCustomName(CameraText); 
} 
public void WriteToPanel(string text1){ 
	IMyTextPanel thisPanel = null; 
	var Panels = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels);  
	thisPanel = Panels[0] as IMyTextPanel; 
	thisPanel.SetValueFloat("FontSize", 1.2f); 
	thisPanel.SetValue("FontColor", Color.Green); 
	thisPanel.SetValue("BackgroundColor", Color.Black); 
	thisPanel.ShowPublicTextOnScreen(); 
	thisPanel.WritePublicTitle("<3 " + langName); 
	thisPanel.WritePublicText(text1); 
} 
public string encodeProgress(double progressbar) { 
	int i; 
	string bar = "", war = ""; 
	if (progressbar > 90) {war = langWaring;} 
	for (i=1; i < progressbar/2; i++) {bar += "|";} 
	for (i=0; i < (100-progressbar)/2; i++) {bar += "'";} 
	return string.Format ("[{0}] {1:0.0}% {2}", bar, progressbar, war); 
}