#pragma once 

//#include "ColorTrafficLight.h"

#include "ColorRGB.h"
#include "Comm.h"
#include "Track.h"

class Track;

enum RaceStatus
{
	PREP_READY_TO_START = 0,
	READY_TO_START = 1,
	READY = 2,
	SET = 3,
	GO = 4,
	RACING = 5,
	OFF_TRACK = 6,
	FINAL_LAP = 7,
	WINNER = 8,
	SHOW_WINNER = 9,
	WAITING = 10,
	DISQUALIFY = 11
};

class RaceController
{
	int carsFinished = 0;
	Comm* controller;
	bool isCurrentlyRacing;
	RaceStatus lastRaceStatus;
	int lastRaceStatusTicks = 0;
	Comm* reporter;
	int ticksRaceStarted;
	std::vector<Track> tracks;
	int tracksReady = 0;
	int trackStatusId;

public:
	int trackCount = 2;
	int trackStart = 1;
	bool useColorSensors = false;
	int raceLaps = 8;
	std::string multicastAddress, multicastMask;

	ColorDisplay* indicator;
	RaceStatus raceStatus;

	RaceController();

	void Initialize();
	void Tick();

	void Blip(Color color = BLACK);
	void ColorChanged(Track* track);
	void Disqualify(Track* track);
	int GetRaceTime(int finishLine);
	bool IsInCountdown(bool includeGo = false);
	bool IsRacingOrPostRace(int ticks);
	bool IsRacing();
	int SendDirect(char* message, unsigned short port);
	int SendRaw(char* message);
	int SendRace(int track, char* key, char* value);
	int SendRace(int track, char* key, int value);
	void StartRace(int ticks);
	void StatusCheck();
	bool TrackLapChanged(Track* track);
	void TrackReady(Track* track);
};


