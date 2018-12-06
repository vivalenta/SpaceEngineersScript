public void Main(string argument) {
List<ITerminalAction> actions = new List<ITerminalAction>();

List<IMyTerminalBlock> tanks = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks); 
	string actionsText = null;
	
tanks[0].GetActions(actions);
foreach (var action in actions)
{
    actionsText += action.Id + " " + action.Name + "\n";
}
WriteToPanel(actionsText, 1.2f, 1);
}



public void WriteToPanel(string Message, float font = 1.2f, string panelName = "Текстова Панель"){ 
	IMyTextPanel thisPanel = GridTerminalSystem.GetBlockWithName(panelName);
	thisPanel.SetValueFloat("FontSize", font); 
	thisPanel.SetValue("FontColor", new Color(204,255,0));  
	thisPanel.SetValue("BackgroundColor", new Color(0,0,0)); 
	thisPanel.ShowPublicTextOnScreen(); 
	thisPanel.WritePublicText(Message); 
}
