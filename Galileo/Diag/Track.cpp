#include "Track.h"
#include <cpprest/basic_types.h>
#include <arduino.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int minimumTrackDuration = 500; //at least 500ms between line crosses
const int timeToOfftrack = 3000;

const int minimumRGBWait = 24;
const bool debugging = false;

Track::Track()
{
}

Track::Track(RaceController* raceController, int trackId, bool useColor, int trackPinStart, int positionalSensorsPerTrack, int colorSensorControlPin)
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

	//add positionals
	positionalSensors = vector<HallEffectSensor>(positionalSensorsPerTrack);
	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		positionalSensors[i] = HallEffectSensor(trackPinStart + (trackId-1) * positionalSensorsPerTrack + i, false);
		positionalSensors[i].Initialize();
	}

	if (!usesColor)
	{
		return;
	}

	pinMode(colorSensorControlPin, OUTPUT);
	pinMode(colorSensorControlPin, HIGH);

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

	//colorSensor->setDelay(false);

	pinMode(colorSensorControlPin, LOW);

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
		raceController->indicator.setDirectColor(isAnythingOnNow ? (trackId == 1 ? RED : GREEN) : BLACK);
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

	if (!usesColor || raceController->IsRacing() || raceController->IsInCountdown(INCLUDE_GO)) //|| ticks-lastReadRGB < minimumRGBWait
	{
		return;
	}

	pinMode(colorSensorControlPin, HIGH);

	try
	{
		colorSensor->begin();
	}
	catch (...)
	{
		Log("Color sensor on track %d not connected/working\n", trackId);
		exit(1);
	}

	delay(35); //CRITICAL delay required - otherwise writing exception occurs

	bool worked = true;
	rgbc rgb;
	try
	{
		rgb = GetRGB();
	}
	catch (...)
	{
		worked = false;
		//unknown issue
		Log("GetRGB failed - ignoring");
	}

	pinMode(colorSensorControlPin, LOW);

	if (!worked)
	{
		return;
	}

	if (debugging)
	{
		char message[100];
		char* hex = toHex(colorSensorFilter.currentAdjustedRGBValues);
		sprintf(message, "{ \"track\": %d, \"color\": \"%s\" }", 2, hex);

		Log("Color sensor %d:%s\n", trackId, toHex(rgb));
	}

	lastReadRGB = ticks;
	int after = GetTickCount();

	if (colorSensorFilter.IsSignificant(rgb, ticks))
	{
		raceController->ColorChanged(this);
	}

	if (colorSensorFilter.IsPersistent(ticks))
	{
		if (!colorSensorFilter.IsEmpty())
		{
			// Race hasn't started yet
			if (raceController->IsNewLapTime(ticks))
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
}

void Track::PositionChanged(int position) {
	raceController->Blip();
	int ticks = GetTickCount();

	if (position == 0)
	{
		if (raceController->IsInCountdown() && !hasStarted)
		{
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

	if (raceController->IsRacing())
	{
		raceController->SendRace(trackId, "position", position);
	}
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
