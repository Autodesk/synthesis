#pragma once
#include "LVector3.h"
#include <list>

using namespace std;

namespace BXDATA{
	class Surface {
	public:
		Surface();

		Surface(bool hasColor, unsigned int color, float transparency, float translucency, float specular, LVector3* lv);

		Surface(Surface*);
		~Surface();			//For unloading the array JIC


	private:
		//0 = NO , 1 = YES
		bool HasColor;

		//HEX color
		unsigned int color;

		//Random stuff
		float transparency;
		float translucency;
		float specular;

		//Inidicies ?
		//LVector3* facet;

		list<double> facet;
	};
}