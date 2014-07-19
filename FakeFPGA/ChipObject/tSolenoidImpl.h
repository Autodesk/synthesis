/*
 * tSolenoidImpl.h
 *
 *  Created on: Jul 18, 2014
 *      Author: localadmin
 */

#ifndef TSOLENOIDIMPL_H_
#define TSOLENOIDIMPL_H_

#include <tSolenoid.h>

namespace nFPGA {

class tSolenoid_Impl: public nFPGA::nFRC_2012_1_6_4::tSolenoid {
public:
	tSolenoid_Impl();
	virtual ~tSolenoid_Impl();
};

}

#endif /* TSOLENOIDIMPL_H_ */
