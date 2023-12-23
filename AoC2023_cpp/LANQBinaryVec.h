#pragma once

#include "LANQQuery.h"

//TODO: make it querytable / iterable by providing ranges and views and iterators
//TODO: add size(), which tracks how many bits its using
namespace LANQ
{
	template<typename T = std::uint32_t>
	class BinaryVec
	{
	public:
		using Type = T;

		Type m_bitHolder = 0;
		size_t m_size = 0;

		BinaryVec() = default;
		BinaryVec(size_t size) :m_size(size) {}
		BinaryVec(const BinaryVec& o) : m_bitHolder(o.m_bitHolder), m_size(o.m_size)
		{ }

		size_t MaxBits()
		{
			return sizeof(Type) * 8;
		}

		size_t Size()
		{
			return m_size;
		}

		BinaryVec& SetSize(size_t size)
		{
			m_size = size;
			return *this;
		}

		const bool& operator[](int ind) const
		{
			return m_bitHolder & (1 << ind);
		}

		std::vector<bool> ToVector() const
		{
			std::vector<bool> v;
			v.resize(Size());

			for (int i = 0; i < Size(); ++i)
			{
				v[i] = GetBit(i);
			}

			return v;
		}

		LVec<bool> ToLVec() const
		{
			LVec<bool> v;

			for (int i = 0; i < Size(); ++i)
			{
				v.Add(GetBit(i));
			}

			return v;
		}

		BinaryVec& Clear()
		{
			m_bitHolder = 0;
		}

		BinaryVec& SetBit(int bit, bool value)
		{
			m_bitHolder |= (1 << bit);
			return *this;
		}

		bool GetBit(int bit) const
		{
			return m_bitHolder & (1 << bit);
		}

		BinaryVec& And(const BinaryVec& o)
		{
			m_bitHolder &= o.m_bitHolder;
			return *this;
		}

		BinaryVec& Or(const BinaryVec& o)
		{
			m_bitHolder |= o.m_bitHolder;
			return *this;
		}

		BinaryVec& Shift(int posOrNegAmt)
		{
			if (posOrNegAmt > 0)
				return ShiftLeft(posOrNegAmt);
			else if (posOrNegAmt < 0)
				return ShiftRight(-1 * posOrNegAmt);

			return *this;
		}

		BinaryVec& ShiftLeft(int amt)
		{
			m_bitHolder = m_bitHolder << amt;
			return *this;
		}

		BinaryVec& ShiftRight(int amt)
		{
			m_bitHolder = m_bitHolder >> amt;
			return *this;
		}

	};

};