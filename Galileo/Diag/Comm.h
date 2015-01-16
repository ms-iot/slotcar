#pragma once

#include <string>

class Comm
{
public:
	std::string broadcastMask, sourceAddress;
	char* recentInput;
	int lastBytesReceived;

	Comm(std::string broadcastMask, std::string sourceAddress);
	virtual void Initialize() {};

	virtual int Send(char* message, int channel) { return 0; };
	virtual int Receive(unsigned short channel) { return 0; };

	virtual void Tick() {};
};
