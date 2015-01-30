// Manages hall-effect sensor (IsTriggered)

#include "HallEffectSensor.h"
#include "arduino.h"

HallEffectSensor::HallEffectSensor()
{
}

HallEffectSensor::HallEffectSensor(int pin, bool isAnalog)
{
	this->pin = pin;
	this->isAnalog = isAnalog;
}

void HallEffectSensor::Initialize()
{
	pinMode(pin, INPUT);
}

bool HallEffectSensor::IsTriggered()
{
	value = isAnalog ? analogRead(pin) : digitalRead(pin);
	bool isTriggered = isAnalog ? value < 1010 : value == 0;
	bool triggerResult = !wasTriggered && isTriggered;
	wasTriggered = isTriggered;
	return triggerResult;
}