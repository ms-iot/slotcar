#include "ColorRGB.h"

#include <arduino.h>
#include <array>

const Color BLACK = Color(0, 0, 0);

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

void ColorRGB::Blip(int intervalMs)
{
	Blip(intervalMs, BLACK);
}

void ColorRGB::Blip(int intervalMs, Color color)
{
	Color savedColor = this->savedColor;
	setDirectColor(color);
	this->savedColor = savedColor;
	ticksToRestoreColor = GetTickCount() + intervalMs;
}

void ColorRGB::Clear()
{
	SetColor(0, 0, 0);
	colors.clear();
}

void ColorRGB::Configure() {
	pinMode(redPin, OUTPUT);
	pinMode(greenPin, OUTPUT);
	pinMode(bluePin, OUTPUT);
}

void ColorRGB::SetColor(byte red, byte green, byte blue) {
	analogWrite(redPin, red);
	analogWrite(greenPin, green);
	analogWrite(bluePin, blue);
}

void ColorRGB::SetColor(Color color) {
	if (colors.size() > 0)
	{
		colors.clear();
	}

	setDirectColor(color);
}

void ColorRGB::setDirectColor(Color color) {
	SetColor(color.red, color.green, color.blue);
	savedColor = color;
}

void ColorRGB::Flash(std::vector<Color> colors, int intervalms)
{
	this->colors.swap(colors);
	this->colorindex = -1;
	this->intervalms = intervalms;
	this->ticks = 0;
}

void ColorRGB::Tick()
{
	int tick = GetTickCount();

	if (tick > ticksToRestoreColor)
	{
		ticksToRestoreColor = INT_MAX;
		setDirectColor(savedColor);
	}

	if (colors.size() > 0 && tick > ticks + intervalms)
	{
		colorindex = (colorindex + 1) % colors.size();
		setDirectColor(colors[colorindex]);
		ticks = tick;
	}
}