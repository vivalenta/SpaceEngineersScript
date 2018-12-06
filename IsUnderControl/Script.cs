/**
Безпека понад усе, мене залюбило ловити дрона, що летить без дамперів на швидкості 100+ м/с
**/
void Main() {
	IMyShipController block = null; 
	List<IMyTerminalBlock> Controls = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(Controls); 
	if (Controls.Count > 0) { 
		block = Controls[0] as IMyShipController;
	} 
	else {throw new Exception("Немає IMyShipController");} 
	
	if (!block.IsUnderControl) block.GetActionWithName("DampenersOverride").Apply(block); 
 }