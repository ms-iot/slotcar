#pragma once 

#include "PracticalSocket.h"
#include "Track.h"
#include "ColorRGB.h"
#include "ColorTrafficLight.h"
#include "Comm.h"

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
	Comm* controller;
	Comm* reporter;
	//UDPSocket sock;
	bool isCurrentlyRacing;
	RaceStatus lastRaceStatus;
	int lastRaceStatusTicks = 0;
	int ticksRaceStarted;
	vector<Track> tracks;
	int carsFinished = 0;
	int trackStatusId;
	int tracksReady = 0;

public:
	//ColorRGB indicator;
	ColorDisplay* indicator;

	RaceStatus raceStatus;

	RaceController();
	void Initialize();
	void StartRace(int ticks);
	void ColorChanged(Track* track);
	int SendRaw(char* message);
	int SendRace(int track, char* key, char* value);
	int SendRace(int track, char* key, int value);
	int SendDirect(char* message, unsigned short port);
	void StatusCheck();
	void Tick();
	bool IsRacing();
	bool IsNewLapTime(int ticks);
	bool IsInCountdown(bool includeGo = false);
	void Blip(Color color = BLACK);
	bool TrackLapChanged(Track* track);
	int GetRaceTime(int finishLine);
	void TrackReady(Track* track);
	void Disqualify(Track* track);
};


