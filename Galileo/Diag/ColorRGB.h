#pragma once

#include <windows.h>
#include <vector>

struct Color {
	byte red;
	byte green;
	byte blue;
public:
	Color();
	Color(byte red, byte green, byte blue);
};

class ColorRGB
{
	int redPin, greenPin, bluePin;
	std::vector<Color> colors;
	int colorindex;
	int intervalms;
	int ticks;
	int ticksToRestoreColor;
	Color savedColor;

public:
	void setDirectColor(Color color);

	ColorRGB();
	ColorRGB(int redPin, int greenPin, int bluePin);
	~ColorRGB();
	void Blip(int intervalMs);
	void Blip(int intervalMs, Color color);
	void Clear();
	void Configure();
	void Flash(std::vector<Color> colors, int intervalms);
	void SetColor(byte red, byte green, byte blue);
	void SetColor(Color color);
	void Tick();
};

