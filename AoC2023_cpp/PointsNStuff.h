#pragma once

namespace PointsNStuff
{
	struct Vec3
	{
		int x = 0;
		int y = 0;
		int z = 0;

		Vec3() = default;
		Vec3(const Vec3& other) = default;
		Vec3(const int _x, const int _y = 0, const int _z = 0) : x(_x), y(_y), z(_z) {}
		Vec3(const std::vector<int>& a) : x(a[0]), y(a[1]), z(a[2]) { }

		bool operator==(const Vec3& o) const
		{
			return x == o.x && y == o.y && z == o.z;
		}

		bool operator!=(const Vec3& o) const
		{
			return !(*this == o);
		}

		Vec3& operator+(const Vec3& o)
		{
			x += o.x;
			y += o.y;
			z += o.z;
			return *this;
		}

		//lame hash
		std::uint64_t HashVec3() const { return (x)+(y * 1000000) + (z * 1000000000000); }
		std::uint64_t HashVec2() const { return (x)+(y * 1000000); }
	};

	struct Bounds
	{
		PointsNStuff::Vec3 min;
		PointsNStuff::Vec3 max;

		Bounds() :
			min(INT32_MAX, INT32_MAX, INT32_MAX)
		{}

		void CalcMinMax(const PointsNStuff::Vec3& v)
		{
			CalcMin(v);
			CalcMax(v);
		}

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

		bool InclusiveInBounds(const Vec3& v)
		{
			bool x = v.x >= min.x && v.x <= max.x;
			bool y = v.y >= min.y && v.y <= max.y;
			bool z = v.z >= min.z && v.z <= max.z;

			return x && y && z;
		}
	};


	std::int64_t Distance(const Vec3& a, const Vec3& b)
	{
		return std::abs(b.x - a.x) + std::abs(b.y - a.y) + std::abs(b.z - a.z);
	}

	Vec3 ComponentWiseDistance(const Vec3& a, const Vec3& b)
	{
		Vec3 v;
		v.x = std::abs(b.x - a.x);
		v.y = std::abs(b.y - a.y);
		v.z = std::abs(b.z - a.z);
		return v;
	}

	int ClampToOne(int i)
	{
		if (i > 0) return 1;
		if (i < 0) return -1;
		return 0;
	}

	Vec3 ClampToOnes(const Vec3& v)
	{
		return Vec3(ClampToOne(v.x), ClampToOne(v.y), ClampToOne(v.z));
	}
};