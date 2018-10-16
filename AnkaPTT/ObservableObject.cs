using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AnkaPTT
{
    public class ObservableObject :INotifyPropertyChanged
    {
        private static Dictionary<string, Dictionary<string, string[]>> calculatedPropertiesOfTypes = new Dictionary<string, Dictionary<string, string[]>>();

        private readonly bool hasComputedProperties;
        public ObservableObject()
        {
            Type t = GetType();
            if (!calculatedPropertiesOfTypes.ContainsKey(t.FullName))
            {
                var props = t.GetProperties();

                foreach (var pInfo in props)
                {
                    var attr = pInfo.GetCustomAttribute<CalculatedProperty>(false);
                    if (attr == null)
                        continue;

                    if (!calculatedPropertiesOfTypes.ContainsKey(t.FullName))
                    {
                        calculatedPropertiesOfTypes[t.FullName] = new Dictionary<string, string[]>();
                    }
                    calculatedPropertiesOfTypes[t.FullName][pInfo.Name] = attr.Properties;
                }
            }

            if (calculatedPropertiesOfTypes.ContainsKey(t.FullName))
                hasComputedProperties = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            if (this.hasComputedProperties)
            {
                //check for any computed properties that depend on this property
                var computedPropNames =
                    calculatedPropertiesOfTypes[this.GetType().FullName]
                    .Where(kvp => kvp.Value.Contains(propertyName))
                    .Select(kvp => kvp.Key);

                if (computedPropNames != null)
                    if (!computedPropNames.Any())
                        return;

                //raise property changed for every computed property that is dependant on the property we did just set
                foreach (var computedPropName in computedPropNames)
                {
                    //to avoid stackoverflow as a result of infinite recursion if a property depends on itself!
                    if (computedPropName == propertyName)
                        throw new InvalidOperationException("A property can't depend on itself");

                    OnPropertyChanged(computedPropName);
                }
            }
        }


        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class CalculatedProperty : Attribute
    {
        private string[] _props;
        public CalculatedProperty(params string[] props)
        {
            this._props = props;
        }

        public string[] Properties
        {
            get
            {
                return _props;
            }
        }
    }
}
