#include "ColorRGB.h"

#include <arduino.h>
#include <array>

Color::Color()
{
}

Color::Color(byte red, byte green, byte blue)
{
	this->red = red;
	this->green = green;
	this->blue = blue;
}

ColorRGB::ColorRGB()
{
}

ColorRGB::ColorRGB(int redPin, int greenPin, int bluePin) {
	this->redPin = redPin;
	this->greenPin = greenPin;
	this->bluePin = bluePin;

	Configure();
}

ColorRGB::~ColorRGB()
{
}

void ColorRGB::Configure() {
	pinMode(redPin, OUTPUT);
	pinMode(greenPin, OUTPUT);
	pinMode(bluePin, OUTPUT);
}

void ColorRGB::SetColor(Color color)
{
	ColorDisplay::SetColor(color);
}

void ColorRGB::SetColor(byte red, byte green, byte blue) {
	analogWrite(redPin, red);
	analogWrite(greenPin, green);
	analogWrite(bluePin, blue);
}		  