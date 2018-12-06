private readonly double _distantionOn = 20; // Дистанція відкривання дверей 			[EN] Distance to open Doors.  
private readonly bool _sensorSetDist = true; // Встановлювати максимальну дистанцію 		[EN] Set max range on sensor ?   
private readonly bool _enableDistantionInName = false; // 
private readonly string _doorIgnoreName = "Ворота";
public int airLock;

public void Main(){ 	 
    List<IMySensorBlock> Sensors = new List<IMySensorBlock>(); 
	List<MyDetectedEntityInfo> detection = new List<MyDetectedEntityInfo>(); 
	GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(Sensors); 
	 
	List<IMyInteriorLight> InteriorLights = new List<IMyInteriorLight>();  
    GridTerminalSystem.GetBlocksOfType<IMyInteriorLight>(InteriorLights);  
	airLock = 0;
	

	
    foreach (IMyInteriorLight thisInteriorLight in InteriorLights){  
		Vector3D thisInteriorLightPosition = thisInteriorLight.GetPosition(); 
		bool thisInteriorLightOn = false; 		
		if (thisInteriorLight.CustomName.Contains(_doorIgnoreName)) continue; 
		foreach (var thisSensor in Sensors) { 
			detection.Clear(); 
			if (_sensorSetDist){  
				thisSensor.SetValueFloat("Left", 50.0f);  
				thisSensor.SetValueFloat("Right", 50.0f);  
				thisSensor.SetValueFloat("Back", 50.0f);  
				thisSensor.SetValueFloat("Top", 50.0f);  
				thisSensor.SetValueFloat("Bottom", 50.0f);  
				thisSensor.SetValueFloat("Front", 50.0f);  
            } 		 
			thisSensor.DetectedEntities(detection);  
			 
			foreach(MyDetectedEntityInfo info in detection) { 
				if (thisInteriorLightOn) continue;
				if (info.Type != MyDetectedEntityType.CharacterHuman) continue; 
				Vector3D thisPosition = info.Position; 
				double distantionOn = _distantionOn; 
				if (_enableDistantionInName) { 
					string[] nameSplit = thisInteriorLight.CustomName.Split(' ');  
					if (!Double.TryParse(nameSplit[nameSplit.Length - 1], out distantionOn)){ 
						distantionOn = _distantionOn; 
					} 
				} 
				if ((Math.Abs(thisInteriorLightPosition.X - thisPosition.X)) < distantionOn & (Math.Abs(thisInteriorLightPosition.Y - thisPosition.Y)) < distantionOn & (Math.Abs(thisInteriorLightPosition.Z - thisPosition.Z)) < distantionOn){ 
					thisInteriorLight.GetActionWithName("OnOff_On").Apply(thisInteriorLight); 
					thisInteriorLightOn = true; 
				} else { 
					thisInteriorLight.GetActionWithName("OnOff_Off").Apply(thisInteriorLight);
				} 
				
			} 
        } 
		
	}
    
}

public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}