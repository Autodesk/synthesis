#pragma once

#include <string>
#include "Submesh.h"
#include "Physics.h"

/*
#ifdef WIN32
#include <Rpc.h>
#else
#include <uuid/uuid.h>
#endif

//credit for this platform independent generation goes to https://stackoverflow.com/users/196807/ubik
std::string newUUID()
{
#ifdef WIN32
	UUID uuid;
	UuidCreate(&uuid);

	unsigned char * str;
	UuidToStringA(&uuid, &str);

	std::string s((char*)str);

	RpcStringFreeA(&str);
#else
	uuid_t uuid;
	uuid_generate_random(uuid);
	char s[37];
	uuid_unparse(uuid, s);
#endif
	return s;
}
*/

namespace BXDA
{
	class Mesh
	{
	public:
		Mesh();
		~Mesh();

		void addSubmesh(Submesh * submesh);

		string getGUID();
		int getVersion();

	private:
		const int CURRENT_VERSION = 0;

		string guid;
		Physics physics;
		vector<Submesh*> submeshes;

	};
}