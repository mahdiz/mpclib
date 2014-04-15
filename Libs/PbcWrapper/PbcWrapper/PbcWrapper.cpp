// Mahdi Zamani
// University of New Mexico
// zamani@cs.unm.edu

#include "PbcWrapper.h"
#include <vector>

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

int Pbc::PbcWrapper::Init()
{
	StreamReader^ din = File::OpenText(
		"C:/Users/Mahdi/Desktop/Dropbox/Svn_Codes/Library/pbc-0.5.14-vc/param/a.param");
	String ^pstr = din->ReadToEnd();

	IntPtr p = Marshal::StringToHGlobalAnsi(pstr);
	const char* param = static_cast<char*>(p.ToPointer());

	pairing_t pairing;
	size_t len = strlen(param);
	pairing_init_set_buf(pairing, param, len);

	element_t g, h;
	element_t public_key, secret_key;
	element_t sig;
	element_t temp1, temp2;

	element_init_G2(g, pairing);
	element_init_G2(public_key, pairing);
	element_init_G1(h, pairing);
	element_init_G1(sig, pairing);
	element_init_GT(temp1, pairing);
	element_init_GT(temp2, pairing);
	element_init_Zr(secret_key, pairing);

	element_random(g);
	element_printf("system parameter g = %B\n\n", g);

	std::vector<unsigned char> aa(1024);
	element_to_bytes(aa.data(), g);

	element_random(secret_key);
	element_printf("private key = %B\n\n", secret_key);

	element_pow_zn(public_key, g, secret_key);
	element_printf("public key = %B\n\n", public_key);

	element_from_hash(h, "hashofmessage", 13);
	element_printf("message hash = %B\n\n", h);

	// h^secret_key is the signature
	element_pow_zn(sig, h, secret_key);
	element_printf("signature = %B\n\n", sig);

	pairing_apply(temp1, sig, g, pairing);
	element_printf("f(sig, g) =\n%B\n\n", temp1);

	pairing_apply(temp2, h, public_key, pairing);
	element_printf("f(hash, public_key) =\n%B\n\n", temp2);

	if (element_cmp(temp1, temp2) == 0) 
		printf("Signature verifies!\n\n");
	else 
		printf("BUG: Signature does not verify!\n\n");

	Marshal::FreeHGlobal(p);
	return 0;
}