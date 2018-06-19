#pragma once
#include "Vector3.h" 

namespace BXDATA {
	//List equivalent for the Vector3 data type
	class LVector3 {
	public:
		LVector3();
		LVector3(const LVector3*);
		~LVector3();

		LVector3* operator+(Vector3&) const;		//Add a Vector3 //I somehow really fuxked this up

		void add(Vector3*);

		int count();

		//add [] operator
		//add >> operator

	private:
		Vector3 * Head;

		void add(Vector3*, Vector3*);

		int count(Vector3*);
	};
}