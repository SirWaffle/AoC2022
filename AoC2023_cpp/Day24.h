#pragma once

#include "CommonHeaders.h"

using namespace LANQ;

class Day24: public DayBase<2023, 24>
{
public:
	void Run()
	{
		DoPart1();
	}

	struct Hail
	{
		Vec3 pos;
		Vec3 vel;
	};

	void DoPart1()
	{
		//TODO: add chunk(x) to LANQ to simplify
		LVec<Hail> input = ReadWholeFile(GetSampleFileName().c_str()).Split("\n").Select<Hail>([](const LStr& line, int ind) {
			return Hail{
				Vec3(line.Remove(" ").Split("@").Get(0).Split(",").Select<int>([](const LStr& item, int ind) {return item.ToInt(); }).ToVector()),
				Vec3(line.Remove(" ").Split("@").Get(1).Split(",").Select<int>([](const LStr& item, int ind) {return item.ToInt(); }).ToVector())
				};
			});

		//auto grouped = input.GroupBy<int>([](const Hail& item) { return item.pos.z; });
	}

};