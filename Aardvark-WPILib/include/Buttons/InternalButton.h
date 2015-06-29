/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2011. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __INTERNAL_BUTTON_H__
#define __INTERNAL_BUTTON_H__

#include "Buttons/Button.h"

class InternalButton : public Button
{
public:
	InternalButton();
	InternalButton(bool inverted);
	virtual ~InternalButton() {}

	void SetInverted(bool inverted);
	void SetPressed(bool pressed);

	virtual bool Get();

private:
	bool m_pressed;
	bool m_inverted;
};

#endif
