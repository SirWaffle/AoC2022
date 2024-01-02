#pragma once

#include "CommonHeaders.h"

using namespace LANQ;
using namespace PointsNStuff;

class Day17 : public DayBase<2023, 17>
{
public:
	void Run()
	{
		Crucify(true); //false = p1, true = p2
	}

	struct SearchPoint
	{
		using Ptr = std::shared_ptr<SearchPoint>;
		
		Vec3 pos;
		Vec3 vel;

		int streak = 0;	
		int heatLoss = 0;

		SearchPoint() {}

		//spawn a new path
		SearchPoint(const SearchPoint::Ptr& _parent, const Vec3& newVel, int _streak)
			: vel(newVel),
			pos(_parent->pos),
			streak(_streak),
			heatLoss(_parent->heatLoss)
		{ }

		int GetDir()
		{
			if (vel.y == -1)
				return 0;
			else if (vel.y == 1)
				return 1;
			else if (vel.x == -1)
				return 2;
			else if (vel.x == 1)
				return 3;
			assert(false);
		}
	};

	using Map = std::vector< std::vector<int> >;
	using ShortestPathCache = std::unordered_map<int, std::unordered_map<int, Map> >;

	void Crucify(const bool isUltra)
	{
		//parse
		Map map = ReadWholeFile(GetFileName().c_str()).Split("\n").Select<std::vector<int>>([](const LStr& item, int yind) {
			return item.ToLVec().Select<int>([&yind](const LChar& c, int xind) { return ((int)c.Value()) - (int)'0'; }).ToVector();
			}).ToVector();

		//shortest path cache for culling
		ShortestPathCache shortestCache;

		for (int i = 0; i < 4; ++i) //num dirs
		{
			for (int j = 0; j < (isUltra? 11: 4); ++j) //streak length
			{
				shortestCache[i][j] = Map(map.size(), std::vector<int>(map[0].size(), INT_MAX));
			}
		}

		std::list<SearchPoint::Ptr> allWork;
		SearchPoint::Ptr bestPath;	

		SearchPoint::Ptr startX(new SearchPoint());
		startX->vel.x = 1;
		allWork.push_back(startX);

		SearchPoint::Ptr startY(new SearchPoint());
		startY->vel.y = 1;
		allWork.push_back(startY);

		Vec3 target((int)map.size() - 1, (int)map[0].size() - 1);

		int update = 0;
		while (allWork.size() > 0)
		{
			if (++update > 9000000)
			{
				update = 0;
				WriteLine(std::format("----working, all work: {}", allWork.size()));
			}
			SearchPoint::Ptr item = allWork.front();
			allWork.pop_front();

			ProccessSingleItem(item, target, map, shortestCache, allWork, bestPath, isUltra);
		}
	}

    void ProccessSingleItem(SearchPoint::Ptr& workItem, const Vec3& target, const Map& map, 
		ShortestPathCache& shortestCache, std::list<SearchPoint::Ptr>& allWork, SearchPoint::Ptr& bestPath, const bool isUltra)
    {
        //next spot
        workItem->pos.x += workItem->vel.x;
        workItem->pos.y += workItem->vel.y;
        workItem->streak += 1;

		//worse than an existing path, or hit an arbitrary ceiling
		if (workItem->heatLoss > 2000 || (bestPath != nullptr && bestPath->heatLoss <= workItem->heatLoss))
			return;

		//OOB
        if (workItem->pos.x < 0 || workItem->pos.x >= map[0].size() || workItem->pos.y < 0 || workItem->pos.y >= map.size())
            return;

		//heatloss update
        workItem->heatLoss += map[workItem->pos.y][workItem->pos.x];

		//hit target
		if (workItem->pos.x == target.x && workItem->pos.y == target.y)
		{
			if (!isUltra || workItem->streak >= 4)
			{
				WriteLine(std::format("New solution found, with heat loss of: {}", workItem->heatLoss));
				if (bestPath == nullptr || workItem->heatLoss <= bestPath->heatLoss)
				{
					bestPath = workItem;
					WriteLine("      new best path found!");
				}
			}
			return;
		}

		//culling
		if (shortestCache[workItem->GetDir()][workItem->streak][workItem->pos.y][workItem->pos.x] <= workItem->heatLoss)
			return;

		//shortest path value cache
		shortestCache[workItem->GetDir()][workItem->streak][workItem->pos.y][workItem->pos.x] = workItem->heatLoss;

		//new paths: L, R, F
		bool doTurn = isUltra ? workItem->streak >= 4 : true;
		bool doStraight = isUltra ? workItem->streak < 10 : workItem->streak < 3;

		if (doTurn)
		{
			Vec3 newVel = workItem->vel;
			newVel.Ortho2D();
			allWork.push_back(SearchPoint::Ptr(new SearchPoint(workItem, newVel, 0)));
			newVel.Invert();
			allWork.push_back(SearchPoint::Ptr(new SearchPoint(workItem, newVel, 0)));
		}

		if (doStraight)
			allWork.push_back(SearchPoint::Ptr(new SearchPoint(workItem, workItem->vel, workItem->streak)));		
    }
};