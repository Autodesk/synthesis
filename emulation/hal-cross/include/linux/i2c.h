#ifndef _HEL_I2C_H
#define _HEL_I2C_H

#include "drm/drm.h"

#define I2C_RDWR 0x0707
#define I2C_M_RD 0x0001

struct i2c_msg{
    __u16 addr;
    __u16 flags;
    __u16 len;
    __u8 *buf;
};

#endif
