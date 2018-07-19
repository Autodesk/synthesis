#pragma once

#include <string>
#include "Submesh.h"
#include "Physics.h"

namespace BXDA
{
	class Mesh
	{
	public:
		Mesh();
		~Mesh();

		friend std::ostream& operator<<(std::ostream&, const Mesh&);

		void addSubmesh(SubMesh * submesh);

		string getGUID();
		int getVersion();

	private:
		const int CURRENT_VERSION = 0;

		string guid;
		Physics physics;
		vector<SubMesh*> subMeshes;

	};
}