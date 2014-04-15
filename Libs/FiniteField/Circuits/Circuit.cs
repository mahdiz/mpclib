using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class Circuit
	{
		//Dictionary<int, Circuit> exponentialGates = new Dictionary<int, Circuit>();
		public IList<Gate> Gates { get; set; }

		public IList<string> Inputs { get; set; }

		public virtual IList<Wire> InputWires
		{
			get
			{
				return GetInputOrOutputWires(true);
			}
		}

		public readonly Gate OutputGate;
		protected internal int Prime;

		public Circuit(int prime, IList<Gate> gates, IList<string> inputs, Gate outputGate)
		{
			Prime = prime;
			Gates = gates;
			Inputs = inputs;
			OutputGate = outputGate;
		}

		public virtual Zp Output
		{
			get
			{
				foreach (var gate in Gates)
				{
					foreach (var wire in gate.OutputWires)
					{
						if (wire.IsOutput)
						{
							Debug.Assert(gate.IsOutputReady);
							return gate.OutputValue;
						}
					}
				}
				return null;
			}
		}

		/// <summary>
		/// The number of inputs declared in code - not all inputs must be used in curcuit
		/// </summary>
		public virtual int InputCount
		{
			get
			{
				return Inputs.Count;
			}
		}

		// Commented by Mahdi: Just do not need!
		///// <summary>
		///// The number of actual inputs - that are used in the circuit
		///// </summary>
		//public virtual int RealInputCount
		//{
		//    get
		//    {
		//        IList<Wire> inputWires = InputWires;
		//        var currInputs = new HashSet<int>();
		//        foreach (Wire wire in inputWires)
		//        {
		//            if (wire.InputIndex != null)
		//                currInputs.Add(wire.InputIndex);
		//        }
		//        return currInputs.Count;
		//    }
		//}

		public virtual bool MultipleContained
		{
			get
			{
				foreach (Gate gate in Gates)
				{
					if (gate.Operation == Operation.Mul || gate.Operation == Operation.Div)
						return true;
				}
				return false;
			}
		}

		private IList<Wire> GetInputOrOutputWires(bool isInput) //todo - check that there are no duplicates
		{
			var inputOrOutputWires = new List<Wire>();
			foreach (Gate gate in Gates)
			{
				var wires = isInput ? gate.InputWires : gate.OutputWires;
				foreach (Wire wire in wires)
				{
					if (wire.IsInput && isInput || wire.IsOutput && !isInput)
						inputOrOutputWires.Add(wire);
				}
			}
			return inputOrOutputWires;
		}

		public virtual void EmptyResult()
		{
			foreach (Gate gate in Gates)
				gate.DeleteOutputValue();
		}

		public override string ToString()
		{
			var sb = "Inputs: ";
			var inputWires = InputWires;
			var usedInputs = new List<int>();
			var inputWiresSB = "";

			foreach (Wire wire in inputWires)
			{
				if (usedInputs.Contains(wire.InputIndex))
					continue;

				inputWiresSB += wire.InputIndex + ", ";
				usedInputs.Add(wire.InputIndex);
			}
			sb += inputWiresSB.Substring(0, inputWiresSB.Length - 2) + " | ";
			foreach (Gate gate in Gates)
				sb += "Gate" + Gates.IndexOf(gate) + ": " + gate.Display(Gates) + " | ";

			sb += "Output: Gate" + Gates.IndexOf(OutputGate);
			return sb.ToString();
		}

		//public static Circuit CreateExponentCircuit(int exp, int p)
		//{
		//    var currWires = new List<Wire>();
		//    for (int i = 0; i < exp; i++)
		//        currWires.Add(new Wire(0, true));

		//    var gates = new List<Gate>();
		//    while (currWires.Count > 1)
		//    {
		//        var outputWires = new List<Wire>();
		//        for (int i = 0; i < currWires.Count - 1; i = i + 2)
		//        {
		//            var outputWire = new Wire();
		//            Gate gate = new Gate(new List<Wire>(){ currWires[i], currWires[i + 1] },
		//                outputWire, Operation.Mul, p);

		//            currWires[i].TargetGate = gate;
		//            currWires[i + 1].TargetGate = gate;
		//            outputWire.SourceGate = gate;
		//            outputWires.Add(outputWire);
		//            gates.Add(gate);
		//        }
		//        if (currWires.Count % 2 != 0)
		//            outputWires.Add(currWires[currWires.Count - 1]);

		//        currWires = outputWires;
		//    }
		//    currWires[0].IsOutput = 0;
		//    return new Circuit(p, gates, new List<string>() { "x" });
		//}

		private static string[] StringSplit(string source, string regexDelimiter, bool trimTrailingEmptyStrings)
		{
			var splitArray = Regex.Split(source, regexDelimiter);

			if (trimTrailingEmptyStrings)
			{
				if (splitArray.Length > 1)
				{
					for (int i = splitArray.Length; i > 0; i--)
					{
						if (splitArray[i - 1].Length > 0)
						{
							if (i < splitArray.Length)
								System.Array.Resize(ref splitArray, i);
							break;
						}
					}
				}
			}
			return splitArray;
		}

		/// <summary>
		/// Returns a List of all outputs.
		/// </summary>
		internal Zp InternalCalculate(string inputs, int prime)
		{
			inputs = inputs.Replace(" ", "");	// remove white spaces
			string[] tokens = StringSplit(inputs, ",", true);
			var usersInputs = new List<Zp>();

			for (int i = 0; i < tokens.Length; i++)
				usersInputs.Add(new Zp(prime, Convert.ToInt32(tokens[i])));

			foreach (Gate gate in Gates)
				gate.InternalCalculate(usersInputs);

			return Output;
		}

		//public Circuit getDivisionExponentialCircuit(int p)
		//{
		//    var c = exponentialGates[p];
		//    if (c == null)
		//    {
		//        c = (new ExponentCircuit(p, p - 2)).Circuit;
		//        if (c == null)
		//            c = createExponentCircuit(p - 2, p);

		//        exponentialGates[p] = c;
		//        return c;
		//    }
		//    c.emptyResult();
		//    return c;
		//}

		//private class ExponentCircuit
		//{
		//    private int prime;
		//    private int exp;
		//    Dictionary<int, Wire> calculatedValues = new Dictionary<int, Wire>();

		//    public ExponentCircuit(int prime, int exp)
		//    {
		//        this.prime = prime;
		//        this.exp = exp;
		//    }

		//    internal virtual Circuit Circuit
		//    {
		//        get
		//        {
		//            var outputWire = getWireForExp(exp);
		//            outputWire.IsOutput = 0;
		//            try
		//            {
		//                return Parser.CreateCircuit(new List<Wire>() { outputWire }, prime, new List<string>(){ "x" });
		//            }
		//            catch (Exception)
		//            {
		//                Debug.Assert(false);
		//                return null;
		//            }
		//        }
		//    }

		//    private Wire getWireForExp(int exp)
		//    {
		//        Wire wire = calculatedValues[exp];
		//        if (wire != null)
		//            return wire.Clone();

		//        if (exp == 1)
		//            return new Wire(0, true);

		//        int biggestExponential = getBiggest2Exponential(exp);
		//        if (biggestExponential == exp)
		//        {
		//            return getWireFor2ExponentialExp(exp);
		//        }
		//        Wire firstWire = getWireForExp(biggestExponential);
		//        Wire secondWire = getWireForExp(exp - biggestExponential);
		//        return createGate(firstWire, secondWire);
		//    }

		//    private Wire getWireFor2ExponentialExp(int exp)
		//    {
		//        int numberOfIterations = get2Log(exp);
		//        var firstWire = new Wire(0, true);
		//        var secondWire = new Wire(0, true);
		//        Wire outputWire = createGate(firstWire, secondWire);
		//        calculatedValues[2] = outputWire;
		//        for (int i = 1; i < numberOfIterations; i++)
		//        {
		//            firstWire = outputWire;
		//            secondWire = outputWire.Clone();
		//            outputWire = createGate(firstWire, secondWire);
		//            calculatedValues[(int)Math.Pow(2, i + 1)] = outputWire;
		//        }
		//        calculatedValues[exp] = outputWire;
		//        return outputWire;
		//    }

		//    private Wire createGate(Wire firstWire, Wire secondWire)
		//    {
		//        var outputWire = new Wire();
		//        var gate = new Gate(
		//            new List<Wire>() { firstWire, secondWire }, outputWire, Operation.Mul, prime);
		//        firstWire.TargetGate = gate;
		//        secondWire.TargetGate = gate;
		//        outputWire.SourceGate = gate;
		//        return outputWire;
		//    }

		//    private int getBiggest2Exponential(int max)
		//    {
		//        if (max == 0)
		//            throw new Exception("Cannot find 2 exponential");

		//        int i = 1;
		//        for (i = 1; i <= max; i = 2 * i) ;
		//        return i / 2;
		//    }

		//    private int get2Log(int num)
		//    {
		//        Debug.Assert(num == getBiggest2Exponential(num));
		//        int j = 0;
		//        for (int i = 1; i <= num; i = 2 * i) j++;
		//        return j - 1;
		//    }
		//}
	}
}