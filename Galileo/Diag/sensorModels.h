

#include <cinttypes>

typedef struct rgbc {
	rgbc(uint16_t r, uint16_t g, uint16_t b)
		: r(r),
		g(g),
		b(b)
	{
	}

	rgbc() {}

	uint16_t r;
	uint16_t g;
	uint16_t b;
	uint16_t c;
} rgbcp;

