
#include "SensorFilter.h"

#include <windows.h>

const rgbc ADJUSTED_BLACK = rgbc(0, 0, 0);

SensorFilter::SensorFilter()
{
}

void SensorFilter::balance(rgbc* rgbvalues, rgbc* black)
{
	black->r -= (black->r - rgbvalues->r) / 10;
	black->g -= (black->g - rgbvalues->g) / 10;
	black->b -= (black->b - rgbvalues->b) / 10;
	black->c -= (black->c - rgbvalues->c) / 10;
}

rgbc SensorFilter::subtract(rgbc fromrgb, rgbc torgb) {
	rgbc result = rgbc();
	result.r = fromrgb.r - torgb.r;
	result.g = fromrgb.g - torgb.g;
	result.b = fromrgb.b - torgb.b;
	result.c = fromrgb.c - torgb.c;
	return result;
}

void SensorFilter::flatten(rgbc* values)
{
	if (values->r > 255) {
		values->r = 0;
	};

	if (values->g > 255) {
		values->g = 0;
	}

	if (values->b > 255) {
		values->b = 0;
	}

	if (values->c > 255) {
		values->c = 0;
	}

	values->r = max(values->r, 0);
	values->g = max(values->g, 0);
	values->b = max(values->b, 0);
	values->c = max(values->c, 0);
}

bool SensorFilter::isSignificantChangeFrom(rgbc adjustedValues)
{
	if (filterType == FILTERTYPE_RGB)
	{
		int diff = abs(adjustedValues.r - currentAdjustedRGBValues.r) + abs(adjustedValues.g - currentAdjustedRGBValues.g) + abs(adjustedValues.b - currentAdjustedRGBValues.b); // +abs(adjustedValues.c - lastRGB.c);
		return diff >= significantVarianceInValue;
	}

	return false;
}

bool SensorFilter::IsSignificant(rgbc rgbvalues, int ticks)
{
	bool isSignificant = true;

	if (filterType == FILTERTYPE_RGB)
	{
		rgbc adjustedRGB = subtract(rgbvalues, black);
		flatten(&adjustedRGB);

		isSignificant = isSignificantChangeFrom(adjustedRGB);
		
		this->currentAdjustedRGBValues = adjustedRGB;
		this->currentRGBValues = rgbvalues;

		if (isSignificant)
		{
			ticksSinceAdjustedRGBValue = ticks;
		}
	}  

	return isSignificant;
}

bool SensorFilter::HasChanged(rgbc rgbc)
{
	bool hasChanged =
		rgbc.r != this->currentRGBValues.r ||
		rgbc.g != this->currentRGBValues.g ||
		rgbc.b != this->currentRGBValues.b;

	this->currentRGBValues = rgbc;
	return hasChanged;
}

bool SensorFilter::IsPersistent(int ticks)
{
	bool isPersistent = ticks > ticksSinceAdjustedRGBValue + persistenceInMs;
	
	// on startup, preserve persistent black/empty in order to subtract
	if (!isBlackSet && isPersistent)
	{
		isPersistent = false;
		isBlackSet = true;
		black = this->currentAdjustedRGBValues;
	}

	return isPersistent;
}

bool SensorFilter::IsEqual(rgbc set1, rgbc set2)
{
	return set1.r == set2.r && set1.g == set2.g && set1.b == set2.b;
}

bool SensorFilter::IsEmpty()
{
	return !isBlackSet || !isSignificantChangeFrom(ADJUSTED_BLACK);
}