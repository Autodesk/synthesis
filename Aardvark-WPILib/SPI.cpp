/*----------------------------------------------------------------------------*/
/* Copyright (c) FIRST 2008. All Rights Reserved.							  */
/* Open Source Software - may be modified and shared by FRC teams. The code   */
/* must be accompanied by the FIRST BSD license file in $(WIND_BASE)/WPILib.  */
/*----------------------------------------------------------------------------*/

#include "SPI.h"

#include "ChipObject/tSPI.h"
#include "DigitalModule.h"
#include "DigitalInput.h"
#include "DigitalOutput.h"
#include "NetworkCommunication/UsageReporting.h"
#include "Synchronized.h"
#include "WPIErrors.h"

#include <math.h>
#include <usrLib.h>

SEM_ID SPI::m_semaphore = NULL;

/**
 * Constructor for input and output.
 *
 * @param clk	The digital output for the clock signal.
 * @param mosi	The digital output for the written data to the slave
 *				(master-out slave-in).
 * @param miso	The digital input for the input data from the slave
 *				(master-in slave-out).
 */
SPI::SPI(DigitalOutput &clk, DigitalOutput &mosi, DigitalInput &miso)
{
	Init(&clk, &mosi, &miso);
}

/**
 * Constructor for input and output.
 *
 * @param clk	The digital output for the clock signal.
 * @param mosi	The digital output for the written data to the slave
 *				(master-out slave-in).
 * @param miso	The digital input for the input data from the slave
 *				(master-in slave-out).
 */
SPI::SPI(DigitalOutput *clk, DigitalOutput *mosi, DigitalInput *miso)
{
	Init(clk, mosi, miso);
}

/**
 * Constructor for output only.
 *
 * @param clk	The digital output for the clock signal.
 * @param mosi	The digital output for the written data to the slave
 *				(master-out slave-in).
 */
SPI::SPI(DigitalOutput &clk, DigitalOutput &mosi)
{
	Init(&clk, &mosi, NULL);
}

/**
 * Constructor for output only.
 *
 * @param clk	The digital output for the clock signal.
 * @param mosi	The digital output for the written data to the slave
 *				(master-out slave-in).
 */
SPI::SPI(DigitalOutput *clk, DigitalOutput *mosi)
{
	Init(clk, mosi, NULL);
}

/**
 * Constructor for input only.
 *
 * @param clk	The digital output for the clock signal.
 * @param miso	The digital input for the input data from the slave
 *				(master-in slave-out).
 */
SPI::SPI(DigitalOutput &clk, DigitalInput &miso)
{
	Init(&clk, NULL, &miso);
}

/**
 * Constructor for input only.
 *
 * @param clk	The digital output for the clock signal.
 * @param miso	The digital input for the input data from the slave
 *				(master-in slave-out).
 */
SPI::SPI(DigitalOutput *clk, DigitalInput *miso)
{
	Init(clk, NULL, miso);
}

/**
 * Destructor.
 */
SPI::~SPI()
{
	delete m_spi;
}

/**
 * Initialize SPI channel configuration.
 *
 * @param clk	The digital output for the clock signal.
 * @param mosi	The digital output for the written data to the slave
 *				(master-out slave-in).
 * @param miso	The digital input for the input data from the slave
 *				(master-in slave-out).
 */
void SPI::Init(DigitalOutput *clk, DigitalOutput *mosi, DigitalInput *miso)
{
	if (m_semaphore == NULL)
	{
		m_semaphore = semMCreate(SEM_Q_PRIORITY | SEM_DELETE_SAFE | SEM_INVERSION_SAFE);
	}

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_spi = tSPI::create(&localStatus);
	wpi_setError(localStatus);

	m_config.BusBitWidth = 8;
	m_config.ClockHalfPeriodDelay = 0;
	m_config.MSBfirst = 0;
	m_config.DataOnFalling = 0;
	m_config.LatchFirst = 0;
	m_config.LatchLast = 0;
	m_config.FramePolarity = 0;
	m_config.WriteOnly = miso ? 0 : 1;
	m_config.ClockPolarity = 0;

	m_channels.SCLK_Channel = clk->GetChannelForRouting();
	m_channels.SCLK_Module = clk->GetModuleForRouting();
	m_channels.SS_Channel = 0;
	m_channels.SS_Module = 0;

	if (mosi)
	{
		m_channels.MOSI_Channel = mosi->GetChannelForRouting();
		m_channels.MOSI_Module = mosi->GetModuleForRouting();
	}
	else
	{
		m_channels.MOSI_Channel = 0;
		m_channels.MOSI_Module = 0;
	}

	if (miso)
	{
		m_channels.MISO_Channel = miso->GetChannelForRouting();
		m_channels.MISO_Module = miso->GetModuleForRouting();
	}
	else
	{
		m_channels.MISO_Channel = 0;
		m_channels.MISO_Module = 0;
	}

	m_ss = NULL;

	static int32_t instances = 0;
	instances++;
	nUsageReporting::report(nUsageReporting::kResourceType_SPI, instances);
}

/**
 * Configure the number of bits from each word that the slave transmits
 * or receives.
 *
 * @param bits	The number of bits in one frame (1 to 32 bits).
 */
void SPI::SetBitsPerWord(uint32_t bits)
{
	m_config.BusBitWidth = bits;
}

/**
 * Get the number of bits from each word that the slave transmits
 * or receives.
 *
 * @return The number of bits in one frame (1 to 32 bits).
 */
uint32_t SPI::GetBitsPerWord()
{
	return m_config.BusBitWidth;
}

/**
 * Configure the rate of the generated clock signal.
 * The default and maximum value is 76,628.4 Hz.
 *
 * @param hz	The clock rate in Hertz.
 */
void SPI::SetClockRate(double hz)
{
	int delay = 0;
	tRioStatusCode localStatus = NiFpga_Status_Success;
	int loopTiming = DigitalModule::GetInstance(m_spi->readChannels_SCLK_Module(&localStatus))->GetLoopTiming();
    wpi_setError(localStatus);
    double v = (1.0 / hz) / (2 * loopTiming / (kSystemClockTicksPerMicrosecond * 1e6));
    if (v < 1) {
        wpi_setWPIErrorWithContext(ParameterOutOfRange, "SPI Clock too high");
    }
    delay = (int) (v + .5);
    if (delay > 255) {
    	wpi_setWPIErrorWithContext(ParameterOutOfRange, "SPI Clock too low");
    }

	m_config.ClockHalfPeriodDelay = delay;
}

/**
 * Configure the order that bits are sent and received on the wire
 * to be most significant bit first.
 */
void SPI::SetMSBFirst()
{
	m_config.MSBfirst = 1;
}

/**
 * Configure the order that bits are sent and received on the wire
 * to be least significant bit first.
 */
void SPI::SetLSBFirst()
{
	m_config.MSBfirst = 0;
}

/**
 * Configure that the data is stable on the falling edge and the data
 * changes on the rising edge.
 */
void SPI::SetSampleDataOnFalling()
{
	m_config.DataOnFalling = 1;
}

/**
 * Configure that the data is stable on the rising edge and the data
 * changes on the falling edge.
 */
void SPI::SetSampleDataOnRising()
{
	m_config.DataOnFalling = 0;
}

/**
 * Configure the slave select line behavior.
 *
 * @param ss slave select digital output.
 * @param mode Frame mode:
 *			   kChipSelect: active for the duration of the frame.
 *			   kPreLatchPulse: pulses before the transfer of each frame.
 *			   kPostLatchPulse: pulses after the transfer of each frame.
 *			   kPreAndPostLatchPulse: pulses before and after each frame.
 * @param activeLow True if slave select line is active low.
 */
void SPI::SetSlaveSelect(DigitalOutput *ss, tFrameMode mode, bool activeLow)
{
	if (ss)
	{
		m_channels.SS_Channel = ss->GetChannelForRouting();
		m_channels.SS_Module = ss->GetModuleForRouting();
	}
	else
	{
		m_channels.SS_Channel = 0;
		m_channels.SS_Module = 0;
	}
	m_ss = ss;

	switch (mode)
	{
		case kChipSelect:
			m_config.LatchFirst = 0;
			m_config.LatchLast = 0;
			break;
		case kPreLatchPulse:
			m_config.LatchFirst = 1;
			m_config.LatchLast = 0;
			break;
		case kPostLatchPulse:
			m_config.LatchFirst = 0;
			m_config.LatchLast = 1;
			break;
		case kPreAndPostLatchPulse:
			m_config.LatchFirst = 1;
			m_config.LatchLast = 1;
			break;
	}

	m_config.FramePolarity = activeLow ? 1 : 0;
}

/**
 * Configure the slave select line behavior.
 *
 * @param ss slave select digital output.
 * @param mode Frame mode:
 *			   kChipSelect: active for the duration of the frame.
 *			   kPreLatchPulse: pulses before the transfer of each frame.
 *			   kPostLatchPulse: pulses after the transfer of each frame.
 *			   kPreAndPostLatchPulse: pulses before and after each frame.
 * @param activeLow True if slave select line is active low.
 */
void SPI::SetSlaveSelect(DigitalOutput &ss, tFrameMode mode, bool activeLow)
{
	SetSlaveSelect(&ss, mode, activeLow);
}

/**
 * Get the slave select line behavior.
 *
 * @param mode Frame mode:
 *			   kChipSelect: active for the duration of the frame.
 *			   kPreLatchPulse: pulses before the transfer of each frame.
 *			   kPostLatchPulse: pulses after the transfer of each frame.
 *			   kPreAndPostLatchPulse: pulses before and after each frame.
 * @param activeLow True if slave select line is active low.
 * @return The slave select digital output.
 */
DigitalOutput *SPI::GetSlaveSelect(tFrameMode *mode, bool *activeLow)
{
	if (mode != NULL)
	{
		*mode = (tFrameMode) (m_config.LatchFirst | (m_config.LatchLast << 1));
	}
	if (activeLow != NULL)
	{
		*activeLow = m_config.FramePolarity != 0;
	}
	return m_ss;
}

/**
 * Configure the clock output line to be active low.
 * This is sometimes called clock polarity high.
 */
void SPI::SetClockActiveLow()
{
	m_config.ClockPolarity = 1;
}

/**
 * Configure the clock output line to be active high.
 * This is sometimes called clock polarity low.
 */
void SPI::SetClockActiveHigh()
{
	m_config.ClockPolarity = 0;
}

/**
 * Apply configuration settings and reset the SPI logic.
 */
void SPI::ApplyConfig()
{
	Synchronized sync(m_semaphore);

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_spi->writeConfig(m_config, &localStatus);
	m_spi->writeChannels(m_channels, &localStatus);
	m_spi->strobeReset(&localStatus);
	wpi_setError(localStatus);
}

/**
 * Get the number of words that can currently be stored before being
 * transmitted to the device.
 *
 * @return The number of words available to be written.
 */
uint16_t SPI::GetOutputFIFOAvailable()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint16_t result = m_spi->readAvailableToLoad(&localStatus);
	wpi_setError(localStatus);
	return result;
}

/**
 * Get the number of words received and currently available to be read from
 * the receive FIFO.
 *
 * @return The number of words available to read.
 */
uint16_t SPI::GetNumReceived()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint16_t result = m_spi->readReceivedElements(&localStatus);
	wpi_setError(localStatus);
	return result;
}

/**
 * Have all pending transfers completed?
 *
 * @return True if no transfers are pending.
 */
bool SPI::IsDone()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool result = m_spi->readStatus_Idle(&localStatus);
	wpi_setError(localStatus);
	return result;
}

/**
 * Determine if the receive FIFO was full when attempting to add new data at
 * end of a transfer.
 *
 * @return True if the receive FIFO overflowed.
 */
bool SPI::HadReceiveOverflow()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	bool result = m_spi->readStatus_ReceivedDataOverflow(&localStatus);
	wpi_setError(localStatus);
	return result;
}

/**
 * Write a word to the slave device.  Blocks until there is space in the
 * output FIFO.
 *
 * If not running in output only mode, also saves the data received
 * on the MISO input during the transfer into the receive FIFO.
 */
void SPI::Write(uint32_t data)
{
	if (m_channels.MOSI_Channel == 0 && m_channels.MOSI_Module == 0)
	{
		wpi_setWPIError(SPIWriteNoMOSI);
		return;
	}

	Synchronized sync(m_semaphore);

	while (GetOutputFIFOAvailable() == 0)
		taskDelay(NO_WAIT);

	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_spi->writeDataToLoad(data, &localStatus);
	m_spi->strobeLoad(&localStatus);
	wpi_setError(localStatus);
}

/**
 * Read a word from the receive FIFO.
 *
 * Waits for the current transfer to complete if the receive FIFO is empty.
 *
 * If the receive FIFO is empty, there is no active transfer, and initiate
 * is false, errors.
 *
 * @param initiate	If true, this function pushes "0" into the
 *				    transmit buffer and initiates a transfer.
 *				    If false, this function assumes that data is
 *				    already in the receive FIFO from a previous write.
 */
uint32_t SPI::Read(bool initiate)
{
	if (m_channels.MISO_Channel == 0 && m_channels.MISO_Module == 0)
	{
		wpi_setWPIError(SPIReadNoMISO);
		return 0;
	}

	tRioStatusCode localStatus = NiFpga_Status_Success;
	uint32_t data;
	{
		Synchronized sync(m_semaphore);

		if (initiate)
		{
			m_spi->writeDataToLoad(0, &localStatus);
			m_spi->strobeLoad(&localStatus);
		}

		// Do we have anything ready to read?
		if (GetNumReceived() == 0)
		{
			if (!initiate && IsDone() && GetOutputFIFOAvailable() == kTransmitFIFODepth)
			{
				// Nothing to read: error out
				wpi_setWPIError(SPIReadNoData);
				return 0;
			}

			// Wait for the transaction to complete
			while (GetNumReceived() == 0)
				taskDelay(NO_WAIT);
		}

		m_spi->strobeReadReceivedData(&localStatus);
		data = m_spi->readReceivedData(&localStatus);
	}
	wpi_setError(localStatus);

	return data;
}

/**
 * Stop any transfer in progress and empty the transmit FIFO.
 */
void SPI::Reset()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_spi->strobeReset(&localStatus);
	wpi_setError(localStatus);
}

/**
 * Empty the receive FIFO.
 */
void SPI::ClearReceivedData()
{
	tRioStatusCode localStatus = NiFpga_Status_Success;
	m_spi->strobeClearReceivedData(&localStatus);
	wpi_setError(localStatus);
}
