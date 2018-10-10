using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z
{
    /// <summary>
    /// Класс описывающий схему данных для сохранения, обработки и передачи результатов вычислений.
    /// </summary>
    public class Message
    {
        //Идентификатор сообщения. Может быть не уникальным.
        public string Id;
        //Данные в сообщении. На данный момент число типа double, но 
        //TODO: сделать универсальный буфер с данными нужной разрядности, извлечение из буфера типизированных данных
        public double Data;
        //Словарь дочерних сообщений
        private Dictionary<string, Message> childs = new Dictionary<string, Message>();
        public Dictionary<string, Message> Childs
        {
            get
            {
                return childs;
            }

            private set
            {
                childs = value;
            }
        }
        
        public Message(string id)
        {
            Id = id;
        }
        
        public Message(string id, double data)
        {
            Id = id;
            Data = data;
        }

        public Message(string id, double data, params Message[] childs)
        {
            Id = id;
            Data = data;
            Childs = childs.ToDictionary(c => c.Id); ;
        }


        /// <summary>
        /// Метод добавляет дочернее сообщение, если оно не null. Если null - ничего не делает
        /// </summary>
        /// <param name="m"></param>
        public void Put(Message m)
        {
            //Пустое сообщение не добавляем
            if (m == null) return;
            //Запись дочернего сообщения по ключу.
            //TODO: Проблема: в данном варианте передачи дочернего сообщения возможна еоллизия - конкурентная перезапись предыдущего значения новым. Нужно продумать, нужна ли перезапись или необходимо разрешать коллизию другим способом.
            childs[m.Id] = m;
        }

        /// <summary>
        /// Метод возвращает дочернее сообщение или this, если найдено соответствие по id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Message Get(string id)
        {
            //Если запрос для id == Id сообщения, возаращаем this
            if (id == Id) return this;
            //Если запрос для id, отсутствуего дочернего сообщения, то возвращаем null
            if (!childs.ContainsKey(id)) return null;
            //Возвращаем дочернее сообщение, если запрошенный id соответствует одному из дочерних сообщений
            return childs[id];
        }

    }
}
