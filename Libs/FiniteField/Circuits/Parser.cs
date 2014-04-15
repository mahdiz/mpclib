using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class Parser
	{
		#region Fields

		private static string s_inputsDec = "Inputs";
		private static string s_outputsDec = "Outputs";
		private static string variableExp = "([a-zA-Z][a-zA-Z0-9]*)";
		private static string conditionalExp = "(.*)?(.*):(.*)";
		private static string variablePattern = variableExp;
		private static string formulaPattern = variableExp + "=(.*)";
		private static string conditionalTerm = conditionalExp;
		private static string inputsDecleration = s_inputsDec + "=(.*)";
		private static string outputsDecleration = s_outputsDec + "=(.*)";

		private string pathOrString;
		private string outputVar;
		private Wire outputWire;
		private int prime;
		private Circuit circuit;
		private IList<string> inputs = new List<string>();
		private Dictionary<string, Wire> varsToWires;

		#endregion Fields

		public virtual Circuit Circuit
		{
			get
			{
				return circuit;
			}
		}

		public Parser(string pathOrString, int prime)
		{
			this.pathOrString = pathOrString;
			this.prime = prime;
		}

		public Parser(FunctionType type, int numPlayers, int prime)
			: this(BuildCircuitDef(type, numPlayers), prime)
		{
		}

		public virtual void Parse()
		{
			StreamReader reader;
			if (File.Exists(pathOrString))
				reader = new StreamReader(pathOrString);
			else
				reader = new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(pathOrString)));

			varsToWires = new Dictionary<string, Wire>();
			string line = null;
			int lineNumber = 0;
			while (!reader.EndOfStream)
			{
				line = reader.ReadLine();
				if (line == null)
					break;

				int commentIndex = line.IndexOf("//");
				if (commentIndex != -1)
					line = line.Substring(0, commentIndex);

				try
				{
					ReadLine(line.Trim().Replace("\t", ""));	// remove white spaces
					lineNumber++;
				}
				catch (Exception e)
				{
					throw new Exception("Error in line " + lineNumber + "\n" + e.Message);
				}
			}
			outputWire = GetWireFromString(outputVar);
			outputWire.IsOutput = true;
			circuit = CreateCircuit(prime, inputs);
			if (circuit == null)
				throw new Exception("Error in parsing the circuit!");
		}

		public Circuit CreateCircuit(int prime, IList<string> inputs)
		{
			var gates = new List<Gate>();
			var currWires = new List<Wire>() { outputWire };

			while (currWires.Count > 0)
			{
				var toRemove = new List<Wire>();
				var toAdd = new List<Wire>();

				foreach (var wire in currWires)
				{
					if (wire.IsInput || wire.ConstValue != null)
					{
						if (wire.IsOutput && wire.IsInput)
							throw new Exception("Forbidden: Input " + wire.InputIndex + " is also output " + wire.IsOutput);

						if (wire.IsOutput && wire.ConstValue != null)
							throw new Exception("Forbidden: Output " + wire.IsOutput + " has a const value " + wire.ConstValue);

						toRemove.Add(wire);
						continue;
					}

					if (!IsTargetInGates(gates, wire) && !wire.IsOutput)
						continue;

					Debug.Assert(wire.SourceGate != null);
					gates.Remove(wire.SourceGate);	// remove source if it is already in the list - it means its output is going to two gates
					gates.Insert(0, wire.SourceGate);
					toRemove.Add(wire);
					toAdd.AddRange(wire.SourceGate.InputWires);
				}
				toRemove.ForEach(w => currWires.Remove(w));
				currWires.AddRange(toAdd);
			}
			if (gates.Count == 0)
				throw new Exception("No valid gates in the circuit.");

			return new Circuit(prime, gates, inputs, outputWire.SourceGate);
		}

		private static string BuildCircuitDef(FunctionType type, int numPlayers)
		{
			int i;
			string output = "", op = "", input = "", function = "";
			bool equalitySelected = false;

			switch (type)
			{
				case FunctionType.Sum:
					op = "+";
					break;

				case FunctionType.Mul:
					op = "*";
					break;

				case FunctionType.Equal:
					op = "==";
					equalitySelected = true;
					break;
			}

			input += "Inputs= X0";
			for (i = 1; i < numPlayers; i++)
				input += ", " + "X" + i;

			output += "Outputs=Y\n";

			if (equalitySelected)
			{
				function = input + "\n" + output;
				for (i = 0; i < numPlayers - 1; i++)
					function += "Y" + i + " =  (X" + i + " ==X" + (i + 1) + ")\n";

				function += "Y=Y0";
				for (i = 1; i < numPlayers - 1; i++)
					function += "*Y" + i;

				function += "\n";
			}
			else
			{
				function += input + "\n" + output + "Y=(X0";
				for (i = 1; i < numPlayers; i++)
					function += op + "X" + i;

				function += ")\n";
			}
			return function;
		}

		#region Private Methods

		private void ReadLine(string line)
		{
			var lineType = GetLineType(line);
			if (lineType == LineType.None)
				throw new Exception("Line is not in a valid format!");

			switch (lineType)
			{
				case LineType.Formula:
					ReadFormula(line);
					break;

				case LineType.Inputs:
					ReadInputOutputDecleration(line, true);
					break;

				case LineType.Output:
					ReadInputOutputDecleration(line, false);
					break;

				case LineType.Empty:
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}

		private Wire GetWireFromCondition(string line)
		{
			return GetWireFromCondition(line, null);
		}

		private Wire GetWireFromCondition(string line, Wire conditionWire)
		{
			int index1 = line.IndexOf("?");
			int index2 = line.IndexOf(":");
			Debug.Assert(index1 != -1 && index2 != -1 && index1 < index2);

			// get all 3 expression wires
			if (conditionWire == null)
				conditionWire = GetWireFromExpression(line.Substring(0, index1));

			var firstWire = GetWireFromExpression(line.Substring(index1 + 1, index2 - (index1 + 1)));
			var secondWire = GetWireFromExpression(line.Substring(index2 + 1));

			// normalizedConditionWire = conditionWire/conditionWire
			var normalizedConditionWire = new Wire();
			SetGate(new List<Wire>() { conditionWire, conditionWire }, normalizedConditionWire, Operation.Div);

			// reverseCondition = 1 - normalizedConditionWire
			var reverseCondition = new Wire();
			SetGate(new List<Wire>() { new Wire(new Zp(prime, 1)), normalizedConditionWire }, reverseCondition, Operation.Sub);

			// firstWireMultiplied = firstWire * normalizedConditionWire
			var firstWireMultiplied = new Wire();
			SetGate(new List<Wire>() { firstWire, normalizedConditionWire }, firstWireMultiplied, Operation.Mul);

			// secondWireMultiplied = secondWire * reverseCondition
			var secondWireMultiplied = new Wire();
			SetGate(new List<Wire>() { secondWire, reverseCondition }, secondWireMultiplied, Operation.Mul);

			// result = firstWireMultiplied + secondWireMultiplied
			var resultWire = new Wire();
			SetGate(new List<Wire>() { firstWireMultiplied, secondWireMultiplied }, resultWire, Operation.Add);
			return resultWire;
		}

		private bool ReadInputOutputDecleration(string line, bool isInput)
		{
			var index = line.IndexOf("=");
			Debug.Assert(index != -1);
			var tokens = line.Substring(index + 1).Split(new char[] { ',' });

			for (int i = 0; i < tokens.Length; i++)
			{
				var token = tokens[i].Trim();
				CheckVariableSynthax(token);
				var wire = new Wire(i, isInput);

				if (varsToWires.ContainsKey(token))
				{
					if (isInput)
						varsToWires[token].InputIndex = index;
					else
						varsToWires[token].IsOutput = true;
				}
				else
					varsToWires[token] = wire;

				if (!isInput)
				{
					Debug.Assert(outputVar == null);
					outputVar = token;
				}
				else
					inputs.Add(token);
			}
			return true;
		}

		private void CheckVariableSynthax(string @var)
		{
			if (!Regex.IsMatch(@var, variablePattern))
				throw new Exception("Variable '" + @var + "' is not a valid variable name");
		}

		private bool ReadFormula(string line)
		{
			CheckParanthesisSyntax(line);
			Debug.Assert(Regex.IsMatch(line, formulaPattern));

			int index = line.IndexOf("=");
			string targetVar = line.Substring(0, index);
			CheckVariableSynthax(targetVar);

			varsToWires[targetVar] = GetWireFromExpression(line.Substring(index + 1));
			return true;
		}

		private int GetIndexOfMinusOrPlus(string line, int fromIndex)
		{
			int index1 = fromIndex == -1 ? line.IndexOf("+") : line.IndexOf("+", fromIndex);
			int index2 = fromIndex == -1 ? line.IndexOf("-") : line.IndexOf("-", fromIndex);
			return index1 == -1 ? index2 : index2 == -1 ? index1 : index1 < index2 ? index1 : index2;
		}

		private int getIndexOfDivOrMul(string line, int fromIndex)
		{
			int index1 = fromIndex == -1 ? line.IndexOf("*") : line.IndexOf("*", fromIndex);
			int index2 = fromIndex == -1 ? line.IndexOf("/") : line.IndexOf("/", fromIndex);
			return index1 == -1 ? index2 : index2 == -1 ? index1 : index1 < index2 ? index1 : index2;
		}

		private Wire GetWireFromExpression(string line)
		{
			int mulIndex = FindCorrectIndex(line, SearchFor.DivMul);
			int addIndex = FindCorrectIndex(line, SearchFor.AddMin);
			int equalsIndex = FindCorrectIndex(line, SearchFor.Equal);
			int questionMark = FindCorrectIndex(line, SearchFor.QuestionMark);
			int parIndex = line.IndexOf("(");

			if (questionMark != -1 &&
				(new Regex(conditionalTerm)).IsMatch(line) &&
					parIndex != 0 &&
					(addIndex == -1 || addIndex > questionMark ||
						addIndex > equalsIndex && addIndex < questionMark &&
						!line.Substring(equalsIndex, questionMark - equalsIndex).Contains("(")) &&
						(mulIndex == -1 || mulIndex > questionMark || mulIndex > equalsIndex &&
						mulIndex < questionMark && !line.Substring(equalsIndex, questionMark - equalsIndex).Contains("(")))

				return GetWireFromCondition(line);

			if (mulIndex == -1 && addIndex == -1 && equalsIndex == -1 && parIndex != 0)
				return GetWireFromString(line);

			if (addIndex > 0 && parIndex != 0 && equalsIndex == -1 && (mulIndex == -1 || addIndex < mulIndex)) //handle add gate
			{
				Wire firstWire = GetWireFromString(line.Substring(0, addIndex));
				return CreateAddGates(line.Substring(addIndex), firstWire);
			}

			if (parIndex == 0)
				return CreateParenthesesWire(line, true);

			if (equalsIndex != -1)
				return CreateEqualsGate(line);

			if (addIndex == -1)
				return CreateMulGate(null, 0, line);
			else
				return CreateAddGates(line.Substring(addIndex),
					CreateMulGate(null, 0, line.Substring(0, addIndex)));
		}

		private Wire CreateEqualsGate(string line)
		{
			return CreateEqualsGate(line, null);
		}

		private Wire CreateEqualsGate(string line, Wire firstWire)
		{
			int equalsIndex = line.IndexOf("==");
			Debug.Assert(equalsIndex != -1);
			if (firstWire == null)
				firstWire = GetWireFromExpression(line.Substring(0, equalsIndex));

			var secondWire = GetWireFromExpression(line.Substring(equalsIndex + 2));
			var compareWire = new Wire();
			SetGate(new List<Wire>() { firstWire, secondWire }, compareWire, Operation.Sub);
			var normilizedWire = new Wire();
			SetGate(new List<Wire>() { compareWire, compareWire }, normilizedWire, Operation.Div);
			var resultWire = new Wire();
			SetGate(new List<Wire>() { new Wire(new Zp(prime, 1)), normilizedWire }, resultWire, Operation.Sub);
			return resultWire;
		}

		private int FindCorrectIndex(string line, SearchFor searchFor)
		{
			int index = searchFor == SearchFor.AddMin ?
				GetIndexOfMinusOrPlus(line, 1) :
				searchFor == SearchFor.DivMul ?
					getIndexOfDivOrMul(line, 1) :
						searchFor == SearchFor.Equal ?
							line.IndexOf("==") : line.IndexOf("?");

			int parIndex = line.IndexOf("(");
			if (index == -1 || parIndex == -1 || index < parIndex)
				return index;

			int parEndIndex = parIndex + FindEndingParantheses(line.Substring(parIndex));
			int correctIndex = FindCorrectIndex(line.Substring(parEndIndex), searchFor);
			return correctIndex != -1 ? parEndIndex + correctIndex : -1;
		}

		private void CheckParanthesisSyntax(string line)
		{
			int opened = 0;
			foreach (char c in line.ToCharArray())
			{
				if (c == '(')
					opened++;

				if (c == ')')
				{
					if (opened == 0)
						throw new Exception("Closing ) without openning (");
					else
						opened--;
				}
			}
			if (opened > 0)
				throw new Exception("Openning '(' without closing ')");
		}

		private int FindEndingParantheses(string line)
		{
			Debug.Assert(line.StartsWith("("));
			int numOfOpennings = 0;

			for (int i = 1; i < line.Length; i++)
			{
				char c = line[i];
				if (c == '(')
					numOfOpennings++;

				if (c == ')')
				{
					if (numOfOpennings == 0)
						return i;
					else
						numOfOpennings--;
				}
			}
			throw new Exception("Openning '(' without closing ')");
		}

		private Wire CreateParenthesesWire(string line, bool keepCalculating)
		{
			int parEndIndex = FindEndingParantheses(line);
			Wire firstWire = GetWireFromExpression(line.Substring(1, parEndIndex - 1));
			line = line.Substring(parEndIndex + 1);

			if (!keepCalculating)
				return firstWire;

			if (line.StartsWith("+") || line.StartsWith("-"))
				return CreateAddGates(line, firstWire);

			if (line.StartsWith("*"))
				return CreateMulGate(firstWire, Operation.Mul, line.Substring(1));

			if (line.StartsWith("/"))
				return CreateMulGate(firstWire, Operation.Div, line.Substring(1));

			if (line.Equals(""))
				return firstWire;

			if (line.StartsWith("=="))
				return CreateEqualsGate(line, firstWire);

			if (line.StartsWith("?"))
				return GetWireFromCondition(line, firstWire);

			throw new Exception("No operation after closing )");
		}

		private Wire CreateMulGate(Wire firstWireOrNull, Operation firstOpOrNull, string line)
		{
			int addIndex = FindCorrectIndex(line, SearchFor.AddMin);
			if (addIndex == -1)
				return CreateMulGates(firstWireOrNull, firstOpOrNull, line);

			var firstWire = CreateMulGates(firstWireOrNull, firstOpOrNull, line.Substring(0, addIndex));
			return CreateAddGates(line.Substring(addIndex), firstWire);
		}

		private void SplitMul(string line, IList<string> operands, IList<Operation> operations)
		{
			int index = FindCorrectIndex(line, SearchFor.DivMul);
			while (index != -1)
			{
				operands.Add(line.Substring(0, index));
				operations.Add(GetOperation(line.Substring(index, 1)));
				line = line.Substring(index + 1);
				index = FindCorrectIndex(line, SearchFor.DivMul);
			}
			operands.Add(line);
		}

		private Wire CreateMulGates(Wire firstWireOrNull, Operation firstOpOrNull, string line)
		{
			var tokens = new List<string>();
			var operations = new List<Operation>();

			SplitMul(line, tokens, operations);
			Debug.Assert(tokens.Count >= 2);
			var firstWire = firstWireOrNull == null ? GetWireFromString(tokens[0]) : firstWireOrNull;
			Operation currOperation = firstOpOrNull;

			if (currOperation == Operation.None)
				currOperation = operations[0];

			for (int i = firstWireOrNull == null ? 1 : 0; i < tokens.Count; i++)
			{
				var secondWire = GetWireFromString(tokens[i]);
				var outputWire = new Wire();
				SetGate(new List<Wire>() { firstWire, secondWire }, outputWire, currOperation);
				firstWire = outputWire;
				if (operations.Count <= i)
					break;

				currOperation = operations[i];
			}
			return firstWire;
		}

		private void SetGate(IList<Wire> inputWires, Wire outputWire, Operation op)
		{
			Gate gate = new Gate(inputWires, new List<Wire>() { outputWire }, op, prime);
			foreach (Wire wire in inputWires)
				wire.TargetGate = gate;

			outputWire.SourceGate = gate;
		}

		private int GetFixedIndex(int index)
		{
			return index == -1 ? int.MaxValue : index;
		}

		private int GetFirstParamterIndex(string line)
		{
			int addIndex = GetIndexOfMinusOrPlus(line, 0);
			int mulIndex = getIndexOfDivOrMul(line, 0);
			int parIndex = line.IndexOf("(");
			int equalsIndex = line.IndexOf("==");
			int questionMarkIndex = line.IndexOf("?");

			int index = Math.Min(GetFixedIndex(addIndex),
				Math.Min(GetFixedIndex(mulIndex),
				Math.Min(GetFixedIndex(parIndex),
				Math.Min(GetFixedIndex(equalsIndex),
				GetFixedIndex(questionMarkIndex)))));

			return index == int.MaxValue ? -1 : index;
		}

		private Operation GetOperation(string str)
		{
			switch (str)
			{
				case "+":
					return Operation.Add;
				case "-":
					return Operation.Sub;
				case "*":
					return Operation.Mul;
				case "/":
					return Operation.Div;
				default:
					return Operation.None;
			}
		}

		private Wire CreateAddGates(string line, Wire firstWire)
		{
			var op = GetOperation(line.Substring(0, 1));
			line = line.Substring(1);

			while (op == Operation.Sub)
			{
				int firstIndex = GetFirstParamterIndex(line);
				if (firstIndex == -1)
					break;

				if (firstIndex == 0 && !line.StartsWith("(") || firstIndex == -1)
					break;

				if (getIndexOfDivOrMul(line, 0) == firstIndex)
					break;

				bool isPar = line.StartsWith("(");
				Wire secondWire = isPar ? CreateParenthesesWire(line, false) : GetWireFromExpression(line.Substring(0, firstIndex));
				Wire outputWire = new Wire();

				SetGate(new List<Wire>() { firstWire, secondWire }, outputWire, op);
				line = line.Substring(isPar ? FindEndingParantheses(line) + 1 : firstIndex);

				if (line.Length == 0)
					return outputWire;

				op = GetOperation(line.Substring(0, 1));
				line = line.Substring(1);
				firstWire = outputWire;
			}
			if (op != Operation.Add && op != Operation.Sub)
				return CreateMulGate(firstWire, op, line);

			var sndWire = GetWireFromExpression(line);
			var outWire = new Wire();
			SetGate(new List<Wire>() { firstWire, sndWire }, outWire, op);
			return outWire;
		}

		private Wire GetWireFromString(string st)
		{
			if (st.StartsWith("("))
				return CreateParenthesesWire(st, false);

			Wire wire = null;
			if (varsToWires.ContainsKey(st))
			{
				wire = varsToWires[st];
				if (wire.IsOutput && wire.SourceGate == null)
					throw new Exception("Variable " + st + " was not initialized");
			}

			if (wire != null)
			{
				if (wire.TargetGate != null)
					return wire.Clone();
				else
					return wire;
			}

			int value;
			try
			{
				value = Convert.ToInt32(st);
			}
			catch (Exception)
			{
				if (st.Contains("("))
					throw new Exception("No operation before opening ( in expression: " + st);
				throw new Exception("variable '" + st + "' is not defined");
			}
			return new Wire(new Zp(prime, value));
		}

		private LineType GetLineType(string line)
		{
			if (line.Equals(""))
				return LineType.Empty;

			if (Regex.IsMatch(line, inputsDecleration))
				return LineType.Inputs;

			if (Regex.IsMatch(line, outputsDecleration))
				return LineType.Output;

			//        matcher = setConditionValue.matcher(line);
			//        if (matcher.matches()){
			//            return LineType.SETTING_CONDITION_VALUE;
			//        }
			if (Regex.IsMatch(line, formulaPattern))
				return LineType.Formula;

			return LineType.None;
		}

		private static bool IsTargetInGates(IList<Gate> gates, Wire outputWire)
		{
			foreach (Gate gate in gates)
			{
				if (outputWire.TargetGate == gate)
					return true;
			}
			return false;
		}

		#endregion Private Methods
	}
}