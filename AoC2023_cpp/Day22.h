#pragma once

#include "CommonHeaders.h"

using namespace LANQ;

class Day22
{
	const char* GetSampleFileName()
	{
		return "..\\Input\\AoC2023\\Day22_sample.txt";
	}

	const char* GetFileName()
	{
		return "..\\Input\\AoC2023\\Day22_1.txt";
	}

public:
	void Run()
	{
		//LANQTest();
		DoPart1();
		DoPart2();
	}

	struct Block
	{
		std::array<PointsNStuff::Vec3, 2> p;
		std::vector<Block*> restingOn; //things this one fell onto
		std::vector<PointsNStuff::Vec3> gridCoverage;

		void CalcCoverage()
		{
			//TODO: create a vec of points that this thing occupies
			PointsNStuff::Vec3 v = PointsNStuff::ComponentWiseDistance(p[0], p[1]);
			PointsNStuff::Vec3 dir = PointsNStuff::ClampToOnes(v);
			PointsNStuff::Vec3 start = p[0];
			while (start != p[1])
			{
				gridCoverage.push_back(start);
				start = start + dir;
			}

			if(p[0] != p[1])
				gridCoverage.push_back(start);
		}
	};

	struct Bounds
	{
		PointsNStuff::Vec3 min;
		PointsNStuff::Vec3 max;

		Bounds() :
			min(INT32_MAX, INT32_MAX, INT32_MAX)
		{}

		void CalcMin(const PointsNStuff::Vec3& v)
		{
			min.x = std::min(min.x, v.x);
			min.y = std::min(min.y, v.y);
			min.z = std::min(min.z, v.z);
		}

		void CalcMax(const PointsNStuff::Vec3& v)
		{
			max.x = std::max(max.x, v.x);
			max.y = std::max(max.y, v.y);
			max.z = std::max(max.z, v.z);
		}
	};

	void DoPart1()
	{
		Bounds bounds;
		auto blocks = ReadWholeFile(GetSampleFileName()).Split("\n").Select<Block>([&bounds](const LStr& item, int ind) {
				auto ps = item.Split("~").Select<PointsNStuff::Vec3>([](const LStr& item, int ind) {
					return PointsNStuff::Vec3(item.Split(",").Select<int>([](const LStr& item, int ind) { return item.ToInt(); }).ToVector());
				});
				Block b{ ps.Get(0), ps.Get(1) };
				b.CalcCoverage();
				bounds.CalcMin(b.p[0]);
				bounds.CalcMin(b.p[1]);
				bounds.CalcMax(b.p[0]);
				bounds.CalcMax(b.p[1]);
				return b;
			});

		//treating Z as height, x,y as coordinates into the map, create grid
		std::unordered_map<std::uint64_t, PointsNStuff::Vec3> heightMap;
		for (int x = bounds.min.x; x <= bounds.max.x; ++x)
		{
			for (int y = bounds.min.y; y <= bounds.max.y; ++y)
			{
				PointsNStuff::Vec3 v(x, y, 0);
				heightMap[v.HashVec2()] = v;
			}
		}

		//drop all blocks down to resting, using heightmap
		for (int i = 0; i < blocks.Size(); ++i)
		{
			for (auto& coverage : blocks[i].gridCoverage)
			{
				//TODO
			}
		}

	}

	void DoPart2()
	{

	}

	void LANQTest()
	{	
		LVec<LStr> input = ReadWholeFile(GetFileName()).Split("\n\n");

		std::vector<int> tv{ 1, 2, 3, 4, 5 };

		LVec<int> test(&tv);

		auto a = test.First();
		auto b = test.Last();
		auto c = test.Size();
		auto d = test.Get(1);

		auto e = test.Any([](const int& item) { return item == 2; });
		auto f = test.Select<std::string>([](const int& item, int ind) { return "farts"; });

		std::unordered_map<std::string, int> tm{ {"one", 1}, {"two", 2} };
		LUMap<std::string, int> testm(&tm);

		//auto b2 = testm.Last();
		auto c2 = testm.Size();
		//auto d2 = testm.Get(1);
		auto e2 = testm.ToVector();

		//auto e = test.Add(10);
		//auto f = test.Reverse();

		//doesnt work with lists due to [], need solution
		/*
		Enumerable<std::vector<int>> test(tv);

		auto a = test.First();
		auto b = test.Last();
		auto c = test.Size();
		auto d = test.Get(1);
		//auto e = test.Add(10);
		auto f = test.Reverse();
		*/

		BinaryVec tb;
		tb.SetBit(0, true);
		tb.ShiftLeft(1);
		tb.ShiftRight(1);
		auto bbb = tb.GetBit(0);
	}
};