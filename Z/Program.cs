using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Z
{
    class Program
    {
        static void Main(string[] args)
        {
            Test3();
            Console.ReadKey();
        }

        static void Test1()
        {
            //main(y): { x->y(x+1)->x }
            Kernel x = new Kernel("x", "y", "y");
            Kernel y = new Kernel
            {
                Id = "y",
                InputId = "x",
                OutputId = "x",
                Expression = new BinaryExpression("+",
                    new NularyExpression(1),
                    new NularyExpression(""))
            };
            Kernel main = new Kernel
            {
                Id = "Main",
                InputId = "Main",
                OutputId = "Main",
                Expression = new NularyExpression("y"),
                Childs = new Dictionary<string, Kernel> { { "x", x }, { "y", y } }
            };
            for (int i = 0; i < 1000; i++)
            {
                main.Evaluate();
                Console.WriteLine(main.Data);
            }
            Console.ReadKey();
        }

        static void Test2()
        {
            Kernel sum = new Kernel("Сумма", "Цикл", "Цикл",
                new NaryExpression("+",
                    new NularyExpression("1"),
                    new NularyExpression("2")),
                new Kernel("1", "Цикл", ""),
                new Kernel("2", "Цикл", "")
                );
            Kernel cicle = new Kernel("Цикл", "Сумма", "Сумма");
            Kernel k = new Kernel("Тест", new NularyExpression("Сумма"), sum, cicle);
            k.Childs["Сумма"].Put(new Message("Цикл", 1));
            for (int i = 0; i < 8; i++)
            {
                k.Evaluate();
                Console.WriteLine(k.Data);
            }
            Console.ReadKey();
        }

        static void Test3()
        {
            //Test : {
            //  s (x1+x2+x3+x4) : { x1, x2, x3, x4 },
            //  a (1/(1+s*s)) : { s },
            //  v : { y1, y2, y3, y4 },
            //  n (a) : { s->.a->.v }
            //}

            Kernel s = new Kernel("s", "input", "s",
                new NaryExpression("+",
                    new NularyExpression("x1"),
                    new NularyExpression("x2"),
                    new NularyExpression("x3"),
                    new NularyExpression("x4")),
                new Kernel("x1"), new Kernel("x2"), new Kernel("x3"), new Kernel("x4"));
            Kernel a = new Kernel("a", "s", "v",
                new BinaryExpression("/",
                    new NularyExpression(1),
                    new BinaryExpression("+",
                        new NularyExpression(1),
                        new UnaryExpression("sqr", new NularyExpression("s")))),
                new Kernel("s"));
            Kernel v = new Kernel("v", "a", "y", null,
                new Kernel("y1", "v", ""),
                new Kernel("y2", "v", ""),
                new Kernel("y3", "v", ""),
                new Kernel("y4", "v", "")
                );
            Kernel n = new Kernel("n", new NularyExpression("a"), s, a, v);
            n["s"].Put(new Message("input", 0,
                            new Message("x1", 1),
                            new Message("x2", 1),
                            new Message("x3", 1),
                            new Message("x4", 1)));
            for (int i = 0; i < 8; i++)
            {
                n.Evaluate();
                Console.WriteLine(n.Data + " ~ " + 1.0/(1.0+Math.Pow(4,2)));
            }
        }
    }
}
