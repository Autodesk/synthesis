#ifndef _HEL_I2C_DEV_H
#define _HEL_I2C_DEV_H

#include "drm/drm.h"

struct i2c_rdwr_ioctl_data {
    struct i2c_msg *msgs;
    __u32 nmsgs;
};

#endif
