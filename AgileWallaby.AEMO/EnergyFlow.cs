using Ardalis.SmartEnum;

namespace AgileWallaby.AEMO
{
    public class EnergyFlow: SmartEnum<EnergyFlow>
    {
        public static EnergyFlow Import = new EnergyFlow(nameof(Import), 1);
        public static EnergyFlow Export = new EnergyFlow(nameof(Export), 2);
        
        private EnergyFlow(string name, int value) : base(name, value)
        {
        }
    }

    public class MeasurementType : SmartEnum<MeasurementType>
    {
        public static MeasurementType Average = new MeasurementType("Average", 1);
        public static MeasurementType Master = new MeasurementType("Master", 2);
        public static MeasurementType Check = new MeasurementType("Check", 3);
        public static MeasurementType Net = new MeasurementType("Net", 4);
        
        private MeasurementType(string name, int value) : base(name, value)
        {
        }
    }

    public class EnergyMetric : SmartEnum<EnergyMetric>
    {
        
        private EnergyMetric(string name, int value) : base(name, value)
        {
        }
    }
}