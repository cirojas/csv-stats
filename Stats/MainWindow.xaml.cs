using CsvHelper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Stats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Columna
    {
        public string Nombre_Columna { get; set; }
        public bool Seleccionada { get; set; }
    }

    public partial class MainWindow : Window
    {
        private const char COMMA = ',';
        private const char SEMICOLON = ';';
        private const char BAR = '|';

        public string Text { get; set; }

        private Window codePopup;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (ListaOpciones.SelectedIndex == 0) //distintos
                    {
                        CountDistincts(openFileDialog.FileName, GetSeparator());
                    }
                    else if (ListaOpciones.SelectedIndex == 1) //frecuencia
                    {
                        CountFrequency(openFileDialog.FileName, GetSeparator());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(),
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Button thisButton = (Button)sender;
            codePopup.Close();
        }

        private char GetSeparator()
        {
            if (ListaSeparador.SelectedIndex == 0) return COMMA;
            else if (ListaSeparador.SelectedIndex == 1) return SEMICOLON;
            else return BAR;
        }

        private void CountDistincts(string fileName, char splitter)
        {
            string[] header = null;
            HashSet<string>[] values = null;
            List<Columna> columnas = null;

            using (StreamReader sr = File.OpenText(fileName))
            {
                bool firstRow = true;
                string s = string.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //do minimal amount of work here
                    string[] row = s.Split(splitter);

                    if (firstRow)
                    {
                        header = row;
                        values = new HashSet<string>[row.Length];
                        firstRow = false;

                        for (int i = 0; i < row.Length && i < values.Length; i++)
                        {
                            values[i] = new HashSet<string>();
                        }

                        columnas = row.Select(x => new Columna() { Nombre_Columna = x, Seleccionada = true }).ToList();
                        SeleccionarColumnas(columnas);
                    }
                    else
                    {
                        for (int i = 0; i < row.Length && i < values.Length; i++)
                        {
                            string key = row[i].Trim();
                            if (!values[i].Contains(key))
                            {
                                values[i].Add(key);
                            }
                        }
                    }
                }
            }

            if (ListaExport.SelectedIndex == 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < header.Length; i++)
                {
                    if (!columnas[i].Seleccionada)
                    {
                        continue;
                    }

                    string columnTitle = header[i];
                    stringBuilder.AppendLine(columnTitle);

                    bool first = true;
                    foreach (var key in values[i])
                    {
                        if (first)
                        {
                            stringBuilder.Append(string.Format("{0}", key));
                            first = false;
                        }
                        else
                        {
                            stringBuilder.Append(string.Format(", {0}", key));
                        }
                    }

                    stringBuilder.AppendLine(string.Empty);
                    stringBuilder.AppendLine(string.Empty);
                }

                OutputTextBlock.Text = stringBuilder.ToString();
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Archivo de texto|*.txt";
                saveFileDialog.Title = "Guardar";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    try
                    {
                        using (TextWriter writer = File.CreateText(saveFileDialog.FileName))
                        {
                            for (int i = 0; i < header.Length; i++)
                            {
                                if (!columnas[i].Seleccionada)
                                {
                                    continue;
                                }

                                string columnTitle = header[i];
                                writer.WriteLine(columnTitle);

                                bool first = true;
                                foreach (var key in values[i])
                                {
                                    if (first)
                                    {
                                        writer.Write(string.Format("{0}", key));
                                        first = false;
                                    }
                                    else
                                    {
                                        writer.Write(string.Format(", {0}", key));
                                    }
                                }

                                writer.WriteLine(string.Empty);
                                writer.WriteLine(string.Empty);
                            }
                        }
                        MessageBox.Show("Se ha finalizado exitosamente.", "Listo!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(),
                                              "Error",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CountFrequency(string fileName, char splitter)
        {
            string[] header = null;
            Dictionary<string, int>[] values = null;
            List<Columna> columnas = null;

            using (StreamReader sr = File.OpenText(fileName))
            {
                bool firstRow = true;
                string s = string.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    //do minimal amount of work here
                    string[] row = s.Split(splitter);

                    if (firstRow)
                    {
                        header = row;
                        values = new Dictionary<string, int>[row.Length];
                        firstRow = false;

                        for (int i = 0; i < row.Length && i < values.Length; i++)
                        {
                            values[i] = new Dictionary<string, int>();
                        }

                        columnas = row.Select(x => new Columna() { Nombre_Columna = x, Seleccionada = true }).ToList();
                        SeleccionarColumnas(columnas);
                    }
                    else
                    {
                        for (int i = 0; i < row.Length && i < values.Length; i++)
                        {
                            string key = row[i].Trim();
                            if (values[i].ContainsKey(key))
                            {
                                values[i][key] = values[i][key] + 1;
                            }
                            else
                            {
                                values[i].Add(key, 1);
                            }
                        }
                    }
                }
            }

            if (ListaExport.SelectedIndex == 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < header.Length; i++)
                {
                    if (!columnas[i].Seleccionada)
                    {
                        continue;
                    }
                    string columnTitle = header[i];
                    stringBuilder.AppendLine(columnTitle);

                    foreach (var kvp in values[i])
                    {
                        stringBuilder.AppendLine(string.Format("{0}: {1}", kvp.Key, kvp.Value));
                    }

                    stringBuilder.AppendLine(string.Empty);
                }
                OutputTextBlock.Text = stringBuilder.ToString();
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Archivo de texto|*.txt";
                saveFileDialog.Title = "Guardar";
                saveFileDialog.ShowDialog();

                if (saveFileDialog.FileName != "")
                {
                    try
                    {
                        using (TextWriter writer = File.CreateText(saveFileDialog.FileName))
                        {
                            for (int i = 0; i < header.Length; i++)
                            {
                                if (!columnas[i].Seleccionada)
                                {
                                    continue;
                                }
                                string columnTitle = header[i];
                                writer.WriteLine(columnTitle);

                                foreach (var kvp in values[i])
                                {
                                    writer.WriteLine(string.Format("{0}: {1}", kvp.Key, kvp.Value));
                                }

                                writer.WriteLine(string.Empty);
                            }
                        }
                        MessageBox.Show("Se ha finalizado exitosamente.", "Listo!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(),
                                              "Error",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SeleccionarColumnas(List<Columna> columnas)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(5);
            DataGrid simpleTable = new DataGrid();

            simpleTable.ItemsSource = columnas;
            simpleTable.AutoGeneratingColumn += SimpleTable_AutoGeneratingColumn;
            simpleTable.CanUserAddRows = false;
            simpleTable.CanUserSortColumns = false;

            stackPanel.Children.Add(simpleTable);

            Button okButton = new Button();
            okButton.Content = "OK";
            okButton.Click += OkButton_Click;
            okButton.Width = 50;
            okButton.Margin = new Thickness(10);

            stackPanel.Children.Add(okButton);

            codePopup = new Window();
            codePopup.SizeToContent = SizeToContent.WidthAndHeight;
            codePopup.WindowStyle = WindowStyle.SingleBorderWindow;
            codePopup.Owner = this;
            codePopup.Content = stackPanel;
            codePopup.VerticalAlignment = VerticalAlignment.Center;
            codePopup.HorizontalAlignment = HorizontalAlignment.Center;
            codePopup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            codePopup.Title = "Selecciona las columnas a mostrar";

            codePopup.ShowDialog();
        }

        private void SimpleTable_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column is DataGridCheckBoxColumn && !e.Column.IsReadOnly)
            {
                var checkboxFactory = new FrameworkElementFactory(typeof(CheckBox));
                checkboxFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                checkboxFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                checkboxFactory.SetBinding(ToggleButton.IsCheckedProperty, new Binding(e.PropertyName) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

                e.Column = new DataGridTemplateColumn
                {
                    Header = e.Column.Header,
                    CellTemplate = new DataTemplate { VisualTree = checkboxFactory },
                    SortMemberPath = e.Column.SortMemberPath
                };
            }
        }
    }
}
