#ifndef _HEL_SPIDEV_H
#define _HEL_SPIDEV_H

#include "drm/drm.h"

struct spi_ioc_transfer {
    __u64       tx_buf;
    __u64       rx_buf;

    __u32       len;
    __u32       speed_hz;

    __u16       delay_usecs;
    __u8        bits_per_word;
    __u8        cs_change;
    __u8        tx_nbits;
    __u8        rx_nbits;
    __u16       pad;
};

#define SPI_IOC_MAGIC 'k'

#define SPI_MSGSIZE(N) \
    ((((N)*(sizeof (struct spi_ioc_transfer))) < (1 << _IOC_SIZEBITS)) \
        ? ((N)*(sizeof (struct spi_ioc_transfer))) : 0)

#define SPI_IOC_MESSAGE(N) _IOW(SPI_IOC_MAGIC, 0, char[SPI_MSGSIZE(N)])
#define SPI_IOC_WR_MAX_SPEED_HZ     _IOW(SPI_IOC_MAGIC, 4, __u32)
#define SPI_IOC_WR_MODE         _IOW(SPI_IOC_MAGIC, 1, __u8)

#endif
