#include "Track.h"
#include <cpprest/basic_types.h>

char* toHex(rgbc values);
const char *st2s(std::stringstream* stream, char *result);

const int minimumTrackDuration = 1500; //at least 1500ms between line crosses

const int positionalSensorsPerTrack = 1;

Track::Track()
{
}

Track::Track(RaceController* raceController, int trackId, bool useColor, int trackPinStart)
{
	this->raceController = raceController;
	this->trackId = trackId;
	this->usesColor = useColor;
	this->trackPinStart = trackPinStart;
}

void Track::Initialize()
{
	StopRace();
	trackReady = false;

	//add positionals
	positionalSensors = vector<HallEffectSensor>(positionalSensorsPerTrack);
	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		positionalSensors[i] = HallEffectSensor(trackPinStart + i, false);
		positionalSensors[i].Initialize();
	}

	if (!usesColor)
	{
		return;
	}

	colorSensor = new Adafruit_TCS34725(TCS34725_INTEGRATIONTIME_24MS, TCS34725_GAIN_4X);

	try
	{
		colorSensor->begin();
	}
	catch (...)
	{
		Log("Color not connected/working");
		exit(1);
	}

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

	for (int i = 0; i < positionalSensorsPerTrack; i++)
	{
		if (positionalSensors[i].IsTriggered())
		{
			Log("(%d): PIN: %d:%d\n", ticks, i + trackPinStart, positionalSensors[i].value);
			raceController->Blip(Color(0, 255, 0));
		}

		//hack
		if (trackId == 2)
		{
			//raceController->indicator.setDirectColor(Color(0, 255-(positionalSensors[i].value * 255), 0));
		}
	}
}

rgbc Track::GetAdjustedRGBValue()
{
	return colorSensorFilter.currentAdjustedRGBValues;
}

void Track::CheckColorSensor()
{
	if (!usesColor)
	{
		return;
	}

	rgbc rgb = GetRGB();
	int ticks = GetTickCount();

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
