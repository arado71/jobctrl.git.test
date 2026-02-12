#pragma once

#include "configs.h"

typedef struct Rectangle1 {
public:
	int Left, Top, Width, Height;
	Rectangle1() {}
	Rectangle1(int left, int top, int width, int height) {
		Left = left;
		Top = top;
		Width = width;
		Height = height;
	}
	bool Contains(int x, int y) {
		return x >= Left && x < Left + Width && y >= Top && y < Top + Height;
	}
} Rectangle1;

constexpr int border_size = 6;

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)
EXTERN_DLL_EXPORT int DetectEmbeds(int *addr, int stride, int width, int height, int* colors, int colorsLen, bool stopAtFirst, Rectangle1 *result);
EXTERN_DLL_EXPORT int DetectDominantColors(int *addr, int stride, int width, int height, int *result);
EXTERN_DLL_EXPORT void ReplaceColors(int *srcScan, int *resScan, int v, int width, int height, int *replaceColors, int count);
EXTERN_DLL_EXPORT void SetConfigs(_Configs *configs);