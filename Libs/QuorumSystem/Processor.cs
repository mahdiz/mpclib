using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MpcLib.DistributedSystem.QuorumSystem.QuorumBuilding;

namespace MpcLib.DistributedSystem.QuorumSystem
{
	public delegate void QbFinishHandler(Dictionary<int, int[]> quorumsMap);

	public abstract class Processor<T> : AsyncParty<T> where T : AsyncProtocol
	{
		/// <summary>
		/// Computes this processor's quorums
		/// </summary>
		private IQuorumBuilder qbuilder;

		public Processor()
		{
		}

		protected void Init(IList<int> processors, int numQuorums, int quorumSize, QuorumBuildingMethod method)
		{
			Debug.Assert(processors != null && processors.Count > 0, "Processors have not been created yet.");
			switch (method)
			{
				case QuorumBuildingMethod.RandomSampler:
					//qbuilder = new StaticSampler(this, processors, numQuorums, quorumSize);
					break;

				case QuorumBuildingMethod.AlmostEverywhereBA:

					//qbuilder = new AlmostEverywhereProtocol(Id, processors, Send);
					break;

				case QuorumBuildingMethod.CuckooRule:

					//qbuilder = new CuckooRule(Id, processors, Send);
					break;

				default:
					throw new NotSupportedException();
			}
			qbuilder.QbFinished += new QbFinishHandler(OnQbFinish);
		}

		/// <summary>
		/// Invoked by an event indicating the quorum building protocol has just finished.
		/// </summary>
		/// <param name="quorumsMap">
		/// Maps quorum IDs to processor IDs.
		/// Note: This mapping does not necessarily have a pair for every quorum.
		/// Only those connected to this processor's quorums are listed here.
		/// </param>
		protected abstract void OnQbFinish(Dictionary<int, int[]> quorumsMap);

		public override void Start()
		{
			qbuilder.Run();
		}

		//public override void Receive(Msg msg)
		//{
		//	Debug.Assert(qbuilder != null);

		//	var qsMsg = msg as IQsMessage;
		//	if (qsMsg != null)
		//		qbuilder.Receive((Msg)qsMsg);		// redirect the message to qbuilder
		//}
	}
}