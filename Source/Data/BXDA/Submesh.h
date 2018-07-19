#pragma once

#include <vector>
#include "Vertex.h"
#include "Surface.h"

using namespace std;

namespace BXDA
{
	class SubMesh
	{
	public:
		SubMesh();
		~SubMesh();

		SubMesh(SubMesh*);
		SubMesh(vector<Vertex*> vertices);
		SubMesh(vector<Vertex*> vertices, vector<Surface*> surfaces);

		friend std::ostream& operator<<(std::ostream&, const SubMesh&);

		void addSurface(Surface*);

	private:
		vector<Vertex*> vertices;
		vector<Surface*> surfaces;
	};
}