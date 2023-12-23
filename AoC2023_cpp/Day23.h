#pragma once

#include "CommonHeaders.h"

using namespace LANQ;
using namespace PointsNStuff;

class Day23: public DayBase<2023, 23>
{
public:
	void Run() override
	{
		//Execute(false); //part 1
		Execute(true); //part 2
	}

	struct Spot
	{
		LChar gridChar;
		Vec3 pos;

		Spot() = default;
		Spot(char c, Vec3 v) : gridChar(c), pos(v) {}
	};

	struct Path
	{
		using Ptr = std::shared_ptr<Path>;
		//using SeenType = std::unordered_map<std::uint64_t, Spot>;
		using SeenType = std::unordered_set<std::uint64_t>;

		SeenType seen;
		Vec3 curPos;

		void AddSeen(const Spot& s)
		{
			//map
			//seen[s.pos.HashVec2()] = s;
			seen.insert(s.pos.HashVec2());
		}

		Path() = delete;
		Path(const Vec3& p) : curPos(p) {}
		Path(const Vec3& p, const SeenType& _seen) : curPos(p), seen(_seen) {}
	};

	void Execute(bool ignoreSlopes = false)
	{
		Bounds bounds;

		std::shared_ptr<std::unordered_map<std::uint64_t, Spot>> map = ReadWholeFile(GetFileName().c_str()).Split("\n").Select<LVec<Spot>>([](const LStr& item, int yind) {
			return item.ToLVec().Select<Spot>([yind](const LChar& c, int xind) { return Spot{ c, Vec3(xind, yind) }; });
			}).Flatten<LVec<Spot>>().ToSharedUnorderedMap<std::uint64_t>([](const Spot& item) { return item.pos.HashVec2(); });
			
		for (const auto& p : *map)
			bounds.CalcMinMax(p.second.pos);

		Vec3 start = LANQit(map).Where([bounds](auto const& p, int ind) { return p.second.pos.y == bounds.min.y; })
			.Where([bounds](auto const& p, int ind) { return p.second.gridChar.Value() == '.'; }).First().second.pos;

		Vec3 target = LANQit(map).Where([bounds](auto const& p, int ind) { return p.second.pos.y == bounds.max.y; })
			.Where([bounds](auto const& p, int ind) { return p.second.gridChar.Value() == '.'; }).First().second.pos;

		Path::Ptr best;
		std::vector<Path::Ptr> solved;
		std::vector<Path::Ptr> searching{ Path::Ptr( new Path(start)) };

		WriteLine(std::format("Bounds: ({},{}) - ({},{})", bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));

		//done lazy with lots of set copies and checking after its been added to search list
		int logCount = 0;
		while (searching.size() > 0)
		{
			if (++logCount == 50000)
			{
				logCount = 0;
				WriteLine(std::format("Working... Search size: {}", searching.size()));
			}

			Path::Ptr curPath = searching.back();
			searching.pop_back();
			
			if (!bounds.InclusiveInBounds(curPath->curPos))
				continue; //out of bounds
			
			if (curPath->seen.contains(curPath->curPos.HashVec2()))
				continue; //visited the same spot twice

			Spot curSpot = (*map)[curPath->curPos.HashVec2()];
			const char gridChar = curSpot.gridChar;
			
			if (gridChar == '#')
				continue; //stepped on a wall
			
			if (curPath->curPos == target)
			{
				//made it
				if (best == nullptr || curPath->seen.size() > best->seen.size())
				{
					best = curPath;
					WriteLine(std::format("Found a new best! size: {}. Search size: {}", best->seen.size(), searching.size()));
				}

				solved.push_back(curPath);
				continue;
			}

			//add this point to our seen, carry on
			curPath->AddSeen(curSpot);

			if (ignoreSlopes == true || gridChar == '.')
			{
				//split path into valid directions to go, add to be searched
				std::array<Vec3, 4> next{ curPath->curPos, curPath->curPos, curPath->curPos, curPath->curPos };
				next[0].x += 1;
				next[1].x -= 1;
				next[2].y += 1;
				next[3].y -= 1;

				for (auto const& n : next)
					searching.push_back(Path::Ptr(new Path(n, curPath->seen)));
			}
			else
			{
				//forced directions
				Vec3 forcedDir = curPath->curPos;

				switch (gridChar)
				{
				case '<': forcedDir.x -= 1; break;
				case '>': forcedDir.x += 1; break;
				case '^': forcedDir.y -= 1; break;
				case 'v': forcedDir.y += 1; break;
				}

				searching.push_back(Path::Ptr(new Path(forcedDir, curPath->seen)));
			}
		}

		WriteLine(std::format("Search complete with a best path length of size: {}", best->seen.size()));
	}
};