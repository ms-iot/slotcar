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

const Color RED = Color(255, 0, 0);
const Color BLACK = Color(0, 0, 0);
const Color WHITE = Color(255, 255, 255);
const Color YELLOW = Color(255, 255, 0);
const Color GREEN = Color(0, 255, 0);

class ColorDisplay
{
	std::vector<Color> colors;
	int colorindex;
	int intervalms;
	int ticks;
	int ticksToRestoreColor;
	Color savedColor;

public:
	void setDirectColor(Color color);

	virtual void Configure()
	{
	}

	virtual void SetColor(byte red, byte green, byte blue)
	{
	}

	virtual ~ColorDisplay();

	ColorDisplay();
	void Blip(int intervalMs);
	void Blip(int intervalMs, Color color);
	void Clear();
	void Flash(std::vector<Color> colors, int intervalms);
	virtual void SetColor(Color color);
	void Tick();
};