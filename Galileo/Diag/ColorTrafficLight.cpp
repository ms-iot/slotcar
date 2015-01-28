#include "ColorTrafficLight.h"
#include <arduino.h>

const byte ON = LOW;
const byte OFF = HIGH;

ColorTrafficLight::ColorTrafficLight()
{
}

ColorTrafficLight::ColorTrafficLight(int redPin, int yellowPin, int greenPin)
{
	this->redPin = redPin;
	this->yellowPin = yellowPin;
	this->greenPin = greenPin;

	Configure();
}

void ColorTrafficLight::Configure()
{
	pinMode(redPin, OUTPUT);
	pinMode(yellowPin, OUTPUT);
	pinMode(greenPin, OUTPUT);
}

void ColorTrafficLight::SetColor(Color color)
{
	ColorDisplay::SetColor(color);
}

void ColorTrafficLight::SetColor(byte red, byte green, byte blue) {
	digitalWrite(redPin, (red == 255 && (blue == 255 || green == 0) ) ? ON : OFF);
	digitalWrite(yellowPin, (red == 255 && green == 255) ? ON : OFF);
	digitalWrite(greenPin, (green == 255 && (blue == 255 || red == 0)) ? ON : OFF);
	
}