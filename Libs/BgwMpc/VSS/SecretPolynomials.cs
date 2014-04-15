using System.Collections.Generic;
using MpcLib.Common.FiniteField;

namespace MpcLib.DistributedSystem.Mpc.Bgw.Vss
{
	public class SecretPolynomials : BgwShare
	{
		private IList<Zp> fi_x;
		private IList<Zp> gi_y;

		public SecretPolynomials()
			: base(BgwShareType.ZP_LISTS)
		{
			fi_x = new List<Zp>();
			gi_y = new List<Zp>();
		}

		public virtual IList<Zp> Fi_xPolynomial
		{
			set
			{
				this.fi_x = value;
			}

			get
			{
				return fi_x;
			}
		}

		public virtual IList<Zp> Gi_yPolynomial
		{
			set
			{
				this.gi_y = value;
			}

			get
			{
				return gi_y;
			}
		}

		public virtual int Fi_xPolynomialLength
		{
			get
			{
				if (fi_x != null)
					return fi_x.Count;
				return 0;
			}
		}

		public virtual int Gi_yPolynomialLength
		{
			get
			{
				if (gi_y != null)
					return gi_y.Count;
				return 0;
			}
		}

		//public override void loadFromByteArrayNoHeader(BitStream bs, int prime)
		//{
		//    if (bs.readBoolean())
		//        fi_x = bs.readList(prime);
		//    if (bs.readBoolean())
		//        gi_y = bs.readList(prime);
		//}

		//public override void writeToBitStreamNoHeader(BitStream bs)
		//{
		//    bs.writeBoolean(fi_x != null);
		//    if (fi_x != null)
		//        bs.writeList(fi_x);
		//    bs.writeBoolean(gi_y != null);
		//    if (gi_y != null)
		//        bs.writeList(gi_y);
		//}

		//public override byte[] writeToByteArray()
		//{
		//    var bs = new BitStream();
		//    bs.writeMessageType(MessageType.ZP_LISTS);
		//    writeToBitStreamNoHeader(bs);
		//    bs.close();
		//    return bs.ByteArray;
		//}

		public virtual IList<Zp> CalculateF_i_xValuesForPlayers(int numOfPlayers, int prime)
		{
			int w_i, w = NumTheoryUtils.GetFieldMinimumPrimitive(prime);

			int value;
			var f_i_xValues = new List<Zp>();
			for (int playerNum = 0; playerNum < numOfPlayers; playerNum++)
			{
				w_i = NumTheoryUtils.ModPow(w, playerNum, prime);
				value = 0;
				for (int j = 0; j < fi_x.Count; j++)
					value += NumTheoryUtils.ModPow(w_i, j, prime) * fi_x[j].Value;
				f_i_xValues.Add(new Zp(prime, value));
			}

			return f_i_xValues;
		}

		public virtual IList<Zp> calculateG_i_yValuesForVerification(int numOfPlayers, int prime)
		{
			int w_i, w = NumTheoryUtils.GetFieldMinimumPrimitive(prime);

			int value;
			var f_i_xValues = new List<Zp>();
			for (int playerNum = 0; playerNum < numOfPlayers; playerNum++)
			{
				w_i = NumTheoryUtils.ModPow(w, playerNum, prime);
				value = 0;
				for (int j = 0; j < gi_y.Count; j++)
					value += NumTheoryUtils.ModPow(w_i, j, prime) * gi_y[j].Value;
				f_i_xValues.Add(new Zp(prime, value));
			}

			return f_i_xValues;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is SecretPolynomials))
				return false;

			var secretPolynomials = (SecretPolynomials)obj;
			if (secretPolynomials.fi_x.Count != fi_x.Count || secretPolynomials.gi_y.Count != gi_y.Count)
				return false;

			for (int i = 0; i < fi_x.Count; i++)
			{
				if (!(fi_x[i].Equals(secretPolynomials.fi_x[i])))
					return false;
			}
			for (int i = 0; i < gi_y.Count; i++)
			{
				if (!(gi_y[i].Equals(secretPolynomials.gi_y[i])))
					return false;
			}
			return true;
		}

		public override string ToString()
		{
			string outputStr = "";

			if (fi_x != null)
			{
				outputStr += "(Fi_x=";
				foreach (var zp in fi_x)
					outputStr += zp + ",";
			}
			outputStr = outputStr.Remove(outputStr.Length - 1) + " | ";
			if (fi_x != null)
			{
				outputStr += "Gi_x=";
				foreach (var zp in gi_y)
					outputStr += zp + ",";
			}
			return outputStr.Remove(outputStr.Length - 1) + ")";
		}
	}
}