using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualSolutionGenerator.Themes
{    
    public partial class ProjectsGridView : UserControl
    {
        public ProjectsGridView()
        {
            InitializeComponent();
        }

        private void _Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var link = e.OriginalSource as Hyperlink; if (link == null) return;

            System.Diagnostics.Process.Start(link.NavigateUri.AbsoluteUri);
        }
    }
}
