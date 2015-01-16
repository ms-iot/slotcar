
#include "sensorModels.h"

#include "limits.h"

enum FilterType
{
	FILTERTYPE_RGB
};

class SensorFilter
{
	void balance(rgbc* rgbvalues, rgbc* black);
	rgbc subtract(rgbc fromrgb, rgbc torgb);
public:
	void* source;
	FilterType filterType;

	explicit SensorFilter(FilterType filter_type)
		: filterType(filter_type)
	{
	}

	int significantVarianceInValue = 0;
	int significantVarianceInMs = INT_MAX;
	rgbc black = rgbc(0, 0, 0);
	bool isBlackSet = false;
	rgbc currentRGBValues, currentAdjustedRGBValues;
	int persistenceInMs, ticksSinceAdjustedRGBValue;

	SensorFilter();
	void flatten(rgbc* adjusted_rgb);
	bool isSignificantChangeFrom(rgbc values);
	bool IsSignificant(rgbc rgbvalues, int ticks);
	bool HasChanged(rgbc rgbc);
	bool IsPersistent(int ticks);
	bool IsEqual(rgbc set1, rgbc set2);
	bool IsEmpty();
};
