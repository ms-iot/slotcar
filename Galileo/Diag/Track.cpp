#include "Track.h"
#include <cpprest/basic_types.h>
#include <arduino.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int minimumTrackDuration = 1500; //at least 1500ms between line crosses

//const int positionalSensorsPerTrack = 6;
const int minimumRGBWait = 24;

const Color RED = Color(255, 0, 0);
const Color BLACK = Color(0, 0, 0);
const Color WHITE = Color(255, 255, 255);
const Color YELLOW = Color(255, 255, 0);
const Color GREEN = Color(0, 255, 0);

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

	colorSensor = new Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_2_4MS, TCS34725_GAIN_4X);
	colorSensor->setDelay(false);

	pinMode(colorSensorControlPin, LOW);

	//add sensor filtering (buffers or filters the values, preventing spikes or misreads)
	colorSensorFilter = SensorFilter(FILTERTYPE_RGB);
	colorSensorFilter.significantVarianceInValue = 8;
	colorSensorFilter.significantVarianceInMs = 250;
	colorSensorFilter.persistenceInMs = 3000;
}

void Track::Tick()
{
	int ticks = GetTickCount();
	CheckColorSensor();

	bool isAnythingTriggered = false;
	bool isAnythingOnNow = false;

	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		int value = positionalSensors[i].value;
		if (positionalSensors[i].IsTriggered())
		{
			Log("(%d): PIN: %d:%d\n", ticks, positionalSensors[i].pin, value);
			isAnythingTriggered = true;
		}

		isAnythingOnNow = isAnythingOnNow || value == 0;

		//Log("%d,", value);

		//jim:TESTING ECHO OF SENSOR TO LED
		//if (trackId == 2)
		//{
		//	//raceController->indicator.setDirectColor(Color(0, 255-(positionalSensors[i].value * 255), 0));
		//}
	}
	//Log("\n");

	/*if (isAnythingTriggered)
	{
		raceController->Blip(Color(0, 255, 0));
	}*/

	raceController->indicator.setDirectColor(isAnythingOnNow ? (trackId == 1 ? RED : GREEN) : BLACK);
}

rgbc Track::GetAdjustedRGBValue()
{
	return colorSensorFilter.currentAdjustedRGBValues;
}

void Track::CheckColorSensor()
{
	int ticks = GetTickCount();

	if (!usesColor) //|| ticks-lastReadRGB < minimumRGBWait
	{
		return;
	}

	pinMode(colorSensorControlPin, HIGH);
/*
	try
	{
		colorSensor->begin();
	}
	catch (...)
	{
		Log("Color not connected/working");
		exit(1);
	}*/

	rgbc rgb = GetRGB();
	pinMode(colorSensorControlPin, LOW);

	lastReadRGB = ticks;
	int after = GetTickCount();
	Log("sensor took %d\n", after - ticks);

	if (colorSensorFilter.HasChanged(rgb))
	{
		//log any changes to raw port
		char message[100];
		char* hex = toHex(colorSensorFilter.currentAdjustedRGBValues);
		sprintf(message, "{ \"track\": %d, \"color\": \"%s\" }", 2, hex);

		raceController->SendRaw(message);

		//Log("Color sensor 2:%s\n", toHex(rgb));
	}

	if (colorSensorFilter.IsSignificant(rgb, ticks))
	{
		raceController->ColorChanged(this);

		bool isRacing = raceController->IsRacing();

		if (raceController->IsRacing() && hasStarted)
		{
			raceController->Blip();

			if (ticks > lastPassedLine + minimumTrackDuration)
			{
				lap = lap + 1;
				lastPassedLine = ticks;

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

		bool isEmpty = colorSensorFilter.IsEmpty();

		if (isEmpty)
		{
			if (isRacing)
			{
				if (!hasStarted)
				{
					hasStarted = true;
				}
			}
			else if (raceController->IsInCountdown())
			{
				//disqualify!
				raceController->Disqualify(this);
			}
		}
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

void Track::StartRace(int ticks)
{
	lastPassedLine = ticks;
	lap = 0;
}

void Track::StopRace()
{
	hasStarted = false;
}

//Adafruit_TCS34725 (Color Sensor) specific methods
rgbc Track::GetRGB() {
	rgbc *values = new rgbc();
	colorSensor->getRawData(&values->r, &values->g, &values->b, &values->c);
	return *values;
}
