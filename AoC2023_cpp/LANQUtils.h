#pragma once

#include "LANQLStr.h"
#include "LANQEnumerable.h"

namespace LANQ
{
	template<typename T>
	static LVec<T> LANQit(const std::vector<T>& v)
	{
		//TODO: nopt sure about this, mayb eownership problems?
		return LVec<T>(&v);
	}

	template<typename K, typename V>
	static LUMap<K,V> LANQit(const std::unordered_map<K,V>& v)
	{
		//todo, not sure about this, potential ownership problems?
		return LUMap<K,V>(&v);
	}

	template<typename K, typename V>
	static LUMap<K, V> LANQit(const std::shared_ptr<std::unordered_map<K, V>>& v)
	{
		return LUMap<K, V>(v);
	}

	template<typename T>
	static LStr LANQit(const std::string& v)
	{
		return LStr(v);
	}


	static LVec<LStr> StringSplit(const std::string& s, std::string delimiter)
	{
		return LStr::StringSplit(s, delimiter);
	}

	static std::string Trim(const std::string& s)
	{
		std::string::const_iterator it = s.begin();
		while (it != s.end() && isspace(*it))
			it++;

		std::string::const_reverse_iterator rit = s.rbegin();
		while (rit.base() != it && isspace(*rit))
			rit++;

		return std::string(it, rit.base());
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
}