#pragma once


namespace BXDATA {
#define EPSILON 1.0E-6F;					//This is a tool that will come in handy later

	//Data point <x, y, z>
	class Vector3 {
	public:
		Vector3();
		Vector3(float, float, float);			//Overloaded Constructor for first item
		Vector3(Vector3*);
		~Vector3();

		//Add + operator
		//Add * operator
		//Add >> operator


		Vector3 * next;
		Vector3 * prev;
		float x;								//X coordinate for the vector
		float y;								//Y coordinate 
		float z;								//Z coordiante

	private:

	};
}
