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
		//DoPart1();
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
			assert(false);
		}
	};

	struct WorkFlow
	{
		std::string name;
		LVec<Rule> rules;
		std::vector<Item> stack;

		WorkFlow() = default;
		WorkFlow(std::string _name) :name(_name) {}
		WorkFlow(std::string _name, LVec<Rule> _rules) :name(_name), rules(_rules) {}
	};

	void DoPart1()
	{
		LVec<LStr> input = ReadWholeFile(GetFileName()).Split("\n\n");

		LVec<Item> parts = input[1].Split("\n").Select<Item>([](const LStr& item, int ind) {
				auto ints = item.Remove("{").Remove("}").Split(",").Select<int>([](const LStr& item, int ind) { return item.Split("=")[1].ToInt(); });
				return Item{ { ints[0], ints[1], ints[2], ints[3] } };
			});

		std::shared_ptr<std::unordered_map<std::string, WorkFlow>> 
			workflowMap = input[0].Split("\n").Select<WorkFlow>([](const LStr& item, int ind) {
					return WorkFlow(item.Split("{")[0], 
									item.Remove("}").Split("{")[1].Split(",").Select<Rule>([](const LStr& item, int ind) { return Rule(item); }));
				})
				.Add(WorkFlow("A")).Add(WorkFlow("R")).ToSharedUnorderedMap<std::string>([](const WorkFlow& item) { return item.name; });

		//now that we have parsed, do work
		//put items into proper workflow queues, loop until queues are empty
		for (auto const& part : parts.GetContainer())
			(*workflowMap)["in"].stack.push_back(part);

		bool hasWork = true;
		while (hasWork)
		{
			hasWork = false;
			for (auto& pairs : *workflowMap)
			{
				while (pairs.second.stack.size() > 0 && pairs.second.rules.GetContainer().size()) //no rules means our dead end container, A, R
				{
					Item i = pairs.second.stack.back();
					pairs.second.stack.pop_back();

					for (auto const& rule : pairs.second.rules.GetContainer())
					{
						if (rule.Process(i).OnSuccess([&hasWork, &workflowMap, &i](std::string& res) { hasWork = true; (*workflowMap)[res].stack.push_back(i); }).Success())
							break;
					}
				}
			}
		}

		//todo: this crashes on exit due to ownership of the vector in stack. need to make more utils that copy, or dont attempt to take ownership
		LVec<Item> accepted( &(*workflowMap)["A"].stack );
		std::cout << "done! value: " << accepted.Aggregate<unsigned int>(0, [](const unsigned int a, const Item& item) { return a + item.props[0] + item.props[1] + item.props[2] + item.props[3]; }) << "\n";
		//p1: 449531
	}

	void DoPart2()
	{
	}
};