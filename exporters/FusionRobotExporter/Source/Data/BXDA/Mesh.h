#pragma once

#include <string>
#include <vector>
#include "BinaryWriter.h"
#include "Physics.h"
#include "../Guid.h"
#include "../Vector3.h"
#include "../BXDJ/ConfigData.h"

namespace BXDA
{
	class SubMesh;

	/// A collection of SubMeshes identified by a common GUID.
	class Mesh : public BinaryWritable
	{
	public:
		///
		/// Creates an empty mesh with the given Guid.
		/// \param guid The Guid of the new mesh.
		///
		Mesh(Guid guid);
		
		void addSubMesh(const SubMesh &); ///< Copies a SubMesh into the Mesh.
		void addSubMesh(std::shared_ptr<SubMesh>); ///< Adds a SubMesh into the Mesh.
		void addPhysics(const Physics &); ///< Averages a set of Physics properties with the existing Physics properties.

		Guid getGUID() const; ///< \return The Guid of the Mesh.
		int getVersion() const; ///< \return The version number of the binary output from the Mesh.

		///
		/// Calculates the radial limits of the Mesh.
		/// \param axis The axis along which the wheel rotates.
		/// \param origin The base point of the wheel's rotation.
		/// \param[out] minWidth The smallest distance along axis at which a vertex resides.
		/// \param[out] maxWidth The largest distance along axis at which a vertex resides.
		/// \param[out] maxRadius The furthest radial distance a vertex is from the axis.
		///
		void calculateWheelShape(Vector3<>, Vector3<>, double & minWidth, double & maxWidth, double & maxRadius) const;

		std::string toString(); ///< \return A debug string representing the Mesh.
		BXDJ::ConfigData config;
	private:
		const int CURRENT_VERSION = 0; ///< The current BXDA version.

		Guid guid; ///< The Globally Unique Identifier (Guid) of the Mesh.
		Physics physics; ///< The Physics properties of the Mesh.
		std::vector<std::shared_ptr<SubMesh>> subMeshes; ///< The SubMeshes contained in the Mesh.

		void write(BinaryWriter &) const;

	};
}
