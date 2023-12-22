#pragma once
#include <vector>
#include <functional>
#include <cassert>

namespace LANQ //Languge INtegrated Query? no! Lazy Ass iNeffecient Query
{
	//this should really all work with a tanlged mess of iterators, instead of actually creating the udnerlying cotnainer everytime
	//but its "LAzy iNeffecient', so whatever


	//fdec
	class IEnumerable;
	template<typename Type> class LData;
	template<typename Type> class LDataRef;



	//result
	template<typename ResultType>
	class Result
	{
	public:
		bool success = false;
		ResultType successResult;

		Result() = default;
		Result(bool _success) :success(_success) {}
		Result(bool _success, const ResultType& t) :success(_success), successResult(t) {}

		bool Success() const { return success; }
		bool Failure() const { return !success; }

		ResultType GetSuccessResult() { return successResult; }

		template<typename T>
		static Result<T> Failure()
		{
			return Result<T>(false);
		}

		template<typename T>
		static Result<T> Success(T resultValue)
		{
			return Result<T>(true, resultValue);
		}
	};



	//static funcs

	template<typename type, typename selectedType>
	static std::vector<selectedType>* Select(const std::vector<type>* v, std::function<selectedType(const type& item, const int ind)> selector)
	{
		std::vector<selectedType>* out = new std::vector<selectedType>();
		out->reserve(v->size());

		for (int i = 0; i < v->size(); ++i)
		{
			out->push_back(selector((*v)[i], i));
		}

		return out;
	}

	template<typename type>
	static std::vector<type>* Where(const std::vector<type>* v, std::function<bool(const type& item, const int& ind)> selector)
	{
		std::vector<type>* out = new std::vector<type>();

		for (int i = 0; i < v->size(); ++i)
		{
			if (selector((*v)[i], i) == true)
				out->push_back((*v)[i]);
		}

		return out;
	}

	template<typename type, typename AggregateType>
	static AggregateType Aggregate(const std::vector<type>* v, AggregateType initialValue, std::function<AggregateType(const AggregateType& agg, const type& item)> selector)
	{
		AggregateType agg = initialValue;

		for (int i = 0; i < v->size(); ++i)
		{
			agg = selector(agg, (*v)[i]);
		}

		return agg;
	}

	//Classes
	//this should wrap anything that can be enumerated to abstract thigns away from teh terrible vector mess
	//and turn this all into a system of iterators
	class IEnumerable
	{

	};

	template<typename type>
	class LVec: IEnumerable
	{
	public:
		typedef std::vector<type> ContainerType;

		//TODO: when feeling energetic, convert to a shared pointers
		std::shared_ptr<ContainerType> m_dataContainer;


		const ContainerType& Data() const { return (*m_dataContainer); }
		ContainerType& MutableData() const { return *const_cast<ContainerType*>(&*m_dataContainer); }


		LVec() : m_dataContainer(new ContainerType()) { }

		LVec(LVec& o)
			:m_dataContainer(o.m_dataContainer)
		{
		}

		LVec(const LVec& o)
			:m_dataContainer(o.m_dataContainer)
		{ }

		LVec(LVec&& o)
			:m_dataContainer(o.m_dataContainer)		
		{ 
		}

		LVec(const std::string& s)
			: m_dataContainer(new ContainerType())
		{
			for (int i = 0; i < s.size(); ++i)
				MutableData().push_back(s[i]);
		}

		//LVec(const LNAQvec& o) = default;
		LVec(const ContainerType* o) : m_dataContainer(o) 
		{}

		//LVec(const LNAQvec& o) = default;
		LVec(ContainerType* o) : m_dataContainer(std::move(o))
		{
			o = nullptr;
		}


		~LVec()
		{
		}



		type& operator[](int ind)
		{
			return MutableData()[ind];
		}

		const type& operator[](int ind) const
		{
			return Data()[ind];
		}

		operator ContainerType* () { return static_cast<ContainerType*>(this); }

		
		LVec<type>& operator=(const LVec<type>& o) noexcept
		{
			this->m_dataContainer = o.m_dataContainer;
			return *this;
		}
		

		bool Any(std::function<bool(const type& item)> selector)
		{
			for (int i = 0; i < m_dataContainer->size(); ++i)
			{
				if (selector( (*m_dataContainer)[i]) )
					return true;
			}
			return false;
		}

		template<typename selectedType>
		LVec<selectedType> Select(std::function<selectedType(const type& item, const int ind)> selector) const
		{
			auto out = LANQ::Select(&*m_dataContainer, selector);
			return out;
		}

		template<typename selectedType>
		selectedType SelectSelf(std::function<selectedType(const LVec<type>& self)> selector) const
		{
			auto out = selector(&*m_dataContainer);
			return out;
		}

		LVec<type> Where(std::function<bool(const type& item, const int ind)> selector) const
		{
			auto out = LANQ::Where<type>(&*m_dataContainer, selector);
			return out;
		}

		template<typename AggregateType>
		AggregateType Aggregate(AggregateType initialValue, std::function<AggregateType(const AggregateType& agg, const type& item)> selector)
		{
			return LANQ::Aggregate(&*m_dataContainer, initialValue, selector);
		}

		template<typename T = std::is_base_of_v<IEnumerable, type>>
		type Flatten(std::function<typename type(const type& v)> selector)
		{
			type out;
			typename type::ContainerType& outData = out.MutableData();
			for (int i = 0; i < m_dataContainer->size(); ++i)
			{
				auto ret = selector( (*m_dataContainer)[i] );
				for (int j = 0; j < ret.Data().size(); ++j)
				{
					outData.push_back(ret.Data()[j]);
				}
			}
			return out;
		}

		template<typename T = std::is_base_of_v<IEnumerable, type>>
		type Flatten()
		{
			type out;
			typename type::ContainerType& outData = out.MutableData();
			for (int i = 0; i < m_dataContainer->size(); ++i)
			{
				for (int j = 0; j < (*m_dataContainer)[i].Data().size(); ++j)
				{
					outData.push_back((*m_dataContainer)[i].Data()[j]);
				}
			}
			return out;
		}

		template<typename Key>
		std::shared_ptr<std::unordered_map<Key, type>> ToUnorderedMap(std::function<Key(const type& item)> selector)
		{
			std::shared_ptr<std::unordered_map<Key, type>> ptr(new std::unordered_map<Key, type>());
			for (int i = 0; i < m_dataContainer->size(); ++i)
				(*ptr)[selector((*m_dataContainer)[i])] = (*m_dataContainer)[i];

			return ptr;
		}

		type First() const
		{
			return (*m_dataContainer)[0];
		}

		type Last() const
		{
			return (*m_dataContainer)[(*m_dataContainer).size() - 1];
		}

		type Get(int index) const
		{
			//negative gets from the back, pos from the front, weee
			if (index >= 0)
				return (*m_dataContainer)[index];
			else
				return (*m_dataContainer)[ m_dataContainer->size() - index];
		}

		size_t Size() const
		{
			return (*m_dataContainer).size();
		}


		//mutable operations
		LVec<type> Add(const type& item)
		{
			MutableData().push_back(item);
			return *this;
		}
	};




	template<typename Type>
	class LData
	{
	public:
		Type m_data;


		const Type& Value() const
		{
			return m_data;
		}

		Type& MutableValue()
		{
			return m_data;
		}

		operator const Type() { return Value(); }
		operator const Type* () { return &Value(); }
		operator const Type& () { return &Value(); }

		LData() = default;
		LData(const Type& s)
		{
			m_data = s;
		}
	};

	typedef LData<char> LChar;
	typedef LData<std::int32_t> LInt32;
	typedef LData<std::uint32_t> LUInt32;
	typedef LData<std::uint64_t> LUInt64;
	typedef LData<std::int64_t> LInt64;





	template<typename Type>
	class LDataRef
	{
	public:
		std::shared_ptr<const Type> m_data;

		const Type& Value() const
		{
			assert(m_data.get() != nullptr);
			return *m_data;
		}

		Type& MutableValue()
		{
			assert(m_data.get() != nullptr);
			return *const_cast<Type*>(&*m_data);
		}

		operator const Type() { return Value(); }
		operator const Type* () { return &Value(); }
		//operator const Type& () { return *Value(); }

		LDataRef() = default;
		LDataRef(const Type& s)
		{
			m_data.reset(new Type(s));
		}

		LDataRef(const Type* s)
		{
			assert(s != nullptr);
			m_data.reset(s);
		}

		~LDataRef()
		{ }
	};



	class LStr : public LDataRef<std::string>
	{
	public:
		static const std::string Empty;

		LStr() = default;

		LStr(const std::string& s): LDataRef<std::string>(s)
		{ }

		LStr(const std::string* s) : LDataRef<std::string>(s)
		{ }



		char& operator[](int ind)
		{
			return MutableValue()[ind];
		}

		const char& operator[](int ind) const
		{
			return Value()[ind];
		}



		std::string ToStr() const
		{
			return Value();
		}

		LVec<LChar> ToLVec() const
		{
			LVec<LChar> out(Value());
			return out;
		}

		LVec<LStr> Split(std::string delimiter) const
		{
			return StringSplit(Value(), delimiter);
		}

		LStr Remove(std::string str) const
		{
			return Replace(Value(), str, Empty);
		}

		LStr Replace(std::string str, std::string with) const
		{
			return Replace(Value(), str, with);
		}

		int AsInt() const
		{
			return std::atoi(Value().c_str());
		}

		static LVec<LStr> StringSplit(const std::string& s, const std::string& delimiter)
		{
			size_t start = 0, end, splitterLen = delimiter.length();
			std::string token;
			LVec<LStr> result;

			while ((end = s.find(delimiter, start)) != std::string::npos)
			{
				token = s.substr(start, end - start);
				start = end + splitterLen;
				result.MutableData().push_back(new std::string(token));
			}

			result.MutableData().push_back(new std::string(s.substr(start)));
			return result;
		}

		static LStr Replace(const std::string& s, const std::string& what, const std::string& with)
		{
			size_t start = 0, end, splitterLen = what.length();
			std::string token;
			std::string newStr;

			while ((end = s.find(what, start)) != std::string::npos)
			{
				token = s.substr(start, end - start);
				start = end + splitterLen;
				newStr += token + with;
			}

			newStr += s.substr(start);
			return newStr;
		}
	};


	const std::string LStr::Empty = std::string();



	static LVec<LStr> StringSplit(const std::string& s, std::string delimiter)
	{
		return LStr::StringSplit(s, delimiter);
	}




	static LStr ReadWholeFile(const char* fname)
	{
		std::cout << "Current path is " << std::filesystem::current_path() << '\n';

		std::ifstream f(fname);
		if (!f)
			throw std::runtime_error("failed to open file: " + std::string(fname));

		std::ostringstream ss;
		ss << f.rdbuf();
		return LStr(ss.str());
	}
};