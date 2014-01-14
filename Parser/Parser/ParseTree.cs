using System;
using System.Collections.Generic;

namespace Parser {
    class ParseTree {

        private ParseTreeItem root;

        public ParseTree() {
            root = new ParseTreeItem();
        }

        public void Add(string s, Operation operation) {
            ParseTreeItem current = root;

            for (int i = 0; i < s.Length; ++i) {

                char key = s[i];

                if (current.Children.ContainsKey(key)) {
                    current = current.Children[key];

                    if (i == s.Length - 1) {
                        //ADD EXCEPTION HERE FOR IF IT FAILS
                        if (current is ParseTreeOperationItem) {
                            ParseTreeOperationItem opItem = current as ParseTreeOperationItem;
                            if (operation is Binary) {
                                opItem.BinaryOperation = operation as Binary;
                            } else if (operation is Function) {
                                opItem.FunctionOperation = operation as Function;
                            }
                        } 
                    }
                } else {
                    ParseTreeItem newItem = null;

                    if (i == s.Length - 1)
                        newItem = new ParseTreeOperationItem(operation);
                    else
                        newItem = new ParseTreeItem();

                    current.Children.Add(key, newItem);
                    current = newItem;
                }
            }
        }

        /*public void Add(string s, object constant) {
            ParseTreeItem current = root;

            for (int i = 0; i < s.Length; ++i) {

                char key = s[i];

                if (current.Children.ContainsKey(key)) {
                    current = current.Children[key];

                    if (i == s.Length - 1) {
                        //ADD EXCEPTION HERE FOR IF IT FAILS
                        if (current is ParseTreeOperationItem) {
                            ParseTreeOperationItem opItem = current as ParseTreeOperationItem;
                            if (operation is Binary) {
                                opItem.BinaryOperation = operation as Binary;
                            } else if (operation is Function) {
                                opItem.FunctionOperation = operation as Function;
                            }
                        }
                    }
                } else {
                    ParseTreeItem newItem = null;

                    if (i == s.Length - 1)
                        newItem = new ParseTreeOperationItem(operation);
                    else
                        newItem = new ParseTreeItem();

                    current.Children.Add(key, newItem);
                    current = newItem;
                }
            }
        }*/


        public Operation GetBinaryOperation(string s) {
            ParseTreeItem current = root;
            
            for (int i = 0; i < s.Length; ++i) {
                char key = s[i];

                if (current.Children.ContainsKey(key)) {
                    current = current.Children[key];
                } else {
                    throw new ArgumentException("There exists no binary operator using name: " + s);
                }
            }

            if (current is ParseTreeOperationItem) {
                ParseTreeOperationItem opItem = current as ParseTreeOperationItem;
                if (opItem.BinaryOperation != null) {
                    return opItem.BinaryOperation;
                } else {
                    throw new ArgumentException("There exists no binary operator using name: " + s);
                }
            } else {
                throw new ArgumentException("There exists no binary operator using name: " + s);
            }
        }

        public Operation GetFunctionOperation(string s) {
            ParseTreeItem current = root;

            for (int i = 0; i < s.Length; ++i) {
                char key = s[i];

                if (current.Children.ContainsKey(key)) {
                    current = current.Children[key];
                } else {
                    throw new ArgumentException("There exists no function operator using name: " + s);
                }
            }

            if (current is ParseTreeOperationItem) {
                ParseTreeOperationItem opItem = current as ParseTreeOperationItem;
                if (opItem.FunctionOperation != null) {
                    return opItem.FunctionOperation;
                } else {
                    throw new ArgumentException("There exists no function operator using name: " + s);
                }
            } else {
                throw new ArgumentException("There exists no function operator using name: " + s);
            }
        }

        public bool Contains(string s) {
            ParseTreeItem current = root;

            for (int i = 0; i < s.Length; ++i) {
                char key = s[i];

                if (current.Children.ContainsKey(key)) {
                    current = current.Children[key];
                } else {
                    return false;
                }
            }

            return !current.IsEmpty();
        }
    }

    class ParseTreeItem {

        public Dictionary<char, ParseTreeItem> Children { get; private set; }

        public virtual bool IsEmpty() { return true; }

        public ParseTreeItem() {
            this.Children = new Dictionary<char, ParseTreeItem>();
        }
    }

    class ParseTreeOperationItem : ParseTreeItem {
        private Binary binary;
        private Function function;

        public ParseTreeOperationItem() : base() { }
        public ParseTreeOperationItem(Operation op) : this() {
            if (op is Binary)
                this.BinaryOperation = op as Binary;
            else if (op is Function)
                this.FunctionOperation = op as Function;
        }


        public override bool IsEmpty() {
            return this.BinaryOperation == null && this.FunctionOperation == null;
        }

        public Binary BinaryOperation {
            get { return this.binary; }
            set {
                if (this.binary == null)
                    this.binary = value;
                else
                    throw new ArgumentException("ParseTreeItem already contains a binary operation.");
            }
        }

        public Function FunctionOperation {
            get { return this.function; }
            set {
                if (this.function == null)
                    this.function = value;
                else
                    throw new ArgumentException("ParseTreeItem already contains a function operation.");
            }
        }
    }


    class ParseTreeConstantItem : ParseTreeItem{
        private object value;

        public ParseTreeConstantItem() : base() { }
        public ParseTreeConstantItem(object value) {
            this.Value = value;
        }


        public override bool IsEmpty() {
            return this.Value == null;
        }

        public object Value {
            get { return this.value; }
            set {
                if (this.value == null)
                    this.value = value;
                else
                    throw new ArgumentException("ParseTreeItem already contains a value.");
            }
        }
    }
}