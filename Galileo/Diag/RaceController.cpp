//#include <cpprest/http_client.h>
//#include <cpprest/filestream.h>
//#define WIN32_LEAN_AND_MEAN

#include "RaceController.h"
#include "arduino.h"
#include "ColorRGB.h"
#include "Adafruit_TCS34725.h"
#include <cpprest/basic_types.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int ticksPerPreRaceStatus = 2000;
const int ticksToAllowRaceStartFromGo = 5000;

const Color RED = Color(255, 0, 0);
const Color BLACK = Color(0, 0, 0);
const Color WHITE = Color(255, 255, 255);
const Color YELLOW = Color(255, 255, 0);
const Color GREEN = Color(0, 255, 0);

const char* raceStatusNames[] = { "Prep Ready to Start", "Ready to Start",
	"Ready", "Set", "Go", "Racing", "Off-Track", "Final Lap", "Winner",
	"Show Winner", "Waiting" };

const int trackCount = 2;
const int raceLaps = 8;

RaceController::RaceController()
{
}

void RaceController::Initialize()
{
	raceStatus = WAITING;
	
	tracks = vector<Track>(trackCount);
	for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
	{
		tracks[trackIndex] = Track(this, trackIndex+1, trackIndex == 1, 0);	//trackIndex == 1 means only turn on color sensor for track #2 - used for testing
		tracks[trackIndex].Initialize();
	}

	//initialize components
	indicator = ColorRGB(11, 10, 9);

	int multicastTTL = 2;
	sock.setMulticastTTL(multicastTTL);
}

void RaceController::Tick()
{
	//check on sensors
	for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
	{
		tracks[trackIndex].Tick();
	}

	StatusCheck();
	
	//show status/indicators
	indicator.Tick();
}

bool RaceController::IsRacing()
{
	return raceStatus >= RACING && raceStatus <= WINNER;
}

bool RaceController::IsNewLapTime(int ticks)
{
	return raceStatus >= RACING;
}

void RaceController::Blip()
{
	indicator.Blip(25);
}

void RaceController::Blip(Color color)
{
	indicator.Blip(25, color);
}

bool RaceController::TrackLapChanged(Track* track)
{
	if (raceStatus == RACING && track->lap == raceLaps - 1)
	{
		this->raceStatus = FINAL_LAP;
	}

	if (raceStatus == FINAL_LAP && track->lap == raceLaps)
	{
		this->raceStatus = WINNER;
		carsFinished = carsFinished + 1;

		if (carsFinished == 1)
		{
			trackStatusId = track->trackId;
		}

		if (carsFinished == 1) //trackCount - jim:fix when more than 1 (color-sensor) track exists!!!! 
		{
			raceStatus = SHOW_WINNER;
		}
	}

	return track->lap == raceLaps;
}

int RaceController::GetRaceTime(int finishLine)
{
	return finishLine - ticksRaceStarted;
}

void RaceController::TrackReady(Track* track)
{
	if (tracksReady < trackCount)
	{
		tracksReady = tracksReady + 1;
		indicator.Blip(125, GREEN);
	}
}

bool RaceController::IsInCountdown()
{
	return raceStatus >= READY && raceStatus < GO;
}

void RaceController::Disqualify(Track* track)
{
	trackStatusId = track->trackId;
	raceStatus = DISQUALIFY;
}

void RaceController::StartRace(int ticks)
{
	ticksRaceStarted = ticks;

	for (int trackIndex = 0; trackIndex < trackCount; trackIndex++)
	{
		tracks[trackIndex].StartRace(ticks);
	}

	carsFinished = 0;
}

void RaceController::StatusCheck()
{
	int ticks = GetTickCount();
	
	if (raceStatus < RACING && ticks >= lastRaceStatusTicks + ticksPerPreRaceStatus)
	{
		raceStatus = static_cast<RaceStatus> (raceStatus + 1);

		if (raceStatus == RACING)
		{
			StartRace(ticks);
		}
	}

	bool statusChanged = raceStatus != lastRaceStatus;

	if (!statusChanged)
	{
		return;
	}

	lastRaceStatusTicks = ticks;
	lastRaceStatus = raceStatus;

	Log("Status=%s\n", (char*) raceStatusNames[raceStatus]);
	SendRace(0, "status", (char*) raceStatusNames[raceStatus]);

	// Set Indicator for racing status -----------------------------------------------------

	switch (raceStatus)
	{
	case PREP_READY_TO_START: 
		indicator.Flash(vector<Color>{ RED, BLACK }, 250);
		break;
	case READY_TO_START:
		indicator.Flash(vector<Color>{ RED, BLACK }, 250);
		break;
	case READY:
		indicator.SetColor(RED);
		break;
	case SET: 
		indicator.SetColor(YELLOW);
		break;
	case GO: 
		indicator.SetColor(GREEN);
		break;
	case RACING:
		break;
	case OFF_TRACK:
		indicator.Flash(vector<Color>{ YELLOW, BLACK }, 250);
		break;
	case FINAL_LAP:
		indicator.Flash(vector<Color>{ WHITE, BLACK }, 250);
		break;
	case WINNER:
		indicator.Flash(vector<Color>{ WHITE, BLACK }, 50);
		break;
	case SHOW_WINNER:
		indicator.Flash(vector<Color>{ GREEN, BLACK, (trackStatusId == 2 ? GREEN : BLACK), BLACK, BLACK, BLACK}, 125);
		break;
	case WAITING:
		indicator.SetColor(BLACK);
		break;
	case DISQUALIFY:
		indicator.Flash(vector<Color>{ RED, BLACK, (trackStatusId == 2 ? RED : BLACK), BLACK, BLACK, BLACK}, 125);
		break;
	default: 
		break;
	}
}

void RaceController::ColorChanged(Track* track)
{
	char* hex = toHex(track->GetAdjustedRGBValue());
	Log("Color sensor %d:significant:%s\n", track->trackId, hex);

	char message[100];
	sprintf(message, "{ \"track\": %d, \"color\": \"%s\" }", track->trackId, hex);

	SendRaw(message);
	Log(message);
	Log("\n");
}

//Communication / Logging
int RaceController::SendRaw(char *message)
{
	return SendUDP(message, 12345);
}

int RaceController::SendRace(int track, char *key, char *value)
{
	char trackMessage[14];
	if (track > 0)
	{
		sprintf(trackMessage, "\"track\": %d, ", track);
	}
	else
	{
		trackMessage[0] = 0;
	}

	char message[100];
	sprintf(message, "{ %s\"%s\": \"%s\" }", trackMessage, key, value);

	return SendUDP(message, 12346);
}

int RaceController::SendRace(int track, char* key, int value)
{
	char s[20];
	sprintf(s, "%d", value);
	return SendRace(track, key, s);
}

int RaceController::SendUDP(char *message, unsigned short port)
{
	string servAddress = "10.125.149.255";
	unsigned char multicastTTL = 2;

	try {
		sock.sendTo(message, strlen(message), servAddress, port);
	}
	catch (SocketException &e) {
		Log(e.what());
		return 1;
	}

	return 0;
}

const char *st2s(std::stringstream* stream, char *result) {
	std::string msg1 = stream->str();
	const char * result1 = msg1.c_str();
	for (int i = 0; i < sizeof(msg1); i++)
	{
		result[i] = result1[i];
	}

	return result;
}

char* toHex(rgbc values)
{
	std::stringstream stream;
	stream << "#";

	stream << (values.r<16 ? "0" : "") << std::hex << values.r;
	stream << (values.g<16 ? "0" : "") << std::hex << values.g;
	stream << (values.b<16 ? "0" : "") << std::hex << values.b;
	//stream << (values.c<16 ? "0" : "") << std::hex << values.c;

	char *result = new char[10];
	st2s(&stream, result);

	return result;
}