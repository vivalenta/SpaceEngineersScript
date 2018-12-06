/** 
* Внутрішньо-ігровий АвтоДор + АйрЛок
* Программа - аналог (зовсім не конкурент) моду AutoDoor  
  
* 1) Поставити сенсор, можна декілька на відстані бажано 100м.  
* 2) Поставити будь-яку кількість дверей    
* 3) Завантажити программу в програмний блок.  
* 4) Перевірити роботу
* 5.1) Якщо в імені дверей буде _airLockName, то одночасно вікриются лише одні, причому можливо багато одночасних шлюзів
* 4.1) Якщо в імені дверей після _airLockName буде "ще щось" то шлюз матеме назву "ще щось")
*      Наприклад "Двері Шлюз Ангару", та "Двері 2 Шлюз Ангару", "Двері 3 Шлюз Ангару", то одночасно вікриются лише одні (всі двері одного шлюза)
*
*
*      Наприклад "Двері Шлюз Ангару", та "Двері Шлюз Ангару2", "Двері 3 Шлюз Бази", то це різні шлюзи ї можуть відкриватись одночасно
* v 3.0 тотально переписано з "0"
*
* ======================= [EN] =============================  
*
* Inner-game AutoDoor + AirLock
* The program is an analogue (not a competitor) of AutoDoor fashion
  
* 1) Put a sensor, you can several at a distance preferably 100m.
* 2) Put any number of doors
* 3) Download the program into the software unit.
* 4) Check work
* 5.1) If the name of the door is _airLockName, then only one will be detected at the same time, and possibly many simultaneous gateways
* 4.1) If there is "something else" in the door name after _airLockName, then the gateway will name "something else")
* 
* For example, Set _airLockName = "Gateway", "Door Gateway Hangar", and "Door 2 Gateway Hangar", "Door 3 Gateway Hangar", then only one will appear at the same time (all doors of the same gateway)
*
*
* For example, Set _airLockName = "Gateway", "Door Gateway Hangar" and "Door Gateway Hangar2", "Door 3 Gateway Bases", then these different gateways can open at the same time.
* v 3.0 totally rewritten with "0"
*/

    // Config
    private readonly double _distantionOpen = 2.2; // Дистанція відкривання дверей              [EN] Distance to open Doors.  
    private readonly bool _sensorSetDist = true;   // Встановлювати максимальну дистанцію 	   [EN] Set max range on sensor ?   
    private readonly bool _offDors = true; // Економія енергії			                       [EN] off doors
    private readonly bool _ignorHangars = true; // Ігнор ангарних 			                   [EN] Ignore Hangar Dors
    private readonly bool _airLockEnable = true; // Айрлок включено				               [EN] AirLock enable
    private readonly string _airLockName = "Шлюз"; // Айрлок ім'я				               [EN] AirLock door name
    private readonly string _doorIgnoreName = "Герметичні"; // Ігнорувати дверіз ім'ям		   [EN] Ignore door name
    private bool thisDoorIsOpen = false;

    public Program()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1; //Load();
    }

    public void Main()
    {
        string outlines = "";
        List<IMySensorBlock> sensors = new List<IMySensorBlock>();
        List<IMyDoor> doors = new List<IMyDoor>();
        List<MyDetectedEntityInfo> detection = new List<MyDetectedEntityInfo>();
        Dictionary<long, Vector3D> allPlayersLocationsDictionary = new Dictionary<long, Vector3D>();
        Dictionary<long, string> allPlayersNamesDictionary = new Dictionary<long, string>();
        Dictionary<IMyDoor, string> allDoorsToOpenDictionary = new Dictionary<IMyDoor, string>();

        GridTerminalSystem.GetBlocksOfType<IMySensorBlock>(sensors);
        GridTerminalSystem.GetBlocksOfType<IMyDoor>(doors);


        foreach (var thisSensor in sensors)
        {
            detection.Clear();
            if (_sensorSetDist)
            {
                thisSensor.SetValueFloat("Left", 50.0f);
                thisSensor.SetValueFloat("Right", 50.0f);
                thisSensor.SetValueFloat("Back", 50.0f);
                thisSensor.SetValueFloat("Top", 50.0f);
                thisSensor.SetValueFloat("Bottom", 50.0f);
                thisSensor.SetValueFloat("Front", 50.0f);
            }
            thisSensor.DetectedEntities(detection);

            foreach (MyDetectedEntityInfo info in detection)
            {
                if (info.Type != MyDetectedEntityType.CharacterHuman) continue;
                try
                {
                    allPlayersLocationsDictionary.Add(info.EntityId, info.Position);
                    allPlayersNamesDictionary.Add(info.EntityId, info.Name);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        
        foreach (IMyDoor thisDoor in doors)
        {
            
            if (thisDoor.CustomName.Contains(_doorIgnoreName)) continue;
            if (_ignorHangars && thisDoor is IMyAirtightHangarDoor) continue;
            int countLoop = 1;
            foreach (KeyValuePair<long, Vector3D> thisPlayer in allPlayersLocationsDictionary)
            {

                int index = thisDoor.CustomName.IndexOf(_airLockName);
                if (thisDoorIsOpen) continue;
                if (allDoorsToOpenDictionary.ContainsKey(thisDoor)) continue;
                if (_airLockEnable & index >= 0) // є айрлок і Мають назву айрлока
                {
                    int startName = index + _airLockName.Length;
                    string airlockName = thisDoor.CustomName
                        .Substring(startName, thisDoor.CustomName.Length - startName).Trim(); //Назва
                    if (airlockName == "") airlockName = _airLockName;

                    int airLock = 0;
                    foreach (IMyDoor thisDoor2 in doors) //Не закрита із цьго шлюзу 
                    {
                        if (thisDoor2.CustomName.Contains(airlockName) && thisDoor2.OpenRatio != 0)
                            airLock = airLock + 1;
                    }
                    if (Vector3D.Distance(thisDoor.GetPosition(), thisPlayer.Value) < _distantionOpen) // Потрібно відкрити ?
                    {
                        thisDoorIsOpen = true;
                        outlines = outlines + _airLockName + " " + airlockName + ": " + allPlayersNamesDictionary[thisPlayer.Key] + "\n";
                        if (airLock == 0)
                        {
                            thisDoorIsOpen = true;
                            allDoorsToOpenDictionary.Add(thisDoor, allPlayersNamesDictionary[thisPlayer.Key]);
                           
                        }
                    }
                    else // потрібно  зачинити
                    {
                        if (!thisDoorIsOpen & thisPlayer.Key == allPlayersLocationsDictionary.Keys.Last())
                        {
                            if (_offDors && thisDoor.OpenRatio == 0)
                            {
                                thisDoor.GetActionWithName("OnOff_Off").Apply(thisDoor);
                            }
                            thisDoor.GetActionWithName("Open_Off").Apply(thisDoor);

                        }
                    }
                    countLoop = countLoop + 1;
                }

                else if (Vector3D.Distance(thisDoor.GetPosition(), thisPlayer.Value) < _distantionOpen) // Потрібно відкрити (не айлок) ?
                {
                    allDoorsToOpenDictionary.Add(thisDoor, allPlayersNamesDictionary[thisPlayer.Key]);
                    thisDoorIsOpen = true;

                }
                else // потрібно  зачинити
                {
                    if (!thisDoorIsOpen)
                    {
                        if (_offDors && thisDoor.OpenRatio == 0)
                            thisDoor.GetActionWithName("OnOff_Off").Apply(thisDoor);
                        thisDoor.GetActionWithName("Open_Off").Apply(thisDoor);
                    }
                }
                countLoop = countLoop + 1;
            }
            thisDoorIsOpen = false;
        }

        foreach (KeyValuePair<IMyDoor, string> thisDoor in allDoorsToOpenDictionary) // Відкривання
        {
            if (_offDors) thisDoor.Key.GetActionWithName("OnOff_On").Apply(thisDoor.Key);

            thisDoor.Key.GetActionWithName("Open_On").Apply(thisDoor.Key);
            outlines = outlines + thisDoor.Key.CustomName + ": " + thisDoor.Value + "\n";
        }
        Echo(outlines);
    }