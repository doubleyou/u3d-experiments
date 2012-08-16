using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class Networking {
	public Socket socket;
	public EndPoint server;
	
	public byte[] recbuf;
	
	public string serverAddress = "127.0.0.1";
	public int serverPort = 8080;
	public int clientPort = 8080;
	public int receiveTimeout = 2000;
	public int receiveBufferSize = 4096;
	
	// Constants
	const string SYN_MSG="SYN";
	const string ACK_MSG="ACK";
	
	public const byte MSG_NOTHING_TO_SEND = 0;
	public const byte MSG_PLAYER_POSITION = 1;
	
	
	public Networking() {
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		socket.ReceiveTimeout = receiveTimeout;
		
		recbuf = new byte[receiveBufferSize];
		
		IPAddress ipaddr = IPAddress.Parse(serverAddress);
		IPEndPoint ep = new IPEndPoint(ipaddr, serverPort);
		server = (EndPoint)(new IPEndPoint(IPAddress.Any, clientPort));
		
		socket.SendTo(Encoding.ASCII.GetBytes(SYN_MSG), ep);
		socket.ReceiveFrom(recbuf, ref server);
		Send(ACK_MSG);	
	}
	
	public void EnterReceiveLoop() {
		socket.BeginReceiveFrom(recbuf, 0, receiveBufferSize, SocketFlags.None, ref server,
			new AsyncCallback(AsyncReceive), this);
	}
	
	public static void AsyncReceive(IAsyncResult res) {
		Networking net = (Networking)res.AsyncState;
		Socket socket = net.socket;
		EndPoint server = net.server;
		
		socket.EndReceiveFrom(res, ref server);
		
		Main.UpdateWorldFromServer(net.recbuf);
		
		socket.BeginReceiveFrom(net.recbuf, 0, net.receiveBufferSize, SocketFlags.None, ref server,
			new AsyncCallback(AsyncReceive), net);
	}
	
	public void Send(string msg) {
		socket.SendTo(Encoding.ASCII.GetBytes(msg), server);
	}
	
	public void Send(byte[] bytes) {
		socket.SendTo(bytes, server);
	}
	
	public int Receive() {
		return socket.ReceiveFrom(recbuf, ref server);
	}
}