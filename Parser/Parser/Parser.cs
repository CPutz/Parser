using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parser {

    public delegate object BinaryDelegate(object a, object b);
    public delegate object FunctionDelegate(object a);


    class MathParser : Parser {

        protected override void DefineTokens() {
            this.parseTree.Add("+", new Binary(6, RightLeft.Left, (object a, object b) => (double)a + (double)b));
            this.parseTree.Add("-", new Binary(6, RightLeft.Left, (object a, object b) => (double)a - (double)b));
            this.parseTree.Add("*", new Binary(5, RightLeft.Left, (object a, object b) => (double)a * (double)b));
            this.parseTree.Add("/", new Binary(5, RightLeft.Left, (object a, object b) => (double)a / (double)b));
            this.parseTree.Add("%", new Binary(5, RightLeft.Left, (object a, object b) => (double)a % (double)b));
            this.parseTree.Add("^", new Binary(4, RightLeft.Right, (object a, object b) => Math.Pow((double)a, (double)b)));
            this.parseTree.Add("sin(", new Function(true, (object a) => Math.Sin((double)a)));
            this.parseTree.Add("cos(", new Function(true, (object a) => Math.Cos((double)a)));
            this.parseTree.Add("tan(", new Function(true, (object a) => Math.Tan((double)a)));
            this.parseTree.Add("-", new Function(false, (object a) => -(double)a));
            this.parseTree.Add("<", new Binary(8, RightLeft.Left, (object a, object b) => (double)a < (double)b));
            this.parseTree.Add(">", new Binary(8, RightLeft.Left, (object a, object b) => (double)a > (double)b));
            this.parseTree.Add("<=", new Binary(8, RightLeft.Left, (object a, object b) => (double)a <= (double)b));
            this.parseTree.Add(">=", new Binary(8, RightLeft.Left, (object a, object b) => (double)a >= (double)b));
            this.parseTree.Add("==", new Binary(9, RightLeft.Left,
                (object a, object b) => {
                    if (a is double && b is double)
                        return (double)a == (double)b;
                    else if (a is bool && b is bool)
                        return (bool)a == (bool)b;
                    else
                        return a == b;
                }));
            this.parseTree.Add("!=", new Binary(9, RightLeft.Left,
                (object a, object b) => {
                    if (a is double && b is double)
                        return (double)a != (double)b;
                    else if (a is bool && b is bool)
                        return (bool)a != (bool)b;
                    else
                        return a != b;
                }));
            this.parseTree.Add("&&", new Binary(13, RightLeft.Left, (object a, object b) => (bool)a && (bool)b));
            this.parseTree.Add("||", new Binary(14, RightLeft.Left, (object a, object b) => (bool)a || (bool)b));
        }
    }


    abstract class Parser {

        private char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        protected ParseTree parseTree;

        public Parser() {
            this.parseTree = new ParseTree();
            this.parseTree.Add("(", new Function(true, (object a) => a));
            DefineTokens();
        }


        protected abstract void DefineTokens();


        public ParseResult Parse(string s) {

            ParseResult res = new ParseResult();
            res.WasSuccesfull = true;

            try {
                res.Value = Compute(ShuntingYard(s));
            }
            catch (Exception e) {
                res.WasSuccesfull = false;
                res.Exception = e;
            }

            return res;
        }


        private Stack<object> ShuntingYard(string s) {

            string[] tokens = GetTokens(s);

            //Shunting-yard
            Stack<Operation> operatorStack = new Stack<Operation>();
            Stack<object> output = new Stack<object>();
            string token;

            for (int i = 0; i < tokens.Length; ++i) {
                token = tokens[i];

                if (numbers.Contains(token[0])) {

                    double number = double.Parse(token);
                    output.Push(number);
                } else {
                    Operation op = null;

                    if (parseTree.Contains(token)) {
                        if (i == 0 || parseTree.Contains(tokens[i - 1])) {
                            op = parseTree.GetFunctionOperation(token);
                        } else {
                            op = parseTree.GetBinaryOperation(token);
                        }
                    }

                    if (op is Function) {
                        Function fOp = op as Function;
                        operatorStack.Push(fOp);
                    } else if (op is Binary) {
                        Binary bOp = op as Binary;
                        while (operatorStack.Count > 0 &&
                            ((bOp.Associativity == RightLeft.Left && operatorStack.Peek().Precedence == bOp.Precedence) ||
                            bOp.Precedence > operatorStack.Peek().Precedence)) {
                            output.Push(operatorStack.Pop());
                        }
                        operatorStack.Push(bOp);
                    } else if (token == ")") {
                        bool succeed = false;

                        while (operatorStack.Count > 0) {
                            Operation top = operatorStack.Peek();
                            if (top is Function && ((Function)top).CloseByBracket) {
                                succeed = true;

                                //HIER MOETEN HAAKJES EIGENLIJK NIET GEPUSHED WORDEN...
                                //output.Push(operatorStack.Pop());

                                if (operatorStack.Count > 0 && operatorStack.Peek() is Function) {
                                    output.Push(operatorStack.Pop());
                                }
                                break;
                            } else {
                                output.Push(operatorStack.Pop());
                            }
                        }

                        if (!succeed)
                            throw new Exception("Wrong parenthesis.");
                    } else {
                        throw new Exception("Token not valid: " + token + ".");
                    }
                }
            }

            while (operatorStack.Count > 0) {
                output.Push(operatorStack.Pop());
            }

            return output;
        }


        private string[] GetTokens(string s) {
            List<string> list = new List<string>();

            string[] split = s.Split();

            foreach (string part in split) {

                for (int i = 0; i < part.Length; ++i) {

                    char c = part[i];
                    int start = i;

                    if (numbers.Contains(c)) {
                        while (i < part.Length - 1 && numbers.Contains(part[i + 1])) {
                            ++i;
                        }
                    } else if (c == ')') {
                        //do nothing
                    } else {
                        for (int k = part.Length - 1; k >= i; --k) {
                            //DONT KNOW WHETER IT IS BETTER TO CHECK FOR BINARY AND FUNCTION OPERATIONS SEPERATELY
                            //NOW IT WILL BREAK IF THERE EXISTS AN OPERATION WITH THAT NAME
                            if (parseTree.Contains(part.Substring(i, k - i + 1))) {
                                i = k;
                                break;
                            }
                        }
                    }

                    list.Add(part.Substring(start, i - start + 1));
                }
            }

            return list.ToArray();
        }


        private object Compute(Stack<object> input) {
            object o = input.Pop();
            if (!(o is Operation)) {
                if (o is double) {
                    return (double)o;
                } else {
                    throw new Exception("Error: TODO");
                }
            } else {
                if (o is Function) {
                    Function u = o as Function;
                    return u.Evaluate(Compute(input));
                } else if (o is Binary) {
                    Binary b = o as Binary;
                    object r1 = Compute(input);
                    object r2 = Compute(input);
                    return b.Evaluate(r2, r1);
                } else {
                    throw new Exception("Error: TODO");
                }

            }
        }
    }


    class ParseResult {

        public object Value { get; set; }

        public bool WasSuccesfull { get; set; }

        public Exception Exception { get; set; }

        public ParseResult() { }
    }



    enum RightLeft { Right, Left }


    abstract class Operation {
        public int Precedence { get; protected set; }

        public Operation(int precendence) {
            this.Precedence = precendence;
        }
    }

    class Binary : Operation {
        public RightLeft Associativity { get; private set; }
        public BinaryDelegate Evaluate { get; private set; }

        public Binary(int precendence, RightLeft associativity, BinaryDelegate evaluate) : base(precendence) {
            this.Associativity = associativity;
            this.Evaluate = evaluate;
        }
    }
    class Function : Operation {
        public FunctionDelegate Evaluate { get; private set; }
        public bool CloseByBracket { get; private set; }

        public Function(bool closeByBracket, FunctionDelegate evaluate) : base(int.MaxValue) {
            this.Evaluate = evaluate;
            this.CloseByBracket = closeByBracket;
        }
    }
}
