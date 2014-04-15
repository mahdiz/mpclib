using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class BigGate
	{
		private readonly BigInteger prime;		// the prime number
		public readonly IList<BigWire> InputWires;
		public readonly IList<BigWire> OutputWires;
		public BigZp OutputValue { get; set; }
		public Operation Operation { get; private set; }

		public bool IsDegreeReductionNeeded
		{
			get
			{
				return (Operation == Operation.Mul || Operation == Operation.Div) &&
					(InputWires.Count(w => w.ConstValue == null) > 1);
			}
		}

		public BigGate(IList<BigWire> inputWires, BigWire outputWire, Operation operation, BigInteger prime)
			: this(inputWires, new List<BigWire>() { outputWire }, operation, prime)
		{
		}

		public BigGate(IList<BigWire> inputWires, IList<BigWire> outputWires, Operation operation, BigInteger prime)
		{
			Debug.Assert(operation != Operation.Mul || inputWires.Count <= 2); //max 2 inputs for mul gate
			InputWires = inputWires;
			OutputWires = new List<BigWire>(outputWires);
			Operation = operation;
			this.prime = prime;
		}

		public bool ReadyToCalculate
		{
			get
			{
				foreach (BigWire wire in InputWires)
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

		internal BigZp InternalCalculate(IList<BigZp> inputs)
		{
			var values = new List<BigZp>();
			foreach (BigWire wire in InputWires)
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
			OutputValue = new BigZp(values[0]);
			values.RemoveAt(0);

			foreach (BigZp value in values)
			{
				OutputValue.Calculate(value,
					Operation == Operation.Div && value.Value == 0 ? Operation.Mul : Operation);
			}
			return OutputValue;
		}

		public void AddOutputWire(BigWire wire)
		{
			OutputWires.Add(wire);
		}

		public void AddInputWire(BigWire wire)
		{
			InputWires.Add(wire);
		}

		public string Display(IList<BigGate> gates)
		{
			var sb = "";
			foreach (BigWire wire in InputWires)
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