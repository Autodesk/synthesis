#pragma once

#include <string>
#include "BinaryWriter.h"
#include "Submesh.h"
#include "Physics.h"
#include "../Guid.h"

namespace BXDA
{
	class Mesh : public BinaryWritable
	{
	public:
		Mesh(Guid guid);
		
		void addSubMesh(const SubMesh &);
		void addSubMesh(std::shared_ptr<SubMesh>);
		void addPhysics(const Physics &);

		Guid getGUID() const;
		int getVersion() const;

		void calculateWheelShape(Vector3<>, Vector3<>, double & minWidth, double & maxWidth, double & maxRadius) const;

		std::string toString();

	private:
		const int CURRENT_VERSION = 0;

		Guid guid;
		Physics physics;
		std::vector<std::shared_ptr<SubMesh>> subMeshes;

		void write(BinaryWriter &) const;

	};
}
