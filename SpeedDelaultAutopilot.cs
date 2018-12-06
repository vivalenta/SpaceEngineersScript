/* ############################################# 
Надстройка над автопілотом
*/

public Program() {}
public void Save() {}
double MaxSpeed = 999; // м/с²

DateTime lastTime;
Vector3D lastPosition;
double lastVelocity;

Vector3D Target = new Vector3D(0,0,0);
List<IMyThrust>[] ThrustersAll = new List<IMyThrust>[6];

public void Main(string argument) {
	string temp = null;
	List<MyWaypointInfo> coords = new List<MyWaypointInfo>();
	
	// get IMyShipController[0]  IMyRemoteControl
	IMyRemoteControl block = null; 
	List<IMyTerminalBlock> Controls = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(Controls);  
	if (Controls.Count > 0) { 
		block = Controls[0] as IMyRemoteControl;
	} 
	else {throw new Exception("Немає IMyRemoteControl");} 
    Vector3D pos = block.GetPosition();
	
	
	block.GetWaypointInfo(coords);
	Target = coords[0].Coords;
	
	List<IMyTerminalBlock> Thrusters = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyThrust>(Thrusters);	
	ThrustersAll = organizeThrusters(Thrusters, block);
	double[] maxForce = getMaxForce(ThrustersAll);
	
    float range = (float)((pos - Target).Length());    
	
	double mass = block.CalculateShipMass().TotalMass;
	double shipSpeed = block.GetShipSpeed();
	double lessAcceleration = maxForce[4]/mass;
	double maxStopPath = (shipSpeed*shipSpeed/(2*lessAcceleration));	
	double lessTime = shipSpeed / lessAcceleration;
	//temp += "Прискорювачі:" + maxForce[0].ToString("N") + " Н\n"; // Право
	//temp += "Прискорювачі:" + maxForce[1].ToString("N") + " Н\n"; // Ліво
	//temp += "Прискорювачі:" + maxForce[2].ToString("N") + " Н\n"; // Вверх
	//temp += "Прискорювачі:" + maxForce[3].ToString("N") + " Н\n"; // Вниз
	//temp += "Прискорювачі:" + maxForce[4].ToString("N") + " Н\n"; // Назад
	temp += "Прискорювачі:" + FormatLargeNumber(maxForce[5]) + "/" + FormatLargeNumber(maxForce[4]) + "Н\n"; // Вперед
	temp += "Дист.Зупинки: " + maxStopPath.ToString("N") + "m \n"; 
	temp += "Щвид.Зупинки: " + lessAcceleration.ToString("N") + " м/с²\n";
	temp += "Час  Зупинки: " + lessTime.ToString("N") + " c\n";
	DateTime currentTime = DateTime.Now;
	Vector3D currentPosition = block.GetPosition();
	
	double deltaTime = (currentTime - lastTime).TotalSeconds;
	double deltaDistance = Vector3D.Distance(lastPosition, currentPosition);
	double Distance = Vector3D.Distance(Target, currentPosition);
	double curentVelocity = deltaDistance / deltaTime;	
	
	
	if(shipSpeed > 90){ // автопілот розігнався ?
		if (Distance > maxStopPath)	{ //чи не пора тормозити ?
			block.SetAutoPilotEnabled(false);
			if (shipSpeed < MaxSpeed) { //Вперед до зірок
				block.DampenersOverride = true;
				SetMaxForce(ThrustersAll[5], true);
			}
			else { //Політ на крейсерській
				block.DampenersOverride = false;
				SetMaxForce(ThrustersAll[5], false); 
			}
		}
		else { //Пора зупинятись і це тепер проблема автопілота
			SetMaxForce(ThrustersAll[5], false);
			block.DampenersOverride = true;
			block.SetAutoPilotEnabled(true);
		};
	}
	else block.SetAutoPilotEnabled(true); //це тепер проблема автопілота


	// Save the current state as input for the next iteration.
	lastPosition = currentPosition;
	lastTime = currentTime;
	lastVelocity = curentVelocity;
	WriteToPanel(temp);
}


public double moduleFromVector(Vector3D vector){
	return System.Math.Sqrt((vector.X*vector.X) + (vector.Y*vector.Y) + (vector.Z*vector.Z));
}

public double[] getMaxForce (List<IMyThrust>[] thrustersAll){
	var force =  new double[6];
	for(int dire = 0;dire < 6; ++dire){
		List<IMyThrust> thrusters = thrustersAll[dire];
		foreach (IMyThrust thisThruster in thrusters) {
			if (thisThruster.IsWorking) force[dire] = force[dire] + ((double)thisThruster.MaxThrust);
		}
	}
	return force;
}

public void SetMaxForce (List<IMyThrust> thrustersAll, bool thrust){
	foreach (IMyThrust thisThruster in thrustersAll) {
		if (thrust) thisThruster.SetValueFloat("Override", (float)thisThruster.MaxThrust);
		else thisThruster.SetValueFloat("Override",0.0f);
	}
}

public List<IMyThrust>[] organizeThrusters(List<IMyTerminalBlock> thrusters, IMyTerminalBlock reference){
	Matrix refm;
	reference.Orientation.GetMatrix(out refm);
	
	var org = new List<IMyThrust>[6];
	for(int dir = 0;dir < 6; ++dir) org[dir]=new List<IMyThrust>();
	for(int i = 0; i < thrusters.Count; ++i){
		Matrix bmat;
		thrusters[i].Orientation.GetMatrix(out bmat);
		bmat=bmat*Matrix.Transpose(refm);
		int dir=(int)bmat.Forward.Dot(new Vector3(1,2,3));
		dir=(2*Math.Abs(dir)-2)+(Math.Sign(dir)+1)/2;
		org[dir].Add(thrusters[i] as IMyThrust);
	}
	return org;
}


public void WriteToPanel (string Message, float font = 1.2f, string panelName = "Дисплей"){ 
	IMyTextPanel thisPanel = GridTerminalSystem.GetBlockWithName(panelName) as IMyTextPanel;
	if (thisPanel == null) throw new Exception("Немає текстової панелі з і`ям " + panelName);
	thisPanel.SetValueFloat("FontSize", font); 
	thisPanel.SetValue("FontColor", new Color(204,255,0));  
	thisPanel.SetValue("BackgroundColor", new Color(0,0,0)); 
	thisPanel.ShowPublicTextOnScreen(); 
	thisPanel.WritePublicText(Message);
}


public static string FormatLargeNumber(double number) { 
	string ordinals = " kMGTPEZY"; 
	double compressed = number;
	var ordinal = 0; 
	while (compressed >= 1000) { 
			compressed /= 1000; 
			ordinal++; 
	}
	string res = Math.Round(compressed, 2, MidpointRounding.AwayFromZero).ToString(); 
	if (ordinal > 0) res += " " + ordinals[ordinal]; 
	return res; 
}

void GetDirectionTo(VRageMath.Vector3D TV, IMyTerminalBlock Origin, ref float Pitch, ref float Yaw)       
{       
  VRageMath.Vector3D OV = Origin.GetPosition();//Get positions of reference blocks.       
  VRageMath.Vector3D FV = Origin.WorldMatrix.Forward + Origin.GetPosition();       
  VRageMath.Vector3D UV = Origin.WorldMatrix.Up + Origin.GetPosition();      
  VRageMath.Vector3D RV = Origin.WorldMatrix.Right + Origin.GetPosition();      
        
  float TVOV = (float)((OV - TV).Length());//Get magnitudes of vectors.       
        
  float TVFV = (float)((FV - TV).Length());       
  float TVUV = (float)((UV - TV).Length());       
  float TVRV = (float)((RV - TV).Length());       
        
  float OVFV = (float)((FV - OV).Length());       
  float OVUV = (float)((UV - OV).Length());       
  float OVRV = (float)((RV - OV).Length());       
        
  float ThetaP = (float)(Math.Acos((TVUV * TVUV - OVUV * OVUV - TVOV * TVOV) / (-2 * OVUV * TVOV)));       
  //Use law of cosines to determine angles.       
  float ThetaY = (float)(Math.Acos((TVRV * TVRV - OVRV * OVRV - TVOV * TVOV) / (-2 * OVRV * TVOV)));       
        
  float RPitch = (float)(90 - (ThetaP * 180 / Math.PI));//Convert from radians to degrees.       
  float RYaw = (float)(90 - (ThetaY * 180 / Math.PI));       
        
  if (TVOV < TVFV) RPitch = 180 - RPitch;//Normalize angles to -180 to 180 degrees.       
  if (RPitch > 180) RPitch = -1 * (360 - RPitch);       
        
  if (TVOV < TVFV) RYaw = 180 - RYaw;       
  if (RYaw > 180) RYaw = -1 * (360 - RYaw);       
        
  Pitch = RPitch;//Set Pitch and Yaw outputs.       
  Yaw = RYaw;       
}