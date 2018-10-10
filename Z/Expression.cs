using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z
{
    public enum EType : byte { MUL, DIV, SUM, DIF, POW, MIN, MAX, NEG, SQR, SQRT, SGN, VAR, CNST};

    public class Expression
    {
        protected Kernel kernel;

        protected double value;
        public double Result
        {
            get
            {
                return value;
            }
        }

        virtual public Kernel Owner
        {
            get
            {
                return kernel;
            }

            set
            {
                kernel = value;
            }
        }

        public string Id = "";

        public string Operation;

        public Expression()
        {
        }

        public Expression(Expression e)
        {
            Id = e.Id;
            value = e.value;
            Operation = e.Operation;
            kernel = e.kernel;
        }

        public virtual double Calculate()
        {
            return value;
        }

        public static Expression Clone(Expression e)
        {
            Expression result;
            string eType = e.GetType().Name;
            switch (eType)
            {
                case "NularyExpression": result = new NularyExpression((NularyExpression)e); break;
                case "UnaryExpression": result = new UnaryExpression((UnaryExpression)e); break;
                case "BinaryExpression": result = new BinaryExpression((BinaryExpression)e); break;
                case "NaryExpression": result = new NaryExpression((NaryExpression)e); break;
                default: throw new Exception("Неизвестный тип выражения: " + eType);
            }
            return result;
        }
        
        public static Expression Clone(Expression e, Kernel owner)
        {
            Expression clone = Expression.Clone(e);
            clone.Owner = owner;
            return clone;
        }
    }

    public class NularyExpression : Expression
    {
        public NularyExpression(NularyExpression e)
            : base(e)
        {
        }

        public NularyExpression(double d)
            : base()
        {
            Operation = "const";
            value = d;
        }

        public NularyExpression(string name)
        {
            Operation = "var";
            Id = name;
        }

        public override double Calculate()
        {
            switch (Operation)
            {
                case "const": break;
                case "var":
                    {
                        //Если имя выражению не присвоено, или имя выражения совпадает с именем ядра, 
                        //то значение берется из Data ядра-владельца данного выражения
                        if (String.IsNullOrEmpty(Id) || Id == Owner.Id)
                            value = Owner.Data;
                        //иначе, - пытаемся получить значение Data из дочернего ядра
                        else
                            value = Owner.Childs[Id].Data;
                    }
                    break;
                default: throw new Exception("Неизвестный тип операции: " + Operation);
            }
            return value;
        }
    }

    public class UnaryExpression : Expression
    {
        private Expression operand;

        public Expression Operand
        {
            get
            {
                return operand;
            }
            set
            {
                operand = value;
                if (operand != null) operand.Owner = this.Owner;
            }
        }

        override public Kernel Owner
        {
            get
            {
                return kernel;
            }

            set
            {
                kernel = value;
                if (operand != null) operand.Owner = kernel;
            }
        }

        public UnaryExpression(string o, double d)
            : base()
        {
            Operation = o;
            Operand = new NularyExpression(d);
        }

        public UnaryExpression(string o, NularyExpression e)
            : base()
        {
            Operation = o;
            Operand = e;
        }

        public UnaryExpression(UnaryExpression e)
            : base(e)
        {
            value = e.value;
            Operand = Expression.Clone(e.Operand);
        }

        public override double Calculate()
        {
            double operandValue = Operand.Calculate();
            switch (Operation)
            {
                case "-": value = -operandValue; break;
                case "sign":
                    if (operandValue > 0) { value = 1; }
                    else if (operandValue < 0) { value = -1; }
                    else value = 0; break;
                case "sqr": value = operandValue * operandValue; break;
                case "sqrt": value = Math.Sqrt(operandValue); break;
                default: throw new Exception("Неизвестный унарный оператор:" + Operation);
            }
            return value;
        }
    }

    public class BinaryExpression : Expression
    {
        private Expression leftOperand;
        private Expression rightOperand;

        public Expression LeftOperand
        {
            get
            {
                return leftOperand;
            }

            set
            {
                leftOperand = value;
                if (leftOperand != null) leftOperand.Owner = this.Owner;
            }
        }

        public Expression RightOperand
        {
            get
            {
                return rightOperand;
            }

            set
            {
                rightOperand = value;
                if (rightOperand != null) rightOperand.Owner = this.Owner;
            }
        }

        override public Kernel Owner
        {
            get
            {
                return kernel;
            }

            set
            {
                kernel = value;
                if (leftOperand != null) leftOperand.Owner = kernel;
                if (rightOperand != null) rightOperand.Owner = kernel;
            }
        }

        public BinaryExpression(string o, Expression left, Expression right)
            : base()
        {
            Operation = o;
            LeftOperand = left;
            RightOperand = right;
        }

        public BinaryExpression(BinaryExpression e)
            : base(e)
        {
            LeftOperand = Expression.Clone(e.LeftOperand);
            RightOperand = Expression.Clone(e.RightOperand);
        }

        public override double Calculate()
        {
            double leftValue = LeftOperand.Calculate();
            double rightValue = RightOperand.Calculate();
            switch (Operation)
            {
                case "*": value = leftValue * rightValue; break;
                case "+": value = leftValue + rightValue; break;
                case "^": value = Math.Pow(leftValue, rightValue); break;
                case "/": value = leftValue / rightValue; break;
                case "-": value = leftValue - rightValue; break;
                default: throw new Exception("Неизвестный бинарный оператор " + Operation);
            }
            return value;
        }
    }

    public class NaryExpression : Expression
    {

        private List<Expression> operands;

        public List<Expression> Operands
        {
            get
            {
                return operands;
            }

            set
            {
                operands = value;
                if (operands != null)
                {
                    operands = new List<Expression>();
                    value.ForEach(o => operands.Add(Expression.Clone(o, this.Owner)));
                }
            }
        }

        /// <summary>
        /// Ссылка на ядро, которому принадлежит данное выражение
        /// </summary>
        override public Kernel Owner
        {
            get
            {
                return kernel;
            }

            set
            {
                kernel = value;
                //Для всех дочерних операндов переопределим ядро-владельца
                operands.ForEach(o => o.Owner = kernel);
            }
        }

        /// <summary>
        /// Конструктор выражения
        /// </summary>
        /// <param name="o">Имя операции</param>
        public NaryExpression(string o)
            : base()
        {
            Operation = o;
            Operands = new List<Expression>();
        }

        public NaryExpression(string o, params Expression[] e)
            : base()
        {
            Operation = o;
            Operands = e.ToList();
        }

        public NaryExpression(NaryExpression e)
            : base(e)
        {
            Operands = new List<Expression>();
            e.Operands.ForEach(o => Expression.Clone(o, this.Owner));
        }

        public override double Calculate()
        {
            List<double> operandsValues = new List<double>();
            foreach (Expression e in Operands)
                operandsValues.Add(e.Calculate());
            switch (Operation)
            {
                case "+": value = operandsValues.Sum(); break;
                case "*": value = operandsValues.Aggregate(1.0, (total, next) => total * next); break;
                case "max": value = operandsValues.Max(); break;
                case "min": value = operandsValues.Min(); break;
                default: throw new Exception("Неизвестный оператор " + Operation);
            }
            return value;
        }
    }


}

