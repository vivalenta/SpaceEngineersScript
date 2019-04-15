public Program() {}
public void Save() {}

string GoTrustGroup = "Thrust"; //name thrusters
double SaveDistans  = 7; // stop distantion
double tagertGravity = 9.81;
bool infoToLcd = false;
bool infoEcho = true;
bool allTanksWorking = true;
// double oxygenCapacity = 100000;
// масса програмного блока

public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string argument) {
	
	string temp = null;
	double elevation = 0;
	float gyroPitch = 0;       
    float gyroYaw = 0;  
	double maxForce = getMaxForce()*0.9;
	
	bool done = false;
	// get IMyShipController[0]
	IMyShipController block = null; 
	List<IMyTerminalBlock> Controls = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(Controls);  
	if (Controls.Count > 0) { 
		block = Controls[0] as IMyShipController;
	} 
	else {throw new Exception("Немає IMyShipController");} 
    Vector3D grav = block.GetNaturalGravity(); 
	Vector3D pos = block.GetPosition();
    List<AdvGyro> AdvancedGyros = new List<AdvGyro>();
	AdvancedGyros = AdvGyro.GetAllGyros(GridTerminalSystem, block, true);  
	
	Vector3D Target = new Vector3D(pos.X-grav.X,pos.Y-grav.Y,pos.Z-grav.Z);     
            
    var x = Math.Floor(Target.GetDim(0)).ToString("0.00");       
    var y = Math.Floor(Target.GetDim(1)).ToString("0.00");       
    var z = Math.Floor(Target.GetDim(2)).ToString("0.00");       
        
    float range = (float)((pos - Target).Length());       	
    GetDirectionTo(Target, block, ref gyroPitch, ref gyroYaw);

	
	double mass = block.CalculateShipMass().TotalMass;
	double shipSpeed = block.GetShipSpeed();
	
	bool gravityExist = block.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation);	
	double gravity =  moduleFromVector(grav);
	double gravityForce = (gravity * mass);
	double maxStopPath = (shipSpeed*shipSpeed/(2*((maxForce/ mass) - tagertGravity)));	
	double lessForse = (maxForce - gravityForce);
	double lessAcceleration = lessForse/mass;
	double lessTime = shipSpeed / lessAcceleration;
	double lessDistantion = ((shipSpeed*shipSpeed)/(2*lessAcceleration));
	
	//temp += gyroPitch.ToString("N") + " - " + gyroYaw.ToString("N") + "\n";;
	temp += "Водню лиш. : " + hydrogenLess().ToString("N");
	if (allTanksWorking) temp += " AllOk!\n"; else temp += " NotOK!\n";
	temp += "Масса кор. : " + mass.ToString("N") + " Kg \n";
	temp += "Макс. Літає: " + (maxForce / mass / tagertGravity).ToString("N") + " G\n";
	//temp += " GPS:Tagert:" + x + ":" + y + ":" + z + ":\n";
	temp += "Макс. час з: " + (shipSpeed / ((maxForce/ mass) - tagertGravity)).ToString("N") + " c\n";
	temp += "Макс. дист.: " + (maxStopPath).ToString("N") + "m \n";
	temp += "Висота     : " + elevation.ToString("N") + " m\n";
	temp += "Гравітація : " + gravity.ToString("N") + " м/с²\n";
	temp += "Ще підніме : " + (lessForse/gravity).ToString("N") + " Kg\n";
	temp += "Приск. під.: " + lessAcceleration.ToString("N") + " м/с²\n";
	temp += "Дистанція  : " + lessDistantion.ToString("N") + " m\n";
	temp += "Зал Часу   : " + lessTime.ToString("N") + " c\n";
	
	if (gravityExist){
		if (elevation > 0.5 * SaveDistans){
			if (gyroPitch > 45 || gyroPitch < -45) AdvGyro.SetAllGyros(AdvancedGyros, true, gyroPitch * 0.5f, null, 0);
			else if (gyroPitch > 10 || gyroPitch < -10) AdvGyro.SetAllGyros(AdvancedGyros, true, gyroPitch * 0.1f, null, 0);
			else if (gyroPitch > 0.1 || gyroPitch < -0.1) AdvGyro.SetAllGyros(AdvancedGyros, true, gyroPitch * 0.01f, null, 0);
			else AdvGyro.SetAllGyros(AdvancedGyros, true, 0, null, 0);
			
			if (gyroYaw >= 45 || gyroYaw < -45) AdvGyro.SetAllGyros(AdvancedGyros, true, null, gyroYaw * 0.5f, 0);
			else if (gyroYaw > 10 || gyroYaw < -10) AdvGyro.SetAllGyros(AdvancedGyros, true, null, gyroYaw * 0.1f, 0);
			else if (gyroYaw > 0.1 || gyroYaw < -0.1) AdvGyro.SetAllGyros(AdvancedGyros, true, null, gyroYaw * 0.01f, 0);
			else AdvGyro.SetAllGyros(AdvancedGyros, true, null, 0, 0);
			
			if (maxStopPath < 6000){
				if (lessDistantion <= elevation - SaveDistans) {
					block.DampenersOverride = false;
				} else { 
					block.DampenersOverride = true;
				}
			} else {
				if (maxStopPath < elevation - SaveDistans & elevation != 0) {
					block.DampenersOverride = false;
					AdvGyro.FreeAllGyros(AdvancedGyros);
				} else { block.DampenersOverride = true;}
			}
		} else {
			AdvGyro.FreeAllGyros(AdvancedGyros);
			if (shipSpeed >= 0.1 ) block.DampenersOverride = !block.DampenersOverride;
			else block.DampenersOverride = false;
			done = true;
		}
	} else {		
		AdvGyro.FreeAllGyros(AdvancedGyros);
	}
	if (infoToLcd) {
		if (done) {
			WriteToPanel("\nDone!", 6);
		}
		else {
			WriteToPanel(temp);
		}
	}
	if (infoEcho) Echo(temp);


}

/* a = F / m
 s = v*v / 2a

Large Grid:
Large Hydrogen Thruster consumes 6,426.7 H/s
Small hydrogen Thruster consumes 1,092.5 H/s

Small Grid:
Large Hydrogen Thruster consumes 514.1 H/s
Small Hydrogen Thruster consumes 109.2 H/s

Large Grid Tank holds 250,000 H
Small Grid Tank holds 40,000 H
*/

public void WriteToPanel(string Message, float font = 1.2f, int panelNumber = 0){ 
	IMyTextPanel thisPanel = null; 
	var Panels = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(Panels);  
	if (Panels.Count > 0) { 
		thisPanel = Panels[panelNumber] as IMyTextPanel; 
		thisPanel.SetValueFloat("FontSize", font); 
		thisPanel.SetValue("FontColor", new Color(204,255,0));  
		thisPanel.SetValue("BackgroundColor", new Color(0,0,0)); 
		thisPanel.ShowPublicTextOnScreen(); 
		thisPanel.WritePublicTitle("<3 " + thisPanel.CustomName ); 
		thisPanel.WritePublicText(Message); 
	} 
	else {throw new Exception("Немає текстової панелі з індексом " + panelNumber);} 
}

public double moduleFromVector(Vector3D vector){
	return System.Math.Sqrt((vector.X*vector.X) + (vector.Y*vector.Y) + (vector.Z*vector.Z)); 
}

public double getMaxForce (){
	double force = 0;
	IMyThrust thisThruster = null;
	List<IMyTerminalBlock> groupThrusters = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlockGroupWithName(GoTrustGroup).GetBlocks(groupThrusters);
	if (groupThrusters.Count > 0){
		for(int j = 0; j < groupThrusters.Count; j++){
			thisThruster = groupThrusters[j] as IMyThrust;
			if (thisThruster.IsWorking) force += (double)thisThruster.MaxThrust;
		}
	} else {throw new Exception("Немає грапи двигунів з ім'ям: " + GoTrustGroup);}
	return force;
}


public double hydrogenLess(){
	var gasTanks = new List<IMyGasTank>();
	GridTerminalSystem.GetBlocksOfType<IMyGasTank>(gasTanks);

	double totalOxygenInTanks = 0;

	foreach (var tank in gasTanks) {
		if(tank.CustomName.Contains("Hydrogen") && !tank.IsWorking) allTanksWorking = false;
		if(tank.CustomName.Contains("Hydrogen") && tank.IsWorking){
			totalOxygenInTanks = totalOxygenInTanks + (tank.Capacity * tank.FilledRatio);
		}
	}
	return totalOxygenInTanks;
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

//------------------------------------------------------------------------------------------     
//-----------------------------AdvGyro class -----------------------------------------------
class AdvGyro          
{          
	public IMyTerminalBlock Gyro          
	{          
		get;          
		private set;          
	}          
          
	public float Pitch          
	{          
		get{ return Gyro.GetValueFloat(strPitch) * intPitch; }          
		set{ Gyro.SetValueFloat(strPitch, value * intPitch); }          
	}          
          
	public float Yaw          
	{          
		get{ return Gyro.GetValueFloat(strYaw) * intYaw; }          
		set{ Gyro.SetValueFloat(strYaw, value * intYaw); }          
	}          
          
	public float Roll          
	{          
		get{ return Gyro.GetValueFloat(strRoll) * intRoll; }          
		set{ Gyro.SetValueFloat(strRoll, value * intRoll); }          
	}          
          
	public float Power          
	{          
		get{ return Gyro.GetValueFloat("Power"); }          
		set{ Gyro.SetValueFloat("Power", value); }          
	}          
          
	public bool Override          
	{          
		get{ return Gyro.GetValue<bool>("Override"); }          
		set{ Gyro.SetValue<bool>("Override", value); }          
	}          
          
	public bool Enabled          
	{          
		get{ return Gyro.GetValue<bool>("OnOff"); }          
		set{ Gyro.SetValue<bool>("OnOff", value); }          
	}          
          
	private string strPitch;          
	private int intPitch;          
	private string strYaw;          
	private int intYaw;          
	private string strRoll;          
	private int intRoll;          
          
	public AdvGyro(IMyTerminalBlock MyGyro, IMyTerminalBlock ForwardCockpit)          
	{          
		Gyro = MyGyro;          
		Orientate(ForwardCockpit);          
	}          
          
	public void Free()          
	{          
		this.Pitch = 0;          
		this.Yaw = 0;          
		this.Roll = 0;          
		this.Override = false;          
	}          
          
	public static List<AdvGyro> GetAllGyros(IMyGridTerminalSystem Term, IMyTerminalBlock ForwardCockpit, bool OnlyOwnGrid = true)          
	{          
		List<IMyTerminalBlock> AllGyros = new List<IMyTerminalBlock>();          
		Term.GetBlocksOfType<IMyGyro>(AllGyros);          
		if (OnlyOwnGrid)          
			AllGyros.RemoveAll(x => x.CubeGrid != ForwardCockpit.CubeGrid);          
          
		List<AdvGyro> AdvGyros = new List<AdvGyro>();          
		foreach (IMyTerminalBlock _Gyro in AllGyros)          
		{          
			AdvGyro NewAdvGyro = new AdvGyro(_Gyro, ForwardCockpit);          
			AdvGyros.Add(NewAdvGyro);          
		}          
		return AdvGyros;          
	}          
          
	public static void SetAllGyros(List<AdvGyro> AllGyros, bool AutoOverride = true, float? NewPitch = null, float? NewYaw = null, float? NewRoll = null)          
	{          
		foreach(AdvGyro _Gyro in AllGyros)          
		{          
			if (NewPitch.HasValue)          
				_Gyro.Pitch = (float)NewPitch;          
          
			if (NewYaw.HasValue)          
				_Gyro.Yaw = (float)NewYaw;          
          
			if (NewRoll.HasValue)          
				_Gyro.Roll = (float)NewRoll;          
          
			if (AutoOverride)          
			{          
				if(_Gyro.Override == false)          
					_Gyro.Override = true;          
			}          
		}          
	}          
          
	public static void FreeAllGyros(List<AdvGyro> AllGyros)          
	{          
		foreach(AdvGyro _Gyro in AllGyros)          
		{          
			_Gyro.Free();          
		}          
	}          
          
	private void Orientate(IMyTerminalBlock ReferencePoint)          
	{ // Big thanks to Skleroz for this awesome stuff.          
		Vector3 V3For = Base6Directions.GetVector(ReferencePoint.Orientation.TransformDirection(Base6Directions.Direction.Forward));          
		Vector3 V3Up = Base6Directions.GetVector(ReferencePoint.Orientation.TransformDirection(Base6Directions.Direction.Up));          
		V3For.Normalize();          
		V3Up.Normalize();          
		Base6Directions.Direction B6DFor = Base6Directions.GetDirection(V3For);          
		Base6Directions.Direction B6DTop = Base6Directions.GetDirection(V3Up);          
		Base6Directions.Direction B6DLeft = Base6Directions.GetLeft(B6DTop, B6DFor);          
		Base6Directions.Direction GyroUp = Gyro.Orientation.TransformDirectionInverse(B6DTop);          
		Base6Directions.Direction GyroForward = Gyro.Orientation.TransformDirectionInverse(B6DFor);          
		Base6Directions.Direction GyroLeft = Gyro.Orientation.TransformDirectionInverse(B6DLeft);          
		switch (GyroUp)          
		{          
			case Base6Directions.Direction.Up:          
				strYaw = "Yaw";          
				intYaw = 1;          
				break;          
			case Base6Directions.Direction.Down:          
				strYaw = "Yaw";          
				intYaw = -1;          
				break;          
			case Base6Directions.Direction.Left:          
				strYaw = "Pitch";          
				intYaw = 1;          
				break;          
			case Base6Directions.Direction.Right:          
				strYaw = "Pitch";          
				intYaw = -1;          
				break;          
			case Base6Directions.Direction.Backward:          
				strYaw = "Roll";          
				intYaw = 1;          
				break;          
			case Base6Directions.Direction.Forward:          
				strYaw = "Roll";          
				intYaw = -1;          
				break;          
		}          
		switch (GyroLeft)          
		{          
			case Base6Directions.Direction.Up:          
				strPitch = "Yaw";          
				intPitch = 1;          
				break;          
			case Base6Directions.Direction.Down:          
				strPitch = "Yaw";          
				intPitch = -1;          
				break;          
			case Base6Directions.Direction.Left:          
				strPitch = "Pitch";          
				intPitch = 1;          
				break;          
			case Base6Directions.Direction.Right:          
				strPitch = "Pitch";          
				intPitch = -1;          
				break;          
			case Base6Directions.Direction.Backward:          
				strPitch = "Roll";          
				intPitch = 1;          
				break;          
			case Base6Directions.Direction.Forward:          
				strPitch = "Roll";          
				intPitch = -1;          
				break;          
		}          
		switch (GyroForward)          
		{          
			case Base6Directions.Direction.Up:          
				strRoll = "Yaw";          
				intRoll = -1;          
				break;          
			case Base6Directions.Direction.Down:          
				strRoll = "Yaw";          
				intRoll = 1;          
				break;          
			case Base6Directions.Direction.Left:          
				strRoll = "Pitch";          
				intRoll = -1;          
				break;          
			case Base6Directions.Direction.Right:          
				strRoll = "Pitch";          
				intRoll = 1;          
				break;          
			case Base6Directions.Direction.Backward:          
				strRoll = "Roll";          
				intRoll = -1;          
				break;          
			case Base6Directions.Direction.Forward:          
				strRoll = "Roll";          
				intRoll = 1;          
				break;          
		}          
	}          
}