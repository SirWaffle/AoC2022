#pragma once

#include "LANQCommonHeaders.h"

namespace LANQ
{
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

		ResultType& GetSuccessResult() { return successResult; }

		Result<ResultType>& OnSuccess(std::function<void(ResultType& res)> handler)
		{
			if (Success())
				handler(GetSuccessResult());
			return *this;
		}

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

		operator const Type& () const { return Value(); }

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
}