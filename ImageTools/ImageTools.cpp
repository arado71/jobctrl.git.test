#include "pch.h"
#include "ImageTools.h"
#include "CoveredArea.h"
#include <iostream>
#include <exception>
#include <fstream>
#include <unordered_map>

using namespace std;

typedef union {
	int color;
	unsigned char P[4];
} ARGB;

int Pow2(int v)
{
	return v * v;
}

bool IsSameColor(int a, int b)
{
	ARGB A, B;
	A.color = a;
	B.color = b;
	int dist = Pow2((int)A.P[0] - (int)B.P[0]) + Pow2((int)A.P[1] - (int)B.P[1]) + Pow2((int)A.P[2] - (int)B.P[2]) + Pow2((int)A.P[3] - (int)B.P[3]);
	return dist < Configs.SimilarColorDistanceP2;
}

bool IsSimilarToBackcolor(int sample, int* colors, int colorsLen) {
	for (int i = 0; i < colorsLen; i++)
		if (IsSameColor(colors[i], sample)) return true;

	return false;
}

typedef class QueueItem {
public:
	int X, Y;

	QueueItem(int x, int y) {
		X = x;
		Y = y;
	}
} QueueItem;

template<class T> class Queue {
	T **items;
	int max_count, count, posIn, posOut;
public:
	Queue(): count(0), posIn(0), posOut(0)
	{
		max_count = 100;
		items = new T*[max_count];
	}

	~Queue()
	{
		if (posOut <= posIn)
			for (int i = posOut; i < posIn; i++)
				delete items[i];
		else
			for (int i = 0; i < max_count; i++)
				if (i < posIn || i >= posOut)
					delete items[i];
		delete items;
	}

	void Enqueue(T *item) {
		if (count >= max_count) {
			T **newItems = new T*[max_count * 2];
			int j = 0;
			for (int i = posOut; i < max_count; i++)
				newItems[j++] = items[i];
			for (int i = 0; i < posIn; i++)
				newItems[j++] = items[i];

			delete items;
			items = newItems;
			max_count *= 2;
			posOut = 0;
			posIn = j;
		}
		items[posIn++] = item;
		count++;
		if (posIn >= max_count) 
			posIn = 0;
	}

	T* Dequeue() {
		if (count <= 0) return NULL;
		T *value = items[posOut++];
		count--;
		if (posOut >= max_count) posOut = 0;
		return value;
	}

	int Count() {
		return count;
	}
};

int Min(int a, int b)
{
	return a < b ? a : b;
}

int Max(int a, int b)
{
	return a > b ? a : b;
}

Rectangle1 *WalkAroundBlob(int x, int y, int *addr, int rowLength, int width, int height, int* backColors, int backColorsLen, CoveredArea& coveredArea) {
	int x0 = x;
	int x1 = x;
	int y0 = y;
	int y1 = y;
	int detectedPixels = 0;
	Queue<QueueItem> backQueue;
	backQueue.Enqueue(new QueueItem(x, y));
	int indColors[500];
	int indColorsLen = 0;

	while (backQueue.Count() > 0)
	{
		QueueItem *item = backQueue.Dequeue();
		x = item->X;
		y = item->Y;
		if (coveredArea.Contains(x, y)) { 
			delete item;
			continue; 
		}
		int *rowAddr = addr + y * rowLength;
		if (y > border_size && !coveredArea.Contains(x, y - 1))
			while (y > border_size && !IsSimilarToBackcolor(rowAddr[x - rowLength], backColors, backColorsLen))
			{
				rowAddr -= rowLength;
				y--;
			}
		while (x > border_size && !IsSimilarToBackcolor(rowAddr[x - 1], backColors, backColorsLen)) x--;
		Rectangle1 rect(x, y, item->X - x + 1, 1);
		if (x < x0) x0 = x;
		if (y < y0) y0 = y;
		if (y > y1) y1 = y;
		bool isDownFilled = true;
		while (x < width - border_size - 1 && !IsSimilarToBackcolor(rowAddr[x + 1], backColors, backColorsLen))
		{
			//OnProcessing ? .Invoke(this, (x, y));
			detectedPixels++;
			x++;
			if (y < height - border_size - 1 && !IsSimilarToBackcolor(rowAddr[x + rowLength], backColors, backColorsLen))
			{
				if (isDownFilled)
				{
					isDownFilled = false;
					backQueue.Enqueue(new QueueItem(x, y + 1));
				}
			}
			else isDownFilled = true;
		}
		if (x > x1) x1 = x;
		rect.Width = x - rect.Left + 1;
		coveredArea.Add(rect.Left, x, y);
		delete item;
	}

	Rectangle1 *rectangle = new Rectangle1(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
	for (y = y0; y <= y1; y++)
	{
		int *rowAddr = addr + y * rowLength + x0;
		for (x = x0; x <= x1; x++)
		{
			int color = *rowAddr++;
			bool found = false;
			for (int i = 0; i < indColorsLen; i++)
				if (IsSameColor(color, indColors[i]))
				{
					found = true;
					break;
				}
			if (!found)
			{
				if (indColorsLen >= Configs.IndividualColorsLimit)
					break;
				indColors[indColorsLen++] = color;
			}
		}
		if (static_cast<unsigned>(indColorsLen) >= sizeof indColors / sizeof(int) || indColorsLen >= Configs.IndividualColorsLimit)
			break;
	}
	if (detectedPixels * 100 / (rectangle->Width * rectangle->Height) < Configs.FillAreaRatio
		|| rectangle->Width < Configs.SizeMinPixels || rectangle->Height < Configs.SizeMinPixels
		|| Max(rectangle->Height, rectangle->Width) / Min(rectangle->Height, rectangle->Width) > Configs.AspectRatioLimit
		|| indColorsLen < Configs.IndividualColorsLimit) {
		delete rectangle;
		return nullptr;
	}

	return rectangle;
}

EXTERN_DLL_EXPORT int DetectDominantColors(int *addr, int stride, int width, int height, int *result)
{
	unordered_map<int, int> colorMap;
	int *ptr = addr + border_size * stride;
	int pixels = 0;
	for (int y = border_size; y < height - border_size; y++) {
		int *row = ptr + border_size;
		for (int x = border_size; x < width - border_size; x += Configs.DetColorStepPixels)
		{
			int cl = *row;
			row += Configs.DetColorStepPixels;
			auto search = colorMap.find(cl);
			if (colorMap.end() != search)
				search->second++;
			else
				colorMap.insert(pair<int, int>(cl, 1));
			pixels++;
		}
		ptr += stride;
	}

	const int limit = pixels * Configs.BackgroundColorPercent / 100;
	int i = 0;
	for (const auto& it : colorMap) {
		if (it.second > limit) result[i++] = it.first;
		if (i >= 100) break;
	}
	return i;

}

EXTERN_DLL_EXPORT int DetectEmbeds(int *addr, int stride, int width, int height, int* colors, int colorsLen, bool stopAtFirst, Rectangle1 *result) {
	CoveredArea coveredArea(height);
	int embeds_count = 0;

	for (int y = border_size; y < height - border_size; y += Configs.StepPixels) {
		//int* row = addr + y * stride;
		for (int x = border_size; x < width - border_size; x += Configs.StepPixels) {

			Rectangle1* found = nullptr;
			for (int i = 0; i < embeds_count; i++) {
				if (result[i].Contains(x, y)) {
					found = &result[i];
					break;
				}
			}
			if (found != nullptr) {
				x = found->Left + found->Width;
				continue;
			}

			if (coveredArea.Contains(x, y)) continue;

			int pixelColor = addr[y * stride + x];
			if (IsSimilarToBackcolor(pixelColor, colors, colorsLen)) continue;

			Rectangle1 *blob = WalkAroundBlob(x, y, addr, stride, width, height, colors, colorsLen, coveredArea);

			if (blob != nullptr) {
				result[embeds_count++] = *blob;
				delete blob;
				if (stopAtFirst)
					return 1;
			}
		}
	}
	return embeds_count;
}

EXTERN_DLL_EXPORT void ReplaceColors(int *srcScan, int *resScan, int stride, int width, int height, int *replaceColors, int count)
{
	int *ptrSrc = srcScan;
	int *ptrRes = resScan;
	int pixels = 0;
	int *mappedColors = replaceColors + count;
	for (int y = 0; y < height; y++) {
		int *rowSrc = ptrSrc;
		int *rowRes = ptrRes;
		for (int x = 0; x < width; x++)
		{
			int pixelColor = *rowSrc;
			for (int i = 0; i < count; i++)
			{
				if (replaceColors[i] == pixelColor || IsSameColor(pixelColor, replaceColors[i]))
				{
					pixelColor = mappedColors[i];
					break;
				}
			}

			*rowRes = pixelColor;
			rowSrc++;
			rowRes++;
		}
		ptrSrc += stride;
		ptrRes += stride;
	}
}

EXTERN_DLL_EXPORT void SetConfigs(_Configs *configs)
{
	Configs = *configs;
}