#include "LVector3.h"

using namespace BXDATA;

#define NULL nullptr

LVector3::LVector3() {
	this->Head = NULL;
}

LVector3::LVector3(const LVector3* v) {
	this->Head = v->Head;
}

LVector3::~LVector3() {

}

//Add a Vector3 to the List using operator overloading
LVector3* LVector3::operator+ (Vector3& v) const {
	//Vector3 *v2 = new Vector3(v->x, v->y, v->z);  //should have just made a const copy constructor
	LVector3 * a = new LVector3(this);

	if(Head != NULL)
		a->add(&v, Head);
	else
		a->Head = &v;

	return a;
}

void LVector3::add(Vector3* v) {
	if (Head != NULL)
		this->add(v, Head);
	else
		Head = new Vector3(v);
}

void LVector3::add(Vector3* v2, Vector3* tempHead) {
	if (tempHead->next != NULL) {
		return(add(v2, tempHead->next));
	}
	else {

		v2->prev = tempHead;
		tempHead->next = new Vector3(v2);
	}
}

int LVector3::count() {
	if (Head != NULL) {
		return count(Head);
	}
	else {
		return 0;
	}
}

int LVector3::count(Vector3* n) {
	if (n != nullptr)
		return 1 + count(n->next);
	else
		return 0;
}