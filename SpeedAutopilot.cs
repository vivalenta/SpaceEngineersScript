//Сам собі автопілот
public Program()
{
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}
private const double MaxSpeed = 999; // м/с²
private const double StopDistance = 5; // M
private float _sensitivity = 1.0f;


private Vector3D _target = new Vector3D(0, 0, 0);
private List<IMyThrust>[] _thrustersAll = new List<IMyThrust>[6];
private List<AdvGyro> _advancedGyros = new List<AdvGyro>();
private List<IMyTerminalBlock> _thrusters = new List<IMyTerminalBlock>();
private List<IMyTerminalBlock> _controls = new List<IMyTerminalBlock>();
private bool tagertLock = false;
private bool needFlip = false;
private bool doneFlip = false;
public void Main(string argument)
{
    double stopDistance = StopDistance;
    string temp = null;
    float gyroPitch = 0;
    float gyroYaw = 0;
    
    IMyShipController block = null;
    GridTerminalSystem.GetBlocksOfType<IMyShipController>(_controls);
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(_thrusters);
	
    if (_controls.Count > 0) block = _controls[0] as IMyShipController; else Echo("Немає чим управляти"); // get IMyShipController[0]
	try
    {
        string[] rawCoords = Me.CustomData.Split(':');
        _target = new Vector3D(Convert.ToDouble(rawCoords[2]), Convert.ToDouble(rawCoords[3]), Convert.ToDouble(rawCoords[4]));
    }
    catch (Exception)
    {
        _target = block.GetPosition();
        Echo("Немає GPS");
    }

    _thrustersAll = OrganizeThrusters(_thrusters, block);
    double[] maxForce = GetMaxForce(_thrustersAll);
    _advancedGyros = AdvGyro.GetAllGyros(GridTerminalSystem, block, true);
    

    double mass = block.CalculateShipMass().TotalMass;
    if (Me.Mass > 140)
    {
        _sensitivity = (float)(mass / (10000000 *  _advancedGyros.Count));
        stopDistance = StopDistance * 10;
    }
    else
    {
        _sensitivity = (float)(mass / (10000 *  _advancedGyros.Count));
    }
    Vector3D currentPosition = block.GetPosition();
    double distance = Vector3D.Distance(_target, currentPosition);
    double currentShipSpeed = block.GetShipSpeed();
    double startAcceleration = 0;
    double stopAcceleration = 0;
	if (needFlip) 
	{
		startAcceleration = maxForce[4] / mass;
		stopAcceleration = maxForce[5] / mass;
	}
	else
	{
		startAcceleration = maxForce[5] / mass;
		stopAcceleration = maxForce[4] / mass;
	}

    double currentStartPath = ((currentShipSpeed - MaxSpeed) * (currentShipSpeed - MaxSpeed) / (2 * startAcceleration)) * 1.0001;
    double currentStopPath = (currentShipSpeed * currentShipSpeed / (2 * stopAcceleration)) * 1.0001;
    double lessTime;
    if (currentStopPath < distance - StopDistance) lessTime = ((currentStartPath / (MaxSpeed - currentShipSpeed)) + (MaxSpeed / stopAcceleration) + ((distance - currentStopPath - currentStartPath) / MaxSpeed));
    else lessTime = currentShipSpeed / stopAcceleration;
    //temp += "Прискорювачі:" + maxForce[0].ToString("N") + " Н\n"; // Право
    //temp += "Прискорювачі:" + maxForce[1].ToString("N") + " Н\n"; // Ліво
    //temp += "Прискорювачі:" + maxForce[2].ToString("N") + " Н\n"; // Вверх
    //temp += "Прискорювачі:" + maxForce[3].ToString("N") + " Н\n"; // Вниз
    //temp += "Прискорювачі:" + maxForce[4].ToString("N") + " Н\n"; // Назад
    temp += "Двиг:" + FormatLargeNumber(maxForce[5]) + "/" + FormatLargeNumber(maxForce[4]) + "Н\n"; // Вперед / Назад
    temp += "Прис: " + startAcceleration.ToString("N") + "/" + stopAcceleration.ToString("N") + " м/с²\n";
    temp += "Зупи:" + FormatLargeNumber(currentStopPath) + "m  " + FormatLargeTime(lessTime) + "\n";
    temp += "Дист: " + FormatLargeNumber(distance) + "m \n";
	temp += "Швид: " + currentShipSpeed + " м/с²\n";
    if (needFlip) _target = currentPosition + currentPosition - _target;
    GetDirectionTo(_target, block, ref gyroPitch, ref gyroYaw);
    
	if (currentShipSpeed > MaxSpeed & distance > currentStopPath + 10 * MaxSpeed & maxForce[4] * 1.2 < maxForce[5]) needFlip = true; //необхідність Фліпу
	if (tagertLock & needFlip) doneFlip = true; else doneFlip = false;
	if (distance > stopDistance) // Ми ще не прибули ???
    {
        SetDirectionTo(gyroPitch, gyroYaw, ref tagertLock);
        if ((distance > currentStopPath & tagertLock )||(needFlip & !doneFlip)) // Тормозимо ?
        {
			if (currentShipSpeed > MaxSpeed*1.1) // Більша швидкість
            {
                block.DampenersOverride = true;
				SetMaxForce(_thrustersAll[4], 0);
                SetMaxForce(_thrustersAll[5], 0);
                temp += "Стан: Більша швидкість";
            }
            else if (currentShipSpeed < MaxSpeed) // Набір швидкості
            {
                block.DampenersOverride = true;
                if (doneFlip)
				{
					SetMaxForce(_thrustersAll[4], 255);
					SetMaxForce(_thrustersAll[5], 0);
					temp += "Стан: Набір швидкості Ф+";
                }
				else {
					SetMaxForce(_thrustersAll[4], 0);
					SetMaxForce(_thrustersAll[5], 255);
					temp += "Стан: Набір швидкості Ф-";
				}
            }
            else // Крейсерська швидкість
            {
                block.DampenersOverride = false;
				SetMaxForce(_thrustersAll[4], 0);
                SetMaxForce(_thrustersAll[5], 0);
				temp += "Стан: Крейсерська";
            }
        }
        else // Зупинка
        {
			SetMaxForce(_thrustersAll[4], 0);
            SetMaxForce(_thrustersAll[5], 0);
            block.DampenersOverride = true;
			temp += "Стан: Зупинка";
        }
    }
    else // Прибули
    {
        block.DampenersOverride = true;
		SetMaxForce(_thrustersAll[4], 0);
        SetMaxForce(_thrustersAll[5], 0);
        AdvGyro.FreeAllGyros(_advancedGyros);
		doneFlip = false;
        needFlip = false;
		temp += "Стан: Прибули";
    }

    Echo(temp);
}


public double ModuleFromVector(Vector3D vector)
{
    return Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
}


public double[] GetMaxForce(List<IMyThrust>[] thrustersAll)
{
    var force = new double[6];
    for (int dire = 0; dire < 6; ++dire)
    {
        List<IMyThrust> thrusters = thrustersAll[dire];
        foreach (IMyThrust thisThruster in thrusters)
        {
            if (thisThruster.IsWorking) force[dire] = force[dire] + thisThruster.MaxThrust;
        }
    }
    return force;
}

public void SetMaxForce(List<IMyThrust> thrustersAll, int thrust)
{
    foreach (IMyThrust thisThruster in thrustersAll)
    {
        if (thrust == 0)
        {
            thisThruster.SetValueFloat("Override", 0.0f);
        }
        else if (thrust == 1)
        {
            thisThruster.SetValueFloat("Override", 1.0f);
        }
        else if (thrust == 255)
        {
            thisThruster.SetValueFloat("Override", thisThruster.MaxThrust);
        }
    }
}

public List<IMyThrust>[] OrganizeThrusters(List<IMyTerminalBlock> thrusters, IMyTerminalBlock reference)
{
    Matrix refm;
    reference.Orientation.GetMatrix(out refm);

    var org = new List<IMyThrust>[6];
    for (int dir = 0; dir < 6; ++dir) org[dir] = new List<IMyThrust>();
    foreach (IMyTerminalBlock t in thrusters)
    {
        Matrix bmat;
        t.Orientation.GetMatrix(out bmat);
        bmat = bmat * Matrix.Transpose(refm);
        int dir = (int)bmat.Forward.Dot(new Vector3(1, 2, 3));
        dir = (2 * Math.Abs(dir) - 2) + (Math.Sign(dir) + 1) / 2;
        org[dir].Add(t as IMyThrust);
    }
    return org;
}


public static string FormatLargeNumber(double number)
{
    string ordinals = " kMGTPEZY";
    double compressed = number;
    var ordinal = 0;
    while (compressed >= 1000)
    {
        compressed /= 1000;
        ordinal++;
    }
    string res = Math.Round(compressed, 2, MidpointRounding.AwayFromZero).ToString();
    if (ordinal > 0) res += " " + ordinals[ordinal];
    return res;
}

public static string FormatLargeTime(double time)
{
    if (time > 59)
    {
        return Math.Ceiling(time / 60).ToString() + " min " + (time % 60).ToString("N") + " sec";
    }
    else { return time.ToString("N") + " sec"; }
}

void SetDirectionTo(float pitch, float yaw, ref bool tLocked)
{
	if (pitch > 90 || pitch < -90) AdvGyro.SetAllGyros(_advancedGyros, true, pitch * _sensitivity, null, 0);
    else if (pitch > 45 || pitch < -45) AdvGyro.SetAllGyros(_advancedGyros, true, pitch * 0.5f * _sensitivity, null, 0);
    else if (pitch > 10 || pitch < -10) AdvGyro.SetAllGyros(_advancedGyros, true, pitch * 0.3f * _sensitivity, null, 0);
    else if (pitch > 0.1 || pitch < -0.1)
    {
        AdvGyro.SetAllGyros(_advancedGyros, true, pitch * 0.2f * _sensitivity, null, 0);
        tLocked = false;
    }
    else
    {
        AdvGyro.SetAllGyros(_advancedGyros, true, 0, null, 0);
        tLocked = true;
    }
	
	if (yaw > 90 || yaw < -90) AdvGyro.SetAllGyros(_advancedGyros, true, null, yaw * _sensitivity, 0);
    else if (yaw > 45 || yaw < -45) AdvGyro.SetAllGyros(_advancedGyros, true, null, yaw * 0.5f * _sensitivity, 0);
    else if (yaw > 10 || yaw < -10) AdvGyro.SetAllGyros(_advancedGyros, true, null, yaw * 0.3f * _sensitivity, 0);
    else if (yaw > 0.1 || yaw < -0.1)
    {
        AdvGyro.SetAllGyros(_advancedGyros, true, null, yaw * 0.2f * _sensitivity, 0);
        tLocked = false;
    }
    else
    {
        AdvGyro.SetAllGyros(_advancedGyros, true, null, 0, 0);
        tLocked = true;
    }
}
void GetDirectionTo(Vector3D tv, IMyTerminalBlock origin, ref float pitch, ref float yaw)
{
    Vector3D ov = origin.GetPosition();//Get positions of reference blocks.
    Vector3D fv = origin.WorldMatrix.Forward + origin.GetPosition();
    Vector3D uv = origin.WorldMatrix.Up + origin.GetPosition();
    Vector3D rv = origin.WorldMatrix.Right + origin.GetPosition();

    float tvov = (float)((ov - tv).Length());//Get magnitudes of vectors.

    float tvfv = (float)((fv - tv).Length());
    float tvuv = (float)((uv - tv).Length());
    float tvrv = (float)((rv - tv).Length());
    float ovuv = (float)((uv - ov).Length());
    float ovrv = (float)((rv - ov).Length());

    float thetaP = (float)(Math.Acos((tvuv * tvuv - ovuv * ovuv - tvov * tvov) / (-2 * ovuv * tvov)));
    //Use law of cosines to determine angles.
    float thetaY = (float)(Math.Acos((tvrv * tvrv - ovrv * ovrv - tvov * tvov) / (-2 * ovrv * tvov)));

    float rPitch = (float)(90 - (thetaP * 180 / Math.PI));//Convert from radians to degrees.
    float rYaw = (float)(90 - (thetaY * 180 / Math.PI));

    if (tvov < tvfv) rPitch = 180 - rPitch;//Normalize angles to -180 to 180 degrees.
    if (rPitch > 180) rPitch = -1 * (360 - rPitch);

    if (tvov < tvfv) rYaw = 180 - rYaw;
    if (rYaw > 180) rYaw = -1 * (360 - rYaw);

    pitch = rPitch;//Set Pitch and Yaw outputs.
    yaw = rYaw;
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
        get { return Gyro.GetValueFloat(_strPitch) * _intPitch; }
        set { Gyro.SetValueFloat(_strPitch, value * _intPitch); }
    }

    public float Yaw
    {
        get { return Gyro.GetValueFloat(_strYaw) * _intYaw; }
        set { Gyro.SetValueFloat(_strYaw, value * _intYaw); }
    }

    public float Roll
    {
        get { return Gyro.GetValueFloat(_strRoll) * _intRoll; }
        set { Gyro.SetValueFloat(_strRoll, value * _intRoll); }
    }

    public float Power
    {
        get { return Gyro.GetValueFloat("Power"); }
        set { Gyro.SetValueFloat("Power", value); }
    }

    public bool Override
    {
        get { return Gyro.GetValue<bool>("Override"); }
        set { Gyro.SetValue("Override", value); }
    }

    public bool Enabled
    {
        get { return Gyro.GetValue<bool>("OnOff"); }
        set { Gyro.SetValue("OnOff", value); }
    }

    private string _strPitch;
    private int _intPitch;
    private string _strYaw;
    private int _intYaw;
    private string _strRoll;
    private int _intRoll;

    public AdvGyro(IMyTerminalBlock myGyro, IMyTerminalBlock forwardCockpit)
    {
        Gyro = myGyro;
        Orientate(forwardCockpit);
    }

    public void Free()
    {
        Pitch = 0;
        Yaw = 0;
        Roll = 0;
        Override = false;
    }

    public static List<AdvGyro> GetAllGyros(IMyGridTerminalSystem term, IMyTerminalBlock forwardCockpit, bool onlyOwnGrid = true)
    {
        List<IMyTerminalBlock> allGyros = new List<IMyTerminalBlock>();
        term.GetBlocksOfType<IMyGyro>(allGyros);
        if (onlyOwnGrid)
            allGyros.RemoveAll(x => x.CubeGrid != forwardCockpit.CubeGrid);

        List<AdvGyro> advGyros = new List<AdvGyro>();
        foreach (IMyTerminalBlock gyro in allGyros)
        {
            AdvGyro newAdvGyro = new AdvGyro(gyro, forwardCockpit);
            advGyros.Add(newAdvGyro);
        }
        return advGyros;
    }

    public static void SetAllGyros(List<AdvGyro> allGyros, bool autoOverride = true, float? newPitch = null, float? newYaw = null, float? newRoll = null)
    {
        foreach (AdvGyro gyro in allGyros)
        {
            if (newPitch.HasValue)
                gyro.Pitch = (float)newPitch;

            if (newYaw.HasValue)
                gyro.Yaw = (float)newYaw;

            if (newRoll.HasValue)
                gyro.Roll = (float)newRoll;

            if (autoOverride)
            {
                if (gyro.Override == false)
                    gyro.Override = true;
            }
        }
    }

    public static void FreeAllGyros(List<AdvGyro> allGyros)
    {
        foreach (AdvGyro gyro in allGyros)
        {
            gyro.Free();
        }
    }

    private void Orientate(IMyTerminalBlock referencePoint)
    { // Big thanks to Skleroz for this awesome stuff.
        Vector3 v3For = Base6Directions.GetVector(referencePoint.Orientation.TransformDirection(Base6Directions.Direction.Forward));
        Vector3 v3Up = Base6Directions.GetVector(referencePoint.Orientation.TransformDirection(Base6Directions.Direction.Up));
        v3For.Normalize();
        v3Up.Normalize();
        Base6Directions.Direction b6DFor = Base6Directions.GetDirection(v3For);
        Base6Directions.Direction b6DTop = Base6Directions.GetDirection(v3Up);
        Base6Directions.Direction b6DLeft = Base6Directions.GetLeft(b6DTop, b6DFor);
        Base6Directions.Direction gyroUp = Gyro.Orientation.TransformDirectionInverse(b6DTop);
        Base6Directions.Direction gyroForward = Gyro.Orientation.TransformDirectionInverse(b6DFor);
        Base6Directions.Direction gyroLeft = Gyro.Orientation.TransformDirectionInverse(b6DLeft);
        switch (gyroUp)
        {
            case Base6Directions.Direction.Up:
                _strYaw = "Yaw";
                _intYaw = 1;
                break;
            case Base6Directions.Direction.Down:
                _strYaw = "Yaw";
                _intYaw = -1;
                break;
            case Base6Directions.Direction.Left:
                _strYaw = "Pitch";
                _intYaw = 1;
                break;
            case Base6Directions.Direction.Right:
                _strYaw = "Pitch";
                _intYaw = -1;
                break;
            case Base6Directions.Direction.Backward:
                _strYaw = "Roll";
                _intYaw = 1;
                break;
            case Base6Directions.Direction.Forward:
                _strYaw = "Roll";
                _intYaw = -1;
                break;
        }
        switch (gyroLeft)
        {
            case Base6Directions.Direction.Up:
                _strPitch = "Yaw";
                _intPitch = 1;
                break;
            case Base6Directions.Direction.Down:
                _strPitch = "Yaw";
                _intPitch = -1;
                break;
            case Base6Directions.Direction.Left:
                _strPitch = "Pitch";
                _intPitch = 1;
                break;
            case Base6Directions.Direction.Right:
                _strPitch = "Pitch";
                _intPitch = -1;
                break;
            case Base6Directions.Direction.Backward:
                _strPitch = "Roll";
                _intPitch = 1;
                break;
            case Base6Directions.Direction.Forward:
                _strPitch = "Roll";
                _intPitch = -1;
                break;
        }
        switch (gyroForward)
        {
            case Base6Directions.Direction.Up:
                _strRoll = "Yaw";
                _intRoll = -1;
                break;
            case Base6Directions.Direction.Down:
                _strRoll = "Yaw";
                _intRoll = 1;
                break;
            case Base6Directions.Direction.Left:
                _strRoll = "Pitch";
                _intRoll = -1;
                break;
            case Base6Directions.Direction.Right:
                _strRoll = "Pitch";
                _intRoll = 1;
                break;
            case Base6Directions.Direction.Backward:
                _strRoll = "Roll";
                _intRoll = -1;
                break;
            case Base6Directions.Direction.Forward:
                _strRoll = "Roll";
                _intRoll = 1;
                break;
        }
    }
}