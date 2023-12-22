#pragma once

namespace PointsNStuff
{
	struct Vec2
	{
		int x = 0;
		int y = 0;

		Vec2() = default;
		Vec2(const Vec2& other) = default;
		Vec2(int _x, int _y) : x(_x), y(_y) {}
	};
};