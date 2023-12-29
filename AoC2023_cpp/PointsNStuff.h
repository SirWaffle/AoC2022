#pragma once

namespace PointsNStuff
{
	template<typename Type>
	struct Vector3
	{
		Type x = 0;
		Type y = 0;
		Type z = 0;

		Vector3() = default;
		Vector3(const Vector3& other) = default;
		Vector3(const Type _x, const Type _y = 0, const Type _z = 0) : x(_x), y(_y), z(_z) {}
		Vector3(const std::vector<Type>& a) : x(a[0]), y(a[1]), z(a[2]) { }

		bool operator==(const Vector3<Type>& o) const
		{
			return x == o.x && y == o.y && z == o.z;
		}

		bool operator!=(const Vector3<Type>& o) const
		{
			return !(*this == o);
		}

		Vector3<Type>& operator+(const Vector3<Type>& o)
		{
			x += o.x;
			y += o.y;
			z += o.z;
			return *this;
		}

		Vector3<Type>& operator*(const Vector3<Type>& o)
		{
			x *= o.x;
			y *= o.y;
			z *= o.z;
			return *this;
		}

		//lame hash
		inline std::uint64_t HashVec3() const { return (x)+(y * 1000000) + (z * 1000000000000); }
		inline std::uint32_t HashVec2() const { return (x)+(y * 1000000); }
	};

	typedef Vector3<int> Vec3;

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

		inline bool InclusiveInBounds(const Vec3& v) const
		{
			bool x = v.x >= min.x && v.x <= max.x;
			bool y = v.y >= min.y && v.y <= max.y;
			bool z = v.z >= min.z && v.z <= max.z;

			return x && y && z;
		}
	};


	template<typename Type>
	Type Distance(const Vector3<Type>& a, const Vector3<Type>& b)
	{
		return std::abs(b.x - a.x) + std::abs(b.y - a.y) + std::abs(b.z - a.z);
	}

	template<typename Type>
	Vector3<Type> ComponentWiseDistance(const Vector3<Type>& a, const Vector3<Type>& b)
	{
		Vector3<Type> v;
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