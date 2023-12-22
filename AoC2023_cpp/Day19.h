#pragma once

#include "CommonHeaders.h"
#include "LANQ.h"


using namespace LANQ;

class Day19
{
	const char* GetSampleFileName()
	{
		return "..\\Input\\AoC2023\\Day19_sample.txt";
	}

	const char* GetFileName()
	{
		return "..\\Input\\AoC2023\\Day19_1.txt";
	}

public:
	void Run()
	{
		DoPart1();
		DoPart2();
	}

	struct Item
	{
		enum Prop { x = 0, m, a, s, len };
		std::array<int, Prop::len> props;
	};

	struct Rule
	{
		int val = 0;
		char op = '\0'; //<, >, G for goto
		Item::Prop prop = Item::Prop::len;
		std::string res;

		Rule() = default;
		Rule(const LStr& lstr)
		{
			std::string str = lstr.ToStr();

			if (str.find(":") != std::string::npos)
			{
				switch (str[0])
				{
				case 'x': prop = Item::Prop::x; break;
				case 'm': prop = Item::Prop::m; break;
				case 'a': prop = Item::Prop::a; break;
				case 's': prop = Item::Prop::s; break;
				}

				op = str[1];
				LVec<LStr> splits = lstr.Split(":");
				val = std::atoi(splits[0].ToStr().substr(2).c_str());
				res = splits[1].ToStr();
			}
			else
			{
				op = 'G';
				res = str;
			}
		}

		Result<std::string> Process(const Item& i) const
		{
			switch (op)
			{
			case 'G': return Result<std::string>::Success(res);
			case '>': return Result<std::string>(i.props[prop] > val, res);
			case '<': return Result<std::string>(i.props[prop] < val, res);
			}
		}
	};

	struct WorkFlow
	{
		std::string name;
		LVec<Rule> rules;
		std::vector<Item> stack;

		WorkFlow() = default;
		WorkFlow(std::string _name) :name(_name) {}
	};

	void DoPart1()
	{
		LVec<LStr> input = ReadWholeFile(GetFileName()).Split("\n\n");

		LVec<Item> parts = input[1].Split("\n").Select<Item>([](const LStr& item, int ind) {
			auto ints = item.Remove("{").Remove("}").Split(",").Select<int>([](const LStr& item, int ind) { return item.Split("=")[1].AsInt(); });
			return Item{ { ints[0], ints[1], ints[2], ints[3] } };
			});

		LVec<WorkFlow> workflows = input[0].Split("\n").Select<WorkFlow>([](const LStr& item, int ind) {
			WorkFlow s;
			s.name = item.Split("{")[0];
			s.rules = item.Remove("}").Split("{")[1].Split(",").Select<Rule>([](const LStr& item, int ind) { return Rule(item); });
			return s;
			});

		auto workflowMap = workflows.Add(WorkFlow("A")).Add(WorkFlow("R")).ToUnorderedMap<std::string>([](const WorkFlow& item) { return item.name; });

		//now that we have parsed, do work
		//put items into proper workflow queues, loop until queues are empty
		for (auto const& part : parts.Data())
			(*workflowMap)["in"].stack.push_back(part);

		bool hasWork = true;
		while (hasWork)
		{
			bool didWork = false;
			for (auto& pairs : *workflowMap)
			{
				while (pairs.second.stack.size() > 0 && pairs.second.rules.Data().size()) //no rules means our dead end container, A, R
				{
					Item i = pairs.second.stack.back();
					pairs.second.stack.pop_back();

					for (auto const& rule : pairs.second.rules.Data())
					{
						auto result = rule.Process(i);
						if (result.Success())
						{
							didWork = true;
							(*workflowMap)[result.GetSuccessResult()].stack.push_back(i);
							break;
						}
					}
				}
			}

			hasWork = didWork;
		}

		//todo: this crashes due to ownership of the vector in stack. need to make more utils that copy, or dont attempt to take ownership
		LVec<Item> accepted( &(*workflowMap)["A"].stack );

		unsigned int total = accepted.Aggregate<unsigned int>(0, [](const unsigned int a, const Item& item) { return a + item.props[0] + item.props[1] + item.props[2] + item.props[3]; });

		std::cout << "done! value: " << total << "\n";
		//p1: 449531
	}

	void DoPart2()
	{

	}
};