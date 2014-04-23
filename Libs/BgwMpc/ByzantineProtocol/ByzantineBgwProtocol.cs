using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using MpcLib.Common.FiniteField;
using MpcLib.Common.FiniteField.Circuits;
using MpcLib.ByzantineAgreement;
using MpcLib.MpcProtocols.Bgw.Vss;
using MpcLib.SecretSharing;
using MpcLib.DistributedSystem;

namespace MpcLib.MpcProtocols.Bgw
{
	public class ByzantineBgwProtocol : BgwProtocol
	{
		#region Fields

		/// <summary>
		/// Shares of my input that are calculated during the input stage.
		/// </summary>
		protected IList<SecretPolynomials> MyInputShares;

		///// <summary>
		///// Input shares that I have received from other players.
		///// </summary>
		//public IMsgCollection<ShareMessage> rxInputShares;

		private List<Zp> sharesForComp = new List<Zp>();
		protected HashSet<int> BadPlayers = new HashSet<int>();
		protected ByzantineBgwProtocol CallerProtocol;		// Mahdi: ??

		private SecureBroadcaster<PlayerNotification> notifBroadcaster;
		private SecureBroadcaster<SecretPolynomialsBundle> secretBroadcaster;

		private int NumGoodPlayers
		{
			get
			{
				return NumParties - BadPlayers.Count;
			}
		}

		#endregion Fields

		public ByzantineBgwProtocol(AsyncParty e, Circuit circuit, ReadOnlyCollection<int> playerIds,
			Zp playerInput, StateKey stateKey)
			: base(e, circuit, playerIds, playerInput, stateKey)
		{
			throw new NotImplementedException();

			// PolynomialDeg = NumParties % 4 == 0 ? (NumParties / 4 - 1) : (NumParties / 4);
			// Mahdi: Changed to the following since n/3 - 1 of players can be dishonest.
			// degree = n - t, where t is the number of dishonest players
			PolynomialDeg = (int)Math.Floor(2 * NumParties / 3.0);
		}

		#region Methods

		/// <summary>
		/// Checks if the received polynomials are not null, from the right size and have no null elements */
		/// </summary>
		protected bool IsSecretPolynomialsLegal(SecretPolynomials secretPolynomial)
		{
			if (secretPolynomial == null)
				return false;
			else
			{
				var fi_xPoly = secretPolynomial.Fi_xPolynomial;
				var gi_yPoly = secretPolynomial.Gi_yPolynomial;

				if (!IsPolynomialLegal(fi_xPoly, PolynomialDeg + 1) || !IsPolynomialLegal(gi_yPoly, PolynomialDeg + 1))
					return false;
			}
			return true;
		}

		protected IList<Coordinate> CompareCoordianteList(List<VerifShareMessage> f_iValues, IList<Zp> g_jValues)
		{
			var wrongCoords = new List<Coordinate>();
			foreach (var f_iValue in f_iValues)
			{
				if (!BadPlayers.Contains(f_iValue.SenderId))
				{
					var g_jVal = g_jValues[f_iValue.SenderId];
					var f_iVal = f_iValues[f_iValue.SenderId];
					if ((f_iVal == null) || (g_jVal == null) || (!f_iVal.Equals(g_jVal)))
						wrongCoords.Add(new Coordinate(f_iValue.SenderId, Party.Id));
				}
			}
			return wrongCoords;
		}

		#region Virtual Methods

		public override void Run()
		{
			//// send my input (secret) to all players
			var mySecretShares = BgwVss.ShareByzantineCase(Input, NumParties, PolynomialDeg);
			Send(GetShareMessages(mySecretShares, Stage.InputReceive), BadPlayers);

			//OnReceive((int)Stage.InputReceive,
			//    delegate(List<ShareMessage> inputShares)
			//    {
			//        SendVerifications(inputShares);
			//        OnReceive((int)Stage.VerificationReceive, NumEntities,
			//            delegate(List<VerifShareMessage> verifShares)
			//            {
			//                ReceiveVerifications(verifShares);
			//            });
			//    });
		}

		//public override void Receive(Message msg)
		//{
		//	ShareMessage shareMsg = null;
		//	var smpcMsg = msg as MpcMessage;
		//	Debug.Assert(smpcMsg != null);

		//	switch (smpcMsg.Stage)
		//	{
		//		case Stage.InputReceive:
		//			shareMsg = smpcMsg as ShareMessage;
		//			RxInputShares = MsgCollector.Collect(shareMsg.SenderId, shareMsg, NumEntities);

		//			if (RxInputShares != null)		// have we received all the shares?
		//				SendVerifications(RxInputShares);
		//			break;

		//		case Stage.VerificationReceive:
		//			shareMsg = smpcMsg as ShareMessage;
		//			var verifShares = MsgCollector.Collect(shareMsg.SenderId, shareMsg, NumEntities);

		//			if (verifShares != null)
		//				ReceiveVerifications(verifShares);
		//			break;

		//		case Stage.SecureBroadcast:
		//			var sbcMsg = smpcMsg as BroadcastMessage<PlayerNotification>;

		//			if (sbcMsg.Stage == BroadcastStage.DealerDataSend && sbcMsg.SenderId != EntityId)
		//			{
		//				throw new NotImplementedException();

		//				//notifBroadcaster = new SecureBroadcaster<PlayerNotification>(EntityId, sbcMsg.Content, EntityIds, false, Prime, Send);
		//				notifBroadcaster.Run();
		//			}
		//			else
		//			{
		//				Debug.Assert(notifBroadcaster != null, "DealerDataSend message is not received yet!");
		//				notifBroadcaster.Receive(msg);
		//			}
		//			break;

		//		default:
		//			base.Receive(smpcMsg);
		//			break;
		//	}
		//}

		protected override void FilterPlayers(IList<int> players)
		{
			foreach (var badPlayer in BadPlayers)
				players.Remove(badPlayer);
		}

		protected override Zp GetRecombinedResult(IList<Zp> recvList, int prime)
		{
			// Scan recvList - if there are null elements replace them arbitrarily to Zp with zero value
			for (int i = 0; i < recvList.Count; i++)
			{
				if (recvList[i] == null)
					recvList[i] = new Zp(prime, 0);
			}

			var xVlaues = new List<Zp>();
			int w = NumTheoryUtils.GetFieldMinimumPrimitive(prime);

			for (int i = 0; i < recvList.Count; i++)
				xVlaues.Add(new Zp(prime, NumTheoryUtils.ModPow(w, i, prime)));

			// Should call Welch-Berlekamp Decoder to fix error at last stage
			var fixedShares = WelchBerlekampDecoder.decode(xVlaues, recvList, PolynomialDeg, PolynomialDeg, prime);
			if (fixedShares == null)
				throw new Exception("There were more then polynomialDegree = " + PolynomialDeg + " Cheaters - cannot extract results.");

			return ShamirSharing.Recombine(fixedShares, PolynomialDeg, prime, true);
		}

		protected virtual void HandleDealer(IList<SecretPolynomials> shareMySecrets, Zp recvSecretShare_i)
		{
			// Should send & receive complaints on the bulletin board  1 - I'm not complaining on myself
			var myDemoComplaint = new PlayerNotification(Confirmation.Approval);

			// broadcast the complaint using BA
			//var bcaster = new SecureBroadcaster<PlayerNotification>(
			//    EntityId, myDemoComplaint, EntityIds.Except(BadPlayers).ToList().AsReadOnly(), true, Prime, Send);
			//bcaster.Run();

			//var complaintesOnMe = SecureBroadcaster.PublishAndRead(myDemoComplaint, Prime, BadPlayers);

			//// Prepare wanted polys according to players complaintes
			//var wantedPolysList = new List<SecretPolynomials>();
			//bool recvCompOnMe = CreateWantedPolynomials(wantedPolysList, complaintesOnMe, shareMySecrets);

			//if (recvCompOnMe)
			//{
			//    // Publish wanted polynomials on the bulletin board
			//    var secretBundle = new SecretPolynomialsBundle(wantedPolysList);
			//    SecureBroadcaster.Publish(secretBundle, BadPlayers);

			//    // Should send&receive complaints on the bulletin board  2 - I'm not complaining on  myself
			//    complaintesOnMe = Sendable.asPlayerNotifications(
			//        BulletinBoardProtocol.PublishAndRead(myDemoComplaint, Prime, BadPlayers));

			//    // Prepare wanted polys according to players complaintes
			//    recvCompOnMe = CreateWantedPolynomials(wantedPolysList, complaintesOnMe, shareMySecrets);

			//    if (recvCompOnMe)
			//    {
			//        // Publish wanted polynomials on the bulletin board
			//        secretBundle = new SecretPolynomialsBundle(wantedPolysList);
			//        BulletinBoardProtocol.Publish(secretBundle, BadPlayers);

			//        // Should send&receive complaints on the bulletin board  3 - I'm not complaining on  myself
			//        complaintesOnMe = Sendable.asPlayerNotifications(
			//            BulletinBoardProtocol.PublishAndRead(myDemoComplaint, Prime, BadPlayers));

			//        // Count complaints number
			//        int numOfcomplaints = GetNumberOfComplaints(complaintesOnMe);

			//        if (numOfcomplaints > PolynomialDeg)
			//        {
			//            // Take the zero polynomial as my input (Not mandatodry) and throw exception
			//            recvSecretShare_i.Value = 0;
			//            throw new Exception("Other players decided that I'm a cheater  :- (  -  taking my input as zero.");
			//        }
			//    }
			//}
		}

		protected virtual void HandleNotDealer(bool isOrigPolyLegal, IList<Coordinate> wrongCoordinatesList, int playerToVerify, Zp recvSecretShare_i, SecretPolynomials secretPoly_i)
		{
			//// Step 2 - Check and advertise results to players on the bulletin board
			//var foundWrongValue1 = !isOrigPolyLegal || (wrongCoordinatesList.Count != 0);
			//var complaintValue = new PlayerNotification(foundWrongValue1 ? Confirmation.Complaint : Confirmation.Approval);

			//var recvComplaintesList = Sendable.asPlayerNotifications(
			//    BulletinBoardProtocol.PublishAndRead(complaintValue, Prime, BadPlayers));

			//// Count complaints number
			//int numOfcomplaints = GetNumberOfComplaints(recvComplaintesList);

			//// Step 3 - If there is a need, receive polynomials for the complainig sides and verify it
			//// currently check only polynomials - not wrong coordinates
			//if (numOfcomplaints != 0)
			//{
			//    // Should get a list of the public polynomials from player  'playerToVerify'  -  get info from the bulletin board
			//    var sendablePolysRecv = BulletinBoardProtocol.Read(playerToVerify, Prime);

			//    IList<SecretPolynomials> recvPublicPolysList1 = null;
			//    if (sendablePolysRecv != null)
			//        recvPublicPolysList1 = sendablePolysRecv.asSecretPolynomialsBundle().List;

			//    if (foundWrongValue1)
			//        UpdateRecvShare(recvPublicPolysList1, recvSecretShare_i);

			//    bool foundWrongValue2 = (!isOrigPolyLegal) || IsPublicDataContradictPrivate(secretPoly_i, recvPublicPolysList1, recvComplaintesList, recvSecretShare_i);

			//    // Check and advertise results to players on the bulletin board
			//    complaintValue = new PlayerNotification(foundWrongValue2 ? Confirmation.Complaint : Confirmation.Approval);
			//    recvComplaintesList = Sendable.asPlayerNotifications(
			//        BulletinBoardProtocol.PublishAndRead(complaintValue, Prime, BadPlayers));

			//    // Count complaints number
			//    numOfcomplaints = GetNumberOfComplaints(recvComplaintesList);

			//    // Step 4 - If there is a need, recieve polynomials for the complainig sides and verify it
			//    if (numOfcomplaints != 0)
			//    {
			//        // Should get a list of the public polynomials from player  'playerToVerify' -  get info from the bulletin board
			//        sendablePolysRecv = BulletinBoardProtocol.Read(playerToVerify, Prime);
			//        IList<SecretPolynomials> recvPublicPolysList2 = null;

			//        if (sendablePolysRecv != null)
			//            recvPublicPolysList2 = sendablePolysRecv.asSecretPolynomialsBundle().List;

			//        if (foundWrongValue2)
			//            UpdateRecvShare(recvPublicPolysList2, recvSecretShare_i);

			//        bool foundWrongValue3 = (!isOrigPolyLegal) || IsPublicDataContradictPrivate(secretPoly_i, recvPublicPolysList2, recvComplaintesList, recvSecretShare_i);
			//        foundWrongValue3 = foundWrongValue3 || IsNewPublicDataContradictOld(recvPublicPolysList1, recvPublicPolysList2);

			//        // Check and advertise results to players on the bulletin board
			//        complaintValue = new PlayerNotification(foundWrongValue3 ? Confirmation.Complaint : Confirmation.Approval);
			//        recvComplaintesList = Sendable.asPlayerNotifications(
			//            BulletinBoardProtocol.PublishAndRead(complaintValue, Prime, BadPlayers));

			//        // Count complaints number
			//        numOfcomplaints = GetNumberOfComplaints(recvComplaintesList);

			//        /* Step 5 - Check if there is more than 'polynomialDeg' complaintes or recption timeout occured  */
			//        if (numOfcomplaints > PolynomialDeg)
			//        {
			//            // Found a cheater player: playerToVerify - taking its input as zero.
			//            // Take the zero polynomial as this user input
			//            recvSecretShare_i.Value = 0;
			//            RemoveCheaterPlayer(playerToVerify); // don't send and receive from this player anymore...
			//        }
			//    }
			//}
			//throw new NotImplementedException();
		}

		/// <summary>
		/// Implementation according to Ran Canetti
		/// </summary>
		public override void RunReductionRandomization(Zp ab)
		{
			throw new NotImplementedException();

			//// performing Improved reduction & randomization step
			//bool cheaterFound = true;
			//IList<Zp> recvSharesFromPlayers = null;

			//while (cheaterFound)
			//{
			//    cheaterFound = false;

			//    /* Share secret by VSS */
			//    recvSharesFromPlayers = SendInput(ab);
			//    /* Generate a t degree polynomial, hi(x) , with a  free coeef  that equals 'ab' and create share for users from it  */
			//    //List<Zp> shareResultWithPlayers = Shamir.primitiveShare(ab, numberOfPlayers , polynomialDeg);
			//    /* Send to the j-th user hi(j) and receive from every other k player hk(i)  */
			//    //recvSharesFromPlayers = shareSimple(shareResultWithPlayers);

			//    /* Check if there were some null elements - from not playing players or cheater players and put zero instead - arbitrarily */
			//    for (int i = recvSharesFromPlayers.Count - 1; i >= 0; i--)
			//    {
			//        if (recvSharesFromPlayers[i] == null)
			//            recvSharesFromPlayers[i] = new Zp(Prime, 0);

			//        if (BadPlayers.Contains(i))
			//            recvSharesFromPlayers.RemoveAt(i);
			//    }

			//    var calculationPolyCoeffs = new List<Zp>();
			//    /* Fill the first 2t+1 coeff with zero arbitrarily  */
			//    for (int i = 0; i < 2 * PolynomialDeg + 1; i++)
			//        calculationPolyCoeffs.Add(new Zp(Prime, 0));

			//    /* Perform the following iteration to calculate the 2t+1...n  coeffs of the  calculation polynomial*/
			//    for (int k = 2 * PolynomialDeg + 1; k < recvSharesFromPlayers.Count; k++)
			//    {
			//        var K_LineAtInvVanderMonde = GetMultStepCoeffsForCheaters(k);
			//        /* Calculate your share of the K-th coeff at the calculation polynomial */
			//        var myK_CoeffShare = new Zp(Prime, 0);
			//        for (int i = 0; i < recvSharesFromPlayers.Count; i++)
			//        {
			//            myK_CoeffShare.Add(recvSharesFromPlayers[i].ConstMul(K_LineAtInvVanderMonde[i]));
			//        }

			//        /* Send this to all other players so all of you could recombine the real k-th coeff  - no need to use the bulletin board */
			//        var K_CoeffShares = ShareSimple(myK_CoeffShare);
			//        /* Fix the received codeword and get the Recombined result */
			//        calculationPolyCoeffs.Add(GetRecombinedResult(K_CoeffShares, Prime));
			//    }
			//    var calculationPoly = new Polynomial(calculationPolyCoeffs);

			//    if (calculationPoly.Degree == -1)
			//    {
			//        /* No one cheated at this stage  */
			//        break;
			//    }

			//    var XValues = new List<Zp>();
			//    int w = Zp.GetFieldMinimumPrimitive(Prime);
			//    for (int i = 0; i < NumPlayers; i++)
			//    {
			//        XValues.Add(new Zp(Prime, Zp.CalculatePower(w, i, Prime)));
			//    }

			//    /* Create the distorted code word */
			//    var distortedCodeword = new List<Zp>();
			//    for (int i = 0; i < NumPlayers; i++)
			//        distortedCodeword.Add(calculationPoly.Sample(XValues[i]));

			//    var fixedCodeword = WelchBerlekampDecoder.decode(XValues, distortedCodeword, PolynomialDeg, 2 * PolynomialDeg, Prime);
			//    // Check For exception in codeword fixing
			//    if (fixedCodeword == null)
			//    {
			//        string errorStr = "There were more then polynomialDegree = " + PolynomialDeg + " Cheaters - cannot complete  mult step.";
			//        throw new Exception(errorStr);
			//    }

			//    for (int i = 0; i < fixedCodeword.Count; i++)
			//    {
			//        if (!BadPlayers.Contains(i) && !fixedCodeword[i].Equals(distortedCodeword[i]))
			//        {
			//            // Player Number i is a multi-step cheater!
			//            cheaterFound = true;
			//            RemoveCheaterPlayer(i);
			//            // Continue at iteration till no one will cheat
			//        }
			//    }
			//}

			///* Finally calculate your share  - we will get here if no one had tried to cheat */
			//var firstLineAtInvVanderMonde = GetMultStepCoeffsForCheaters(0);

			///* Calculate your share of the K-th coeff at the calculation polynomial */
			//var tempSecret = new Zp(Prime, 0);
			//for (int i = 0; i < recvSharesFromPlayers.Count; i++)
			//{
			//    tempSecret.Add(recvSharesFromPlayers[i].ConstMul(firstLineAtInvVanderMonde[i]));
			//}
			//return tempSecret;
		}

		/// <summary>
		/// Implementation according to GRR.
		/// </summary>
		public virtual Zp ReductionRandomizationStep(Zp a, Zp b, Zp ab)
		{
			throw new NotImplementedException();

			//// performing reduction & randomization step
			///* Create a detailed share of a,b and ab */
			//var aSharesDetails = Shamir.DetailedShare(a, NumPlayers, PolynomialDeg);
			//var bSharesDetails = Shamir.DetailedShare(b, NumPlayers, PolynomialDeg);
			//var abSharesDetails = Shamir.DetailedShare(ab, NumPlayers, PolynomialDeg);

			///* Share a random polynomial r(x) of degree 2t-1 */
			//var rSharesDetails = Shamir.DetailedShare(
			//    new Zp(Prime, (int)(new Random(1).NextDouble() * Prime)), NumPlayers, 2 * PolynomialDeg - 1);

			//var RxPolynomial = new MultStepVerificationPoly(ConstructRxPolynomial(aSharesDetails, bSharesDetails, abSharesDetails, rSharesDetails));

			///* Build verified shares for users */
			//var aShares = aSharesDetails.CreatedShares;
			//var bShares = bSharesDetails.CreatedShares;
			//var abShares = abSharesDetails.CreatedShares;
			//var rShares = rSharesDetails.CreatedShares;
			//var toSendmultStepShares = new List<MultStepBCaseShare>();

			//for (int i = 0; i < NumPlayers; i++)
			//{
			//    var playerShares = new MultStepBCaseShare();
			//    playerShares.AShare = aShares[i];
			//    playerShares.BShare = bShares[i];
			//    playerShares.AbShare = abShares[i];
			//    playerShares.RShare = rShares[i];
			//    toSendmultStepShares.Add(playerShares);
			//}

			///* Send and receive shares from all players */
			//var toRecvmultStepShares = Sendable.asMultStepBCaseShares(Adapter.shareSecrets(toSendmultStepShares, Prime, BadPlayers));

			///* Start verifying player shares and construct a list of the received shares of ab  - this list will be used to compute the regular multStep*/
			//int playerToVerify = 0;
			//var abVerifiedList = new List<Zp>();

			//foreach (var recvShareFromPlayer_i in toRecvmultStepShares)
			//{
			//    if (playerToVerify == EntityId)
			//    {
			//        // I'm the dealer
			//        // Put Rx polynomial on the bulletin board
			//        BulletinBoardProtocol.publish(RxPolynomial, BadPlayers, Adapter);

			//        // Should advertise a demo complaint on the bulletin board - I'm not complaining about myself
			//        var myDemoComplaint = new PlayerNotification(PlayerNotification.Confirmation.APPROVAL);

			//        var complaintesOnMe = Sendable.asPlayerNotifications(BulletinBoardProtocol.publishAndRead(myDemoComplaint, Prime, BadPlayers, Adapter));
			//        // Build a message containing all the requested data
			//        var requstedData = new List<MultStepBCaseShare>();

			//        for (int i = 0; i < NumPlayers; i++)
			//        {
			//            if ((complaintesOnMe[i] != null) && (complaintesOnMe[i].Msg.Equals(PlayerNotification.Confirmation.COMPLAINT)))
			//            {
			//                requstedData.Add(toSendmultStepShares[i]);
			//            }
			//            else requstedData.Add(null);
			//        }
			//        var requstedDataToPublish = new MultStepBCaseShareBundle(requstedData);
			//        // Put the requested shares on the bulletin board .
			//        // Note : even if you are a cheater you should publish the real shares here otherwise you  will be considered as cheater
			//        BulletinBoardProtocol.publish(requstedDataToPublish, BadPlayers, Adapter);

			//        // Add my ab share to abShares any way
			//        abVerifiedList.Add(toSendmultStepShares[EntityId].AbShare);
			//    }
			//    else
			//    {
			//        if (BadPlayers[playerToVerify])
			//        {
			//            // Mult Step - verifying player number playerToVerify share
			//            bool dealerIsCheater = false;
			//            PlayerNotification myComplaint;
			//            IList<PlayerNotification> recvComplaints;
			//            MultStepBCaseShareBundle recvPublicShares;

			//            // Get Rx polynomial of the 'playerToVerify' from bulletin board
			//            var recvVerificationPolynomial = BulletinBoardProtocol.read(playerToVerify, Prime, Adapter).asMultStepVerificationPoly();
			//            if (IsMultStepPolynomialLegal(recvVerificationPolynomial))
			//            {
			//                if (!IsMultStepShareLegal(recvShareFromPlayer_i, recvVerificationPolynomial))
			//                {
			//                    // Should prepare a massege that te dealer is a cheater
			//                    myComplaint = new PlayerNotification(PlayerNotification.Confirmation.COMPLAINT);
			//                }
			//                else
			//                {
			//                    // Should prepare a massege that te dealer is not a cheater
			//                    myComplaint = new PlayerNotification(PlayerNotification.Confirmation.APPROVAL);
			//                }

			//                // Help verify complaining users shares  :
			//                // Should advertise what you thinks about the  dealer on the bulletin board
			//                recvComplaints = Sendable.asPlayerNotifications(BulletinBoardProtocol.publishAndRead(myComplaint, Prime, BadPlayers, Adapter));

			//                // Get complaining players share from the bulletin board - it is now a public share
			//                recvPublicShares = BulletinBoardProtocol.read(playerToVerify, Prime, Adapter).asMultStepBCaseShareBundle();
			//                // Check that the dealer had published all the requested data and that the data is consistent

			//                IList<MultStepBCaseShare> publishedShareList = null;
			//                if (recvPublicShares != null)
			//                    publishedShareList = recvPublicShares.List;

			//                MultStepBCaseShare publishedShare;
			//                for (int i = 0; i < NumPlayers; i++)
			//                {
			//                    if ((recvComplaints[i] != null) &&
			//                        (recvComplaints[i].Msg.Equals(PlayerNotification.Confirmation.COMPLAINT)))
			//                    {
			//                        publishedShare = publishedShareList[i];
			//                        if (!IsMultStepShareLegal(publishedShare, recvVerificationPolynomial))
			//                        {
			//                            // No need to send any more complaints or aproval- the shares and the R(x) polynomial is public data
			//                            dealerIsCheater = true;
			//                            Adapter.removePlayer(playerToVerify); //don't send and receive from this player anymore...
			//                            break;
			//                        }
			//                    }
			//                }
			//            }
			//            else  // Multistep polynomial, R(x) isn't legal
			//                dealerIsCheater = true;

			//            if (dealerIsCheater)
			//            {
			//                // Found a cheater in multiplication step. Cheater is player number playerToVerify.
			//                RemoveCheaterPlayer(playerToVerify);
			//            }
			//            else abVerifiedList.Add(recvShareFromPlayer_i.AbShare);
			//        }
			//    }
			//    playerToVerify++;
			//}

			//// Finally
			//var firstLineAtInvVanderMonde = GetMultStepCoeffsForCheaters(0); // Contains only the needed coeffs

			//// Calculate the value of the  polynomial H(x)  at i = H(i) as defined at GRR
			//var tempSecret = new Zp(Prime, 0);
			//int goodPlayerNum = NumGoodPlayers;
			//for (int i = 0; i < goodPlayerNum; i++)
			//{
			//    tempSecret.Add(abVerifiedList[i].Mul(firstLineAtInvVanderMonde[i]));
			//}
			//return tempSecret;
		}

		#endregion Virtual Methods

		#region Private Methods

		private void SendVerifications(IEnumerable<ShareMsg<Zp>> recvShares)
		{
			// input stage checking - check that each input is legal
			// each iteration in the loop is a verification of player with ID 'playerToVerify'.
			foreach (var recvShare in recvShares)
			{
				var playerToVerify = recvShare.SenderId;
				var secretPoly = recvShare.Share as SecretPolynomials;

				// first check if the verified dealer had already been caught as a cheater
				if (BadPlayers.Contains(playerToVerify))
				{
					// no need to check a cheater player's input
					sharesForComp.Add(new Zp(Prime, 0));
					continue;
				}

				/* Step 1 - Verify player 'playerToVerify' input  */

				// Extract the received share from the polynomial you received from player with ID 'playerToVerify'
				// Assume I'm the j-th player. Calculate fj(w^i).
				// first check if you received a proper polynomial.
				var isOrigPolyLegal = IsSecretPolynomialsLegal((SecretPolynomials)secretPoly);

				if (isOrigPolyLegal)
				{
					var verifList_f_j_w_i = secretPoly.CalculateF_i_xValuesForPlayers(NumParties, Prime);
					var verifMsgs = GetVerifShareMessages(verifList_f_j_w_i, playerToVerify, true);
					Send(verifMsgs);
				}
				else // received polynomials are corrupted Send random list and remember to complain
				{
					// received a corrupted polynomials from player with ID 'playerToVerify'
					var verifList_f_j_w_i = ZpMatrix.GetRandomMatrix(1, NumParties, Prime).GetMatrixRow(0);

					var verifMsgs = GetVerifShareMessages(verifList_f_j_w_i, playerToVerify, false);
					Send(verifMsgs);
				}
			}
		}

		protected IList<ShareMsg<Zp>> GetVerifShareMessages(IList<Zp> zPs, int playerToVerify, bool receivedGoodPoly)
		{
			var shareMsgs = new List<ShareMsg<Zp>>();
			foreach (var zp in zPs)
				shareMsgs.Add(new VerifShareMessage(new BgwShare(zp), playerToVerify, receivedGoodPoly));

			return shareMsgs;
		}

		private void ReceiveVerifications(List<VerifShareMessage> verifShares, List<ShareMsg<Zp>> inputShares)
		{
			Zp recvShare_i = null;
			IList<Coordinate> wrongCoordinatesList = null;

			// all verif. shares must have the same state key
			var secretPoly = inputShares[verifShares[0].PlayerToVerify].Share as SecretPolynomials;		// TODO: inefficient casting!

			if (verifShares[0].ReceivedGoodPoly)		// had received a valid polynomial?
			{
				recvShare_i = new Zp(secretPoly.Fi_xPolynomial[0]);
				var verifyWithList_g_j_w_i = secretPoly.calculateG_i_yValuesForVerification(NumParties, Prime);
				wrongCoordinatesList = CompareCoordianteList(verifShares, verifyWithList_g_j_w_i);
			}
			else
			{
				// received a corrupted polynomials from player with ID 'playerToVerify'
				recvShare_i = new Zp(Prime, 0);
			}

			if (verifShares[0].PlayerToVerify == Party.Id)	// am I the dealer?
			{
				Debug.Assert(MyInputShares != null);
				HandleDealer(MyInputShares, recvShare_i);
			}
			else
			{
				HandleNotDealer(verifShares[0].ReceivedGoodPoly, wrongCoordinatesList,
					verifShares[0].PlayerToVerify, recvShare_i, secretPoly);
			}
			sharesForComp.Add(recvShare_i);
		}

		private int GetNumberOfComplaints(IList<PlayerNotification> complaints)
		{
			int numOfcomplaints = 0;
			foreach (var playerNote in complaints)
			{
				if ((playerNote != null) && (playerNote.Confirmation == Confirmation.Complaint))
					numOfcomplaints++;
			}
			return numOfcomplaints;
		}

		private void RemoveCheaterPlayer(int playerToVerify)
		{
			BadPlayers.Add(playerToVerify);
			if (CallerProtocol != null)
				CallerProtocol.BadPlayers.Add(playerToVerify);

			// Commented by Mahdi: I think we don't need this.
			//Adapter.removePlayer(playerToVerify);
		}

		private void UpdateRecvShare(IList<SecretPolynomials> recvPublicPolysList, Zp recvSecretShare_i)
		{
			if ((recvPublicPolysList != null))
			{
				var newRecvSercretPoly = recvPublicPolysList[Party.Id];
				if (newRecvSercretPoly != null)
				{
					var recvShare = newRecvSercretPoly.Fi_xPolynomial[0];
					if (recvShare != null)
						recvSecretShare_i.Value = recvShare.Value;
				}
			}
		}

		// TODO: MAHDI: HAS WRONG ORDERING!
		private IList<Zp> GetMultStepCoeffsForCheaters(int j)
		{
			var rowsToRemove = new bool[NumParties];
			for (int i = 0; i < NumParties; i++)
				rowsToRemove[i] = BadPlayers.Contains(i);

			var vanderMonde = ZpMatrix.GetSymmetricPrimitiveVandermondeMatrix(NumParties, Prime).Transpose;
			var filteredMatrix = vanderMonde.RemoveRowsFromMatrix(rowsToRemove).Inverse;
			return filteredMatrix.GetMatrixRow(j);
		}

		private IList<Zp> ConstructRxPolynomial(ShareDetails aSharesDetails,
			ShareDetails bSharesDetails, ShareDetails abSharesDetails, ShareDetails rSharesDetails)
		{
			var fax = aSharesDetails.RandomPolynomial;
			var fbx = bSharesDetails.RandomPolynomial;
			var hx = abSharesDetails.RandomPolynomial;
			var rx = rSharesDetails.RandomPolynomial;

			var RxPolynomial = new Zp[2 * PolynomialDeg + 1];
			/* Initialize RxPolynomial coefs with zeros  */
			for (int i = 0; i < 2 * PolynomialDeg + 1; i++)
				RxPolynomial[i] = new Zp(Prime, 0);

			/* First calculate fax*fbx - hx */
			for (int i = 0; i < fax.Count; i++)
			{
				Zp temp = fax[i];
				for (int j = 0; j < fax.Count; j++)
					RxPolynomial[i + j].Add(temp.ConstMul(fbx[j]));

				RxPolynomial[i].Sub(hx[i]);
			}
			/* Calculate x*rx+fax*fbx - hx*/
			for (int i = 0; i < rx.Count; i++)
				RxPolynomial[i + 1].Add(rx[i]);

			return new List<Zp>(RxPolynomial);
		}

		private bool IsPolynomialLegal(IList<Zp> polynomial, int polySize)
		{
			if ((polynomial == null) || (polynomial.Count != polySize))
				return false;

			foreach (Zp zp in polynomial)
			{
				if (zp == null)
					return false;
			}
			return true;
		}

		private bool IsMultStepPolynomialLegal(MultStepVerificationPoly recvVerifcationPolynomial)
		{
			if (recvVerifcationPolynomial != null)
			{
				var RxPolynomial = recvVerifcationPolynomial.RxPolynomial;
				if (!IsPolynomialLegal(RxPolynomial, 2 * PolynomialDeg + 1))
					return false;
			}
			else
				return false;
			return true;
		}

		private bool IsRecvShareLegal(MultStepBCaseShare recvShareFromPlayer)
		{
			if ((recvShareFromPlayer == null) ||
				(recvShareFromPlayer.AShare == null) ||
				(recvShareFromPlayer.BShare == null) ||
				(recvShareFromPlayer.AbShare == null) ||
				(recvShareFromPlayer.RShare == null))
				return false;
			return true;
		}

		private bool IsMultStepShareLegal(MultStepBCaseShare recvShareFromPlayer_i, MultStepVerificationPoly recvVerifcationPolynomial)
		{
			if (!IsRecvShareLegal(recvShareFromPlayer_i))
				return false;

			var RxPolynomial = recvVerifcationPolynomial.RxPolynomial;
			Zp Ratpoint0 = Zp.EvalutePolynomialAtPoint(RxPolynomial, new Zp(Prime, 0));
			if (!Ratpoint0.Equals(new Zp(Prime, 0)))
			{
				return false;
			}
			int w = NumTheoryUtils.GetFieldMinimumPrimitive(Prime);
			var w_InMyIndex = new Zp(Prime, NumTheoryUtils.ModPow(w, Party.Id, Prime));
			Zp RjFromPublicPolynomial = Zp.EvalutePolynomialAtPoint(RxPolynomial, w_InMyIndex);

			Zp temp = recvShareFromPlayer_i.AShare.ConstMul(recvShareFromPlayer_i.BShare).ConstSub(recvShareFromPlayer_i.AbShare);
			Zp RjFromRecvPrivateInfo = w_InMyIndex.ConstMul(recvShareFromPlayer_i.RShare).ConstAdd(temp);
			if (!RjFromPublicPolynomial.Equals(RjFromRecvPrivateInfo))
				return false;

			return true;
		}

		private bool CreateWantedPolynomials(IList<SecretPolynomials> wantedPolysList, IList<PlayerNotification> complaintesOnMe, IList<SecretPolynomials> shareMySecrets)
		{
			bool recvCompOnMe = false;
			wantedPolysList.Clear();
			for (int j = 0; j < complaintesOnMe.Count; j++)
			{
				if ((complaintesOnMe[j] != null) && (complaintesOnMe[j].Confirmation == Confirmation.Complaint))
				{
					wantedPolysList.Add(shareMySecrets[j]);
					recvCompOnMe = true;
				}
				else
					wantedPolysList.Add(null);
			}
			return recvCompOnMe;
		}

		private void ShareSimple(IList<Zp> sharedSecrets, Stage targetStage)
		{
			Send(GetShareMessages(sharedSecrets, targetStage), BadPlayers);
		}

		private void ShareSimple(Zp sharedSecrets, Stage targetStage)
		{
			Send(new ShareMsg<Zp>(new Share<Zp>(sharedSecrets), targetStage), BadPlayers);
		}

		private bool IsNewPublicDataContradictOld(IList<SecretPolynomials> oldPublicData, IList<SecretPolynomials> newPublicData)
		{
			for (int i = 0; i < NumParties; i++)
			{
				var secretToCheck = oldPublicData[i];
				if ((secretToCheck != null) && (!secretToCheck.Equals(newPublicData[i])))
					return true;
			}
			return false;
		}

		private bool IsPublicDataContradictPrivate(SecretPolynomials myRecvPolys, IList<SecretPolynomials> recvPublicPolysList, IList<PlayerNotification> recvComplaintesList, Zp myRecvShare)
		{
			if ((recvPublicPolysList == null) || (recvPublicPolysList.Count != NumParties))
				return true;

			var myG_j_w_iValues = myRecvPolys.calculateG_i_yValuesForVerification(NumParties, Prime);
			var myF_j_w_iValues = myRecvPolys.CalculateF_i_xValuesForPlayers(NumParties, Prime);

			for (int k = 0; k < NumParties; k++)
			{
				if ((recvComplaintesList[k] != null) && (recvComplaintesList[k].Confirmation == Confirmation.Complaint))
				{
					var playerKNewPolys = recvPublicPolysList[k];

					// Check if the dealer  didn't publish all the required data or published a corrupted data
					if (!IsSecretPolynomialsLegal(playerKNewPolys))
						return true;

					// Verify that the public information doesn't contradict itself - check that : f(w^k, w^k) = fk(w^k) = gk(w^k) = f(w^k, w^k)
					var playerKNewFk_x_w_iValues = playerKNewPolys.CalculateF_i_xValuesForPlayers(NumParties, Prime);
					var playerKNewGk_y_w_iValues = playerKNewPolys.calculateG_i_yValuesForVerification(NumParties, Prime);
					if (!playerKNewFk_x_w_iValues[k].Equals(playerKNewGk_y_w_iValues[k]))
						return true;

					if (k == Party.Id)
					{
						// Verify that the new public polynomials equals the old polynomials
						if (!IsSecretPolynomialsLegal(myRecvPolys) || !myRecvPolys.Equals(playerKNewPolys))
							return true;
					}
					else
					{
						// Verify that the public information doesn't contradict the old information :
						// f(w^j, w^k) = fk(w^j) = gj(w^k) = f(w^j, w^k) && f(w^k, w^j) = fj(w^k) = gk(w^j) = f(w^k, w^j)
						if ((!myG_j_w_iValues[k].Equals(playerKNewFk_x_w_iValues[Party.Id])) || (!myF_j_w_iValues[k].Equals(playerKNewGk_y_w_iValues[Party.Id])))
							return true;
					}
				}
			}
			return false;
		}

		#endregion Private Methods

		#endregion Methods
	}
}