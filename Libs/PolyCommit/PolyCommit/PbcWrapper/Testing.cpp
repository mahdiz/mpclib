#include <iostream>
#include <time.h>
#include "PbcWapper.h"

using namespace std;

int main(int argc, char **argv)
{
  const char *paramFileName = (argc > 1) ? argv[1] : "pairing.param";
  FILE *sysParamFile = fopen(paramFileName, "r");
  if (sysParamFile == NULL) {
    cerr<<"Can't open the parameter file " << paramFileName << "\n";
    cerr<<"Usage: " << argv[0] << " [paramfile]\n";
    return 0;
  }
  Pairing e(sysParamFile);
  cout<<"Is symmetric? "<< e.isSymmetric()<< endl;
  cout<<"Is pairing present? "<< e.isPairingPresent()<< endl;  
  fclose(sysParamFile);

  G1 p(e,false);
  p = G1(e, "1234567", 7);
  p.dump(stdout,"Hash for 1234567 is ",16);
  G1 p2(e, (const unsigned char *)"[1550802208750744164432135979414125328650318814188862492225146851836769390500258399788367615142135849505646841663601231932608834381650558011983930152205480, 162426794903089459059163657485909203049527624684632613984369979812948511813352043323998239353170652927595733566299330078984161336240682404138940861563814]", 311, false, 10);
  p2.dump(stdout,"p2 is               ",16);
  cout << "p == p2 ? " << ((p == p2) ? "YES" : "NO") << "\n";
  G1 p3(e, (const unsigned char *)"\x1d\x9c\x29\xcd\xc7\x2a\xca\x6c\x73\x53\xf9\x59\x92\x78\x39\xe9\x4e\x2e\x73\x13\xac\x66\xcc\xb2\x15\xfd\x3f\xc1\x61\x51\x89\xf4\xad\xe6\x57\x5f\x43\xf0\xa8\xc0\xbd\x3e\xc8\xe1\x9f\x86\x96\x92\x4f\x41\x22\x20\x96\xd3\x9b\xd8\x3c\xd8\xfe\x14\xb6\xf4\x3c\xa8\x03\x19\xec\xf6\x71\xe1\x03\x92\x9c\x86\x81\xbf\x8d\x5a\xc8\x69\xff\xac\x18\x41\x34\x57\xfe\x0f\xfe\xfc\xe3\xea\xd8\x0c\x4e\xdb\x45\x15\x3d\x35\xc0\x4c\x14\x27\xf5\x19\x1e\x03\xd2\x0e\x57\x91\xb4\x04\x18\x17\x05\x50\xdc\xd4\x80\x23\x90\x19\x98\x5c\xc3\xa6", 128); // , false, 0 is the default for compressed, base
  p3.dump(stdout,"p3 is               ",16);
  cout << "p == p3 ? " << ((p == p3) ? "YES" : "NO") << "\n";
  G1 p4(e, (const unsigned char *)"\x1d\x9c\x29\xcd\xc7\x2a\xca\x6c\x73\x53\xf9\x59\x92\x78\x39\xe9\x4e\x2e\x73\x13\xac\x66\xcc\xb2\x15\xfd\x3f\xc1\x61\x51\x89\xf4\xad\xe6\x57\x5f\x43\xf0\xa8\xc0\xbd\x3e\xc8\xe1\x9f\x86\x96\x92\x4f\x41\x22\x20\x96\xd3\x9b\xd8\x3c\xd8\xfe\x14\xb6\xf4\x3c\xa8\x00", 65, true); // 0 is the default base
  p4.dump(stdout,"p4 is               ",16);
  cout << "p == p4 ? " << ((p == p4) ? "YES" : "NO") << "\n";
  G2 q(e,false);
  Zr r(e,(long int)10121);
  r.dump(stdout,"r",10);
  Zr r2(e,(const unsigned char *)"\x12\x34", 2);
  Zr r3(e,(const unsigned char *)"1234", 4, 16);
  Zr r4(e,(const unsigned char *)"1234", 4, 10);
  r2.dump(stdout,"r2",16);
  r3.dump(stdout,"r3",16);
  r4.dump(stdout,"r4",16);
  // Create a random element of Zr
  Zr s(e,true);
  s.dump(stdout,"s",10);
  r =s;
  r.dump(stdout,"new r",10);
  GT LHS = e(p,q)^r;
  G1 pr(p^r);
  GPP<G1> pp(e, p);
  GPP<G2> qp(e, q);
  GPP<GT> LHSp(e, LHS);
  struct timeval st, et;
  const int niter = 1000;
  gettimeofday(&st, NULL);
  G1 Q(e);
  for(int i=0;i<niter;++i) {
    Q *= (p^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsed = (et.tv_sec-st.tv_sec)*1000000 +
			      (et.tv_usec-st.tv_usec);
  gettimeofday(&st, NULL);
  G1 Qp(e);
  for(int i=0;i<niter;++i) {
    Qp *= (pp^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsed_pre = (et.tv_sec-st.tv_sec)*1000000 +
				  (et.tv_usec-st.tv_usec);
  gettimeofday(&st, NULL);
  G2 Q2(e);
  for(int i=0;i<niter;++i) {
    Q2 *= (q^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsed2 = (et.tv_sec-st.tv_sec)*1000000 +
			      (et.tv_usec-st.tv_usec);
  gettimeofday(&st, NULL);
  G2 Q2p(e);
  for(int i=0;i<niter;++i) {
    Q2p *= (qp^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsed2_pre = (et.tv_sec-st.tv_sec)*1000000 +
				  (et.tv_usec-st.tv_usec);
  gettimeofday(&st, NULL);
  GT QT(e);
  for(int i=0;i<niter;++i) {
    QT *= (LHS^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsedT = (et.tv_sec-st.tv_sec)*1000000 +
			      (et.tv_usec-st.tv_usec);
  gettimeofday(&st, NULL);
  GT QTp(e);
  for(int i=0;i<niter;++i) {
    QTp *= (LHSp^r);
  }
  gettimeofday(&et, NULL);
  unsigned long uselapsedT_pre = (et.tv_sec-st.tv_sec)*1000000 +
				  (et.tv_usec-st.tv_usec);
  G1 prpp(pp^r);
  p.dump(stdout,"p", 10);
  q.dump(stdout, "q", 10);
  pr.dump(stdout,"p^r (reg)", 10);
  prpp.dump(stdout,"p^r (pre)", 10);
  Q.dump(stdout,"Q (reg)", 10);
  Qp.dump(stdout,"Q (pre)", 10);
  Q2.dump(stdout,"Q2 (reg)", 10);
  Q2p.dump(stdout,"Q2 (pre)", 10);
  QT.dump(stdout,"QT (reg)", 10);
  QTp.dump(stdout,"QT (pre)", 10);
  cout << "time G1 (reg) = " << uselapsed << " us / " << niter << " = " <<
      uselapsed / niter << " us\n";
  cout << "time G1 (pre) = " << uselapsed_pre << " us / " << niter << " = " <<
      uselapsed_pre / niter << " us\n";
  cout << "time G2 (reg) = " << uselapsed2 << " us / " << niter << " = " <<
      uselapsed2 / niter << " us\n";
  cout << "time G2 (pre) = " << uselapsed2_pre << " us / " << niter << " = " <<
      uselapsed2_pre / niter << " us\n";
  cout << "time GT (reg) = " << uselapsedT << " us / " << niter << " = " <<
      uselapsedT / niter << " us\n";
  cout << "time GT (pre) = " << uselapsedT_pre << " us / " << niter << " = " <<
      uselapsedT_pre / niter << " us\n";
  GT RHS = e((p^r),q);
  LHS.dump(stdout,"LHS", 10);
  RHS.dump(stdout,"RHS", 10);

  if((e(p,q)^r) == e(p^r,q))
	cout<<"Correct Pairing Computation"<<endl;
  else
	cout<<"Incorrect Pairing Computation"<<endl;
  if((p.inverse()).square() == (p.square()).inverse())
	cout<<"Inverse, Square works"<<endl;
  else
	cout<<"Inverse, Square does not work."<<endl;
  G1 a;
  a = p;
  p.dump(stdout,"p is ") ;
  a.dump(stdout,"a is ") ;
  // Create the identity element b (in the same group as a)
  G1 b(a,true);
  b.dump(stdout,"b is ") ;
  return 0;
}
