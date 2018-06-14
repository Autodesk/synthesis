#pragma once


class Surface {
public:
	Surface();
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
	int * indicies;
};