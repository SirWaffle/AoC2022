#pragma once

#include <iostream>
#include <fstream>
#include <string>
#include <stdexcept>
#include <sstream>
#include <filesystem>
#include <regex>
#include <algorithm>
#include <cassert>
#include <cmath>
#include <functional>

class Day18
{
	const char* GetSampleFileName()
	{
		return "..\\Input\\AoC2023\\Day18_sample.txt";
	}

	const char* GetFileName()
	{
		return "..\\Input\\AoC2023\\Day18_1.txt";
	}

public:
	void Run()
	{
		//DoPart1();
		DoPart2();
	}

	static bool IsNeg(int i)
	{
		return i < 0;
	}

	void ReadWholeFile(const char* fname, std::string &data)
	{
		std::cout << "Current path is " << std::filesystem::current_path() << '\n';

		std::ifstream f(fname);
		if (!f) 
			throw std::runtime_error("failed to open file: " + std::string(fname));

		std::ostringstream ss;
		ss << f.rdbuf();
		data = ss.str();
	}

	std::vector<std::string> SplitString(std::string s, std::string delimiter) 
	{
		size_t start = 0, end, splitterLen = delimiter.length();
		std::string token;
		std::vector<std::string> result;

		while ((end = s.find(delimiter, start)) != std::string::npos) 
		{
			token = s.substr(start, end - start);
			start = end + splitterLen;
			result.push_back(token);
		}

		result.push_back(s.substr(start));
		return result;
	}


	struct Vec2
	{
		int x = 0;
		int y = 0;

		Vec2() = default;
		Vec2(const Vec2& other) = default;
		Vec2(int _x, int _y) : x(_x), y(_y) {}
	};

	struct Node
	{
		Vec2 point;
		Vec2 edge;

		char edgeDir;
		int edgeLen = 0;
		std::string edgeColor;

		bool ContainsPoint(const Vec2 p)
		{
			int dy = p.y - point.y;
			int dx = p.x - point.x;

			if (dy == 0 && dx == 0)
				return true;

			if (IsNeg(dy) != IsNeg(edge.y) && dy != 0)
				return false;

			if (IsNeg(dx) != IsNeg(edge.x) && dx != 0)
				return false;

			if (std::abs(dy) <= std::abs(edge.y) && std::abs(dx) <= std::abs(edge.x))
				return true;

			return false;
		}

		bool RaycastRow(int rowInd)
		{
			int d = rowInd - point.y;

			if (d == 0)
				return true;

			if (IsNeg(d) != IsNeg(edge.y) && d != 0)
				return false;

			if (std::abs(d) <= std::abs(edge.y))
				return true;

			return false;
		}

	};

	struct Bounds
	{
		int xMin = 0;
		int yMin = 0;
		int xMax = 0;
		int yMax = 0;

		int Width()
		{
			return std::abs(xMax - xMin);
		}

		int Height()
		{
			return std::abs(yMax - yMin);
		}
	};

	void DoPart1()
	{
		std::string input;
		ReadWholeFile(GetFileName(), input);
		std::vector<std::string> lines = SplitString(input, "\n");

		std::vector<Node*> nodes;
		nodes.reserve(lines.size());

		Vec2 curPoint;
		Bounds bounds;
		int pathLen = 0;

		for (int line = 0; line < lines.size(); ++line)
		{
			std::vector<std::string> step = SplitString(lines[line], " ");

			Node* node = new Node();
			node->point = curPoint;
			node->edgeDir = step[0][0];
			node->edgeLen = std::stoi(step[1]);
			node->edgeColor = step[2];
			std::erase(node->edgeColor, '(');
			std::erase(node->edgeColor, ')');
			nodes.push_back(node);

			pathLen += node->edgeLen;

			bounds.xMin = std::min(bounds.xMin, curPoint.x);
			bounds.yMin = std::min(bounds.yMin, curPoint.y);
			bounds.xMax = std::max(bounds.xMax, curPoint.x);
			bounds.yMax = std::max(bounds.yMax, curPoint.y);

			switch (node->edgeDir)
			{
			case 'U':
				node->edge = { 0, node->edgeLen };
				curPoint.y += node->edgeLen; break;
			case 'D':
				node->edge = { 0, -1 * node->edgeLen };
				curPoint.y -= node->edgeLen; break;
			case 'L':
				node->edge = { -1 * node->edgeLen, 0 };
				curPoint.x -= node->edgeLen; break;
			case 'R':
				node->edge = { node->edgeLen, 0 };
				curPoint.x += node->edgeLen; break;
			default:
				assert(false);
			}
		}

		//lets do some ascii art
		std::vector<std::string> grid;
		grid.resize(bounds.Height() + 1);

		int pounds = 0;

		for (int y = bounds.Height(); y >= 0; --y)
		{
			grid[y].reserve(bounds.Width() + 1);
			std::vector<Node*> foundNodes;

			for (int x = 0; x <= bounds.Width(); ++x)
			{
				for (int n = 0; n < nodes.size(); ++n)
				{
					if (nodes[n]->ContainsPoint(Vec2(bounds.xMin + x, bounds.yMin + y)))
					{
						//only keep U and D dirs.
						if(nodes[n]->edgeDir == 'U' || nodes[n]->edgeDir == 'D')
							foundNodes.push_back(nodes[n]);

						//break;
					}
				}
			}

			//now fill in across the map, we have a ways to go...we need to pair them up...
			int lastDirSign = -1;
			int curNode = 0;
			bool xin = false;
			for (int x = 0; x <= bounds.Width(); ++x)
			{
				if (curNode < foundNodes.size() && foundNodes[curNode]->ContainsPoint(Vec2(bounds.xMin + x, bounds.yMin + y)))
				{
					if (lastDirSign == -1 || lastDirSign != (int)IsNeg(foundNodes[curNode]->edge.y))
					{
						lastDirSign = (int)IsNeg(foundNodes[curNode]->edge.y);
						xin = !xin;
					}

					curNode++;
				}
				//xin = false;
				if (xin == true)
				{
					grid[y] += "#";
					pounds++;
				}
				else
				{
					bool found = false;
					for (int n = 0; n < nodes.size(); ++n)
					{
						if (nodes[n]->ContainsPoint(Vec2(bounds.xMin + x, bounds.yMin + y)))
						{
							//if (nodes[n]->edgeDir == 'U' || nodes[n]->edgeDir == 'D')
							//	grid[y] += nodes[n]->edgeDir;
							//else
							//	grid[y] += "*";
							found = true; 
							break;
						}
					}

					if(found == false)
						grid[y] += ".";
					else
					{
						pounds++;
						grid[y] += "#";
					}
				}
			}

			std::cout << grid[y] << '\n';
		}		

		std::cout << "#'s = " << pounds;
		//49897
	}

	void DoPart2()
	{
		std::string input;
		ReadWholeFile(GetSampleFileName(), input);
		std::vector<std::string> lines = SplitString(input, "\n");

		std::vector<Node*> nodes;
		nodes.reserve(lines.size());

		Vec2 curPoint;
		Bounds bounds;
		int pathLen = 0;

		for (int line = 0; line < lines.size(); ++line)
		{
			std::vector<std::string> step = SplitString(lines[line], " ");

			Node* node = new Node();
			node->point = curPoint;
			node->edgeDir = step[0][0];
			node->edgeLen = std::stoi(step[1]);
			node->edgeColor = step[2];
			std::erase(node->edgeColor, '(');
			std::erase(node->edgeColor, ')');
			nodes.push_back(node);

			pathLen += node->edgeLen;

			bounds.xMin = std::min(bounds.xMin, curPoint.x);
			bounds.yMin = std::min(bounds.yMin, curPoint.y);
			bounds.xMax = std::max(bounds.xMax, curPoint.x);
			bounds.yMax = std::max(bounds.yMax, curPoint.y);

			switch (node->edgeDir)
			{
			case 'U':
				node->edge = { 0, node->edgeLen };
				curPoint.y += node->edgeLen; break;
			case 'D':
				node->edge = { 0, -1 * node->edgeLen };
				curPoint.y -= node->edgeLen; break;
			case 'L':
				node->edge = { -1 * node->edgeLen, 0 };
				curPoint.x -= node->edgeLen; break;
			case 'R':
				node->edge = { node->edgeLen, 0 };
				curPoint.x += node->edgeLen; break;
			default:
				assert(false);
			}
		}

		//lets do some ascii art
		std::vector<std::string> grid;
		grid.resize(bounds.Height() + 1);

		int pounds = 0;

		for (int y = bounds.Height(); y >= 0; --y)
		{
			grid[y].resize(bounds.Width() + 1);

			std::vector<Node*> foundNodes;
			int totalHit = 0;

			for (int n = 0; n < nodes.size(); ++n)
			{
				if (nodes[n]->RaycastRow(bounds.yMin + y))
				{
					totalHit++;

					//only keep U and D dirs.
					if (nodes[n]->edgeDir == 'U' || nodes[n]->edgeDir == 'D')
					{
						foundNodes.push_back(nodes[n]);
					}
					//else //we add the length of the horizontal stuff to trench size
					//	pounds += std::abs(nodes[n]->edgeLen - 1);
				}
			}


			//now fill in across the map

			std::sort(std::begin(foundNodes), std::end(foundNodes), [](Node* a, Node* b)
				{
					return a->point.x < b->point.x;
				});

			Node* lastNodeCrossed = nullptr;
			int lastDirSign = -1;
			int curNode = 0;
			bool xin = false;

			for (int curNode = 0; curNode < foundNodes.size(); ++curNode)
			{
				if (lastDirSign == -1 || lastDirSign != (int)IsNeg(foundNodes[curNode]->edge.y))
				{
					if (lastDirSign == -1)
					{
						int empty = foundNodes[curNode]->point.x - bounds.xMin;
						while (empty-- > 0)
							grid[y] += ".";
					}

					lastDirSign = (int)IsNeg(foundNodes[curNode]->edge.y);
					xin = !xin;

					if (xin == false) //hit closing edge
					{
						int num = std::abs((foundNodes[curNode]->point.x - lastNodeCrossed->point.x)) + 1;
						pounds += num;

						while (num-- > 0)
							grid[y] += "#";
						
					}
					else if(lastNodeCrossed != nullptr)
					{
						int num = std::abs((foundNodes[curNode]->point.x - lastNodeCrossed->point.x));

						while (num-- > 0)
							grid[y] += "*";
					}

					lastNodeCrossed = foundNodes[curNode];
				}
				else //same dir, like two ups, add the x line between them, or hit the backside
				{
					int num = std::abs((foundNodes[curNode]->point.x - lastNodeCrossed->point.x)) - 1;
					pounds += num;
					while (num-- > 0)
						grid[y] += "_";
				}				
			}

			std::cout << grid[y] << '\n';
			//pounds -= (totalHit - 1);
		}

		std::cout << "#'s = " << pounds;
		//49897
	}
};