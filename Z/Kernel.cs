/*
 * Слеповичев И.И. 02.02.2018.
 * Модуль ядра содержит класс ядра Kernel. Класс является минимальной неделимой структурной единицей
 * схемы вычислений. Содержит в себе все необходимые элементы для получения исходных данных,
 * производства вычислений, передачи результатов вычислений.
 * ------------------------------------------------------------------------------------------------------------
 * Основной цикл вычислений состоит из формирования сообщения и передачи этого сообщения.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z
{
    public class Kernel
    {
        /// <summary>
        /// Идентификатор (имя) ядра. Должен быть уникальным в составе другого ядра.
        /// </summary>
        public string Id;
        /// <summary>
        /// Идентификатор с именем выхода. Только сообщения с Id = InputId будут приниматься данным ядром.
        /// По умолчанию инициализируется значением Id
        /// </summary>
        public string OutputId;
        /// <summary>
        /// Идентификатор с именем выхода. Ядро будет отправлять сообщения с Id = OutputId.
        /// По умолчанию инициализируется значением Id
        /// </summary>
        public string InputId;
        /// <summary>
        /// Пакет данных с результатами вычислений данного ядра
        /// </summary>
        public Message State;
        /// <summary>
        /// Словарь с дочерними ядрами
        /// </summary>
        public Dictionary<string, Kernel> Childs;

        //TODO: Сделать типизацию выражения. Расширить до типов: char, bool, int8, int32, int64, string.
        /// <summary>
        /// Выражение, вычисляемое ядром. Выражение вычисляет значение типа double и может быть инициализировано значением дочернего ядра.
        /// Может принимать значение null
        /// </summary>
        private Expression expression;

        //TODO: Сделать типизацию вычисляемого ядром значения. На первом этапе: char, bool, int8, int32, int64, string.
        /// <summary>
        /// Результат вычислений ядра
        /// </summary>
        public double Data = 0;

        public Expression Expression
        {
            get
            {
                return expression;
            }
            set
            {
                expression = value;
                if (expression != null)
                    expression.Owner = this;
            }
        }

        public Kernel this[string id]
        {
            get
            {
                return Childs[id];
            }
            set
            {
                Childs[id] = value;
            }
        }

        public Kernel()
        {
            Id = Guid.NewGuid().ToString();
            OutputId = Id;
            InputId = Id;
        }

        public Kernel(string id)
        {
            Id = id;
            OutputId = Id;
            InputId = Id;
        }

        public Kernel(string id, string inputId, string outputId)
        {
            Id = id;
            InputId = inputId;
            OutputId = outputId;
        }

        public Kernel(string id, string inputId, string outputId, Expression expr)
        {
            Id = id;
            InputId = inputId;
            OutputId = outputId;
            Expression = expr;
        }

        public Kernel(string id, Expression expr)
        {
            Id = id;
            OutputId = Id;
            InputId = Id;
            Expression = expr;
        }

        public Kernel(string id, Expression expr, params Kernel[] childs)
        {
            Id = id;
            OutputId = Id;
            InputId = Id;
            Expression = expr;
            Childs = childs.ToDictionary(c => c.Id);
        }

        public Kernel(string id, string inputId, string outputId, Expression expr, params Kernel[] childs)
        {
            Id = id;
            InputId = inputId;
            OutputId = outputId;
            Expression = expr;
            Childs = childs.ToDictionary(c => c.Id);
        }

        /// <summary>
        /// Метод формирует пакет данных для передачи на другие ядра
        /// </summary>
        public Message Get()
        {
            //Проверяем возможность/необходимость отправки сообщения ядром
            if (!ReadyToSendMessage()) return null;
            //Создаем сообщение с идентификатором OutputId
            State = new Message(OutputId);
            if (Childs != null)
            {
                //Добавляем в пакет данных дочернее сообщение из дочерних ядер. Дочернее сообщение будет проигнорировано, если = null
                foreach (var c in Childs.Values) State.Put(c.Get());
            }
            //TODO: Отделить процесс формирования и отправки сообщения от вычислений.
            //Формируем пакет данных, вычисляя значение ядра
            if (Expression != null)
            {
                Data = Expression.Calculate();
            }
            State.Data = Data;
            return State;
        }

        /// <summary>
        /// Функция возвращает истину, если ядро готово отправить сообщение
        /// </summary>
        /// <returns></returns>
        private bool ReadyToSendMessage()
        {
            if (String.IsNullOrEmpty(OutputId)) return false;
            return true;
        }

        /// <summary>
        /// Метод получает пакет данных m и распределяет данные из пакета по дочерним ядрам. Метод является инициализатором ядра.
        /// </summary>
        /// <param name="m"></param>
        public void Put(Message m)
        {
            //Проверяем возможность получения ядром сообщения m
            if (!ReadyToReceiveMessage(m)) return;
            //Забираем пакет данных в ядро. Сейчас реализовано через передачу ссылки на сообщение, однако для многозадачных систем
            //необходимо проработать вариант с копированием содержимого сообщения в память ядра.
            State = m;
            //Инициализируем данные
            Data = State.Data;
            if (Childs != null)
            {
                //Забираем в дочерние ядра дочерние пакеты. При этом сообщение не будет положено, если оно = null
                foreach (var c in Childs.Values) c.Put(State.Get(c.InputId));
            }
        }

        /// <summary>
        /// Функция возвращает истину, если ядро готово принять сообщение m
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private bool ReadyToReceiveMessage(Message m)
        {
            //пустое сообщение не обрабатывается и не инициирует вычислений
            if (m == null) return false;
            //Если InputId пусто, то вычисления не проводятся, вне зависимости от содержимого m
            if (String.IsNullOrEmpty(InputId)) return false;
            //Ядро принимает только те сообщения, идентификатор которых совпадает с InputId
            if (InputId != m.Id) return false;
            return true;
        }

        public void Evaluate()
        {
            //Вычисляем и формируем пакет данных
            State = Get();
            //Рассылаем данные по адресатам
            Put(State);
        }
    }
}
