#pragma once

#include "CommonHeaders.h"

using namespace LANQ;
using namespace PointsNStuff;

class Day22 : public DayBase<2023, 22>
{
public:
	void Run()
	{
		DoPart1();
	}

	struct Block
	{
		std::array<Vec3, 2> p;
		std::vector<Block*> restingOn; //things this one fell onto
		std::vector<Vec3> gridCoverage;
		Bounds bbox;

		void CalcCoverage()
		{
			//TODO: create a vec of points that this thing occupies
			Vec3 v = ComponentWiseDistance(p[0], p[1]);
			Vec3 dir = ClampToOnes(v);
			Vec3 start = p[0];
			while (start != p[1])
			{
				gridCoverage.push_back(start);
				start = start + dir;
			}

			if(p[0] != p[1])
				gridCoverage.push_back(start);

			bbox.CalcMinMax(p[0]);
			bbox.CalcMinMax(p[1]);
		}
	};



	void DoPart1()
	{
		Bounds bounds;
		auto blocks = ReadWholeFile(GetSampleFileName().c_str()).Split("\n").Select<Block>([&bounds](const LStr& item, int ind) {
				auto ps = item.Split("~").Select<Vec3>([](const LStr& item, int ind) {
					return Vec3(item.Split(",").Select<int>([](const LStr& item, int ind) { return item.ToInt(); }).ToVector());
				});
				Block b{ ps.Get(0), ps.Get(1) };
				b.CalcCoverage();
				bounds.CalcMinMax(b.p[0]);
				bounds.CalcMinMax(b.p[1]);
				return b;
			});

		//treating Z as height, x,y as coordinates into the map, create grid
		std::unordered_map<std::uint64_t, Vec3> heightMap;
		for (int x = bounds.min.x; x <= bounds.max.x; ++x)
		{
			for (int y = bounds.min.y; y <= bounds.max.y; ++y)
			{
				Vec3 v(x, y, 0);
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
};