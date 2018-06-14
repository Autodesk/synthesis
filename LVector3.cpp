#include "LVector3.h"

#define NULL 0

LVector3::LVector3() {

}

LVector3::LVector3(const LVector3* v) {
	this->Head = v->Head;
}

LVector3::~LVector3() {

}

//Add a Vector3 to the List using operator overloading
LVector3* LVector3::operator+ (const Vector3* v) const {
	Vector3 *v2 = new Vector3(v->x, v->y, v->z);  //should have just made a const copy constructor
	LVector3 * a = new LVector3(this);

	if(Head != NULL)
		a->add(v2, Head);

	if (Head == NULL) {
		a->Head = v2;
	}
}

void LVector3::add(Vector3* v, Vector3* tempHead) {
	if (tempHead->next != NULL)
		return(add(v, tempHead->next));

	tempHead->next = v;
}