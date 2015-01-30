#include "CommUDP.h"
#include <arduino.h>
#include <string>

void CommUDP::Initialize()
{
	int multicastTTL = 2;
	sock.setMulticastTTL(multicastTTL);
	
	recentInput = new char[256];
}

void CommUDP::Tick()
{
}

int CommUDP::Send(char *message, int channel){
	try {
		sock.sendTo(message, strlen(message), broadcastMask, channel);
	}
	catch (SocketException &e) {
		Log(e.what());
		return 1;
	}

	return 0;
}

int CommUDP::Receive(unsigned short channel)
{
	const int MAXRCVSTRING = 255;

	int bytesReceived = sock.recvFrom(recentInput, MAXRCVSTRING, sourceAddress, channel);
	recentInput[bytesReceived] = '\0';

	return bytesReceived;
}
