/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#ifndef __IMAGE_BASE_H__
#define __IMAGE_BASE_H__

#include <stdio.h>
#include "nivision.h"
#include "ErrorBase.h"

#define DEFAULT_BORDER_SIZE 3

class ImageBase : public ErrorBase
{
public:
	ImageBase(ImageType type);
	virtual ~ImageBase();
	virtual void Write(const char *fileName);
	int GetHeight();
	int GetWidth();
	Image *GetImaqImage();
protected:
	Image *m_imaqImage;
};

#endif
