using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualSolutionGenerator
{
    static class Extensions
    {
        public static void InvokeWithUserInterface(this Action action)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                action.Invoke();
            }

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
            }
        }

    }
}
