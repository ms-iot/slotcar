﻿#pragma once

#include "Adafruit_TCS34725.h"
#include "SensorFilter.h"
#include <memory>
#include "RaceController.h"
#include "HallEffectSensor.h"

class RaceController;

class Track
{
	RaceController* raceController;

	Adafruit_TCS34725* colorSensor;
	int lastPassedLine;
	bool usesColor = false;
	int ticksFinishLine = 0;
	bool hasStarted = false;
	bool trackReady = false;
	vector<HallEffectSensor> positionalSensors;
	int trackPinStart;

public:
	int trackId;
	int lap;
	SensorFilter colorSensorFilter;

	Track();
	Track(RaceController* raceController, int trackId, bool useColor, int trackPinStart);
	void Initialize();
	void CheckColorSensor();
	void StartRace(int ticks);
	void StopRace();
	rgbc GetRGB();
	void Tick();
	rgbc GetAdjustedRGBValue();
};
