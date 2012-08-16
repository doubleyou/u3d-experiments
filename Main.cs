using UnityEngine;
using System.Collections;
using System;

public class Main : MonoBehaviour {
	
	Networking net;
	
	// Use this for initialization
	void Start () {
		net = new Networking();
		
		int loading = 1;
		while (loading == 1) {
			net.Receive();
			loading = Serializer.Unserialize(net.recbuf);
//			loading = UpdateWorldFromServer(net.recbuf);
		}
		
		net.EnterReceiveLoop();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static int UpdateWorldFromServer(byte[] packet) {
		switch(packet[0]) {
		case Networking.MSG_NOTHING_TO_SEND:
			return 0;
		case Networking.MSG_PLAYER_POSITION:
			Vector3 position = new Vector3(
				BitConverter.ToSingle(packet, 1), BitConverter.ToSingle(packet, 5), BitConverter.ToSingle(packet, 9));
			Vector3 rotation = new Vector3(
				BitConverter.ToSingle(packet, 13), BitConverter.ToSingle(packet, 17), BitConverter.ToSingle(packet, 21));
			
			GameObject player = Player();
			player.transform.position = position;
			player.transform.localEulerAngles = rotation;
			return 1;
		}
		return -1;
	}
	
	public static GameObject Player() {
		return GameObject.Find("First Person Controller");
	}
}