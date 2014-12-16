#undef UNICODE

#include "stdafx.h"
#include "arduino.h"

// Need to link with Ws2_32.lib
#pragma comment (lib, "Ws2_32.lib")

#include "RaceController.h"

int _tmain(int argc, _TCHAR* argv[])
{
	return RunArduinoSketch();
}

RaceController raceController = RaceController();

void setup()
{
	raceController.Initialize();
}

void loop()
{
	raceController.Tick();
}
