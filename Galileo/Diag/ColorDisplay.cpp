﻿#include "ColorDisplay.h"

ColorDisplay::ColorDisplay()
{
}

ColorDisplay::~ColorDisplay()
{
}

bool ColorDisplay::IsEqual(Color color1, Color color2)
{
	return color1.blue == color2.blue && color1.green == color2.green && color1.red == color2.red;
}

void ColorDisplay::Blip(int intervalMs)
{
	Blip(intervalMs, BLACK);
}

void ColorDisplay::Blip(int intervalMs, Color color)
{
	if (IsEqual(this->savedColor, color)) {
		color = IsEqual(BLACK, color) ? GREEN : BLACK;
	}

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