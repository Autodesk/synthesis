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

		std::string getGUID();
		int getVersion();

	private:
		const int CURRENT_VERSION = 0;

		std::string guid;
		Physics physics;
		std::vector<SubMesh*> subMeshes;

	};
}