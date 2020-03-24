/* 
 * Copyright (C) 2012-2014 Artem Los,
 * All rights reserved.
 * 
 * Please see the license file in the project folder,
 * or, goto https://mathosparser.codeplex.com/license.
 * BSD 3-Clause License

  Copyright (c) 2012-2019, Mathos Project
  All rights reserved.

  Redistribution and use in source and binary forms, with or without
  modification, are permitted provided that the following conditions are met:

  1. Redistributions of source code must retain the above copyright notice, this
     list of conditions and the following disclaimer.

  2. Redistributions in binary form must reproduce the above copyright notice,
     this list of conditions and the following disclaimer in the documentation
     and/or other materials provided with the distribution.

  3. Neither the name of the copyright holder nor the names of its
     contributors may be used to endorse or promote products derived from
     this software without specific prior written permission.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
  IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
  DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
  FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
  DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
  SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
  CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
  OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

 *  * Please feel free to ask me directly at my email!
 *  artem@artemlos.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace MathParserMathos
{
    /// <summary>
    /// This is a mathematical expression parser that allows you to parser a string value,
    /// perform the required calculations, and return a value in form of a double.
    /// </summary>
    public class MathParserMathos
    {
    	public bool Radiant;
        /// <summary>
        /// This constructor will add some basic operators, functions, and variables
        /// to the parser. Please note that you are able to change that using
        /// boolean flags
        /// </summary>
        /// <param name="loadPreDefinedFunctions">This will load "abs", "cos", "cosh", "arccos", "sin", "sinh", "arcsin", "tan", "tanh", "arctan", "sqrt", "rem", "round"</param>
        /// <param name="loadPreDefinedOperators">This will load "%", "*", ":", "/", "+", "-", ">", "&lt;", "="</param>
        /// <param name="loadPreDefinedVariables">This will load "pi" and "e"</param>
        /// <param name="radiant">This will set rad or deg
        public MathParserMathos(bool loadPreDefinedFunctions = true, bool loadPreDefinedOperators = true, bool loadPreDefinedVariables = true, bool Radiant= true)
        {
            if (loadPreDefinedOperators)
            {
                // by default, we will load basic arithmetic operators.
                // please note, its possible to do it either inside the constructor,
                // or outside the class. the lowest value will be executed first!
                OperatorList.Add("?"); // z.B. 5E5 , E wird durch "?" ersetzt
                
                OperatorList.Add("%"); // modulo
                OperatorList.Add("^"); // to the power of
                OperatorList.Add(":"); // division 1
                OperatorList.Add("/"); // division 2
                OperatorList.Add("*"); // multiplication
                OperatorList.Add("-"); // subtraction
                OperatorList.Add("+"); // addition

                OperatorList.Add(">"); // greater than
                OperatorList.Add("<"); // less than
                OperatorList.Add("="); // are equal
				
                

                // when an operator is executed, the parser needs to know how.
                // this is how you can add your own operators. note, the order
                // in this list does not matter.
                _operatorAction.Add("?", (numberA, numberB) => (double) numberA * Math.Pow(10, (double)numberB));
                _operatorAction.Add("%", (numberA, numberB) => numberA % numberB);
                _operatorAction.Add("^", (numberA, numberB) => (double)Math.Pow((double)numberA, (double)numberB));
                _operatorAction.Add(":", (numberA, numberB) => numberA / numberB);
                _operatorAction.Add("/", (numberA, numberB) => numberA / numberB);
                _operatorAction.Add("*", (numberA, numberB) => numberA * numberB);
                _operatorAction.Add("+", (numberA, numberB) => numberA + numberB);
                _operatorAction.Add("-", (numberA, numberB) => numberA - numberB);

                _operatorAction.Add(">", (numberA, numberB) => numberA > numberB ? 1 : 0);
                _operatorAction.Add("<", (numberA, numberB) => numberA < numberB ? 1 : 0);
                _operatorAction.Add("=", (numberA, numberB) => numberA == numberB ? 1 : 0);
           
            }


            if (loadPreDefinedFunctions)
            {
                // these are the basic functions you might be able to use.
                // as with operators, localFunctions might be adjusted, i.e.
                // you can add or remove a function.
                // please open the "MathosTest" project, and find MathParser.cs
                // in "CustomFunction" you will see three ways of adding 
                // a new function to this variable!
                // EACH FUNCTION MAY ONLY TAKE ONE PARAMETER, AND RETURN ONE
                // VALUE. THESE VALUES SHOULD BE IN "double FORMAT"!
                LocalFunctions.Add("abs", x => (double)Math.Abs((double)x[0]));

                if (Radiant)
                {
                	LocalFunctions.Add("cos", x => (double)Math.Cos((double)x[0]));
                	LocalFunctions.Add("cosh", x => (double)Math.Cosh((double)x[0]));
                	LocalFunctions.Add("arccos", x => (double)Math.Acos((double)x[0]));

                	LocalFunctions.Add("sin", x => (double)Math.Sin((double)x[0]));
                	LocalFunctions.Add("sinh", x => (double)Math.Sinh((double)x[0]));
                	LocalFunctions.Add("arcsin", x => (double)Math.Asin((double)x[0]));

                	LocalFunctions.Add("tan", x => (double)Math.Tan((double)x[0]));
                	LocalFunctions.Add("tanh", x => (double)Math.Tanh((double)x[0]));
                	LocalFunctions.Add("arctan", x => (double)Math.Atan((double)x[0]));
                	//LocalFunctions.Add("arctan2", x => (double)Math.Atan2((double)x[0], (double)x[1]));
                }
                else
                {
                	LocalFunctions.Add("cos", x => (double)Math.Cos((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("cosh", x => (double)Math.Cosh((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("arccos", x => (double)Math.Acos((double)x[0] * Math.PI / 180));

                	LocalFunctions.Add("sin", x => (double)Math.Sin((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("sinh", x => (double)Math.Sinh((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("arcsin", x => (double)Math.Asin((double)x[0] * Math.PI / 180));

                	LocalFunctions.Add("tan", x => (double)Math.Tan((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("tanh", x => (double)Math.Tanh((double)x[0] * Math.PI / 180));
                	LocalFunctions.Add("arctan", x => (double)Math.Atan((double)x[0] * Math.PI / 180));
                	//LocalFunctions.Add("arctan2", x => (double)Math.Atan2((double)x[0] * Math.PI / 180, (double)x[1]));
                }
                
                LocalFunctions.Add("sqrt", x => (double)Math.Sqrt((double)x[0]));
                LocalFunctions.Add("cur", x => (double)Math.Pow((double)x[0], 1.0d/3.0d));
                
                LocalFunctions.Add("fact", delegate(double[] input)
                {
                    double a = 1;
                   	for (int i = 1; i<= input[0];i++)
                    	a *= i;
                                   	
                   	return (double)a;
                });
                
                LocalFunctions.Add("rem", x => (double)Math.IEEERemainder((double)x[0], (double)x[1]));
                LocalFunctions.Add("root", x => (double)Math.Pow((double)x[0], 1.0 / (double)x[1]));

                LocalFunctions.Add("pow", x => (double)Math.Pow((double)x[0], (double)x[1]));
                LocalFunctions.Add("ten", x => (double)Math.Pow(10, (double)x[0]));

                LocalFunctions.Add("exp", x => (double)Math.Exp((double)x[0]));
                LocalFunctions.Add("ln", x => (double)Math.Log((double)x[0]));
                //LocalFunctions.Add("log10", x => (double)Math.Log10((double)x[0]));
                LocalFunctions.Add("log", x => (double)Math.Log10((double)x[0]));

                LocalFunctions.Add("rcp", x => (double) 1 /(double)x[0]);
                
                LocalFunctions.Add("dbplus", delegate(double[] input)
                {
                   	double sum = 0;
                   	for (int i = 0; i < input.Length; i++)
                   	{
                   		sum += (double)Math.Pow(10, 0.1 * (double)input[i]);
                   	}
                    return (double) 10 * Math.Log10(sum);
                });
                
                LocalFunctions.Add("dbmean", delegate(double[] input)
                {
                   	double sum = 0;
                   	for (int i = 0; i < input.Length; i++)
                   	{
                   		sum += (double)Math.Pow(10, 0.1 * (double)input[i]);
                   	}
                   	sum = sum / ((double) input.Length);
                    return (double) 10 * Math.Log10(sum);
                });
                
                LocalFunctions.Add("dbminus", delegate(double[] input)
                {
                   	double sum = (double)Math.Pow(10, 0.1 * (double)input[0]);
                   	for (int i = 1; i < input.Length; i++)
                   	{
                   		sum -= (double)Math.Pow(10, 0.1 * (double)input[i]);
                   	}
                    return (double) 10 * Math.Log10(sum);
                });
                
                LocalFunctions.Add("round", x => (double)Math.Round((double)x[0]));
                LocalFunctions.Add("truncate", x => (double)(x[0] < 0.0d ? -Math.Floor(-(double)x[0]) : Math.Floor((double)x[0])));
                LocalFunctions.Add("floor", x => (double)Math.Floor((double)x[0]));
                LocalFunctions.Add("ceiling", x => (double)Math.Ceiling((double)x[0]));
                LocalFunctions.Add("sign", x => (double)Math.Sign((double)x[0]));

            }

            if (loadPreDefinedVariables)
            {
                // local variables such as pi can also be added into the parser.
                LocalVariables.Add("pi", (double)3.14159265358979323846264338327950288); // the simplest variable!
//                LocalVariables.Add("pi2", (double)6.28318530717958647692528676655900576);
//                LocalVariables.Add("pi05", (double)1.57079632679489661923132169163975144);
//                LocalVariables.Add("pi025", (double)0.78539816339744830961566084581987572);
//                LocalVariables.Add("pi0125", (double)0.39269908169872415480783042290993786);
//                LocalVariables.Add("pitograd", (double)57.2957795130823208767981548141051704);
//                LocalVariables.Add("piofgrad", (double)0.01745329251994329576923690768488612);

                LocalVariables.Add("e", (double)2.71828182845904523536028747135266249);
//                LocalVariables.Add("phi", (double)1.61803398874989484820458683436563811);
//                LocalVariables.Add("major", (double)0.61803398874989484820458683436563811);
//                LocalVariables.Add("minor", (double)0.38196601125010515179541316563436189);
            }
        }


        /* THE LOCAL VARIABLES */
        private List<string> _operatorList = new List<string>();
        /// <summary>
        /// All operators should be inside this property.
        /// The first operator is executed first, et cetera.
        /// An operator may only be ONE character.
        /// </summary>
        public List<string> OperatorList
        {
            get { return _operatorList; }
            set { _operatorList = value; }
        }
        private Dictionary<string, Func<double, double, double>> _operatorAction = new Dictionary<string, Func<double, double, double>>();
        /// <summary>
        /// When adding a variable in the OperatorList property, you need to assign how that operator should work.
        /// </summary>
        public Dictionary<string, Func<double, double, double>> OperatorAction
        {
            get { return _operatorAction; }
            set { _operatorAction = value; }
        }
        private Dictionary<string, Func<double[], double>> _localFunctions = new Dictionary<string, Func<double[], double>>();
        /// <summary>
        /// All functions that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, Func<double[], double>> LocalFunctions
        {
            get { return _localFunctions; }
            set { _localFunctions = value; }
        }
        private Dictionary<string, double> _localVariables = new Dictionary<string, double>();
        /// <summary>
        /// All variables that you want to define should be inside this property.
        /// </summary>
        public Dictionary<string, double> LocalVariables
        {
            get { return _localVariables; }
            set { _localVariables = value; }
        }

        private readonly CultureInfo _cultureInfo = CultureInfo.InvariantCulture;
        /// <summary>
        /// When converting the result from the Parse method or ProgrammaticallyParse method ToString(),
        /// please use this cultur info.
        /// </summary>
        public CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
        }


        /* PARSER FUNCTIONS, PUBLIC */
        /// <summary>
        /// Enter the math expression in form of a string.
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <returns></returns>
        public double Parse(string mathExpression)
        {
        	// first look, if an "xEy" exists and change "E" to "?" 
        	string a = "";
        	for (int i = 0; i < mathExpression.Length; i++)
        	{
        		var ch = mathExpression[i];
        		
        		
        		if (i > 0 && ch == 'E')
        		{
        			var ch1 = mathExpression[i-1];
        			if (Char.IsDigit(ch1))
        				ch = '?';
        		}
        		a = a + ch;
        	}
        	mathExpression = a;
        	mathExpression = Correction(mathExpression); // correct different valid writings
        	mathExpression = mathExpression.ToLower();   // uppercase to lower 
            return MathParserLogic(Scanner(mathExpression)); // parse
        }

        /// <summary>
        /// Enter the math expression in form of a list of tokens.
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <returns></returns>
        public double Parse(ReadOnlyCollection<string> mathExpression)
        {
            return MathParserLogic(new List<string>(mathExpression));
        }

        /// <summary>
        /// Enter the math expression in form of a string. You might also add/edit variables using "let" keyword.
        /// For example, "let sampleVariable = 2+2".
        /// 
        /// Another way of adding/editing a variable is to type "varName := 20"
        /// 
        /// Last way of adding/editing a variable is to type "let varName be 20"
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <param name="correctExpression"></param>
        /// <param name="identifyComments"></param>
        /// <returns></returns>
        public double ProgrammaticallyParse(string mathExpression, bool correctExpression = true, bool identifyComments = true)
        {
            if (identifyComments)
            {
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#\\{.*\\}#", ""); // Delete Comments #{Comment}#
                mathExpression = System.Text.RegularExpressions.Regex.Replace(mathExpression, "#.*$", ""); // Delete Comments #Comment
            }

            if (correctExpression)
                mathExpression = Correction(mathExpression);
                    // this refers to the Correction function which will correct stuff like artn to arctan, etc.

            string varName;
            double varValue;
            if (mathExpression.Contains("let"))
            {
                if (mathExpression.Contains("be"))
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3, mathExpression.IndexOf("be", StringComparison.Ordinal) - mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                    mathExpression = mathExpression.Replace(varName + "be", "");
                }
                else
                {
                    varName = mathExpression.Substring(mathExpression.IndexOf("let", StringComparison.Ordinal) + 3, mathExpression.IndexOf("=", StringComparison.Ordinal) - mathExpression.IndexOf("let", StringComparison.Ordinal) - 3);
                    mathExpression = mathExpression.Replace(varName + "=", "");
                }

                varName = varName.Replace(" ", "");
                mathExpression = mathExpression.Replace("let", "");
                
                varValue = Parse(mathExpression);

                if (LocalVariables.ContainsKey(varName))
                    LocalVariables[varName] = varValue;
                else
                    LocalVariables.Add(varName, varValue);

                return varValue;
            }

            if (!mathExpression.Contains(":=")) return Parse(mathExpression);
            
            //mathExpression = mathExpression.Replace(" ", ""); // remove white space
            varName = mathExpression.Substring(0, mathExpression.IndexOf(":=", StringComparison.Ordinal));
            mathExpression = mathExpression.Replace(varName + ":=", "");

            varValue = Parse(mathExpression);
            varName = varName.Replace(" ", "");

            if (LocalVariables.ContainsKey(varName))
            {
                LocalVariables[varName] = varValue;
            }
            else
            {
                LocalVariables.Add(varName, varValue);
            }

            return varValue;
        }

        /// <summary>
        /// This will convert a string expression into a list of tokens that can be later executed by Parse or ProgrammaticallyParse methods.
        /// </summary>
        /// <param name="mathExpression"></param>
        /// <returns>A ReadOnlyCollection</returns>
        public ReadOnlyCollection<string> GetTokens(string mathExpression)
        {
            return Scanner(mathExpression).AsReadOnly();
        }

        /// <summary>
        /// This will correct sqrt() and arctan() written in different ways only.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string Correction(string input)
        {
            // Word corrections

            input =System.Text.RegularExpressions. Regex.Replace(input, "\\b(sqr|sqrt)\\b", "sqrt", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(atan2|arctan2)\\b", "arctan2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(acos|arccos)\\b", "arccos", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(asin|arcsin)\\b", "arcsin", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(acs|arccos)\\b", "arccos", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(asn|arcsin)\\b", "arcsin", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(atn|arctan)\\b", "arctan", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(log10|log)\\b", "log", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            input = System.Text.RegularExpressions.Regex.Replace(input, "\\b(fac|fact)\\b", "fact", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            //... and more

            return input;
        }


        /* UNDER THE HOOD - THE CORE OF THE PARSER */
        private List<string> Scanner(string expr)
        {
            // SCANNING THE INPUT STRING AND CONVERT IT INTO TOKENS

            var tokens = new List<string>();
            var vector = "";

            //_expr = _expr.Replace(" ", ""); // remove white space
            expr = expr.Replace("+-", "-"); // some basic arithmetical rules
            expr = expr.Replace("-+", "-");
            expr = expr.Replace("--", "+");

           /* foreach (var item in LocalVariables)
            {
                // replace the current variables with their value
                _expr = _expr.Replace(item.Key, "(" + item.Value.ToString(CULTURE_INFO) + ")");
            }*/

            for (var i = 0; i < expr.Length; i++)
            {
                var ch = expr[i];

                if (char.IsWhiteSpace(ch)) { } // could also be used to remove white spaces.
                else if (Char.IsLetter(ch))
                {
                    if (i != 0 && (Char.IsDigit(expr[i - 1]) || Char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                    {
                        tokens.Add("*");
                    }

                    vector = vector + ch;
                    
                    while ((i + 1) < expr.Length && Char.IsLetterOrDigit(expr[i + 1])) // here is it is possible to choose whether you want variables that only contain letters with or without digits.
                    {
                        i++;
                        vector = vector + expr[i];
                    }
                    
                    tokens.Add(vector);
                    vector = "";
                }
                else if (Char.IsDigit(ch))
                {
                    vector = vector + ch;
                    
                    while ((i + 1) < expr.Length && (Char.IsDigit(expr[i + 1]) || expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                    {
                        i++;
                        vector = vector + expr[i];
                    }
                    
                    tokens.Add(vector);
                    vector = "";
                }
                else if ((i + 1) < expr.Length && (ch == '-' || ch == '+') && Char.IsDigit(expr[i + 1]) && (i == 0 || OperatorList.IndexOf(expr[i - 1].ToString(CultureInfo.InvariantCulture)) != -1 || ((i - 1) > 0 && expr[i - 1] == '(')))
                {
                    // if the above is true, then, the token for that negative number will be "-1", not "-","1".
                    // to sum up, the above will be true if the minus sign is in front of the number, but
                    // at the beginning, for example, -1+2, or, when it is inside the brakets (-1).
                    // NOTE: this works for + sign as well!
                    vector = vector + ch;

                    while ((i + 1) < expr.Length && (Char.IsDigit(expr[i + 1]) || expr[i + 1] == '.')) // removed || _expr[i + 1] == ','
                    {
                        i++;
                        vector = vector + expr[i];
                    }

                    tokens.Add(vector);
                    vector = "";
                }
                else if (ch == '(')
                {
                    if (i != 0 && (Char.IsDigit(expr[i - 1]) || Char.IsDigit(expr[i - 1]) || expr[i - 1] == ')'))
                    {
                        tokens.Add("*");
                        // if we remove this line(above), we would be able to have numbers in function names. however, then we can't parser 3(2+2)
                        tokens.Add("(");
                    }
                    else
                        tokens.Add("(");
                }
                else
                {
                    tokens.Add(ch.ToString());
                }
            }

            return tokens;
            //return MathParserLogic(_tokens);
        }

        private double MathParserLogic(List<string> tokens)
        {
            // CALCULATING THE EXPRESSIONS INSIDE THE BRACKETS
            // IF NEEDED, EXECUTE A FUNCTION

            // Variables replacement
            for (var i = 0; i < tokens.Count; i++)
            {
                if (LocalVariables.Keys.Contains(tokens[i]))
                {
                    tokens[i] = LocalVariables[tokens[i]].ToString(CultureInfo);
                }
            }
            while (tokens.IndexOf("(") != -1)
            {
                // getting data between "(", ")"
                var open = tokens.LastIndexOf("(");
                var close = tokens.IndexOf(")", open); // in case open is -1, i.e. no "(" // , open == 0 ? 0 : open - 1

                if (open >= close)
                {
                    // if there is no closing bracket, throw a new exception
                    throw new ArithmeticException("No closing bracket/parenthesis! tkn: " + open.ToString(CultureInfo.InvariantCulture));
                }
                
                var roughExpr = new List<string>();
                
                for (var i = open + 1; i < close; i++)
                {
                    roughExpr.Add(tokens[i]);
                }

                double result; // the temporary result is stored here

                var functioName = tokens[open == 0 ? 0 : open - 1];
                var args = new double[0];
                
                if (LocalFunctions.Keys.Contains(functioName))
                {
                    if (roughExpr.Contains(";"))
                    {
                        // converting all arguments into a double array
                        for (var i = 0; i < roughExpr.Count; i++)
                        {
                            var firstCommaOrEndOfExpression = roughExpr.IndexOf(";", i) != -1 ? roughExpr.IndexOf(";", i) : roughExpr.Count;
                            var defaultExpr = new List<string>();

                            while (i < firstCommaOrEndOfExpression)
                            {
                                defaultExpr.Add(roughExpr[i]);
                                i++;
                            }

                            // changing the size of the array of arguments
                            Array.Resize(ref args, args.Length + 1);
                            
                            if (defaultExpr.Count == 0)
                            {
                                args[args.Length - 1] = 0;
                            }
                            else
                            {
                                args[args.Length - 1] = BasicArithmeticalExpression(defaultExpr);
                            }
                        }

                        // finnaly, passing the arguments to the given function
                        result = double.Parse(LocalFunctions[functioName](args).ToString(CultureInfo), CultureInfo);
                    }
                    else
                    {
                        // but if we only have one argument, then we pass it directly to the function
                        result = double.Parse(LocalFunctions[functioName](new[] { BasicArithmeticalExpression(roughExpr) }).ToString(CultureInfo), CultureInfo);
                    }
                }
                else
                {
                    // if no function is need to execute following expression, pass it
                    // to the "BasicArithmeticalExpression" method.
                    result = BasicArithmeticalExpression(roughExpr);
                }

                // when all the calculations have been done
                // we replace the "opening bracket with the result"
                // and removing the rest.
                tokens[open] = result.ToString(CultureInfo);
                tokens.RemoveRange(open + 1, close - open);
                if (LocalFunctions.Keys.Contains(functioName))
                {
                    // if we also executed a function, removing
                    // the function name as well.
                    tokens.RemoveAt(open - 1);
                }
            }

            // at this point, we should have replaced all brackets
            // with the appropriate values, so we can simply
            // calculate the expression. it's not so complex
            // any more!
            return BasicArithmeticalExpression(tokens);
        }

        private double BasicArithmeticalExpression(List<string> tokens)
        {
            // PERFORMING A BASIC ARITHMETICAL EXPRESSION CALCULATION
            // THIS METHOD CAN ONLY OPERATE WITH NUMBERS AND OPERATORS
            // AND WILL NOT UNDERSTAND ANYTHING BEYOND THAT.

            switch (tokens.Count)
            {
                case 1:
                    return double.Parse(tokens[0], CultureInfo);
                case 2:
                {
                    var op = tokens[0];

                    if (op == "-" || op == "+")
                        return double.Parse((op == "+" ? "" : "-") + tokens[1], CultureInfo);

                    return OperatorAction[op](0, double.Parse(tokens[1], CultureInfo));
                }
                case 0:
                    return 0;
            }

            foreach (var op in OperatorList)
            {
                while (tokens.IndexOf(op) != -1)
                {
                    var opPlace = tokens.IndexOf(op);

                    var numberA = Convert.ToDouble(tokens[opPlace - 1], CultureInfo);
                    var numberB = Convert.ToDouble(tokens[opPlace + 1], CultureInfo);

                    var result = OperatorAction[op](numberA, numberB);

                    tokens[opPlace - 1] = result.ToString(CultureInfo);
                    tokens.RemoveRange(opPlace, 2);
                }
            }
            return Convert.ToDouble(tokens[0], CultureInfo);
        }
    }
}