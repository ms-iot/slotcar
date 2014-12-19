class HallEffectSensor
{
	bool wasTriggered = false;
	bool isAnalog = false;

public:
	int pin;
	int value;

	HallEffectSensor();
	HallEffectSensor(int pin, bool isAnalog);

	void Initialize();
	bool IsTriggered();
};
