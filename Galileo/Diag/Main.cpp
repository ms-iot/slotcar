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
	raceController.trackCount = 2;			// number of tracks on race track
	raceController.useColorSensors = true;	// use color sensors to detect cars ready to start
	raceController.raceLaps = 8;			// how many laps make a race?
	raceController.multicastAddress = "234.5.6.7";	 // multicast group
	raceController.multicastMask = "169.254.255.255"; // multicast mask

	raceController.Initialize();

}

void loop()
{
	//Log(".");
	raceController.Tick();
	//Log(";");
}
