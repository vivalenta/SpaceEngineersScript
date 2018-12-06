/** 
* Бета версія, можливі баги! 
* Автопілот що не розбивае все у кінцевій точці 
* Вже є такі функції: 
* Завчасно скидає швидкість 
* Виводить інфо на текстову панель та маяк 
* Маяк працює тільки після прибуття до цілі ( не помітність підчас пересування) 
* Бере координати з текстової панелі 
* Економія енергії. ( контролює свою швидкість ) 
* Вказує час до прибуття ( +/- 5 секунд) 
* Перехват помилок 
* Самозбереження, дрон повинен тікати якщо його хочуть розрізати 
* Сенсор, пошук обхідного маршруту, поки криво. 
* Зараз працює у тестовому режимі. Якщо бачить щось - намагаеться облетіти під кутом 25 градусів. Польот по тунелям або дірам меншим за 75 метрів буде зроблено пізніше 
* Кілікість сенсорів обмежена здоровим глуздом (немає потреби ставити ближче ніх 50 м. рекомендовано 1 на 100 метрів) 
* Будуть використовуватись всі наявні гіроскопи. 
* Заплановано додати: 
* Ідея виявляти оріентацію гіроскопу. можливо автоматично 
* Польот по тунелям або дірам меншим за 75 метрів буде зроблено пізніше 
* Польот по радіусу 
* Патрулювання 
* Ідея бойового дрону 
 
* Виявлено повну не сумісність з модом "Швидкість Світла" (сила гіроскопу х1000) 
* протестовано на малих і великих кораблях 
* Про баги повідомляйте у коментарях або на пошту VivaLenta@ukr.net 
* Автор vivalenta 
* ===================================== EN ====================================================== 
* Beta version, possible bugs! 
* Autopilot not crash at all endpoint 
* Already have the following functions: 
* Throws advance rate 
* Displays more info on text panel and beacon 
* Beacon is on only after the arrival to the target (not visibility during movement) 
* Takes the coordinates of the text panel 
* Save energy. (Controlling his speed) 
* Indicates time to arrival ( +/- 5 seconds) 
* Interception errors 
* Self-preservation, drone should run if it wants to grind 
* Sensor search roundabout route until crooked. 
* Currently working in test mode. If you see something - tries to fly at an angle of 25 degrees. Flight through tunnels or holes less than 75 meters will be made later 
* Count sensors is limited by common sense (no need to put closer 50 m. Recommended 1 100 meters) 
* Now will use all available gyroscopes. 
* Planned to add: 
* Idea gyroscopes to detect orientation. may automatically 
* Flight in tunnels or holes less than 75 meters will be made later 
* Flight radial 
* Patrol 
* The idea of fighting Dron 
* 
* Tested on small & large ships 
* Please send bugs report in comment or e-mail vivalenta@ukr.net 
* By vivalenta 
 
* v 0.5.1 
*/ 
 
#region Config 
string PanelName = "Текстова панель";	// Частина або повна назва тектової панелі	[EN] Part or full name text panel 
string GoTrustGroup = "Trust";			// Ім'я группи пришвидшувачів вперед		[EN] Name of group thruster forward 
double distance = 300;					// Радіус гальмування до швидкості			[EN] Radius braking to rate 
double MaxSpeed = 90;					// Максимальна (Крейсерська) швидкість		[EN] Maximum (cruising) speed 
double distance_S = 30;					// Радіус Завершення						[EN] Radius ResultDrive 
double MaxSpeed_D = 10;					// Максимальна швидкість у радіусі 			[EN] Maximum speed within a radius of 
float MaxAcceleration = 12000f;			// Максимальна тяга пришвидшувача 			[EN] Max Acceleration Thruster, on end "f" 
float Speed_spin = 0.1f;				// Швидкість обертання гіроскопу. 			[EN] The speed of rotation of the gyroscope 
string escapeCoord = "200,200,200";		// Координати для втечі						[EN] Coordinates to escape 
bool Debug = true;	// Виводити на екран повну інформацію польоту					[EN] Debug on/off 
int accuracy = 2;						// Точність розрахунків						[EN] Accuracy of 
// ===================================== Мовні налаштування ====================================== 
// ===================================== Lang settings =========================================== 
string BeaconName = "AiShip";			// Частина або повна назва маяка			[EN] Part or full name Beacon 
string langTagert = "Ціль";				//											[EN] Text Target 
string langCoord = "Зараз";				//											[EN] Text Now Chords 
string langTime = "Зараз часу";			//											[EN] Text Now Time 
string langSpeed = "Швидкість";			//											[EN] Text Speed 
string langDiffToArrival = "Дистанція";		//										[EN] Text Distance to target 
string langTimeToArrival = "До прибуття";	//										[EN] Text time to Arrival 
string langAngle = "Відхилення";			//										[EN] Text Angle Target 
string langFound = " об'екта";				//									[EN] Text to found 
string langDone = "Чекаю на подальші інструкції";	// Текст успіху					[EN] Text Done 
#endregion 
// ======================================== Робоча Зона =========================================== 
// ======================================== work body ============================================= 
void Main()	{ 
	double pitch_angle, yaw_angle, OldTime_fff = 0, OldTime_ss = 0, Read_X = 0, Read_Y = 0, Read_Z = 0, Tagert_x = 0, Tagert_y = 0, Tagert_z = 0, TotalDistance = 0, TgDiff =0, CenterX = 0, CenterY = 0, CenterZ = 0, LeftX = 0, LeftY = 0, LeftZ = 0, TopX = 0, TopY = 0, TopZ = 0, speed = 0, totalDistance = 0, OldTime = 0; 
	bool C_found = false, L_found = false, T_found = false; 
	string NowCoord = "", TimeToArrival="", ResultDrive = "", noCoord = ""; 
	IMyTextPanel thisPanel = null; 
	IMyBeacon thisBeacon = null; 
 
	List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>(); 
	blocks.Clear(); 
 
	double NewTimeK = GetNowTime(); 
 
	GridTerminalSystem.SearchBlocksOfName("GPS", blocks); 
	for (int i = 0; i < blocks.Count; i++) { 
		IMyTerminalBlock block = blocks[i]; 
		if (block.CustomName.Contains("GPS Center")) { 
				CenterX = block.GetPosition().GetDim(0); 
				CenterY = block.GetPosition().GetDim(1); 
				CenterZ = block.GetPosition().GetDim(2); 
				C_found = true; 
			} 
		else if (block.CustomName.Contains("GPS Left")) { 
				LeftX = block.GetPosition().GetDim(0); 
				LeftY = block.GetPosition().GetDim(1); 
				LeftZ = block.GetPosition().GetDim(2); 
				L_found = true; 
			} 
		else if (block.CustomName.Contains("GPS Top")) { 
				TopX = block.GetPosition().GetDim(0); 
				TopY = block.GetPosition().GetDim(1); 
				TopZ = block.GetPosition().GetDim(2); 
				T_found = true; 
			} 
	} 
 
	NowCoord = Math.Round(CenterX, accuracy).ToString() + "," + Math.Round(CenterY, accuracy).ToString() + "," + Math.Round(CenterZ, accuracy).ToString(); 
 
	for(int i = 0; i < GridTerminalSystem.Blocks.Count; i++) { 
	if(GridTerminalSystem.Blocks[i].CustomName.Contains(PanelName)) { 
		thisPanel = GridTerminalSystem.Blocks[i] as IMyTextPanel; 
		if(thisPanel != null) 
			break; 
		} 
	} 
	if(thisPanel == null) throw new Exception("Немає панелі з ім'ям '" + PanelName + "'"); 
	 
	thisPanel.SetValueFloat("FontSize", 1.5f); 
	thisPanel.SetValue("FontColor", Color.Green); 
	thisPanel.SetValue("BackgroundColor", Color.Black); 
	thisPanel.ShowPublicTextOnScreen(); 
	thisPanel.WritePublicTitle("Координати для дрона", false); 
 
	string Tagq = thisPanel.GetPublicText(); 
	if (Tagq.Length > 2) { 
		string[] ReadedInfo = Tagq.Split('|',',',':','\n'); 
		Tagert_x = Convert.ToDouble(ReadedInfo[1]); 
		Tagert_y = Convert.ToDouble(ReadedInfo[2]); 
		Tagert_z = Convert.ToDouble(ReadedInfo[3]); 
		noCoord = Convert.ToString(Tagert_x) + "," + Convert.ToString(Tagert_y) + "," + Convert.ToString(Tagert_z);	//Потрібно щоб позбавитись пробілів 
		Read_X = Math.Round(Convert.ToDouble(ReadedInfo[5]), accuracy); 
		Read_Y = Math.Round(Convert.ToDouble(ReadedInfo[6]), accuracy); 
		Read_Z = Math.Round(Convert.ToDouble(ReadedInfo[7]), accuracy); 
		OldTime = Convert.ToDouble(ReadedInfo[9]); 
	} 
	else { 
		noCoord = NowCoord; 
	} 
	double LeVecX, LeVecY, LeVecZ, ToVecX, ToVecY, ToVecZ, ForwardX, ForwardY, ForwardZ, ForwDist, TopDiff, LeftDiff; 
	if (C_found && L_found && T_found) { 
		double TgVecX = Tagert_x - CenterX; 
		double TgVecY = Tagert_y - CenterY; 
		double TgVecZ = Tagert_z - CenterZ; 
		TgDiff = Math.Sqrt(TgVecX * TgVecX + TgVecY * TgVecY + TgVecZ * TgVecZ); //calculating target vector 
		LeVecX = LeftX - CenterX; 
		LeVecY = LeftY - CenterY; 
		LeVecZ = LeftZ - CenterZ; 
		LeftDiff = Math.Sqrt(LeVecX * LeVecX + LeVecY * LeVecY + LeVecZ * LeVecZ);		//GPS Left vector 
		yaw_angle = Math.Round(Math.Acos((LeVecX * TgVecX + LeVecY * TgVecY + LeVecZ * TgVecZ) / (LeftDiff * TgDiff)) * 180 / Math.PI - 90, 1);		// angle Left vector to target vector   (Yaw) 
		ToVecX = TopX - CenterX; 
		ToVecY = TopY - CenterY; 
		ToVecZ = TopZ - CenterZ; 
		TopDiff = Math.Sqrt(ToVecX * ToVecX + ToVecY * ToVecY + ToVecZ * ToVecZ);		//GPS Top vector 
		pitch_angle = Math.Round(-1 * (Math.Acos((ToVecX * TgVecX + ToVecY * TgVecY + ToVecZ * TgVecZ) / (TopDiff * TgDiff)) * 180 / Math.PI - 90), 1);		// angle Top vector to target vector (Pitch) 
		ForwardX = LeVecY * ToVecZ - LeVecZ * ToVecY; 
		ForwardY = LeVecZ * ToVecX - LeVecX * ToVecZ; 
		ForwardZ = LeVecX * ToVecY - LeVecY * ToVecX; 
		ForwDist = Math.Sqrt(ForwardX * ForwardX + ForwardY * ForwardY + ForwardZ * ForwardZ); 
		double forward_angle = Math.Round(Math.Acos((ForwardX * TgVecX + ForwardY * TgVecY + ForwardZ * TgVecZ) / (ForwDist * TgDiff)) * 180 / Math.PI, 1);		// Check if the target is behind the ship and adjust yaw angle 
		if(forward_angle > 90) { 
			yaw_angle = (yaw_angle<0)?-180-yaw_angle:180-yaw_angle; 
		} 
	}  else {throw new Exception("Щось не так з трьома блоками GPS");} 
	 
	totalDistance = System.Math.Sqrt(((Read_X - CenterX)*(Read_X - CenterX)) + ((Read_Y - CenterY)*( Read_Y - CenterY)) + ((Read_Z - CenterZ)*(Read_Z - CenterZ))); 
	if(totalDistance > 0.01 && OldTime !=0) { 
		speed = Math.Round((totalDistance * 1000 / ( NewTimeK - OldTime)), accuracy); 
	} 
 
	double SensX = 0, SensY = 0, SensZ = 0, DistToFound, PithAngleSens = 0, YawAngleSens = 0, TgVecSensX, TgVecSensY, TgVecSensZ; 
	bool SensorFound = false; 
	var Sensors = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(Sensors); 
	 
	for (int s = 0; Sensors.Count > s; s++) { 
		IMySensorBlock thisSensor = Sensors[s] as IMySensorBlock; 
		var entity = thisSensor.LastDetectedEntity; 
		 
		thisSensor.SetValueFloat("Left", 50.0f); 
		thisSensor.SetValueFloat("Right", 50.0f); 
		thisSensor.SetValueFloat("Back", 50.0f); 
		thisSensor.SetValueFloat("Top", 50.0f); 
		thisSensor.SetValueFloat("Bottom", 50.0f); 
		thisSensor.SetValueFloat("Front", 50.0f); 
 
		if (entity != null) { 
			SensX = thisSensor.LastDetectedEntity.GetPosition().GetDim(0); 
			SensY = thisSensor.LastDetectedEntity.GetPosition().GetDim(1); 
			SensZ = thisSensor.LastDetectedEntity.GetPosition().GetDim(2); 
			SensorFound = true; 
		} 
	} 
	 
	if (SensorFound) { 
		TgVecSensX = SensX - CenterX; 
		TgVecSensY = SensY - CenterY; 
		TgVecSensZ = SensZ - CenterZ; 
		DistToFound = Math.Sqrt(TgVecSensX * TgVecSensX + TgVecSensY * TgVecSensY + TgVecSensZ * TgVecSensZ); //calculating target vector 
		YawAngleSens = Math.Round(Math.Acos((LeVecX * TgVecSensX + LeVecY * TgVecSensY + LeVecZ * TgVecSensZ) / (LeftDiff * DistToFound)) * 180 / Math.PI - 90, 1);		// angle Left vector to target vector   (Yaw) 
		PithAngleSens = Math.Round(-1 * (Math.Acos((ToVecX * TgVecSensX + ToVecY * TgVecSensY + ToVecZ * TgVecSensZ) / (TopDiff * DistToFound)) * 180 / Math.PI - 90), 1);		// angle Top vector to target vector (Pitch) 
		//double ForwardAngleSens = Math.Round(Math.Acos((ForwardX * TgVecSensX + ForwardY * TgVecSensY + ForwardZ * TgVecSensZ) / (ForwDist * DistToFound)) * 180 / Math.PI, 1);		// Check if the target is behind the ship and adjust yaw angle 
		//if(ForwardAngleSens > 90) { 
		//	YawAngleSens = (YawAngleSens<0)?-180-YawAngleSens:180-YawAngleSens; 
		//} 
	} 
 
	if (BeingHacked()){ 
		acceleration(MaxAcceleration); 
		ResultDrive = "Hack"; 
		noCoord = escapeCoord; 
	} 
 
	if (SensorFound) { 
		if (Math.Abs(YawAngleSens) < 25) { 
			if (speed > MaxSpeed_D) { 
				GyroSet("Yaw", -Speed_spin * 10f * Math.Sign(YawAngleSens)); 
				acceleration(0f); 
			} 
		} 
		else { 
			GyroSet("Yaw", 0f); 
			acceleration(MaxAcceleration / 5); 
		} 
		if (Math.Abs(PithAngleSens) < 25) { 
			if (speed > MaxSpeed_D) { 
				GyroSet("Pitch", -Speed_spin * 10f * Math.Sign(PithAngleSens)); 
				acceleration(0f); 
			}  
		} 
		else { 
			GyroSet("Pitch", 0f); 
			acceleration(MaxAcceleration / 5); 
		} 
	} 
	else { 
		ResultDrive = GoToTagert(speed, (float)yaw_angle, (float)pitch_angle, TgDiff); 
	} 
	 
	TimeToArrival = TimeLess(TgDiff); 
	 
	string PanelText = langTagert + ": " + noCoord + "\n" + langCoord + ": " + NowCoord + "\n" + langTime + ": " + NewTimeK + "\n" + langSpeed + ": " + speed + "m/s \n" + langDiffToArrival + ": " + Math.Ceiling(TgDiff) + "m \n" + langTimeToArrival + ": ~" + TimeToArrival + "\n" + ResultDrive; 
	if (Debug){  PanelText +=  "\n" + langAngle + ": " + yaw_angle + ", " + pitch_angle + "\n" + langAngle + langFound + ": " +YawAngleSens + "," + PithAngleSens + "\n" + langCoord + langFound + ": " + SensX + "," + SensY + "," + SensZ;} 
 
	thisPanel.WritePublicText(PanelText, false); 
	 
	if (ResultDrive == "Done") { 
		SetBeacon(langCoord + ": " + NowCoord + "\n" + langDone, true); 
	}  
	else { 
		SetBeacon("", false); 
	} 
} 
 
public  void acceleration (float forse){ 
	 
	List<IMyBlockGroup> groups = new List<IMyBlockGroup>(); 
	List<IMyTerminalBlock> thrusters = new List<IMyTerminalBlock>(); 
	groups = GridTerminalSystem.BlockGroups; 
	for(int i = 0; i < groups.Count; i++){ 
		if (groups[i].Name == GoTrustGroup) { 
			thrusters = groups[i].Blocks; 
		break; 
		} 
	} 
	for(int j = 0; j < thrusters.Count; j++){ 
		//if (forse == 1f) { thrusters[j].GetActionWithName("OnOff_Off").Apply(thrusters[j]);} 
		//else { 
		thrusters[j].SetValueFloat("Override", forse); 
		thrusters[j].GetActionWithName("OnOff_On").Apply(thrusters[j]); 
		// 
	} 
} 
 
public string GoToTagert(double MySpeed , float YawAngle, float PitchAngle, double Destination) { 
	float MaxAngle = 1f; 
	float Speed_spin_Yaw = Speed_spin; 
	float Speed_spin_Pitch = Speed_spin; 
	string Result = ""; 
	 
	if (Math.Abs(YawAngle) > 90f ) {Speed_spin_Yaw *= 2; }		// x8 
	if (Math.Abs(PitchAngle) > 90f ) {Speed_spin_Pitch *= 2; }	// x8 
	if (Math.Abs(YawAngle) > 30f ) {Speed_spin_Yaw *= 4; } 
	if (Math.Abs(PitchAngle) > 30f ) {Speed_spin_Pitch *= 4; } 
	 
	if (YawAngle > MaxAngle) { GyroSet("Yaw", Speed_spin_Yaw); } 
	if (YawAngle < MaxAngle * -1f) {GyroSet("Yaw", Speed_spin_Yaw * -1f); } 
	if (YawAngle > MaxAngle * -1f && YawAngle < MaxAngle) { GyroSet("Yaw", 0f);} 
	if (PitchAngle > MaxAngle) {GyroSet("Pitch", Speed_spin_Pitch); } 
	if (PitchAngle < MaxAngle * -1f) {GyroSet("Pitch", Speed_spin_Pitch *- 1f); } 
	if (PitchAngle > MaxAngle * -1f && PitchAngle < MaxAngle) { GyroSet("Pitch", 0f);} 
	 
	if (PitchAngle > MaxAngle * -2f && PitchAngle < MaxAngle * 2f && YawAngle > MaxAngle * -2f && YawAngle < MaxAngle * 2f ) { 
		if (Destination > distance) { 
			if (MySpeed < MaxSpeed) {acceleration(MaxAcceleration);} 
			if (MySpeed > MaxSpeed ) {acceleration(MaxAcceleration * 0.011f);} 
			if (MySpeed > (MaxSpeed + 10)) {acceleration(0f);} 
			Result = "Travel"; 
		} 
		if (Destination < distance && Destination > distance_S) { 
			if (MySpeed > MaxSpeed_D) {acceleration(MaxAcceleration * 0.011f);} 
			if (MySpeed > MaxSpeed_D + 5) {acceleration(0f);} 
			if (MySpeed < MaxSpeed_D) {acceleration(MaxAcceleration / 5);} 
			Result = "Travel_low_speed"; 
		} 
		if (Destination < distance_S) { 
			acceleration(0f); 
			Result = "Done"; 
		} 
	} 
	else { 
	acceleration(0f); 
	Result = "Stop"; 
	} 
	return Result; 
} 
 
public string TimeLess(double Destination1) { 
	double t = 0; 
	if (Destination1 > distance) { 
		t = Math.Ceiling((((Destination1 - ( distance + distance_S ))/ MaxSpeed) + 30)); 
	} 
	else { 
		t = Math.Ceiling(((Destination1 - 30) / MaxSpeed_D)); 
	} 
	if (t>59) { 
		return Convert.ToString(Math.Ceiling(t/60))+" min "+Convert.ToString(t%60)+" sec"; 
	} else { return Convert.ToString(t)+" sec";} 
} 
 
public double GetNowTime() { 
	string NowTime = DateTime.Now.ToString("ss:fff"); 
	string[] NewTime = NowTime.Split(':'); 
	double NewTime_ss = Convert.ToDouble(NewTime[0]); 
	double NewTime_fff = Convert.ToDouble(NewTime[1]); 
	return ((NewTime_ss * 1000) + NewTime_fff); 
} 
 
public bool BeingHacked() { 
	for(int i = 0; i < GridTerminalSystem.Blocks.Count; i++) { 
		if (GridTerminalSystem.Blocks[i].IsBeingHacked) { 
			return true ; 
		} 
	} 
	return false; 
} 
 
public void GyroSet(string GyroCommand, float GyroSpeed) { 
	IMyGyro thisGyro = null; 
	var Gyroscops = new List<IMyTerminalBlock>(); 
	GridTerminalSystem.GetBlocksOfType<IMyGyro>(Gyroscops); 
	if (Gyroscops != null) { 
		for (int g = 0; g < Gyroscops.Count; g++) { 
			thisGyro = Gyroscops[g] as IMyGyro; 
			thisGyro.SetValueFloat(GyroCommand, GyroSpeed); 
			if (!thisGyro.GyroOverride) {thisGyro.GetActionWithName("Override").Apply(thisGyro);} 
		} 
	} 
	else { throw new Exception ("Як літатии без гіроскопу ?");} 
} 
 
public void SetBeacon(string nameBeacon, bool BeaconOnOff) { 
	IMyBeacon thisBeacon = null; 
	for(int i = 0; i < GridTerminalSystem.Blocks.Count; i++) { 
		if(GridTerminalSystem.Blocks[i].CustomName.Contains(BeaconName)) { 
			thisBeacon = GridTerminalSystem.Blocks[i] as IMyBeacon; 
			if(thisBeacon != null) 
				break; 
		} 
	} 
	if(thisBeacon != null) { 
		if (BeaconOnOff) { 
			thisBeacon.GetActionWithName("OnOff_On").Apply(thisBeacon); 
			thisBeacon.SetCustomName ( BeaconName + "\n" + nameBeacon); 
		} 
		else { 
			thisBeacon.GetActionWithName("OnOff_Off").Apply(thisBeacon); 
		} 
	} 
	else { throw new Exception("Немає маяка з ім'ям '" + BeaconName + "'");} 
}