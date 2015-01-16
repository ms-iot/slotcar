#include "Comm.h"

Comm::Comm(std::string broadcastMask, std::string sourceAddress)
{
	this->broadcastMask = broadcastMask;
	this->sourceAddress = sourceAddress;
}