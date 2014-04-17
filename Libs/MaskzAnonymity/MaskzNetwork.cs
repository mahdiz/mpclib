using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MpcLib.Common.FiniteField;
using MpcLib.MpcProtocols.Dkms;
using MpcLib.DistributedSystem.QuorumSystem;
using MpcLib.SecretSharing;
using MpcLib.Simulation.Des;

namespace MpcLib.DistributedSystem.Anonymity.Maskz
{
	// Represents a network for protocol of Zamani, Saia, Movahedi, and Khoury (MaSKZ'13).
	public class MaskzNetwork : DkmsNetwork
	{
		public MaskzNetwork(Simulator s, int seed)
			: base(s, seed)
		{
		}

		public void Init(int numPlayers, int numQuorums, int numSlots, int quorumSize, QuorumBuildingMethod qbMethod, AdversaryModel model, Zp[] inputs, int prime)
		{
			switch (model)
			{
				case AdversaryModel.HonestButCurious:
					Circuit = CreateHbcCircuit(numPlayers, numQuorums, numSlots, quorumSize, prime);
					break;

				case AdversaryModel.Byzantine:
					Circuit = CreateByzantineCircuit(numPlayers, numQuorums, numSlots, quorumSize, prime);
					break;

				default:
					throw new Exception("Unknown adversary model.");
			}
			base.Init(numPlayers, numQuorums, numSlots, quorumSize, qbMethod, Circuit, inputs, prime);
		}

		#region Circuit Creation Methods

		protected static Circuit CreateHbcCircuit(int numPlayers, int numQuorums, int numSlots, int quorumSize, int prime)
		{
			var inputGates = new List<Gate>();
			var internalGates = new List<Gate>();
			var quorumIndex = new Zp(numQuorums);

			// TODO: THE NUMBER OF INPUTS OF ALL GATES MUST BE 3 * THE QUORUM SIZE NOT JUST ONE OR TWO.
			// WE HAVE TO ADD A RECOMBINE CIRCUIT (DISCUSSED IN A PAPER) TO REDUCE THE NUMBER OF INPUTS TO ONE OR TWO.
			for (int i = 0; i < numPlayers; i++)
			{
				for (int j = 0; j < numSlots; j++)
					inputGates.Add(new Gate(quorumIndex++));
			}

			// create anonymous broadcaster
			// the broadcaster consists of t binary trees each with n leaves.
			// Sample circuit definition:
			/*
				Inputs=S1,r11,r12,r13,S2,r21,r22,r23,rg0,rg1,rg2,rg3
				Outputs=Y
				R1=r11+r12+r13
				O1=(S1-R1)
				R2=r21+r22+r23
				O2=(S2-R2)
				O=(O1+O2)
				RG=rg0+rg1+rg2+rg3
				Y=(O+RG)
			 */

			var circuitDef = "Inputs=S1,r11";
			var R1 = "R1=r11";
			for (int i = 2; i < quorumSize; i++)
			{
				var r = "r1" + i.ToString();
				circuitDef += "," + r;
				R1 += "+" + r;
			}

			circuitDef += ",S2,r21";
			var R2 = "R2=r21";
			for (int i = 2; i < quorumSize; i++)
			{
				var r = "r2" + i.ToString();
				circuitDef += "," + r;
				R2 += "+" + r;
			}

			circuitDef += ",rg0";
			var RG = "RG=rg0";
			for (int i = 1; i < quorumSize; i++)
			{
				circuitDef += ",rg" + i;
				RG += "+rg" + i;
			}

			circuitDef += " \n Outputs=Y \n " + R1 + " \n O1=(S1-R1) \n " + R2 + " \n O2=(S2-R2) \n O=(O1+O2) \n " + RG +
				" \n Y=(O+RG)";

			var sum_parser = new Common.FiniteField.Circuits.Parser(circuitDef, prime);
			sum_parser.Parse();

			// create t binary trees with n leaves
			var adder_roots = new Gate[numSlots];
			var adder_leaves = new List<Gate[]>(numSlots);		// adder_leaves[i][j] is the j-th leave of the i-th tree
			for (int i = 0; i < numSlots; i++)
			{
				adder_roots[i] = new Gate(GateType.Internal, quorumIndex++, sum_parser.Circuit);
				CreateGateTree(adder_roots[i], numPlayers / 2, quorumIndex, false);

				var leaves = GetGatesAndLeaves(adder_roots[i], false, internalGates).ToArray();
				Debug.Assert(leaves.Length == numPlayers / 2);
				adder_leaves.Add(leaves);
			}

			// connect input gates to the broadcaster leaves
			// note: i-th input gate of two adjacent players must be connected to the same leaf
			for (int i = 0; i < numSlots; i++)
			{
				for (int j = 0, k = 0; j < numPlayers; j++, k = j / 2)
					inputGates[j * numSlots + i].AddEdgeTo(adder_leaves[i][k]);
			}
			return new Circuit(inputGates.AsReadOnly(), internalGates);
		}

		protected static MpcProtocols.Dkms.Circuit CreateByzantineCircuit(int numPlayers, int numQuorums, int numSlots, int quorumSize, int prime)
		{
			var inputGates = new List<Gate>();
			var internalGates = new List<Gate>();
			var quorumIndex = new Zp(numQuorums);

			// TODO: THE NUMBER OF INPUTS OF ALL GATES MUST BE IN ORDER OF THE QUORUM SIZE NOT JUST ONE OR TWO.
			// WE HAVE TO ADD A RECOMBINE CIRCUIT (DISCUSSED IN A PAPER) TO REDUCE THE NUMBER OF INPUTS TO ONE OR TWO.

			//var G1_parser = new Parser("Inputs=X \n Outputs=Y \n Y0=(X/X) \n Y=(1-Y0)", prime);
			//G1_parser.Parse();

			//var G2_parser = new Parser("Inputs=X1,X2 \n Outputs=Y \n S=(X1+X2) \n X0=(S==0) \n Y0=(X0/X0) \n Y1=(1-Y0) \n W0=(S==1) \n Z0=(W0/W0) \n Z1=(1-Z0) \n Z2=(2*Z1) \n Z=(Z0+Z2) \n Y=(Y1*Z)", prime);
			//G2_parser.Parse();

			//var G3_parser = new Parser("Inputs=X \n Outputs=Y \n E1=(X==0) \n E2=(X==1) \n X0=(E1+E2) \n Y0=(X0/X0) \n Y=(1-Y0)", prime);
			//G3_parser.Parse();

			//var G4_parser = new Parser("Inputs=X1,X2 \n Outputs=Y \n X=(X1==0) \n Y0=(X/X) \n Y1=(1-Y0) \n Y=(Y0*X2)", prime);
			//G4_parser.Parse();

			//////////////////////////////////// FOR 8/22 SUBMISSION ONLY
			var G1_parser = new Common.FiniteField.Circuits.Parser(Common.FiniteField.FunctionType.Sum, 2 * quorumSize, prime);
			var G2_parser = new Common.FiniteField.Circuits.Parser(Common.FiniteField.FunctionType.Sum, 3 * quorumSize, prime);
			G1_parser.Parse();
			G2_parser.Parse();
			var G3_parser = G1_parser;
			var G4_parser = G2_parser;
			//////////////////////////////////// FOR 8/22 SUBMISSION ONLY

			var G4_gatesList = new List<Gate[]>(numPlayers);

			for (int i = 0; i < numPlayers; i++)
			{
				var inputs = new Gate[numSlots];
				var G1_gates = new Gate[numSlots];
				var G4_gates = new Gate[numSlots];

				for (int j = 0; j < numSlots; j++)
				{
					inputs[j] = new Gate(quorumIndex++);		// input gates

					// create G1's and connect input gates to them
					G1_gates[j] = new Gate(GateType.Internal, quorumIndex++, G1_parser.Circuit);
					inputs[j].AddEdgeTo(G1_gates[j]);

					// create G4's and connect input gates to them
					G4_gates[j] = new Gate(GateType.Internal, quorumIndex++, G4_parser.Circuit);
					inputs[j].AddEdgeTo(G4_gates[j]);

					inputGates.Add(inputs[j]);
					internalGates.Add(G1_gates[j]);
					internalGates.Add(G4_gates[j]);
				}

				// create G2's
				var G2_root = new Gate(GateType.Internal, quorumIndex++, G2_parser.Circuit);
				CreateGateTree(G2_root, numSlots / 2, quorumIndex, false);

				// connect leaf G2's to G1's
				var G2_leaves = GetGatesAndLeaves(G2_root, false, internalGates).ToList();
				Debug.Assert(G2_leaves.Count() == numSlots / 2);
				for (int j = 0, k = 0; j < numSlots; j++, k = j / 2)
					G1_gates[j].AddEdgeTo(G2_leaves[k]);

				// create G3's
				var G3_root = new Gate(GateType.Internal, quorumIndex++, G3_parser.Circuit);
				CreateGateTree(G3_root, numSlots / 2, quorumIndex, true);

				// connect root G3 to root G2
				G2_root.AddEdgeTo(G3_root);

				// connect leaf G3's to G4's
				var G3_leaves = GetGatesAndLeaves(G3_root, true, internalGates).ToList();
				Debug.Assert(G3_leaves.Count() == numSlots / 2);
				for (int j = 0, k = 0; j < numSlots; j++, k = j / 2)
					G3_leaves[k].AddEdgeTo(G4_gates[j]);

				G4_gatesList.Add(G4_gates);
			}

			// create anonymous broadcaster
			// the broadcaster consists of t binary trees each of which has n leaves.
			var sum_parser = new Common.FiniteField.Circuits.Parser("Inputs=X1,X2 \n Outputs=Y \n Y=(X1+X2)", prime);
			sum_parser.Parse();

			// create t binary trees with n leaves
			var adder_roots = new MpcProtocols.Dkms.Gate[numSlots];
			var adder_leaves = new List<MpcProtocols.Dkms.Gate[]>(numSlots);
			for (int i = 0; i < numSlots; i++)
			{
				adder_roots[i] = new Gate(GateType.Internal, quorumIndex++, sum_parser.Circuit);
				CreateGateTree(adder_roots[i], numPlayers / 2, quorumIndex, false);

				var leaves = GetGatesAndLeaves(adder_roots[i], false, internalGates).ToArray();
				Debug.Assert(leaves.Length == numPlayers / 2);
				adder_leaves.Add(leaves);
			}

			// connect G4's to the broadcaster leaves
			// note: i-th G4's of two adjacent players must be connected to the same leaf
			for (int i = 0; i < numSlots; i++)
			{
				for (int j = 0, k = 0; j < numPlayers; j++, k = j / 2)
					G4_gatesList[j][i].AddEdgeTo(adder_leaves[i][k]);
			}
			return new MpcProtocols.Dkms.Circuit(inputGates.AsReadOnly(), internalGates);
		}

		protected static void CreateGateTree(MpcProtocols.Dkms.Gate root, int numLeaves, Zp quorumIndex, bool topToBottom)
		{
			if (numLeaves > 1)
			{
				var left = new MpcProtocols.Dkms.Gate(root.Type, quorumIndex++, root.MpcCircuit);
				var right = new MpcProtocols.Dkms.Gate(root.Type, quorumIndex++, root.MpcCircuit);
				if (topToBottom)
				{
					root.AddEdgeTo(left);
					root.AddEdgeTo(right);
				}
				else
				{
					left.AddEdgeTo(root);
					right.AddEdgeTo(root);
				}
				if (numLeaves >= 4)
				{
					CreateGateTree(left, numLeaves / 2, quorumIndex, topToBottom);
					CreateGateTree(right, numLeaves / 2, quorumIndex, topToBottom);
				}
			}
		}

		protected static IEnumerable<Gate> GetGatesAndLeaves(Gate root, bool topToBottom, List<Gate> gates)
		{
			ReadOnlyCollection<Gate> branches;
			gates.Add(root);

			if (topToBottom)
				branches = root.OutNodes;
			else
				branches = root.InNodes;

			if (branches.Count == 0)
				return new List<Gate>() { root };

			Debug.Assert(branches.Count == 2);
			var leftLeaves = GetGatesAndLeaves(branches[0], topToBottom, gates);
			var rightLeaves = GetGatesAndLeaves(branches[1], topToBottom, gates);
			return leftLeaves.Concat(rightLeaves);
		}

		#endregion Circuit Creation Methods
	}
}