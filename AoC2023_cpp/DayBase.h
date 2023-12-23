#pragma once

#include <string>
#include <format>

template<unsigned int Y, unsigned int D>
class DayBase
{
protected:
	std::string GetSampleFileName()
	{
		std::string path = std::format("..\\Input\\AoC{}\\Day{}_sample.txt", std::to_string(Y), std::to_string(D));
		return path;
	}

	std::string GetFileName()
	{
		std::string path = std::format("..\\Input\\AoC{}\\Day{}_1.txt", std::to_string(Y), std::to_string(D));
		return path;
	}

public:
	virtual void Run() = 0;

	void WriteLine(std::string str)
	{
		std::cout << str << '\n';
	}
};