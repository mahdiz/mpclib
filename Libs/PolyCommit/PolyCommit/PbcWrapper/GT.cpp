#include "GT.h"
#include <cstring>
#include "PBCExceptions.h"

GT GT::pow2(const Pairing &e, const GT &base1, const Zr &exp1, const GT &base2, const Zr &exp2)
{
	GT gout(e, true);
	G::pow2(gout, base1, exp1, base2, exp2);
	return gout;
}

GT GT::pow3(const Pairing &e, const GT &base1, const Zr &exp1, const GT &base2, const Zr &exp2, const GT &base3, const Zr &exp3)
{
	GT gout(e, true);
	G::pow3(gout, base1, exp1, base2, exp2, base3, exp3);
	return gout;
}

//Create and initialize an element
GT::GT(const Pairing &e): G(e){
  if(elementPresent){
	element_init_GT(g, *(pairing_t*)&e.getPairing());
  }else throw UndefinedPairingException();
}

//Create an identity or a random element
GT::GT(const Pairing &e, bool identity): G(e){
  if(elementPresent){
	element_init_GT(g, *(pairing_t*)&e.getPairing());
	if (identity)
	  element_set1(g);
	else
	  element_random(g);
  }else throw UndefinedPairingException();
}

//Create an element from import 
GT::GT(const Pairing &e, const unsigned char *data, 
	   unsigned short len, unsigned short base): G(e){
  if(elementPresent){
	element_init_GT(g, *(pairing_t*)&e.getPairing());
	importElement(data, len, false, base);
  }else throw UndefinedPairingException();
}

//Create an element from hash
GT::GT(const Pairing &e, const void *data, 
	   unsigned short len): G(e){
  if(elementPresent){
	element_init_GT(g, *(pairing_t*)&e.getPairing());
	element_from_hash(g,  *(void**)&data, len);
  }else throw UndefinedPairingException();
}


