using BTD6AutoCommunity.Core;
using BTD6AutoCommunity.ScriptEngine.InstructionSystem;
using BTD6AutoCommunity.ScriptEngine.ScriptSystem;
using BTD6AutoCommunity.ScriptEngine;
using BTD6AutoCommunity.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTD6AutoCommunity.Services
{
    public class StrategyManager
    {
        private readonly ScriptSettings _settings;
        private readonly LogHandler _logHandler;

        private CustomStrategy CustomStrategy;
        private CollectionStrategy CollectionStrategy;
        private CirculationStrategy CirculationStrategy;
        private RaceStrategy RaceStrategy;

        public StrategyManager(ScriptSettings settings, LogHandler logHandler)
        {
            _settings = settings;
            _logHandler = logHandler;
        }

        public void RunStrategy(UserSelection selection)
        {
            FunctionTypes func = selection.selectedFunction;
            if (func == FunctionTypes.Custom)
            {
                RunCustomStrategy(selection);
            }
            else if (func == FunctionTypes.Collection)
            {
                RunCollectionStrategy();
            }
            else if (func == FunctionTypes.Circulation)
            {
                RunCirculationStrategy(selection);
            }
            else if (func == FunctionTypes.Race)
            {
                RunRaceStrategy(selection);
            }
        }

        public void StopAll()
        {
            if (CustomStrategy != null)
            {
                CustomStrategy.Stop();
                CustomStrategy = null;
            }
            if (CollectionStrategy != null)
            {
                CollectionStrategy.Stop();
                CollectionStrategy = null;
            }
            if (CirculationStrategy != null)
            {
                CirculationStrategy.Stop();
                CirculationStrategy = null;
            }
            if (RaceStrategy != null)
            {
                RaceStrategy.Stop();
                RaceStrategy = null;
            }
        }

        private void RunCustomStrategy(UserSelection selection)
        {
            CustomStrategy = new CustomStrategy(_logHandler, selection);
            if (CustomStrategy.ReadyToStart)
            {
                CustomStrategy.Start();
            }
        }

        private void RunCollectionStrategy()
        {
            CollectionStrategy = new CollectionStrategy(_logHandler);
            if (CollectionStrategy.ReadyToStart)
            {
                CollectionStrategy.Start();
            }
        }

        private void RunCirculationStrategy(UserSelection selection)
        {
            CirculationStrategy = new CirculationStrategy(_logHandler, selection);
            if (CirculationStrategy.ReadyToStart)
            {
                CirculationStrategy.Start();
            }
        }

        private void RunRaceStrategy(UserSelection selection)
        {
            RaceStrategy = new RaceStrategy(_logHandler, selection);
            if (RaceStrategy.ReadyToStart)
            {
                RaceStrategy.Start();
            }
        }
    }
}
