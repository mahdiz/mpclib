#include "G2.h"
#include <cstring>
#include "PBCExceptions.h"
#include <vector>

G2 G2::pow2(const Pairing &e, const G2 &base1, const Zr &exp1, const G2 &base2, const Zr &exp2)
{
	G2 gout(e, true);
	G::pow2(gout, base1, exp1, base2, exp2);
	return gout;
}

G2 G2::pow3(const Pairing &e, const G2 &base1, const Zr &exp1, const G2 &base2, const Zr &exp2, const G2 &base3, const Zr &exp3)
{
	G2 gout(e, true);
	G::pow3(gout, base1, exp1, base2, exp2, base3, exp3);
	return gout;
}

//Create and initialize an element
G2::G2(const Pairing &e) : G(e)
{
	if (elementPresent)
	{
		element_init_G2(g, *(pairing_t*)&e.getPairing());
	}
	else throw UndefinedPairingException();
}

//Create an identity or a random element
G2::G2(const Pairing &e, bool identity) : G(e)
{
	if (elementPresent)
	{
		element_init_G2(g, *(pairing_t*)&e.getPairing());
		if (identity)
			element_set1(g);
		else
			element_random(g);
	}
	else throw UndefinedPairingException();
}

//Create an element from import 
G2::G2(const Pairing &e, const unsigned char *data,
	unsigned short len, bool compressed,
	unsigned short base) : G(e)
{
	if (elementPresent)
	{
		element_init_G2(g, *(pairing_t*)&e.getPairing());
		importElement(data, len, compressed, base);
	}
	else throw UndefinedPairingException();
}

//Create an element from hash
G2::G2(const Pairing &e, const void *data,
	unsigned short len) : G(e)
{
	if (elementPresent)
	{
		element_init_G2(g, *(pairing_t*)&e.getPairing());
		element_from_hash(g, *(void**)&data, len);
	}
	else throw UndefinedPairingException();
}

//Overriden getElementSize to take care of compressed elements
unsigned short G2::getElementSize(bool compressed) const
{
	if (!elementPresent)
		throw UndefinedElementException();
	else if (compressed)
		return (unsigned short)
		element_length_in_bytes_compressed(*(element_t*)&g);
	else return G::getElementSize();
}

//Overriden toString to take care of compressed elements
string G2::toString(bool compressed) const
{
	string str;
	if (compressed)
	{
		//unsigned char buf[1];
		//buf[0] = elementPresent & 0xff;
		//str.append((char*)buf,1);
		if (elementPresent)
		{
			short len = element_length_in_bytes_compressed(*(element_t*)&g);

			vector<unsigned char> data(len);
			element_to_bytes_compressed(data.data(), *(element_t*)&g);
			str.append((char*)data.data(), len);
		}
	}
	else str.append(G::toString());
	return str;
}
