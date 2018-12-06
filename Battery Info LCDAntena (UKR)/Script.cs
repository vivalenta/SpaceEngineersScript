/**
* Скрипт для виводу стану батарей, відсотку використанняя та залишку єнергії
* Виводить інфо на текстову панель та або антену
* Подяка Clockwork Gremlin, Gandur за зразок коду
* Автор vivalenta
*
*/

string langBattery = "Батареї";		// Регіональна назва батарей (Аккумуляторів)
string langCharged = "Залишилось";	// Регіональна назва залишку
string AntenaName = "Корито";		// Частина або повна назва антени, якщо включено відображення на антену
bool enableAntena = true;			// Ввімкненість відображення на антену
string PanelName = "Текстова панель";	// Частина назви панелі, якщо включено відображення на текстову панель
bool enablePanel = true; 			// Ввімкненість відображення на текстову панель false
double BateeryCap = 500000;			// Об'ем одної батареї (Великий корабель 4000000; Малий 1440000)

System.Text.RegularExpressions.Regex batteryRegex = new System.Text.RegularExpressions.Regex(
"Максимальна ємність енергії:(\\d+\\.?\\d*) (\\w?)Wh.*Поточнє введення:(\\d+\\.?\\d*) (\\w?)W.*Поточнє виведення:(\\d+\\.?\\d*) (\\w?)W.*Збережена єнергія:(\\d+\\.?\\d*) (\\w?)Wh"
, System.Text.RegularExpressions.RegexOptions.Singleline);

IMyRadioAntenna thisAntena;
IMyTextPanel thisPanel;

void Main() {
	double fullPower = 0, batteryPower = 0, batteryCurrent = 0, batteryStoredCurrent = 0, batteryStoredMax = 0;
	var batteries = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries);

	for ( int i = 0; i <batteries.Count; i++ ) {
		IMyBatteryBlock thisBattery = batteries[i] as IMyBatteryBlock;
		System.Text.RegularExpressions.Match match = batteryRegex.Match( thisBattery.DetailedInfo );
		Double n;
		if ( match.Success ) {
			Double pwrMaxStored = 0.0;
			Double pwrNowStored = 0.0;
			Double pwrCurrent = 0.0;
			Double pwrInput = 0.0;
			Double pwrStoredPercent = 0.0;
			//Seriously, thanks "me 10 Jin." I couldn't have done this without your example.
			if ( Double.TryParse( match.Groups[5].Value, out n ) )
				batteryCurrent += n * Math.Pow( 1000.0, ".kMGTPEZY".IndexOf( match.Groups[6].Value ) );

			if ( Double.TryParse( match.Groups[1].Value, out n ) )
				pwrMaxStored = n * Math.Pow( 1000.0, ".kMGTPEZY".IndexOf( match.Groups[2].Value ) );

			if ( Double.TryParse( match.Groups[3].Value, out n ) )
				pwrInput = n * Math.Pow( 1000.0, ".kMGTPEZY".IndexOf( match.Groups[4].Value ) );

			if ( Double.TryParse( match.Groups[5].Value, out n ) )
				pwrCurrent = n * Math.Pow( 1000.0, ".kMGTPEZY".IndexOf( match.Groups[6].Value ) );

			if ( Double.TryParse( match.Groups[7].Value, out n ) )
				pwrNowStored = n * Math.Pow( 1000.0, ".kMGTPEZY".IndexOf( match.Groups[8].Value ) );

			batteryPower += BateeryCap;
			pwrStoredPercent = pwrNowStored/pwrMaxStored*100;
			batteryStoredMax += pwrMaxStored;
			batteryStoredCurrent += pwrNowStored;
			string percent_b = String.Format("{0:0.###}", pwrStoredPercent);
			string batteryInfo = thisBattery.DetailedInfo;
			thisBattery.SetCustomName(System.Text.RegularExpressions.Regex.Replace(thisBattery.CustomName, " \\(.*", "") + " ("+percent_b+"%"+")");
		}
	}
	string percent = String.Format("{0:0.##}", batteryStoredCurrent / batteryStoredMax * 100)+"% " + langCharged;
	string output = " \n" + langBattery + ":" + encodePower(batteryCurrent) + "/" + encodePower(batteryPower) + "\n["+percent+"]";
	
	if(thisAntena == null && enableAntena ) {
		for(int i = 0; i < GridTerminalSystem.Blocks.Count; i++) {
			if(GridTerminalSystem.Blocks[i].CustomName.Contains(AntenaName)) {
				thisAntena = GridTerminalSystem.Blocks[i] as IMyRadioAntenna;
				if(thisAntena != null)
					break;
			}   
		}  
		 
		 if(thisAntena == null) throw new Exception("Немає антени з ім'ям '" + AntenaName + "'"); 
		 thisAntena.SetCustomName(AntenaName + output); 
	} 

	if(thisPanel == null && enablePanel ) {
		for(int i = 0; i < GridTerminalSystem.Blocks.Count; i++) {
		if(GridTerminalSystem.Blocks[i].CustomName.Contains(PanelName)) 
			thisPanel = GridTerminalSystem.Blocks[i] as IMyTextPanel;
			if(thisPanel != null){break;}
		}
	}

		if(thisPanel == null) {throw new Exception("Немає панелі з ім'ям '" + PanelName + "'");}
		thisPanel.SetValueFloat("FontSize", 1.8f);
		thisPanel.SetValue("FontColor", Color.Green);
		thisPanel.SetValue("BackgroundColor", Color.Black);
		thisPanel.WritePublicText(output);
		thisPanel.ShowPublicTextOnScreen();
		
}


string encodePower(double power)
{
	string unit = "W";
	if (power > 1000000) {
		unit = "MW";
		power /= 1000000;
	} else if (power > 1000) {
		unit = "kW";
		power /= 1000;
	}

	return String.Format("{0:0.##}", power) + unit;
}