#include "Vector3.h"

using namespace BXDATA;

#define NULL 0

//default constructor
Vector3::Vector3() {
	next = NULL;
	prev = NULL;
	x = 0;
	y = 0;
	z = 0;
}

//first item constructor JIC
Vector3::Vector3(float _x, float _y, float _z) : x(_x), y(_y), z(_z) {
	next = NULL;
	prev = NULL;
}

//copy constructor just in case 
Vector3::Vector3(Vector3* v) {
	this->next = v->next;
	this->prev = v->prev;
	this->x = v->x;
	this->y = v->y;
	this->z = v->z;

	if (v->prev) {
		v->prev->next = this;
	}
	
	if (v->next) {
		v->next->prev = this;
	}
}