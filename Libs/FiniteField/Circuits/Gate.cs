using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class Gate
	{
		private readonly int prime;		// the prime number
		public readonly IList<Wire> InputWires;
		public readonly IList<Wire> OutputWires;
		public Zp OutputValue { get; set; }
		public Operation Operation { get; private set; }

		public bool IsDegreeReductionNeeded
		{
			get
			{
				return (Operation == Operation.Mul || Operation == Operation.Div) &&
					(InputWires.Count(w => w.ConstValue == null) > 1);
			}
		}

		public Gate(IList<Wire> inputWires, Wire outputWire, Operation operation, int prime)
			: this(inputWires, new List<Wire>() { outputWire }, operation, prime)
		{
		}

		public Gate(IList<Wire> inputWires, IList<Wire> outputWires, Operation operation, int prime)
		{
			Debug.Assert(operation != Operation.Mul || inputWires.Count <= 2); //max 2 inputs for mul gate
			InputWires = inputWires;
			OutputWires = new List<Wire>(outputWires);
			Operation = operation;
			this.prime = prime;
		}

		public bool ReadyToCalculate
		{
			get
			{
				foreach (Wire wire in InputWires)
				{
					if (!wire.Valid)
						return false;
				}
				return true;
			}
		}

		public bool IsOutputReady
		{
			get
			{
				return OutputValue != null;
			}
		}

		public void DeleteOutputValue()
		{
			OutputValue = null;
		}

		internal Zp InternalCalculate(IList<Zp> inputs)
		{
			var values = new List<Zp>();
			foreach (Wire wire in InputWires)
			{
				if (wire.IsInput)
				{
					if (inputs.Count <= wire.InputIndex)
						throw new Exception("Input " + wire.InputIndex + " is expected - not found in the list given");

					values.Add(inputs[wire.InputIndex]);
				}
				else
				{
					Debug.Assert(wire.SourceGate != null && wire.SourceGate.IsOutputReady);
					values.Add(wire.ConstValue != null ? wire.ConstValue : wire.SourceGate.OutputValue);
				}
			}
			OutputValue = new Zp(values[0]);
			values.RemoveAt(0);

			foreach (Zp value in values)
			{
				OutputValue.Calculate(value,
					Operation == Operation.Div && value.Value == 0 ? Operation.Mul : Operation);
			}
			return OutputValue;
		}

		public void AddOutputWire(Wire wire)
		{
			OutputWires.Add(wire);
		}

		public void AddInputWire(Wire wire)
		{
			InputWires.Add(wire);
		}

		public string Display(IList<Gate> gates)
		{
			var sb = "";
			foreach (Wire wire in InputWires)
			{
				sb += wire.IsInput ? "{" + wire.InputIndex + "}" :
					wire.IsOutput ? "{" + wire.IsOutput + "}" :
					wire.ConstValue != null ? wire.ConstValue.ToString() :
					"Gate" + gates.IndexOf(wire.SourceGate);

				sb += " " + Operation + " ";
			}
			return sb.Substring(0, sb.Length - Operation.ToString().Length - 1);
		}
	}
}