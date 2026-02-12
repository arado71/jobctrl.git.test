#pragma once

typedef class ConfigClass {
public:
	int BackgroundColorPercent = 2;
	int SimilarColorDistanceP2 = 200;
	int FillAreaRatio = 60;
	int SizeMinPixels = 100;
	int StepPixels = 20;
	int DetColorStepPixels = 20;
	int AspectRatioLimit = 5;
	int IndividualColorsLimit = 50;
} _Configs;

extern _Configs Configs;