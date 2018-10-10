using Microsoft.VisualStudio.TestTools.UnitTesting;
using Z;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z.Tests
{
    [TestClass()]
    public class KernelTests
    {
        [TestMethod()]
        public void KernelTest()
        {
            //Тестируем конструкторы
            string name = "kernel1";
            Kernel k = new Kernel(name);
            Assert.AreEqual(k.Id, name);
            Assert.AreEqual(k.OutputId, k.Id);
            Assert.AreEqual(k.InputId, k.Id);
            //----------------------------------------------------------
            Kernel k1 = new Kernel(name, new NularyExpression(1));
            Assert.AreEqual(k1.Id, name);
            Assert.AreEqual(k1.OutputId, k1.Id);
            Assert.AreEqual(k1.InputId, k1.Id);
            //Проверка наличия выражения типа const в ядре k1
            Assert.AreEqual(k1.Expression.Owner, k1);
            Assert.AreEqual(k1.Expression.Result, 1);
            Assert.AreEqual(k1.Expression.Operation, "const");
            //----------------------------------------------------------
            Kernel k2 = new Kernel(name, new NularyExpression(1),
                new Kernel("1"),
                new Kernel("2"));
            Assert.AreEqual(k2.Id, name);
            Assert.AreEqual(k2.OutputId, k2.Id);
            Assert.AreEqual(k2.InputId, k2.Id);
            //Проверка наличия выражения типа const в ядре k1
            Assert.AreEqual(k2.Expression.Owner, k2);
            Assert.AreEqual(k2.Expression.Result, 1);
            Assert.AreEqual(k2.Expression.Operation, "const");
            //Проверка наличия ядер "1" и "2" в "kernel1"
            Assert.IsNotNull(k2.Childs["1"]);
            Assert.IsNotNull(k2.Childs["2"]);
        }

        [TestMethod()]
        public void GetTest()
        {
            //Создание ядра с четырьмя дочерними ядрами. Выражение ядра вычисляет сумму значений дочерних ядер.
            //Дочерние ядра - константы
            Kernel k = new Kernel("Сумма",
                new NaryExpression("+",
                    new NularyExpression("1"),
                    new NularyExpression("2"),
                    new NularyExpression("3"),
                    new NularyExpression("4")),
                new Kernel("1", new NularyExpression(1)),
                new Kernel("2", new NularyExpression(1)),
                new Kernel("3", new NularyExpression(1)),
                new Kernel("4", new NularyExpression(1)));
            Message m = k.Get();
            Assert.AreEqual(m.Data, 4);
        }

        [TestMethod()]
        public void PutTest()
        {
            Kernel k = new Kernel("Сумма", "Вход", "Выход",
                new NaryExpression("+",
                    new NularyExpression("x1"),
                    new NularyExpression("x2"),
                    new NularyExpression("x3"),
                    new NularyExpression("x4")),
                new Kernel("x1"),
                new Kernel("x2"),
                new Kernel("x3"),
                new Kernel("x4"));
            k.Put(new Message("Вход", 5,
                    new Message("x1", 1),
                    new Message("x2", 1),
                    new Message("x3", 1),
                    new Message("x4", 1)
                    )
                );
            Assert.AreEqual(k["x1"].Data, 1);
            Assert.AreEqual(k["x2"].Data, 1);
            Assert.AreEqual(k["x3"].Data, 1);
            Assert.AreEqual(k["x4"].Data, 1);
            Assert.AreEqual(k.Data, 5);
        }

        [TestMethod()]
        public void EvaluateTest()
        {
            //Ядро "Сумма", вход и выход - "Цикл", содержит два потомка - "1", "2", производит суммирование их значений
            Kernel sum = new Kernel("Сумма", "Цикл", "Цикл",
                new NaryExpression("+", new NularyExpression("1"), new NularyExpression("2")),
                new Kernel("1", "Цикл", "", null), //принимает значение от ядра "Цикл", ничего не вычисляет, ничего не передает (просто хранит)
                new Kernel("2", "Цикл", "", null)  
                );
            //Ядро "Цикл", вход и выход - "Сумма", вычислений нет, потомков нет
            Kernel cicle = new Kernel("Цикл", "Сумма", "Сумма", null);
            //Ядро "Тест", потомки "Сумма" и "Цикл", вычисления - значение ядра "Сумма"
            Kernel k = new Kernel("Тест", new NularyExpression("Сумма"), sum, cicle);
            //Формируем сообщение для ядра "Сумма" (Id сообщения при этом - "Цикл", т.к. оно подается на вход "Сумма" с таким именем)
            k.Childs["Сумма"].Put(new Message("Цикл", 1));
            //Производим 8 итераций вычислений ядра k
            for (int i = 0; i < 8; i++)
                k.Evaluate();
            //Процесс вычислений: 1+1 -> 2+2 -> 4+4 -> 8+8 -> 16+16 -> 32+32 -> 64+64 -> 128+128,
            //То есть результат должен быть 256
            Assert.AreEqual(k.Data, 256);
        }

    }
}