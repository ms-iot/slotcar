#pragma once

#include "Comm.h"
#include "arduino.h"


class CommTCP : public Comm
{
public:

	//Keyboard Controls:
	//
	// 1 -Motor 1 Left
	// 2 -Motor 1 Stop
	// 3 -Motor 1 Right
	//
	// 4 -Motor 2 Left
	// 5 -Motor 2 Stop
	// 6 -Motor 2 Right

	// Motor 1
	int dir1PinA = D8;
	int dir2PinA = D11;

	int dir1PinB = D13;
	int dir2PinB = D12;

	int speedPinA = D10; // Needs to be a PWM pin to be able to control motor speed
	int speedPinB = D9; // Needs to be a PWM pin to be able to control motor speed

	CommTCP(std::string sourceAddress) : Comm("", sourceAddress) {};
	void Initialize() override;


};