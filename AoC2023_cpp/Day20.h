#pragma once

#include "CommonHeaders.h"

#include "CommonHeaders.h"

using namespace LANQ;

class Day20 : public DayBase<2023, 20>
{
public:
	void Run()
	{
		DoPart1();
	}

	enum PULSE
	{
		NoPulse = 0,
		LowPulse = 1,
		HighPulse = 2
	};

	struct NameValuePair
	{
		std::string name;
		PULSE value;
	};

	struct ModUpdate
	{
		std::string target;
		NameValuePair modOutput;
	};


	struct Module
	{
		using SPtr = std::shared_ptr<Module>;



		std::unordered_map<std::string, Module::SPtr> InputModules;
		std::unordered_map<std::string, Module::SPtr> OutputModules;

		LVec<std::string> OutputNames;
		std::string name;

		std::unordered_map<std::string, NameValuePair> InputValues;
		PULSE OutputValue = NoPulse;

		virtual void InitModule()
		{
			InputValues = LANQit(InputModules).Select<NameValuePair>([this](const auto& item, int ind) { return NameValuePair{ item.first, NoPulse }; }).ToUnorderedMap<std::string>([](const NameValuePair& nvp) {return nvp.name;});
		}

		virtual void SetInput(const NameValuePair& nvp)
		{
			InputValues[nvp.name] = nvp;
		}

		PULSE GetOutput() const 
		{ 
			std::cout << name << " -" << OutputValue << "-";
			return OutputValue; 
		}

		virtual void Process() = 0;
	};


	struct FlipFlop : public Module
	{
		bool OnState = false;

		virtual void SetInput(const NameValuePair& nvp) override
		{
			if (nvp.value == LowPulse)
			{
				OnState = !OnState;

				if (OnState)
					OutputValue = HighPulse;
				else
					OutputValue = LowPulse;

				return;
			}
			OutputValue = NoPulse;
		}

		virtual void Process() override
		{
		};
	};

	struct Conjunction : public Module
	{
		bool OnState = false;

		virtual void InitModule()
		{
			Module::InitModule();
			for (auto& nvp : InputValues)
				InputValues[nvp.first].value = LowPulse;
		}

		virtual void Process() override
		{
			bool allHigh = LANQit(InputValues).All([this](const auto& pair) { return pair.second.value == HighPulse; });
			if (allHigh)
				OutputValue = LowPulse;
			else
				OutputValue = HighPulse;
		};
	};

	struct Broadcast : public Module
	{
		virtual void SetInput(const NameValuePair& nvp) override
		{
			OutputValue = nvp.value;
		}

		virtual void Process() override
		{
		};
	};

	struct Output : public Module
	{
		virtual void SetInput(const NameValuePair& nvp) override
		{
			OutputValue = nvp.value;
			std::cout << "****** Output Module Value: " << OutputValue << "\n";
		}

		virtual void Process() override
		{
		};
	};

	std::shared_ptr<Module> ModFactory(std::string name, std::vector<std::string> outputs)
	{
		std::shared_ptr<Module> mod;
		if (name[0] == '%')
		{
			mod.reset(new FlipFlop());
		}
		else if (name[0] == '&')
		{
			mod.reset(new Conjunction());
		}
		else if (name == "broadcaster")
		{
			name = "#" + name;
			mod.reset(new Broadcast());
		}
		else if (name == "output")
		{
			name = "#" + name;
			mod.reset(new Broadcast());
		}

		mod->name = name.substr(1);
		mod->OutputNames = LANQit(outputs);

		return mod;
	}

	void DoPart1()
	{
		//create map of modules
		auto modMap = LANQit( ReadWholeFile(GetSampleFileName().c_str()).Split("\n").Select<Module::SPtr>([this](const LStr& item, int ind) {  
				auto splits = item.Remove(" ").Split("->");
				return ModFactory(splits[0].ToStr(), splits[1].Split(",").Select<std::string>([](const LStr& item, int ind) { return item.ToStr(); }).ToVector());
			}).Add(ModFactory("output", std::vector<std::string>())).ToUnorderedMap<std::string>([](const Module::SPtr& item) { return item->name; }) );
		
		//wireup the inputs/outputs
		for (auto& mod : modMap)
		{
			for (std::string& name : mod.second->OutputNames.ToVector())
			{
				mod.second->OutputModules[name] = modMap[name];
				mod.second->OutputModules[name]->InputModules[mod.second->name] = mod.second;
			}
		}

		for (auto& mod : modMap)
			mod.second->InitModule();

		//fire the button and resolve
		int numPresses = 1;
		for (int i = 0; i < numPresses; ++i)
		{
			std::vector<ModUpdate> processQueue;

			Module::SPtr button = modMap["broadcaster"];
			NameValuePair buttonPress{ "button", LowPulse };
			button->SetInput(buttonPress);
			for (const std::string& name : button->OutputNames)
			{
				ModUpdate up;
				up.target = name;
				up.modOutput.name = button->name;
				up.modOutput.value = button->GetOutput();

				std::cout << "> " + name << "\n";

				processQueue.push_back(up);
			}

			while (processQueue.size() > 0)
			{
				//process
				ModUpdate modUpdate = processQueue.front();
				std::swap(processQueue.front(), processQueue.back()); //emulate a queue with vector
				processQueue.pop_back();

				Module::SPtr mod = modMap[modUpdate.target];

				mod->SetInput(modUpdate.modOutput);

				mod->Process();

				if (mod->name == "output")
					continue;

				//push back everything but output
				for (const std::string& name : mod->OutputNames)
				{
					ModUpdate up;
					up.target = name;
					up.modOutput.name = mod->name;
					up.modOutput.value = mod->GetOutput();

					std::cout << "> " + name << "\n";

					//if we update entries, when B is processed, it processes every pulse sent ot it at once
					//pushing_back instead of updating is not what is wanted for part 1
					processQueue.push_back(up); //if we pushg back everything, pulses arrive in order they are created

				}
			}
		}

		std::cout << "Final output value: " << modMap["output"]->GetOutput() << "\n";
	}

};