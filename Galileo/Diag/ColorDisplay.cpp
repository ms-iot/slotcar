#include "ColorDisplay.h"
#include <arduino.h>
#include "ColorRGB.h"

ColorDisplay::ColorDisplay()
{
}

ColorDisplay::~ColorDisplay()
{
}

void ColorDisplay::Blip(int intervalMs)
{
	Blip(intervalMs, BLACK);
}

void ColorDisplay::Blip(int intervalMs, Color color)
{
	Color savedColor = this->savedColor;
	setDirectColor(color);
	this->savedColor = savedColor;
	ticksToRestoreColor = GetTickCount() + intervalMs;
}

void ColorDisplay::Clear()
{
	SetColor(0, 0, 0);
	colors.clear();
}

void ColorDisplay::SetColor(Color color) {
	if (colors.size() > 0)
	{
		colors.clear();
	}

	setDirectColor(color);
}

void ColorDisplay::setDirectColor(Color color) {
	SetColor(color.red, color.green, color.blue);
	savedColor = color;
}

void ColorDisplay::Flash(std::vector<Color> colors, int intervalms)
{
	this->colors.swap(colors);
	this->colorindex = -1;
	this->intervalms = intervalms;
	this->ticks = 0;
}

void ColorDisplay::Tick()
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