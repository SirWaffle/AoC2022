#pragma once

#include "CommonHeaders.h"

using namespace LANQ;

class Day21
{
	const char* GetSampleFileName()
	{
		return "..\\Input\\AoC2023\\Day21_sample.txt";
	}

	const char* GetFileName()
	{
		return "..\\Input\\AoC2023\\Day21_1.txt";
	}

public:
	void Run()
	{
		DoPart1();
		DoPart2();
	}


	struct Point
	{
		PointsNStuff::Vec2 pos;
		char c;

		std::uint64_t Hash() const { return (pos.y * 100000 + pos.x); }
	};

	struct Step
	{
		PointsNStuff::Vec2 pos;
		int stepNum = 0;

		Step() = default;
		Step(const Point& p, int step) :pos(p.pos), stepNum(step) { }

		std::uint64_t Hash() const { return (pos.y * 100000 + pos.x); }
	};


	void DoPart1()
	{
		LVec<LVec<Point>> input = ReadWholeFile(GetFileName()).Split("\n").Select<LVec<Point>>( [](const LStr& str, int yind) {
				return str.ToLVec().Select<Point>([yind](const LChar& c, int xind) {
						return Point{ { xind, yind}, c.Value() };
					} );
			} );
		
		auto map = input.Flatten<LVec<Point>>().ToUnorderedMap<std::uint64_t>([](const Point& p) { return p.Hash(); });


		Point startPoint = input.Flatten<LVec<Point>>().Where([](const Point& p, int i) { return p.c == 'S'; }).First();
		std::list<Step> queuedSteps{ Step(startPoint, 0) };

		std::unordered_map<std::uint64_t, Step> steps;

		const int totalSteps = 64 + 1; //64

		for (int stepCount = 0; stepCount < totalSteps; ++stepCount)
		{
			std::cout << "Step: " << stepCount << "  Step points: " << steps.size() << "  queued: " << queuedSteps.size() << "\n";

			while (queuedSteps.size() > 0)
			{
				Step& p = queuedSteps.front();
				p.stepNum = stepCount;
				steps[p.Hash()] = p;
				queuedSteps.pop_front();
			}

			for (auto const& pair: steps) 
			{
				if (pair.second.stepNum < stepCount)
					continue;

				std::array<Step, 4> s{ { pair.second, pair.second, pair.second, pair.second } };
				s[0].pos.x += 1;
				s[1].pos.x -= 1;
				s[2].pos.y += 1;
				s[3].pos.y -= 1;

				for (int i = 0; i < 4; ++i)
				{
					if ((*map).contains(s[i].Hash())) //its on the board. can just do a range check here for speed
					{
						if ((*map)[s[i].Hash()].c != '#') //is valid to step on
						{
							queuedSteps.push_back(s[i]);
						}
					}
				}
			}

		}

		//count total uniquely positioned steps
		std::unordered_set<std::uint64_t> set;
		for (const auto& pair: steps)
		{
			if(pair.second.stepNum == totalSteps - 1)
				set.insert(pair.second.Hash());
		}
		
		std::cout << "unique step locations: " << set.size() << "\n";
		//p1 3809
	}

	void DoPart2()
	{
	
	}
};