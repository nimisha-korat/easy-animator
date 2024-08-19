
namespace EasyAnimator.Units
{
    
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DegreesAttribute : UnitsAttribute
    {
        public DegreesAttribute() : base(" º") { }
    }

     

   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DegreesPerSecondAttribute : UnitsAttribute
    {
        public DegreesPerSecondAttribute() : base(" º/s") { }
    }

     

    
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersAttribute : UnitsAttribute
    {
        public MetersAttribute() : base(" m") { }
    }

     

     
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersPerSecondAttribute : UnitsAttribute
    {
        public MetersPerSecondAttribute() : base(" m/s") { }
    }

     

   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersPerSecondPerSecondAttribute : UnitsAttribute
    {
        public MetersPerSecondPerSecondAttribute() : base(" m/s²") { }
    }

     

  
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MultiplierAttribute : UnitsAttribute
    {
        public MultiplierAttribute() : base(" x") { }
    }

     

   
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class SecondsAttribute : UnitsAttribute
    {
        public SecondsAttribute() : base(" s") { }
    }

     
}

