#pragma once

#include "Adafruit_TCS34725.h"
#include "HallEffectSensor.h"
#include "RaceController.h"
#include "SensorFilter.h"

class RaceController;

class Track
{
	RaceController* raceController;
	Adafruit_TCS34725* colorSensor;

	int colorSensorControlPin;
	bool crossedStartingLine;
	bool hasStarted = false;
	int lastPassedLine;
	int lastReadRGB = 0;
	std::vector<HallEffectSensor> positionalSensors;
	int positionalSensorsPerTrack = 0;
	int ticksCrossedStartingLine;
	int ticksFinishLine = 0;
	int ticksLastTrigged;
	int trackPinStart;
	bool trackReady = false;
	bool usesColor = false;

public:
	int trackId;
	int lap;
	bool isOfftrack;
	SensorFilter colorSensorFilter;

	Track();
	Track(RaceController* raceController, int trackId, bool useColor, int trackPinStart, int positionalSensorsPerTrack, int colorSensorControlPin);

	void Initialize();
	void Tick();

	void CheckColorSensor();
	rgbc GetAdjustedRGBValue();
	rgbc GetRGB();
	void PositionChanged(int position);
	void StartRace(int ticks);
	void StopRace();
};
