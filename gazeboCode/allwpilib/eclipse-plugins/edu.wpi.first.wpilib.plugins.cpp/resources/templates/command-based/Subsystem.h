#ifndef $classname_H
#define $classname_H

#include "Commands/Subsystem.h"
#include "WPILib.h"

class $classname: public Subsystem
{
private:
	// It's desirable that everything possible under private except
	// for methods that implement subsystem capabilities
public:
	$classname();
	void InitDefaultCommand();
};

#endif
