#include "ColorDisplay.h"

class ColorTrafficLight	: public ColorDisplay
{
public:
	int redPin, yellowPin, greenPin;
	
	ColorTrafficLight();
	ColorTrafficLight(int redPin, int yellowPin, int greenPin);

	void Configure() override;
	void SetColor(Color color) override;
	void SetColor(byte red, byte green, byte blue) override;
};
