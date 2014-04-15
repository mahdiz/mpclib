using System.Diagnostics;

namespace MpcLib.Common.FiniteField.Circuits
{
	public class BigWire
	{
		public int InputIndex;
		public bool IsOutput;
		public readonly BigZp ConstValue;
		private BigGate sourceGate;
		private BigGate targetGate;

		#region Constructors

		public BigWire()
		{
			InputIndex = -1;
		}

		public BigWire(BigZp constValue)
			: this()
		{
			this.ConstValue = constValue;
		}

		public BigWire(int inputIndex, bool isInput)
		{
			if (isInput)
				this.InputIndex = inputIndex;
			else
			{
				this.IsOutput = true;
				InputIndex = -1;
			}
		}

		public BigWire(int inputIndex, BigGate targetGate)
		{
			this.InputIndex = inputIndex;
			this.targetGate = targetGate;
		}

		public BigWire(BigGate sourceGate, bool isOutput)
		{
			this.IsOutput = isOutput;
			this.sourceGate = sourceGate;
			InputIndex = -1;
		}

		public BigWire(BigGate sourceGate, BigGate targetGate)
			: this()
		{
			this.sourceGate = sourceGate;
			this.targetGate = targetGate;
		}

		public BigWire(int inputIndex, bool isOutput, BigZp constValue)
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

		public virtual BigGate TargetGate
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

		public virtual BigGate SourceGate
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

		public BigWire Clone()
		{
			var wire = new BigWire(InputIndex, IsOutput, ConstValue);
			wire.SourceGate = sourceGate;
			wire.TargetGate = targetGate;
			if (wire.SourceGate != null)
				wire.SourceGate.AddOutputWire(wire);
			return wire;
		}
	}
}