#pragma once

#include "LANQCommonHeaders.h"
#include "LANQInterfaces.h"
#include "LANQTraitsAndConcepts.h"

namespace LANQ
{

	template<typename T> class LVec;
	template<typename K, typename V> class LUMap;

	/*
template <typename Container>
typename std::enable_if<has_const_iterator<Container>::value, void>::type
	func() {}
	*/

	template<typename type>
	requires std::forward_iterator<typename type::iterator>
	class EnumerableBase : public IEnumerable//, public ContainerIteratorHelpers::cbegin_cend<EnumerableBase<type>>
	{
	public:
		using value_type = typename type::value_type;
		using sharedPtrType = std::shared_ptr<const type>;
		using weakPtrType = std::weak_ptr<const type>;
		using vectorType = std::vector<value_type>;

		sharedPtrType m_container;
		//weakPtrType m_weakContainer;

		//const type& GetContainer() const { return *(m_weakContainer.lock().get()); }
		//type& GetMutableContainer() const { return *const_cast<type*>(m_weakContainer.lock().get()); }

		const type& GetContainer() const { return *(m_container.get()); }
		type& GetMutableContainer() const { return *const_cast<type*>(m_container.get()); }


		EnumerableBase()
			:m_container(new type())
			//, m_weakContainer(m_container)
		{}

		EnumerableBase(const sharedPtrType t)
			:m_container(t)
			//, m_weakContainer(m_container)
		{}

		EnumerableBase(const type* t)
			:m_container(new type(*t)) 
			//, m_weakContainer(t)
		{}
		//EnumerableBase(const type&& t)
		//	:m_container(&t)
		//{}


		//iteration
		auto begin() const { return m_container->begin(); }
		auto end() const { return m_container->end(); }

		//operator overloads

		template <typename T = typename std::enable_if_t<traits::implements_index<type>()>>
		value_type& operator[](int ind)
		{
			return GetMutableContainer()[ind];
		}

		type& operator=(const type& o) noexcept
		{
			this->m_container = o.m_container;
			return *this;
		}

		template <typename T = typename std::enable_if_t<traits::implements_index<type>()>>
		const value_type& operator[](int ind) const
		{
			return GetContainer()[ind];
		}


		template <typename T = typename std::enable_if_t<traits::implements_index<type>()>>
		value_type& Get(int index) const
		{
			return GetMutableContainer()[index];
		}

		size_t Size() const
		{
			return m_container->size();
		}

		vectorType ToVector()
		{
			vectorType v;
			v.reserve(m_container->size());

			for (auto& val : *m_container)
				v.push_back(val);

			return v;
		}

		std::shared_ptr<vectorType> ToSharedVector()
		{
			std::shared_ptr<vectorType> ptr(new vectorType());
			ptr->reserve(m_container->size());

			for(auto& val: *m_container)
				ptr->push_back(val);

			return ptr;
		}

		template<typename Key>
		std::unordered_map<Key, value_type> ToUnorderedMap(std::function<Key(const value_type& item)> selector)
		{
			std::unordered_map<Key, value_type> m;
			for (auto& val : *m_container)
				m[selector(val)] = val;

			return m;
		}

		template<typename Key>
		std::shared_ptr<std::unordered_map<Key, value_type>> ToSharedUnorderedMap(std::function<Key(const value_type& item)> selector)
		{
			std::shared_ptr<std::unordered_map<Key, value_type>> ptr(new std::unordered_map<Key, value_type>());
			for (auto& val : *m_container)
				(*ptr)[selector(val)] = val;

			return ptr;
		}

		bool Any(std::function<bool(const value_type& item)> selector)
		{
			for (auto& val : *m_container)
				if (selector(val))
					return true;
			return false;
		}

		template<typename selectedType>
		LVec<selectedType> Select(std::function<selectedType(const value_type& item, const int ind)> selector) const
		{
			LVec<selectedType> out(new std::vector<selectedType>());
			auto& outData = out.GetMutableContainer();
			outData.reserve(Size());

			int ind = 0;
			for (auto& val : *m_container)
				outData.push_back(selector(val, ind++));

			return out;
		}

		LVec<value_type> Where(std::function<bool(const value_type& item, const int& ind)> selector)
		{
			LVec<value_type> out(new vectorType());
			auto& outData = out.GetMutableContainer();

			int ind = 0;
			for (auto& val : *m_container)
				if (selector(val, ind++) == true)
					outData.push_back(val);

			return out;
		}

		template<typename AggregateType>
		AggregateType Aggregate(AggregateType initialValue, std::function<AggregateType(const AggregateType& agg, const value_type& item)> selector)
		{
			AggregateType agg = initialValue;

			for (auto& val : *m_container)
				agg = selector(agg, val);

			return agg;
		}

		template<typename T = std::is_base_of_v<IEnumerable, value_type>>
		LVec<value_type> Flatten(std::function<typename value_type&(const value_type& v)> selector) const
		{
			LVec<value_type> out;
			typename LVec<value_type>::type& outData = out.GetMutableContainer();

			for (auto& val : *m_container)
			{
				auto ret = selector(val);
				for (auto& selectedVal : ret)
				{
					outData.push_back(selectedVal);
				}
			}
			return out;
		}
		
		template<typename T = std::is_base_of_v<IEnumerable, typename value_type>>
		LVec<typename T::value_type> Flatten() const
		{
			LVec<typename T::value_type> out;
			typename LVec<typename T::value_type>::type& outData = out.GetMutableContainer();

			for (auto& val : *m_container)
			{
				for (auto& selectedVal : val)
				{
					outData.push_back(selectedVal);
				}
			}
			return out;
		}
	};

	template<typename T>
	class LVec : public EnumerableBase<std::vector<T>>
	{
	public:
		using parent = EnumerableBase<std::vector<T>>;
		using type = std::vector<T>;
		using value_type = typename type::value_type;


		LVec()
			:EnumerableBase<type>()
		{}

		LVec(const std::shared_ptr<type> t)
			:EnumerableBase<type>(t)
		{}

		LVec(const type* t)
			:EnumerableBase<type>(t)
		{}

		//LEnumerableVec(const type&& t)
		//	:EnumerableBase<type>(t)
		//{}

		value_type& First() const
		{
			return parent::GetMutableContainer().front();
		}

		value_type& Last() const
		{
			return parent::GetMutableContainer().back();
		}

		LVec<value_type>& Add(const value_type& v)
		{
			parent::GetMutableContainer().push_back(v);
			return *this;
		}
	};

	template<typename K, typename V>
	class LUMap : public EnumerableBase<std::unordered_map<K, V>>
	{
	public:
		using type = std::unordered_map<K, V>;
		using value_type = typename type::value_type;

		LUMap() :EnumerableBase<type>()
		{}

		LUMap(const std::shared_ptr<type> t)
			:EnumerableBase<type>(t)
		{}

		LUMap(const type* t)
			:EnumerableBase<type>(t)
		{}

		//LUnorderedMap(const type&& t)
		//	:EnumerableBase<type>(t)
		//{}
	};

};