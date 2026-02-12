#pragma once

typedef class CoveredItem  {
public:
	int x1, x2;
	CoveredItem() {};
	CoveredItem(int _x1, int _x2) {
		x1 = _x1;
		x2 = _x2;
	}
	CoveredItem(CoveredItem& other) {
		x1 = other.x1;
		x2 = other.x2;
	}
} CoveredItem;

typedef class CoveredRow {
	CoveredItem* items;
	int count, max_count;
public:
	CoveredRow() {
		max_count = 100;
		items = new CoveredItem[max_count];
		count = 0;
	}

	~CoveredRow() {
		delete items;
	}

	void Add(int x1, int x2) {
		if (count >= max_count) {
			CoveredItem *newItems = new CoveredItem[max_count * 2];
			for (int i = 0; i < max_count; i++) newItems[i] = items[i];
			delete items;
			items = newItems;
			max_count *= 2;
		}
		items[count++] = CoveredItem(x1, x2);
	}

	bool Contains(int x) {
		for (int i = 0; i < count; i++)
			if (items[i].x1 <= x && x <= items[i].x2) return true;
		return false;
	}
} CoveredRow;

typedef class CoveredArea {
	CoveredRow** rows;
	int height;
public:
	CoveredArea(int height) {
		this->height = height;
		rows = new CoveredRow*[height];
		for (int i = 0; i < height; i++)
			rows[i] = nullptr;
	}

	~CoveredArea()
	{
		for (int i = 0; i < height; i++)
			if (rows[i] != nullptr)
				delete rows[i];
		delete rows;
	}

	void Add(int x1, int x2, int y) {
		if (rows[y] == nullptr) {
			rows[y] = new CoveredRow();
		}
		rows[y]->Add(x1, x2);
	}

	bool Contains(int x, int y) {
		if (rows[y] == nullptr) return false;
		return rows[y]->Contains(x);
	}
} CoveredArea;

