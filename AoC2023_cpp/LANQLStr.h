#pragma once

#include <string>
#include "LANQDataRefs.h"
#include "LANQEnumerable.h"

namespace LANQ
{
	class LStr : public LDataRef<std::string>
	{
	public:
		LStr() = default;

		LStr(const std::string& s) : LDataRef<std::string>(s)
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
			std::shared_ptr<std::vector<LChar>> conv(new std::vector<LChar>());
			conv->reserve(Value().size());

			for (auto const& c : Value())
				conv->push_back(c);

			LVec<LChar> out(conv);
			return out;
		}

		LVec<LStr> Split(std::string delimiter) const
		{
			return StringSplit(Value(), delimiter);
		}

		LStr Remove(std::string str) const
		{
			return Replace(Value(), str, "");
		}

		LStr Replace(std::string str, std::string with) const
		{
			return Replace(Value(), str, with);
		}

		int ToInt() const
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
				result.GetMutableContainer().push_back(new std::string(token));
			}

			result.GetMutableContainer().push_back(new std::string(s.substr(start)));
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
}