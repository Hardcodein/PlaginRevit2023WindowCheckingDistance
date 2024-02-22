using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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


namespace Creative_Project_BIM_Programming
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public  partial class Set_Distance_Window : Window
    {
        public Document document;
        // constructor
        public  double DoubleDistance = 1 ;
        public bool IsCorrectDistance = true;
        public Set_Distance_Window(Document doc)
        {
            // assign value to field
            document = doc;
            InitializeComponent();
            
           
        }

        public double Return_Distance()
        {
            return DoubleDistance;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxDistance.Text.ToString(), out  DoubleDistance))
            {
                TaskDialog.Show("Данные", "Вы успешно ввели данные");
                Close();
            }
            else
            {
                Close();
                TaskDialog.Show("Ошибка", "Введены несоответствующие данные");
                IsCorrectDistance = false;
                

            }
            
            
        }
    }
}
