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
	bool triggered = isAnalog ? value < 1010 : value == 0;
	bool triggeredForward = !isTriggered && triggered;
	isTriggered = triggered;
	return triggeredForward;
}