using Capital.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capital.Entity
{
    public class Data
    {
        public Data(decimal depoStart, StrategyType strategyType)  // заплняем поля StrategyType и Depo через конструктор класса
        {
            StrategyType = strategyType;

            Depo = depoStart;
        }

        #region Properties ==========================================================================================================

        public StrategyType StrategyType { get; set; }

        /// <summary>
        /// Начальный депозит
        /// </summary>
        public decimal Depo
        {
            get => _depo;

            set
            {
                _depo = value;
                ResultDepo = value;
            }
        }
        decimal _depo;

        /// <summary>
        /// Результирующее эквити (депо)
        /// </summary>
        public decimal ResultDepo
        {
            get => _resultDepo;

            set
            {
                _resultDepo = value;

                Profit = ResultDepo - Depo;  // Расчитываем Profit

                PercentProfit = Profit * 100 / Depo;  // Расчитываем Profit в %

                ListEquity.Add(ResultDepo);  // сохраняем каждое значение эквити (депо) в список для расчета относительной просадки

                CalcDrowDown();  // метод расчета абсолютной просадки
            }
        }
        decimal _resultDepo;

        /// <summary>
        /// Абсолютный профит в деньгах
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// Относительный профит в процентах
        /// </summary>
        public decimal PercentProfit { get; set; }

        /// <summary>
        /// Максимальная абсолютная просадка в деньгах
        /// </summary>
        public decimal MaxDrawDown
        {
            get => _maxDrawDown;

            set
            {
                _maxDrawDown = value;

                CalPercentDrowDown();
            }
        }
        decimal _maxDrawDown;

        /// <summary>
        /// Максимальная относительная просадка в процентах
        /// </summary>
        public decimal PercentDrawDown { get; set; }

        #endregion

        #region Fields ==============================================================================================================

        List<decimal> ListEquity = new List<decimal>();  // создаем список меняющегося эквити (депо)

        private decimal _max = 0;

        private decimal _min = 0;

        #endregion


        #region Methods =============================================================================================================

        public List<decimal> GetListEquity()  // мотод который выдает наружу приватный список List<decimal>
        {
            return ListEquity;
        }

        private void CalcDrowDown()
        {
            if (ResultDepo > _max)  // если эквити (депо) пробивает максимум
            {
                _max = ResultDepo;  // сохраняем новый максимум
                _min = ResultDepo;  // обновляем минимум для расчета новой просадки
            }

            if (ResultDepo < _min)
            {
                _min = ResultDepo;

                if (_max - _min > MaxDrawDown)  // если появилась просадка больше предыдущей
                {
                    MaxDrawDown = _max - _min;  
                }
            }
        }
        /// <summary>
        /// Расчет относительной просадки
        /// </summary>
        private void CalPercentDrowDown() 
        {
            decimal percent = MaxDrawDown * 100 / ResultDepo;  // рассчитываем просадку в % от текущего депо

            if (percent > PercentDrawDown) PercentDrawDown = Math.Round(percent);  // округлил до целого числа
        }

        #endregion
    }
}
