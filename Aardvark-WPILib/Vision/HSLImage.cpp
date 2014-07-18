/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "HSLImage.h"

/**
 * Create a new image that uses the Hue, Saturation, and Luminance planes.
 */
HSLImage::HSLImage() : ColorImage(IMAQ_IMAGE_HSL)
{
}

/**
 * Create a new image by loading a file.
 * @param fileName The path of the file to load.
 */
HSLImage::HSLImage(const char *fileName) : ColorImage(IMAQ_IMAGE_HSL)
{
	int success = imaqReadFile(m_imaqImage, fileName, NULL, NULL);
	wpi_setImaqErrorWithContext(success, "Imaq ReadFile error");
}

HSLImage::~HSLImage()
{
}
