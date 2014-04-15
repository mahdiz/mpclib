using System.Diagnostics;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class Wire
	{
		public int InputIndex;
		public bool IsOutput;
		public readonly Zp ConstValue;
		private Gate sourceGate;
		private Gate targetGate;

		#region Constructors

		public Wire()
		{
			InputIndex = -1;
		}

		public Wire(Zp constValue)
			: this()
		{
			this.ConstValue = constValue;
		}

		public Wire(int inputIndex, bool isInput)
		{
			if (isInput)
				this.InputIndex = inputIndex;
			else
			{
				this.IsOutput = true;
				InputIndex = -1;
			}
		}

		public Wire(int inputIndex, Gate targetGate)
		{
			this.InputIndex = inputIndex;
			this.targetGate = targetGate;
		}

		public Wire(Gate sourceGate, bool isOutput)
		{
			this.IsOutput = isOutput;
			this.sourceGate = sourceGate;
			InputIndex = -1;
		}

		public Wire(Gate sourceGate, Gate targetGate)
			: this()
		{
			this.sourceGate = sourceGate;
			this.targetGate = targetGate;
		}

		public Wire(int inputIndex, bool isOutput, Zp constValue)
		{
			this.InputIndex = inputIndex;
			this.IsOutput = isOutput;
			ConstValue = constValue;
		}

		#endregion Constructors

		public virtual bool Valid
		{
			get
			{
				return InputIndex != -1 || sourceGate != null && sourceGate.ReadyToCalculate;
			}
		}

		public virtual bool IsInput
		{
			get
			{
				return InputIndex != -1;
			}
		}

		public virtual Gate TargetGate
		{
			get
			{
				return targetGate;
			}

			set
			{
				Debug.Assert(!IsOutput, "output wire should not have target gate");
				this.targetGate = value;
			}
		}

		public virtual Gate SourceGate
		{
			get
			{
				return sourceGate;
			}

			set
			{
				Debug.Assert(value == null || InputIndex == -1, "input wire should not have source gate");
				this.sourceGate = value;
			}
		}

		public Wire Clone()
		{
			var wire = new Wire(InputIndex, IsOutput, ConstValue);
			wire.SourceGate = sourceGate;
			wire.TargetGate = targetGate;
			if (wire.SourceGate != null)
				wire.SourceGate.AddOutputWire(wire);
			return wire;
		}
	}
}