using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace VisualSolutionGenerator
{
    public abstract class BindableBase : System.ComponentModel.INotifyPropertyChanged
    {
        protected virtual void RaiseChanged(params string[] ps)
        {
            #if DEBUG
            if (ps != null)
            {
                foreach (var p in ps.Where(item => item != null))
                {
                    System.Diagnostics.Debug.Assert(this.GetType().GetTypeInfo().GetProperty(p) != null, "Property " + p + " in object " + this.GetType() + " not found.");
                }
            }
            #endif

            if (PropertyChanged == null) return;

            if (ps == null || ps.Length == 0) { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(null)); return; }

            foreach (var p in ps) PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(p));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}
