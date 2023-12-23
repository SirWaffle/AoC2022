#pragma once

#include "LANQCommonHeaders.h"
#include "LANQInterfaces.h"

namespace LANQ
{
	//TODO: lets convert stuff to ranges instead of the hacky crap i made.
	template<std::ranges::input_range RangeType>
	class Enumerable : IEnumerable
	{
	public:
		typedef RangeType::value_type Type;
		typedef RangeType ContainerType;
		//typedef std::ranges::ref_view<ContainerType> ViewType;
		typedef std::ranges::view_interface<ContainerType> ViewType;

		ViewType m_view;

		Enumerable(const RangeType& range)
			:m_view(ViewType{ std::views::all(range) })
		{
		}

		Enumerable(const ViewType& view)
			:m_view(view)
		{
		}

		~Enumerable() {}



		Type First() const
		{
			return m_view.front();
		}

		Type Last() const
		{
			return m_view.back();
		}

		Type Get(int index) const
		{
			//indexing wont work on all view types... need a fancy way to manage that
			return m_view[index];
		}

		IEnumerable Reverse() const
		{
			assert(false);
			//std::ranges::reverse_view<RangeType> rv{ m_view };
			//auto v = std::views::reverse(m_view);
			//return Enumerable<RangeType>(rv);
		}

		size_t Size() const
		{
			return m_view.size();
		}


		//mutable operations
		int Add(const Type& item)
		{
			assert(false);
			return 0;
		}
	};
};
