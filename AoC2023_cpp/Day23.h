#pragma once

#include "CommonHeaders.h"
#include "ThreadPool.h"
#include "LockableQueue.h"
#include "LockableStack.h"
#include "ThreadQueueWorker.h"
#include "LockablePriorityQueue.h"

using namespace LANQ;
using namespace PointsNStuff;
using namespace ThreadsNThings;


//NOTE: reduce map size by turning it into nodes + edges, nodes at start / finish and anywhere the path splits, edges are the paths between. less to track.
class Day23: public DayBase<2023, 23>
{
public:
	void Run() override
	{
		//Execute(false); //part 1
		//Execute(true); //part 2
		ThreadExecute(true); //part 2
	}

	//********************
	//Data types for puzzle
	//********************

	struct Spot
	{
		LChar gridChar;
		Vec3 pos;

		Spot() = default;
		Spot(char c, const Vec3& v) : gridChar(c), pos(v) {}
	};

	struct Path
	{
		using Ptr = std::shared_ptr<Path>;
		Vec3 curPos;
		size_t pathLen = 0;
		Path::Ptr parent;
		uint32_t posHash = 0;

		bool HasSeen(const uint32_t& pHash)
		{
			if (posHash == pHash)
				return true;

			if (parent != nullptr)
				return parent->HasSeen(pHash);

			return false;
		}

		inline size_t PathLen()
		{
			return pathLen;
		}

		Path() = delete;
		Path(const Vec3& p) : curPos(p) {}
		Path(const Vec3& p, const Path::Ptr& _parent, const size_t plen) : curPos(p), parent(_parent), pathLen(plen), posHash(p.HashVec2()) {}
	};


	//********************
	//Typedefs for puzzle
	//********************	

	struct PriorityCompare
	{
		bool operator()(const Path::Ptr& l, const Path::Ptr& r) const { return l->PathLen() < r->PathLen(); }
	};

	using LockablePathContainer = LockablePriorityQueue<Path::Ptr, PriorityCompare>;
	//using LockablePathContainer = LockableStack<Path::Ptr>;


	//********************
	//Member vars for puzzle
	//********************

	std::mutex bestLock;


	//********************
	//Funcs for puzzle
	//********************

	void ThreadWork(Path::Ptr& curPath, const Bounds& bounds, std::queue<Path::Ptr>& nextPaths, const Vec3& start, const Vec3& target, Path::Ptr& best, const std::shared_ptr<std::unordered_map<std::uint32_t, Spot>>& map, bool ignoreSlopes)
	{
		const Spot& curSpot = (*map)[curPath->curPos.HashVec2()];
		const char gridChar = curSpot.gridChar;

		if (curPath->curPos == target)
		{
			//made it, lock and update
			std::lock_guard<std::mutex> guard(bestLock);

			if (best == nullptr || curPath->PathLen() > best->PathLen())
			{
				auto tid = std::this_thread::get_id();
				std::stringstream ss;
				ss << tid;
				std::string tidstr = ss.str();

				best = curPath;
				WriteLine(std::format("{} -> Found a new best! size: {}", tidstr, best->PathLen()));
			}
			return;
		}

		//next spots
		if (ignoreSlopes == true || gridChar == '.')
		{
			//split path into valid directions to go, add to be searched
			std::array<Vec3, 4> next{ curPath->curPos, curPath->curPos, curPath->curPos, curPath->curPos };
			next[0].x += 1;
			next[1].x -= 1;
			next[2].y += 1;
			next[3].y -= 1;


			for (auto const& n : next)
			{
				if (!bounds.InclusiveInBounds(n) || (*map)[n.HashVec2()].gridChar == '#' || curPath->HasSeen(n.HashVec2()))
					continue;//out of bounds, invalid spot, same spot twice

				nextPaths.push(Path::Ptr(new Path(n, curPath, curPath->PathLen() + 1)));
			}
		}
		else
		{
			//forced directions
			Vec3 v = curPath->curPos;
			switch (gridChar)
			{
			case '<': v.x -= 1; break;
			case '>': v.x += 1; break;
			case '^': v.y -= 1; break;
			case 'v': v.y += 1; break;
			}

			if (!bounds.InclusiveInBounds(v) || (*map)[v.HashVec2()].gridChar == '#' || curPath->HasSeen(v.HashVec2()))
				return;  //out of bounds, invalid spot, same spot twice

			nextPaths.push(Path::Ptr(new Path(v, curPath, curPath->PathLen() + 1)));
		}
	}

	void ThreadExecute(bool ignoreSlopes = false)
	{
		//parsing
		Bounds bounds;

		std::shared_ptr<std::unordered_map<std::uint32_t, Spot>> map = ReadWholeFile(GetFileName().c_str()).Split("\n").Select<LVec<Spot>>([](const LStr& item, int yind) {
			return item.ToLVec().Select<Spot>([yind](const LChar& c, int xind) { return Spot{ c, Vec3(xind, yind) }; });
			}).Flatten<LVec<Spot>>().ToSharedUnorderedMap<std::uint32_t>([](const Spot& item) { return item.pos.HashVec2(); });

		for (const auto& p : *map)
			bounds.CalcMinMax(p.second.pos);

		Vec3 start = LANQit(map).Where([bounds](auto const& p, int ind) { return p.second.pos.y == bounds.min.y; })
			.Where([bounds](auto const& p, int ind) { return p.second.gridChar.Value() == '.'; }).First().second.pos;

		Vec3 target = LANQit(map).Where([bounds](auto const& p, int ind) { return p.second.pos.y == bounds.max.y; })
			.Where([bounds](auto const& p, int ind) { return p.second.gridChar.Value() == '.'; }).First().second.pos;

		Path::Ptr best;
		LockablePathContainer searching;
		searching.push_back(Path::Ptr(new Path(start)));

		WriteLine(std::format("Bounds: ({},{}) - ({},{})", bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y));


		//threading
		BS::thread_pool threadPool(8, 0);
		uint32_t numThreads = threadPool.get_thread_count();
		WriteLine(std::format("Starting thread pool with {} threads", numThreads));

		std::vector<ThreadQueueWorker<LockablePathContainer, LockablePathContainer>*> workerThreads;
		workerThreads.reserve(numThreads);

		for (uint32_t i = 0; i < numThreads - 2; ++i)
		{
			auto workerThread = new ThreadQueueWorker<LockablePathContainer, LockablePathContainer>(&searching, &searching, 4); // 4);// 32768 / 4);
			workerThreads.push_back(workerThread);

			std::function<void(Path::Ptr&, std::queue<Path::Ptr>&)> workFunc = [this, &bounds, &searching, &start, &target, &best, &map, &ignoreSlopes](Path::Ptr& item, std::queue<Path::Ptr>& out) { 
					ThreadWork(item, bounds, out, start, target, best, map, ignoreSlopes);
				};

			auto trash = threadPool.submit(&ThreadQueueWorker<LockablePathContainer, LockablePathContainer>::EnterProcFunc, workerThread, workFunc);
		}


		//reporting while we work
		auto lastStatusReport = std::chrono::high_resolution_clock::now();

		while (true)
		{
			std::this_thread::sleep_for(std::chrono::seconds(5));

			auto curTime = std::chrono::high_resolution_clock::now();
			auto totalElapsedTime = std::chrono::duration_cast<std::chrono::seconds>(curTime - m_startTime);

			WriteLine("-----------------");
			WriteLine(std::format("[ elapsed time: {}s ]", totalElapsedTime.count()));

			if(best != nullptr)
				WriteLine(std::format("[ best path length: {} ]", best->PathLen()));

			WriteLine(std::format("[ searches in queue: {} ]", searching.size()));

			std::uint64_t proc = 0;
			for (auto const& t : workerThreads)
			{
				proc += t->GetProcessedItemCount();
			}

			WriteLine(std::format("[ Total processed: {} ]", proc));
		}
			
		WriteLine(std::format("Search complete with a best path length of size: {}", best->PathLen()));
	}

	void Execute(bool ignoreSlopes = false)
	{
		/*
		Bounds bounds;

		std::shared_ptr<std::unordered_map<std::uint32_t, Spot>> map = ReadWholeFile(GetFileName().c_str()).Split("\n").Select<LVec<Spot>>([](const LStr& item, int yind) {
			return item.ToLVec().Select<Spot>([yind](const LChar& c, int xind) { return Spot{ c, Vec3(xind, yind) }; });
			}).Flatten<LVec<Spot>>().ToSharedUnorderedMap<std::uint32_t>([](const Spot& item) { return item.pos.HashVec2(); });
			
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
			if (++logCount == 4000000)
			{
				logCount = 0;
				WriteLine(std::format("Working... Search size: {}", searching.size()));
			}

			Path::Ptr curPath = searching.back();
			searching.pop_back();
			
			Spot& curSpot = (*map)[curPath->curPos.HashVec2()];
			const char gridChar = curSpot.gridChar;

			if (curPath->curPos == target)
			{
				//made it
				if (best == nullptr || curPath->PathLen() > best->PathLen())
				{
					best = curPath;
					WriteLine(std::format("Found a new best! size: {}. Search size: {}", best->PathLen(), searching.size()));
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

				bool copiedOriginal = false;
				for (auto const& n : next)
				{
					if (!bounds.InclusiveInBounds(n) || (*map)[n.HashVec2()].gridChar == '#' || curPath->seen.contains(n.HashVec2()))
						continue;//out of bounds, invalid spot, same spot twice

					if (!copiedOriginal) //reuse original to reduce copies
					{
						copiedOriginal = true;
						curPath->curPos = n;
						searching.push_back(curPath);
					}
					else
					{
						searching.push_back(Path::Ptr(new Path(n, curPath->seen)));
					}
				}
			}
			else
			{
				//forced directions
				switch (gridChar)
				{
				case '<': curPath->curPos.x -= 1; break;
				case '>': curPath->curPos.x += 1; break;
				case '^': curPath->curPos.y -= 1; break;
				case 'v': curPath->curPos.y += 1; break;
				}

				if (gridChar == '#' || !bounds.InclusiveInBounds(curPath->curPos) || curPath->seen.contains(curPath->curPos.HashVec2()))
					continue;  //out of bounds, invalid spot, same spot twice

				searching.push_back(curPath);
			}
		}

		WriteLine(std::format("Search complete with a best path length of size: {}", best->PathLen));
		*/
	}
	
};