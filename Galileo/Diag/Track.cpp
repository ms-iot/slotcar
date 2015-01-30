#include "Track.h"
#include <arduino.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int minimumTrackDuration = 500; //at least 500ms between line crosses
const int timeToOfftrack = 3000;	  //time between positional sensors before a car is marked as 'off-track' (caution)

const int minimumRGBWait = 24;
const bool debugging = false;

const bool usesShield = true;	      //uses i2c multiplexor shield
#define SHIELD_ADDR_ON_ON_OFF (0x73)

Track::Track()
{
}

Track::Track(RaceController* raceController, int trackId, bool useColor, int trackPinStart, int positionalSensorsPerTrack)
{
	this->raceController = raceController;
	this->trackId = trackId;
	this->usesColor = useColor;
	this->trackPinStart = trackPinStart;
	this->positionalSensorsPerTrack = positionalSensorsPerTrack;
	this->colorSensorControlPin = colorSensorControlPin;
}

void Track::Initialize()
{
	StopRace();
	trackReady = false;

	//add positional sensors
	positionalSensors = std::vector<HallEffectSensor>(positionalSensorsPerTrack);
	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		int pin = trackPinStart + (trackId - 1) * positionalSensorsPerTrack + i;

		positionalSensors[i] = HallEffectSensor(pin, false);
		positionalSensors[i].Initialize();
	}

	if (!usesColor)
	{
		return;
	}

	Wire.begin();

	try
	{
		delay(10);
		colorSensor = new Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_24MS, TCS34725_GAIN_4X);
		delay(25);
	}
	catch (...)
	{
		Log("Color sensor on track %d not connected/working\n", trackId);
		exit(1);
	}

	//add sensor filtering (buffers or filters the values, preventing spikes or misreads)
	colorSensorFilter = SensorFilter(FILTERTYPE_RGB);
	colorSensorFilter.significantVarianceInValue = 10;
	colorSensorFilter.significantVarianceInMs = 250;
	colorSensorFilter.persistenceInMs = 3000;
}

void Track::Tick()
{
	int ticks = GetTickCount();
	CheckColorSensor();

	bool isAnythingTriggered = false;
	bool isAnythingOnNow = false;
	int positionTriggered = 0;

	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		int value = positionalSensors[i].value;
		if (positionalSensors[i].IsTriggered())
		{
			Log("(%d): PIN: %d:%d\n", ticks, positionalSensors[i].pin, value);
			isAnythingTriggered = true;
			positionTriggered = i;
		}

		isAnythingOnNow = isAnythingOnNow || value == 0;
	}

	if (isAnythingTriggered)
	{
		this->PositionChanged(positionTriggered);
		ticksLastTrigged = ticks;
	}

	if (debugging)
	{
		raceController->indicator->setDirectColor(isAnythingOnNow ? (trackId == 1 ? RED : GREEN) : BLACK);
	}

	isOfftrack = ticksLastTrigged > 0 && (ticks - ticksLastTrigged) > timeToOfftrack;
}

rgbc Track::GetAdjustedRGBValue()
{
	return colorSensorFilter.currentAdjustedRGBValues;
}

const bool INCLUDE_GO = true;

void Track::CheckColorSensor()
{
	int ticks = GetTickCount();

	//color sensor takes too much time away from sensing positional sensors, at least on a single thread - if racing (or not using), ignore color sensors.
	if (!usesColor || raceController->IsRacing() || raceController->IsInCountdown(INCLUDE_GO))
	{
		return;
	}

	//Log("1");
	if (usesShield) 
	{
		Wire.beginTransmission(SHIELD_ADDR_ON_ON_OFF);
		Wire.write(1 << trackId - 1);
		Wire.endTransmission();
		delay(500);
	}

	//Log("2");
	try
	{
		colorSensor->begin();
	}
	catch (...)
	{
		Log("Color sensor on track %d not connected/working\n", trackId);
		//exit(1);
	}

	//Log("3");
	delay(100); //CRITICAL delay required - otherwise writing exception occurs

	//Log("4");
	rgbc rgb;
	try
	{
		rgb = GetRGB();
	}
	catch (...)
	{
		Log("GetRGB failed - ignoring");
		return;
	}
	//Log("5");

	lastReadRGB = ticks;
	int after = GetTickCount();

	if (colorSensorFilter.IsSignificant(rgb, ticks))
	{
		raceController->ColorChanged(this);
	}

	//Log("6");

	if (colorSensorFilter.IsPersistent(ticks))
	{
		if (!colorSensorFilter.IsEmpty())
		{
			// Race hasn't started yet
			if (raceController->IsRacingOrPostRace(ticks))
			{
				// Car waiting to start
				raceController->raceStatus = PREP_READY_TO_START;
			}
		}
		else if (raceController->raceStatus == WAITING)
		{
			if (!trackReady)
			{
				trackReady = true;
				raceController->TrackReady(this);
			}
		}
	}

	//Log("7");
}

// track recorded a new position sensed
void Track::PositionChanged(int position) {
	raceController->Blip();
	int ticks = GetTickCount();

	//if at beginning of track (1st sensor)
	if (position == 0)
	{
		if (raceController->IsInCountdown() && !hasStarted)
		{
			//oops, still counting down!
			raceController->Disqualify(this);
		}
		else if (!hasStarted)
		{
			//ignore remaining
		}
		else if (!crossedStartingLine)
		{
			crossedStartingLine = true;
			ticksCrossedStartingLine = ticks;
			lastPassedLine = ticks;
			lap = 0;
		}
		else if (ticks > lastPassedLine + minimumTrackDuration)
		{
			lastPassedLine = ticks;
			lap = lap + 1;

			Log("lap=%d", lap);

			bool isFinishLine = raceController->TrackLapChanged(this);
			if (isFinishLine) 
			{
				ticksFinishLine = ticks;
				raceController->SendRace(trackId, "finished", raceController->GetRaceTime(ticksFinishLine));
				StopRace();
			}
		}
	}

	raceController->SendRace(trackId,const_cast<char*>(raceController->IsRacing() ? "position" : "position"), position);
}

void Track::StartRace(int ticks)
{
	hasStarted = true;
	crossedStartingLine = false;
	ticksLastTrigged = 0;
}

void Track::StopRace()
{
	hasStarted = false;
	crossedStartingLine = false;
	ticksLastTrigged = 0;
}

//Adafruit_TCS34725 (Color Sensor) specific methods
rgbc Track::GetRGB() {
	rgbc *values = new rgbc();
	colorSensor->getRawData(&values->r, &values->g, &values->b, &values->c);
	return *values;
}
