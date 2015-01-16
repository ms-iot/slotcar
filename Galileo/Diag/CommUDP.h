#pragma once

#include "PracticalSocket.h"
#include "comm.h"

class CommUDP : public Comm
{
	UDPSocket sock;
public:

	CommUDP(string broadcastMask, string sourceAddress) : Comm(broadcastMask, sourceAddress) {};
	void Initialize() override;
	int Send(char* message, int channel) override;
	int Receive(unsigned short channel) override;

	void Tick() override;
};
