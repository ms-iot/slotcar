#pragma once

#include <windows.h>
#include <vector>
#include "ColorDisplay.h"

class ColorRGB : public ColorDisplay
{
	int redPin, greenPin, bluePin;

public:
	ColorRGB();
	ColorRGB(int redPin, int greenPin, int bluePin);
	~ColorRGB();

	void Configure() override;
	void SetColor(Color color) override;
	void SetColor(byte red, byte green, byte blue) override;
};

