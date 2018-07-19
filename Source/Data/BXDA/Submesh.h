#pragma once

#include <vector>
#include "Vertex.h"
#include "Surface.h"

using namespace std;

namespace BXDA
{
	class Submesh
	{
	public:
		Submesh();
		~Submesh();

		Submesh(Submesh*);
		Submesh(vector<Vertex*> vertices);
		Submesh(vector<Vertex*> vertices, vector<Surface*> surfaces);

		void addSurface(Surface*);

	private:
		vector<Vertex*> vertices;
		vector<Surface*> surfaces;
	};
}