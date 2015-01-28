#include "Track.h"
#include <arduino.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int minimumTrackDuration = 500; //at least 500ms between line crosses
const int timeToOfftrack = 3000;

const int minimumRGBWait = 24;
const bool debugging = false;

const bool usesShield = true;
#define SHIELD_ADDR_ON_ON_OFF (0x73)

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
	positionalSensors = std::vector<HallEffectSensor>(positionalSensorsPerTrack);
	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		//special D6->D8 logic in order to use D6(PWM) elsewhere
		int pin = trackPinStart + (trackId - 1) * positionalSensorsPerTrack + i;
		/*if (pin == D6)
		{
			pin = D8;
		}*/

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

	//colorSensor->setDelay(false);

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

	if (!usesColor || raceController->IsRacing() || raceController->IsInCountdown(INCLUDE_GO)) //|| ticks-lastReadRGB < minimumRGBWait
	{
		return;
	}

	if (usesShield) 
	{
		Wire.beginTransmission(SHIELD_ADDR_ON_ON_OFF);
		Wire.write(1 << trackId - 1);
		Wire.endTransmission();
	}

	try
	{
		colorSensor->begin();
	}
	catch (...)
	{
		Log("Color sensor on track %d not connected/working\n", trackId);
		exit(1);
	}

	delay(100); //CRITICAL delay required - otherwise writing exception occurs

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

	if (!worked)
	{
		return;
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

	raceController->SendRace(trackId,const_cast<char*>(raceController->IsRacing() ? "position" : "pin"), position);
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
