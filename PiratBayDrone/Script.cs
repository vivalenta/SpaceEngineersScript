void Main(){
	List<IMyTerminalBlock>() list = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(list);
	if (list.Count > 0){
		IMyRemoteControl remote = list[0] as IMyRemoteControl;
		remote.ClearWaypoints();
		Vector3D player = new Vector3D(0, 0, 0);
		bool success = remote.GetNearestPlayer(out player);
		if (success){
			remote.AddWaypoint(player, "Player");
			remote.SetAutoPilotEnabled(true);
		}
	}
}