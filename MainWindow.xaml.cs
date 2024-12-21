/* 
 * Фиксированный лот. Торговля происходит всегда с одним и тем же лотом. То есть прибыль и убыток всегда неизменны.
 * 
 * Капитализация. Рабочий лот рассчитывается исходя из текущего депо. Если депо растёт, то лот увеличивается.
 * Если депо уменьшается, то рабочий лот не изменяется.
 * 
 * Прогрессирующий. Если предидущая сделка была прибыльной, то увеличиваем рабочий лот на соотношения тейка и стопа. 
 * (Пример: тейк 300, стоп 100, коэффициент увеличения лота равен 300/100 = 3).
 * 
 * Понижение. Здесь обратная стратегия - если текущая сделк была убыточной, то следующую совершаем в два раза меньшим обьемом. 
 * Если получили прибыль, то лот сразу возвращается к первоначальному размеру.
 */


using Capital.Enums;
using Capital.Entity;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using System;

namespace Capital
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Init();
        }


        #region Fields =====================================================================

        List<StrategyType> _strategies = new List<StrategyType>()  // Создаем список типа ENUM и заполняем его стратегиями (для ComboBox)
        {
            StrategyType.FIX,
            StrategyType.CAPITALIZATION,
            StrategyType.PROGRESS,
            StrategyType.DOWNGRADE
        };

        Random _random = new Random();

        List<Data> datas;

        bool combobox_Drow_on = false;  // переменная для включения / выключения метода Drow после метода 

        //=====================

        private void eventhandler(object sender, EventArgs e)
        {
            Drow(datas);
        }

        //==========================
        #endregion


        #region Methods ====================================================================

        private void Init()  // Инициализация / заполнение всех данных в окне
        {
            _comboBox.ItemsSource = _strategies;  // Добавляем в комбобокс список объектов _strategies (созданный из ENUM)

            _comboBox.SelectionChanged += _comboBox_SelectionChanged;  // подписываемся на событие выбора поля в комбобоксе 
            _comboBox.SelectedIndex = 0;  // присваиваем начальное значение comboBox (что-бы было не пустое значение)

            _depo.Text = "100000";
            _startLot.Text = "10";
            _take.Text = "300";
            _stop.Text = "100";
            _comiss.Text = "5";
            _percentProfit.Text = "30";
            _countTrades.Text = "1000";
            _minStartPercent.Text = "20";
            _go.Text = "5000";
        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)//Метод срабатывает при выборе стратегии в ComboBox через событие выше
        {
            ComboBox comboBox = (ComboBox)sender;  // берем наш ComboBox типа объект и преобразуем его в тип ComboBox и сохраняем в переменную comboBox

            int index = comboBox.SelectedIndex;  // Берем индекс с нашей переменной типа ComboBox и записываем его в переменную index

            if (combobox_Drow_on)  Drow(datas);   // при первой отработке метода Calculat станет True
        }

        private void Button_Click(object sender, RoutedEventArgs e)  // Метод вызывается при клике на кнопку рассчитать
        {
            List<Data> datas = Calculate();

            Drow(datas);
        }

        private List<Data> Calculate()
        {
            combobox_Drow_on = true;

            decimal depoStart = GetDecimalFromString(_depo.Text);  // Конвертируем данные в decimal и сохраняем в переменные
            int startLot = GetIntFromString(_startLot.Text);
            decimal take = GetDecimalFromString(_take.Text);
            decimal stop = GetDecimalFromString(_stop.Text);
            decimal comiss = GetDecimalFromString(_comiss.Text);
            decimal percProfit = GetDecimalFromString(_percentProfit.Text);
            int countTrades = GetIntFromString(_countTrades.Text);
            decimal minStartPercent = GetDecimalFromString(_minStartPercent.Text);
            decimal go = GetDecimalFromString(_go.Text);

            datas = new List<Data>();  // Создаем список объектов с данными для таблицы

            foreach (StrategyType type in _strategies) 
            {
                datas.Add(new Data(depoStart, type));   // вызываем конструктор класса для заполнения полей StrategyType и Depo
            }

            int lotPercent = startLot;
            decimal percent = startLot * go * 100 / depoStart;

            decimal multiply = take / stop;  // коэфицент для т-3 стратегии
            int lotProgress = CalculateLot(depoStart, minStartPercent, go);

            int lotDown = startLot;

            for (int i = 0; i < countTrades; i++)
            {
                int rnd = _random.Next(1, 100);  // рандомное значение от 1 до 100

                if (rnd <= percProfit)  // Сделка прибыльная
                {
                    //================ 1 strategy ============================================ 

                    datas[0].ResultDepo += (take - comiss) * startLot;

                    //================ 2 strategy ============================================ 

                    datas[1].ResultDepo += (take - comiss) * lotPercent;

                    int newLot = CalculateLot(datas[1].ResultDepo, percent, go);

                    if (lotPercent < newLot) lotPercent = newLot;

                    //================ 3 strategy ============================================ 

                    datas[2].ResultDepo += (take - comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent * multiply, go);

                    //================ 4 strategy ============================================ 

                    datas[3].ResultDepo += (take - comiss) * lotDown;

                    lotDown = startLot;

                }
                else  // Сделка убыточная
                {
                    //================ 1 strategy ============================================ 

                    datas[0].ResultDepo -= (stop + comiss) * startLot;

                    //================ 2 strategy ============================================ 

                    datas[1].ResultDepo -= (stop + comiss) * lotPercent;
                    //================ 3 strategy ============================================ 

                    datas[2].ResultDepo -= (stop + comiss) * lotProgress;

                    lotProgress = CalculateLot(depoStart, minStartPercent, go);

                    //================ 4 strategy ============================================ 

                    datas[3].ResultDepo -= (stop + comiss) * lotDown;

                    lotDown /= 2;

                    if (lotDown == 0) lotDown = 1;
                }
            }

            _dataGrid.ItemsSource = datas;  // Заполняем окно данных _dataGrid листом с объектами datas

            return datas;
        }

        /// <summary>
        /// Метод рисования графика эквити
        /// </summary>
        /// <param name="datas"></param>
        private void Drow(List<Data> datas)   // метод рисования графика эквити
        {
            _canvas.Children.Clear();

            int index = _comboBox.SelectedIndex;  // сохраняем индекс выбраной стратегии в переменную

            List<decimal> listEquity = datas[index].GetListEquity();  // лист эквити (накопленный капитал)

            int count = listEquity.Count;  // сохраняем количество сделок (точек, для рисования графика)
            decimal maxEquity = listEquity.Max();  // сохраняем max число
            decimal minEquity = listEquity.Min();   // сохраняем min число

            double stepX = _canvas.ActualWidth / count;  // высчитываем нужный шаг по горизонтали на поле для графика

            double koef = (double)(maxEquity - minEquity) / _canvas.ActualHeight;

            double x = 0;
            double y = 0;

            double last_x1 = 0;
            double last_y1 = 0;

            for (int i = 0; i < count; i++)
            {
                y = _canvas.ActualHeight - (double)(listEquity[i] - minEquity) / koef;  // вычитание делается для зеркаливания т.к. y считается сверху вниз, а нам надо наоборот
                
                /*
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 10;
                ellipse.Height = 10;
                ellipse.Stroke = Brushes.DarkBlue; 

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);  // расчет сверху вниз

                _canvas.Children.Add(ellipse);
                */

                Line line = new Line();

                line.X1 = last_x1;
                line.Y1 = last_y1;
                line.X2 = x;
                line.Y2 = y;

                line.Stroke = Brushes.Black;

                _canvas.Children.Add(line);

                last_x1 = x;
                last_y1 = y;

                x += stepX;
            }
        }

        private int CalculateLot(decimal currentDepo, decimal percent, decimal go)  // расчет лота для второй стратегии CAPITALIZATION
        {
            if (percent > 100) { percent = 100; }

            decimal lot = currentDepo / go / 100 * percent;

            return (int)lot;
        }

        private decimal GetDecimalFromString(string str)  // Метод конвертации строки в decimal
        {
            if (decimal.TryParse(str, out decimal result)) return result;  // Если конвертировать получилось, то возвращаем результат

            return 0;  // Если конвертация не удалась, то возвращаем 0
        }

        private int GetIntFromString(string str)  // Метод конвертации строки в int
        {
            if (int.TryParse(str, out int result)) return result;  // Если конвертировать получилось, то возвращаем результат

            return 0;  // Если конвертация не удалась, то возвращаем 0
        }
        #endregion
    }
}