#pragma once

#include <string>
#include "BinaryWriter.h"
#include "Submesh.h"
#include "Physics.h"

namespace BXDA
{
	class Mesh : public BinaryWritable
	{
	public:
		Mesh();
		~Mesh();
		
		void addSubMesh(const SubMesh &);
		void addPhysics(const Physics &);

		std::string getGUID() const;
		int getVersion() const;

		std::string toString();

	private:
		const int CURRENT_VERSION = 0;

		std::string guid;
		Physics physics;
		std::vector<SubMesh*> subMeshes;

		void write(BinaryWriter &) const;

	};
}
