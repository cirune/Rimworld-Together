using System;
using System.Collections.Generic;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OnlineActivityData
    {
        public OnlineActivityStepMode _stepMode;

        public OnlineActivityType _activityType;

        //Map

        public MapFile _mapFile;

        public HumanFile[] _guestHumans = new HumanFile[0];

        public AnimalFile[] _guestAnimals = new AnimalFile[0];

        //Misc

        public string _engagerName;

        public int _fromTile;

        public int _toTile;

        //Orders

        public PawnOrderData _pawnOrder;

        public CreationOrderData[] _creationOrder = new CreationOrderData[0];

        public DestructionOrderData[] _destructionOrder = new DestructionOrderData[0];

        public DamageOrderData[] _damageOrder = new DamageOrderData[0];

        public HediffOrderData[] _hediffOrder = new HediffOrderData[0];

        public TimeSpeedOrderData[] _timeSpeedOrder = new TimeSpeedOrderData[0];

        public GameConditionOrderData[] _gameConditionOrder = new GameConditionOrderData[0];

        public WeatherOrderData[] _weatherOrder = new WeatherOrderData[0];
    }
}
